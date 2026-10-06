namespace GmisApp;

public partial class MainPage : ContentPage
{
	private bool _loaded;

	public MainPage()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (_loaded)
			return;

		_loaded = true;
		Browser.Source = InstitutionConfig.StartUrl;
	}

	private void OnNavigating(object? sender, WebNavigatingEventArgs e)
	{
		LoadingIndicator.IsVisible = true;
		LoadingIndicator.IsRunning = true;
	}

	private void OnNavigated(object? sender, WebNavigatedEventArgs e)
	{
		LoadingIndicator.IsVisible = false;
		LoadingIndicator.IsRunning = false;
	}

	protected override bool OnBackButtonPressed()
	{
		if (Browser.CanGoBack)
		{
			Browser.GoBack();
			return true;
		}

		return base.OnBackButtonPressed();
	}
}
