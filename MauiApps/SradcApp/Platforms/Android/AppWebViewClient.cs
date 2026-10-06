using Android.Net.Http;
using Android.Webkit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AWebView = Android.Webkit.WebView;

namespace SradcApp;

/// <summary>Load signals from the Android WebView to the page (raised on the UI thread).</summary>
internal static class WebLoadEvents
{
	public static event Action<int>? Progress;
	public static event Action? ContentVisible;
	public static event Action<string, string?>? MainFrameError; // kind ("ssl"), url
	public static event Action? RendererGone;

	public static void RaiseProgress(int p) { try { Progress?.Invoke(p); } catch { } }
	public static void RaiseContentVisible() { try { ContentVisible?.Invoke(); } catch { } }
	public static void RaiseMainFrameError(string kind, string? url) { try { MainFrameError?.Invoke(kind, url); } catch { } }
	public static void RaiseRendererGone() { try { RendererGone?.Invoke(); } catch { } }
}

/// <summary>
/// MAUI's WebViewClient (keeps Navigating/Navigated) plus:
///  - first-paint signal (OnPageCommitVisible) so the loading spinner goes away as soon as the page is visible;
///  - TLS errors are refused (never proceed) and reported, so the app shows a retry page instead of a blank screen;
///  - a crashed/killed WebView renderer no longer takes the whole app down (Android kills the app when this returns false).
/// </summary>
public class AppWebViewClient : MauiWebViewClient
{
	public AppWebViewClient(WebViewHandler handler) : base(handler)
	{
	}

	public override void OnPageCommitVisible(AWebView? view, string? url)
	{
		try { base.OnPageCommitVisible(view, url); } catch { }
		WebLoadEvents.RaiseContentVisible();
	}

	public override void OnReceivedSslError(AWebView? view, SslErrorHandler? handler, SslError? error)
	{
		try { handler?.Cancel(); } catch { }
		WebLoadEvents.RaiseMainFrameError("ssl", error?.Url);
	}

	public override bool OnRenderProcessGone(AWebView? view, RenderProcessGoneDetail? detail)
	{
		WebLoadEvents.RaiseRendererGone();
		return true; // handled: the page recreates the WebView
	}
}
