using Microsoft.Extensions.Logging;

namespace DascApp;

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
				}
				catch
				{
				}
			});
#endif

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
