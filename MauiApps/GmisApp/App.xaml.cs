namespace GmisApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// Direct page avoids Shell startup crashes on some Android devices
		return new Window(new MainPage());
	}
}
