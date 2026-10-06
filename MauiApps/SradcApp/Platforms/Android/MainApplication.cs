using Android.App;
using Android.Runtime;

namespace SradcApp;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
		// A stray exception in a Java callback (WebView, download, print...) must not close the app.
		AndroidEnvironment.UnhandledExceptionRaiser += (_, e) =>
		{
			Android.Util.Log.Error("App", "Unhandled: " + e.Exception);
			e.Handled = true;
		};
		TaskScheduler.UnobservedTaskException += (_, e) =>
		{
			Android.Util.Log.Error("App", "Unobserved task: " + e.Exception);
			e.SetObserved();
		};
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
