using Microsoft.Extensions.Logging;

namespace SradcApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				// Crisp vector icons (never stretched like the old hand-drawn PNGs)
				fonts.AddFont("fa-solid-900.ttf", "FASolid");
				fonts.AddFont("fa-brands-400.ttf", "FABrands");
			})
			.ConfigureMauiHandlers(handlers =>
			{
#if ANDROID
				// Pull-to-refresh: RefreshView backed by a SwipeRefreshLayout that only pulls at the top of the page.
				handlers.AddHandler<RefreshView, WebRefreshViewHandler>();
#endif
			});

#if ANDROID
		Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping(
			"SikkhaloyAndroidWebView",
			(handler, view) =>
			{
				var webView = handler.PlatformView;
				if (webView is null)
					return;

				// Each tweak is isolated: one failing setting must never stop the page from loading.
				Try(() =>
				{
					var st = webView.Settings;
					st.JavaScriptEnabled = true;
					st.DomStorageEnabled = true;
					st.LoadsImagesAutomatically = true;
					st.MediaPlaybackRequiresUserGesture = false;
					st.SetSupportMultipleWindows(false);
					st.MixedContentMode = Android.Webkit.MixedContentHandling.CompatibilityMode;
				});
				Try(() => Android.Webkit.CookieManager.Instance?.SetAcceptThirdPartyCookies(webView, true));
				// lets the page tell pull-to-refresh whether a pull may start (top of page, no popup/inner scroll)
				Try(() => webView.AddJavascriptInterface(new PullBridge(), PullBridge.Name));

				if (handler is Microsoft.Maui.Handlers.WebViewHandler webViewHandler)
				{
					// first-paint / TLS-error / renderer-crash handling (keeps MAUI's Navigating/Navigated)
					Try(() => webView.SetWebViewClient(new AppWebViewClient(webViewHandler)));
					// File upload (camera + gallery), getUserMedia camera, load progress
					Try(() => webView.SetWebChromeClient(new AppWebChromeClient(webViewHandler)));
				}

				// PDF / attachment downloads from the portal
				Try(() => webView.SetDownloadListener(new AppDownloadListener()));
			});
#endif

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	static void Try(Action action)
	{
		try
		{
			action();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine("WebView setup: " + ex);
		}
	}
}
