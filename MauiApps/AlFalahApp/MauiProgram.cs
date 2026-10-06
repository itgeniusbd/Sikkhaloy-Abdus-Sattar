using Microsoft.Extensions.Logging;

namespace AlFalahApp;

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
				try
				{
					var webView = handler.PlatformView;
					if (webView?.Settings is null)
						return;

					webView.Settings.JavaScriptEnabled = true;
					webView.Settings.DomStorageEnabled = true;
					webView.Settings.LoadsImagesAutomatically = true;
					webView.Settings.MediaPlaybackRequiresUserGesture = false;
					webView.Settings.SetSupportMultipleWindows(false);
					webView.Settings.MixedContentMode = Android.Webkit.MixedContentHandling.CompatibilityMode;
					// lets the page tell pull-to-refresh whether a pull may start (top of page, no popup/inner scroll)
					webView.AddJavascriptInterface(new PullBridge(), PullBridge.Name);
				}
				catch
				{
					// Ignore platform WebView tweaks if device WebView is restricted
				}
			});
#endif

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
