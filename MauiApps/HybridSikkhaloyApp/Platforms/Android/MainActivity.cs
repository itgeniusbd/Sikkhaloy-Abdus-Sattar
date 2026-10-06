using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace HybridSikkhaloyApp;

[Activity(
	Theme = "@style/Maui.SplashTheme",
	MainLauncher = true,
	LaunchMode = LaunchMode.SingleTop,
	ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);

		try
		{
			Window?.SetSoftInputMode(SoftInput.AdjustResize);
#if DEBUG
			Android.Webkit.WebView.SetWebContentsDebuggingEnabled(true);
#endif
		}
		catch
		{
		}
	}

	protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
	{
		base.OnActivityResult(requestCode, resultCode, data);
		// Result of the WebView file chooser / camera (see AppWebChromeClient)
		AppWebChromeClient.HandleActivityResult(requestCode, resultCode, data);
	}
}
