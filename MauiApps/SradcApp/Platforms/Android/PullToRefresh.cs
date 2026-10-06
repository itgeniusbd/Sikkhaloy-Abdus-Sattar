using Android.Content;
using Android.Views;
using Android.Webkit;
using Java.Interop;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;
using AWebView = Android.Webkit.WebView;

namespace SradcApp;

/// <summary>Shared pull-to-refresh state, written by the page (via <see cref="PullBridge"/>) and read by the swipe layout.</summary>
internal static class PullState
{
	/// <summary>False while the page says a pull must not start (popup open, touch in a scrolled inner list, not at top).</summary>
	public static volatile bool PageAllows = true;

	/// <summary>Raised (on a background thread) when the page reports panel mode / URL changes.</summary>
	public static event Action<bool, string?>? PageStateChanged;

	public static void RaisePageState(bool panel, string? url) => PageStateChanged?.Invoke(panel, url);
}

/// <summary>
/// JavaScript interface <c>window.SradcBridge</c>:
/// <c>set('1' | '0')</c> – whether a native pull-to-refresh may start;
/// <c>state('1' | '0', url)</c> – the page reports logged-in panel mode and its URL (Blazor navigates
/// with history.pushState, which raises no WebView Navigated event).
/// </summary>
public sealed class PullBridge : Java.Lang.Object
{
	public const string Name = "SradcBridge";

	[JavascriptInterface]
	[Export("set")]
	public void Set(string? allow) => PullState.PageAllows = allow == "1";

	[JavascriptInterface]
	[Export("state")]
	public void State(string? panel, string? url) => PullState.RaisePageState(panel == "1", url);

	/// <summary>Called from the page when <c>window.print()</c> runs (we override it in injected JS).</summary>
	[JavascriptInterface]
	[Export("print")]
	public void Print() => MainThread.BeginInvokeOnMainThread(() => AppPrint.PrintCurrentWebView());
}

/// <summary>RefreshView handler that uses <see cref="WebSwipeRefreshLayout"/> (native SwipeRefreshLayout).</summary>
public class WebRefreshViewHandler : RefreshViewHandler
{
	protected override MauiSwipeRefreshLayout CreatePlatformView() => new WebSwipeRefreshLayout(Context);
}

/// <summary>
/// SwipeRefreshLayout that only starts a pull when the WebView is scrolled to the very top
/// (ScrollY == 0) and the page itself allows it.
/// </summary>
public class WebSwipeRefreshLayout : MauiSwipeRefreshLayout
{
	private AWebView? _web;

	public WebSwipeRefreshLayout(Context context) : base(context)
	{
		try
		{
			// Longer pull before refresh fires (reduces accidental refreshes while scrolling up)
			var density = Resources?.DisplayMetrics?.Density ?? 2f;
			SetDistanceToTriggerSync((int)(140 * density));
		}
		catch { }
	}

	public override bool OnInterceptTouchEvent(Android.Views.MotionEvent? ev)
	{
		// Never steal the gesture unless the page (JS) says we are at the absolute top
		if (!PullState.PageAllows)
			return false;
		if (!IsWebAtTop())
			return false;
		return base.OnInterceptTouchEvent(ev);
	}

	public override bool CanChildScrollUp()
	{
		// "true" = child can still scroll up → SwipeRefreshLayout must NOT start a refresh
		if (!PullState.PageAllows)
			return true;
		return !IsWebAtTop();
	}

	bool IsWebAtTop()
	{
		if (_web is null || _web.Handle == IntPtr.Zero || _web.Parent is null)
			_web = FindWebView(this);
		var web = _web;
		if (web is null)
			return false;
		try
		{
			if (web.ScrollY > 0)
				return false;
			if (web.CanScrollVertically(-1))
				return false;
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static AWebView? FindWebView(AView? view)
	{
		if (view is AWebView web)
			return web;
		if (view is AViewGroup group)
		{
			for (var i = 0; i < group.ChildCount; i++)
			{
				var found = FindWebView(group.GetChildAt(i));
				if (found is not null)
					return found;
			}
		}
		return null;
	}
}
