namespace SradcApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		try
		{
			return new Window(new MainPage());
		}
		catch (Exception ex)
		{
			return new Window(new ContentPage
			{
				BackgroundColor = Colors.White,
				Content = new ScrollView
				{
					Content = new Label
					{
						Text = ex.ToString(),
						TextColor = Colors.Black,
						Margin = 20
					}
				}
			});
		}
	}
}
