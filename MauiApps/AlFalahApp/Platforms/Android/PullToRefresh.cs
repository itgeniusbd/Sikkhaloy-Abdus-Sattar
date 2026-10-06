using Android.Content;
using Android.Webkit;
using Java.Interop;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;
using AWebView = Android.Webkit.WebView;

namespace AlFalahApp;

/// <summary>Shared pull-to-refresh state, written by the page (via <see cref="PullBridge"/>) and read by the swipe layout.</summary>
internal static class PullState
{
	/// <summary>False while the page says a pull must not start (popup open, touch in a scrolled inner list, not at top).</summary>
	public static volatile bool PageAllows = true;
}

/// <summary>JavaScript interface: <c>window.AlfalahPull.set('1' | '0')</c>.</summary>
public sealed class PullBridge : Java.Lang.Object
{
	public const string Name = "AlfalahPull";

	[JavascriptInterface]
	[Export("set")]
	public void Set(string? allow) => PullState.PageAllows = allow == "1";
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
	}

	public override bool CanChildScrollUp()
	{
		if (_web is null || _web.Handle == IntPtr.Zero || _web.Parent is null)
			_web = FindWebView(this);
		var web = _web;
		if (web is null)
			return base.CanChildScrollUp();

		// "true" = the child can still scroll up, so the layout must not start a refresh
		return web.ScrollY > 0 || web.CanScrollVertically(-1) || !PullState.PageAllows;
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
