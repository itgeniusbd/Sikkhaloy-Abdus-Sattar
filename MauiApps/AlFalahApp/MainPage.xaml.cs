namespace AlFalahApp;

public partial class MainPage : ContentPage
{
	private enum Tab { Home, Login, More }

	private bool _loaded;
	private Tab _tab = Tab.Home;
	// True while a logged-in web panel page (/Admin, /Member, ...) is shown: the site then draws its own
	// bottom nav, so the native tab bar is hidden; public pages (/, /login, /projects, ...) show it again.
	private bool _panel;
	private string? _currentUrl;
	private int _refreshId;

	// Injected into every page: tells the native pull-to-refresh whether a pull may start. Allowed only when
	// the page is scrolled to the very top, no popup/modal is open and the touch isn't inside a scrolled list.
	private const string PullScript =
		"(function(){if(window.__alfPull)return;window.__alfPull=1;var B=window.AlfalahPull;if(!B||!B.set)return;var last=null;" +
		"function send(v){if(v!==last){last=v;try{B.set(v?'1':'0');}catch(e){}}}" +
		"function top(){return (window.scrollY||document.documentElement.scrollTop||document.body.scrollTop||0)<=0;}" +
		"function modal(){return !!document.querySelector('dialog[open],[aria-modal=\"true\"]:not([hidden]),.modal.open,.modal.show,.sheet.open,.lightbox.open');}" +
		"function inner(e){for(;e&&e.nodeType===1&&e!==document.body&&e!==document.documentElement;e=e.parentElement){var s=getComputedStyle(e);" +
		"if(/(auto|scroll)/.test(s.overflowY)&&e.scrollHeight>e.clientHeight+1&&e.scrollTop>0)return true;}return false;}" +
		"function base(){send(top()&&!modal());}" +
		"var o={capture:true,passive:true};" +
		"document.addEventListener('touchstart',function(ev){send(top()&&!modal()&&!inner(ev.target));},o);" +
		"document.addEventListener('touchend',base,o);document.addEventListener('touchcancel',base,o);window.addEventListener('scroll',base,o);" +
		"base();})();";

	public MainPage()
	{
		InitializeComponent();
		Wire(TabHome, () => Go(InstitutionConfig.HomeUrl, Tab.Home));
		Wire(TabLogin, () => Go(InstitutionConfig.LoginUrl, Tab.Login));
		Wire(TabMore, ToggleMore);
		foreach (var item in InstitutionConfig.MoreItems)
		{
			var url = item.Url;
			var link = new Label
			{
				Text = item.Title,
				FontSize = 16,
				TextColor = Color.FromArgb("#0c4a2a"),
				BackgroundColor = Color.FromArgb("#f4f8f6"),
				Padding = new Thickness(14, 12)
			};
			Wire(link, () => Go(url, Tab.More));
			MoreLinks.Add(link);
		}
		ShowTab(Tab.Home);
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (_loaded)
			return;

		_loaded = true;
		Browser.Source = InstitutionConfig.HomeUrl;
	}

	private void ToggleMore()
	{
		MoreSheet.IsVisible = !MoreSheet.IsVisible;
		if (MoreSheet.IsVisible)
			ShowTab(Tab.More);
	}

	private void OnCloseMore(object? sender, TappedEventArgs e) => MoreSheet.IsVisible = false;

	private void Go(string url, Tab tab)
	{
		MoreSheet.IsVisible = false;
		ShowTab(tab);
		Browser.Source = url;
	}

	private void OnNavigating(object? sender, WebNavigatingEventArgs e)
	{
		// while pull-to-refresh is spinning, its own spinner is enough
		var show = !Refresher.IsRefreshing;
		LoadingIndicator.IsVisible = show;
		LoadingIndicator.IsRunning = show;
	}

	private void OnRefreshing(object? sender, EventArgs e)
	{
		MoreSheet.IsVisible = false;
		var url = _currentUrl;
		if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
			url = InstitutionConfig.HomeUrl;

		// Reload the current page with a plain GET (never re-posts a form); drop any #fragment so it really reloads.
		var hash = url.IndexOf('#');
		if (hash >= 0)
			url = url[..hash];
		Browser.Source = new UrlWebViewSource { Url = url };

		// safety net: never leave the spinner running if no Navigated event arrives
		var id = ++_refreshId;
		Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(20), () =>
		{
			if (id == _refreshId && Refresher.IsRefreshing)
				Refresher.IsRefreshing = false;
		});
	}

	private async void OnNavigated(object? sender, WebNavigatedEventArgs e)
	{
		LoadingIndicator.IsVisible = false;
		LoadingIndicator.IsRunning = false;
		if (Refresher.IsRefreshing)
			Refresher.IsRefreshing = false; // finished or failed: stop the pull-to-refresh spinner
		_currentUrl = e.Url;

		// First guess from the URL, so the bar switches immediately.
		var panelByPath = IsPanelPath(e.Url);
		SetPanelMode(panelByPath);
		if (!_panel && !MoreSheet.IsVisible)
			ShowTab(TabFor(e.Url));

		try
		{
			await Browser.EvaluateJavaScriptAsync(
				"(function(){var s=document.getElementById('alfalah-app');if(!s){s=document.createElement('style');s.id='alfalah-app';s.textContent='.hp-bottom{display:none!important}';document.head.appendChild(s);}})();");
			await Browser.EvaluateJavaScriptAsync(PullScript);

			// Then confirm from the page itself: the site's logged-in layout has <body class="with-nav"> and
			// its own <nav class="tabbar"> (also on /Receipt/{id} etc.), public pages don't.
			var r = await Browser.EvaluateJavaScriptAsync(
				"(function(){var b=document.body;return ((b&&b.classList.contains('with-nav'))||document.querySelector('nav.tabbar'))?'1':'0';})()");
			if (!string.IsNullOrEmpty(r))
				SetPanelMode(panelByPath || r.Contains('1'));
		}
		catch
		{
		}
	}

	private void SetPanelMode(bool panel)
	{
		_panel = panel;
		if (panel)
			MoreSheet.IsVisible = false;
		if (TabBar.IsVisible == panel)
			TabBar.IsVisible = !panel; // the Auto row collapses, so the WebView takes the freed space
	}

	private static bool IsPanelPath(string? url)
	{
		if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var u))
			return false;

		var p = u.AbsolutePath;
		return HasSegmentPrefix(p, "/admin") || HasSegmentPrefix(p, "/member"); // not the public /members page
	}

	private static bool IsPanelHome(string? url)
	{
		if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var u))
			return false;

		var p = u.AbsolutePath.TrimEnd('/');
		return p.Equals("/admin", StringComparison.OrdinalIgnoreCase) || p.Equals("/member", StringComparison.OrdinalIgnoreCase);
	}

	private static bool HasSegmentPrefix(string path, string prefix) =>
		path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
		&& (path.Length == prefix.Length || path[prefix.Length] == '/');

	protected override bool OnBackButtonPressed()
	{
		if (MoreSheet.IsVisible)
		{
			MoreSheet.IsVisible = false;
			return true;
		}

		// On the panel's home (dashboard) going "back" would land on the login form or a pre-login page,
		// so leave the app instead (it stays logged in).
		if (_panel && IsPanelHome(_currentUrl))
			return base.OnBackButtonPressed();

		if (Browser.CanGoBack)
		{
			Browser.GoBack();
			return true;
		}

		return base.OnBackButtonPressed();
	}

	private static void Wire(Label label, Action action)
	{
		label.FontSize = 15;
		label.HorizontalTextAlignment = TextAlignment.Center;
		label.VerticalTextAlignment = TextAlignment.Center;
		label.Padding = new Thickness(4, 10);
		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) => action();
		label.GestureRecognizers.Add(tap);
	}

	private void ShowTab(Tab tab)
	{
		_tab = tab;
		Mark(TabHome, tab == Tab.Home);
		Mark(TabLogin, tab == Tab.Login);
		Mark(TabMore, tab == Tab.More);
	}

	private static void Mark(Label label, bool on)
	{
		label.TextColor = on ? Color.FromArgb("#f3d98b") : Colors.White;
		label.FontAttributes = on ? FontAttributes.Bold : FontAttributes.None;
		label.BackgroundColor = on ? Color.FromArgb("#136b43") : Colors.Transparent;
	}

	private static Tab TabFor(string? url)
	{
		if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var u))
			return Tab.Home;

		var path = u.AbsolutePath.TrimEnd('/');
		if (path.Equals("/login", StringComparison.OrdinalIgnoreCase))
			return Tab.Login;

		if (path.StartsWith("/members", StringComparison.OrdinalIgnoreCase)
			|| path.StartsWith("/committee", StringComparison.OrdinalIgnoreCase)
			|| path.StartsWith("/advisors", StringComparison.OrdinalIgnoreCase)
			|| path.StartsWith("/projects", StringComparison.OrdinalIgnoreCase)
			|| path.StartsWith("/project", StringComparison.OrdinalIgnoreCase))
			return Tab.More;

		if (u.Fragment is "#about" or "#projects" or "#notices" or "#join" or "#contact")
			return Tab.More;

		return Tab.Home;
	}
}
