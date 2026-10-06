using Microsoft.Extensions.Logging;

namespace MarkazulUloomApp;

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

					// File upload (camera + gallery) and getUserMedia camera for our own pages
					if (handler is Microsoft.Maui.Handlers.WebViewHandler webViewHandler)
						webView.SetWebChromeClient(new MunaWebChromeClient(webViewHandler));
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
