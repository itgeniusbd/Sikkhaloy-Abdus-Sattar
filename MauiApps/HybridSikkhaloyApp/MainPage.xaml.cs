namespace HybridSikkhaloyApp;

public partial class MainPage : ContentPage
{
	private enum Tab { Home, Login, Contact, More }

	private const string HybridHost = "hybrid.sikkhaloy.com";

	private bool _loaded;
	private Tab _tab = Tab.Home;
	// True while the logged-in portal is shown: the site then draws its own navigation, so the native
	// tab bar is hidden; public pages (/, /login, ...) show it again.
	private bool _panel;
	// host+path of the first portal page after login (the dashboard): "back" there leaves the app
	// instead of returning to the login form.
	private string? _panelHome;
	private string? _currentUrl;
	private string? _failedUrl;
	private int _refreshId;

	// Startup / load failsafe: never leave the user on a blank or splash-like screen.
	private const int LoadTimeoutSeconds = 15;
	private int _loadId;
	private bool _contentShown;
	private bool _loadError;
	private string? _loadingUrl;
	private WebView? _web;
	private WebView Web => _web ??= Browser;

	// Paths on hybrid.sikkhaloy.com that are public (everything else there is the logged-in portal).
	private static readonly string[] PublicPaths =
	[
		"", "/login", "/logout", "/forgot", "/forgot-password", "/reset-password", "/register", "/signup",
		"/privacy", "/privacy-policy", "/terms", "/about", "/contact",
	];

	// Injected into every page (window.HybridApp bridge):
	//  - tells the native pull-to-refresh whether a pull may start: only at the very top of the page, no
	//    popup open, touch not inside a scrolled list, and not inside the portal (it has its own refresh);
	//  - reports portal mode + URL whenever the DOM/URL changes (Blazor navigates without page loads).
	private const string PageScript =
		"(function(){if(window.__hsApp)return;window.__hsApp=1;var B=window.HybridApp;if(!B||!B.set)return;var last=null,ls=null;" +
		"function send(v){if(v!==last){last=v;try{B.set(v?'1':'0');}catch(e){}}}" +
		// Absolute top only: window/document AND every nested scrollable (portal lists, etc.)
		"function top(){var y=(window.scrollY||document.documentElement.scrollTop||document.body.scrollTop||0);if(y>1)return false;" +
		"var nodes=document.querySelectorAll('.app-content,.sp-main,main,.overflow-auto,.overflow-y-auto,[data-scroll]');" +
		"for(var i=0;i<nodes.length;i++){if((nodes[i].scrollTop||0)>1)return false;}" +
		"var all=document.querySelectorAll('*');for(var j=0;j<Math.min(all.length,80);j++){var el=all[j];if(!el||el===document.body||el===document.documentElement)continue;" +
		"try{var s=getComputedStyle(el);if(/(auto|scroll)/.test(s.overflowY)&&el.scrollHeight>el.clientHeight+2&&el.scrollTop>1)return false;}catch(e){}}" +
		"return true;}" +
		"function modal(){return !!document.querySelector('dialog[open],[aria-modal=\"true\"]:not([hidden]),.modal.open,.modal.show,.sheet.open,.lightbox.open');}" +
		"function sitePtr(){return !!document.querySelector('.app-content,.sp-main');}" +
		"function panel(){return !!document.querySelector('.sp-app,.sp-bottom,.app-content,.sp-main');}" +
		"function inner(e){for(;e&&e.nodeType===1&&e!==document.body&&e!==document.documentElement;e=e.parentElement){var s=getComputedStyle(e);" +
		"if(/(auto|scroll)/.test(s.overflowY)&&e.scrollHeight>e.clientHeight+1&&e.scrollTop>0)return true;}return false;}" +
		"function base(){send(top()&&!modal()&&!sitePtr());}" +
		"function report(){var p=panel()?'1':'0',u=location.href,k=p+'|'+u;if(k!==ls){ls=k;try{B.state(p,u);}catch(e){}}base();}" +
		"var o={capture:true,passive:true};" +
		"document.addEventListener('touchstart',function(ev){send(top()&&!modal()&&!sitePtr()&&!inner(ev.target));},o);" +
		"document.addEventListener('touchmove',function(){if(!top())send(false);},o);" +
		"document.addEventListener('touchend',base,o);document.addEventListener('touchcancel',base,o);" +
		"window.addEventListener('scroll',function(){send(top()&&!modal()&&!sitePtr());},o);" +
		"document.addEventListener('scroll',function(){send(top()&&!modal()&&!sitePtr());},o);" +
		"try{new MutationObserver(function(){clearTimeout(window.__hsT);window.__hsT=setTimeout(report,150);}).observe(document.documentElement,{childList:true,subtree:true});}catch(e){}" +
		"window.addEventListener('popstate',report);window.addEventListener('hashchange',report);setInterval(report,800);" +
		// Print: route window.print to the native PrintManager
		"try{window.print=function(){try{B.print();}catch(e){}};}catch(e){}" +
		"report();})();";

	public MainPage()
	{
		InitializeComponent();
		WireTab(TabHome, () => Go(InstitutionConfig.HomeUrl, Tab.Home));
		WireTab(TabLogin, () => Go(InstitutionConfig.LoginUrl, Tab.Login));
		WireTab(TabContact, ShowContact);
		WireTab(TabMore, ToggleMore);
		foreach (var item in InstitutionConfig.MoreItems)
		{
			var url = item.Url;
			var link = new Label
			{
				Text = item.Title,
				FontSize = 16,
				TextColor = Color.FromArgb("#0B6E2E"),
				BackgroundColor = Color.FromArgb("#F1F8F3"),
				Padding = new Thickness(14, 12)
			};
			if (url == "app:contact")
				Wire(link, ShowContact);
			else
				Wire(link, () => Go(url, Tab.More));
			MoreLinks.Add(link);
		}
		ShowTab(Tab.Home);

#if ANDROID
		PullState.PageStateChanged += OnPageState;
		WebLoadEvents.Progress += OnLoadProgress;
		WebLoadEvents.ContentVisible += OnContentVisible;
		WebLoadEvents.MainFrameError += OnMainFrameError;
		WebLoadEvents.RendererGone += OnRendererGone;
#endif
		Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (_loaded)
			return;

		_loaded = true;
		// Always try to load: the network state can be misreported (VPN, Chromebook, captive Wi-Fi...).
		// A real failure shows the retry page (Navigated failure or the 15 s failsafe).
		Load(InstitutionConfig.HomeUrl);
	}

	/// <summary>Loads a URL in the WebView and arms the 15 s failsafe.</summary>
	private void Load(string url)
	{
		try
		{
			StartLoadWatch(url);
			Web.Source = new UrlWebViewSource { Url = url };
		}
		catch
		{
			ShowOffline(url);
		}
	}

	private void StartLoadWatch(string? url)
	{
		_contentShown = false;
		_loadError = false;
		_loadingUrl = url;
		var id = ++_loadId;
		SetProgress(5);
		Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(LoadTimeoutSeconds), () =>
		{
			if (id != _loadId || _contentShown || OfflineView.IsVisible || ContactView.IsVisible)
				return;
			ShowOffline(url ?? _currentUrl, timedOut: true);
		});
	}

	private void MarkContentShown()
	{
		_contentShown = true;
		LoadingIndicator.IsVisible = false;
		LoadingIndicator.IsRunning = false;
	}

	private void SetProgress(int p)
	{
		try
		{
			if (p >= 100)
			{
				PageProgress.IsVisible = false;
				PageProgress.Progress = 0;
				return;
			}
			PageProgress.IsVisible = true;
			PageProgress.Progress = Math.Clamp(p, 5, 100) / 100.0;
		}
		catch
		{
		}
	}

	private void OnLoadProgress(int p)
	{
		Dispatcher.Dispatch(() =>
		{
			SetProgress(p);
			if (p >= 70 && !_loadError)
				MarkContentShown();
		});
	}

	private void OnContentVisible() => Dispatcher.Dispatch(() =>
	{
		if (!_loadError)
			MarkContentShown();
	});

	private void OnMainFrameError(string kind, string? url)
	{
		Dispatcher.Dispatch(() =>
		{
			// only the page itself counts (a broken image on another host must not block the page)
			if (!SameHost(url, _loadingUrl ?? _currentUrl))
				return;
			_loadError = true;
			ShowOffline(_loadingUrl ?? url, ssl: kind == "ssl");
		});
	}

	private static bool SameHost(string? a, string? b) =>
		Uri.TryCreate(a, UriKind.Absolute, out var ua) && Uri.TryCreate(b, UriKind.Absolute, out var ub)
		&& string.Equals(ua.Host, ub.Host, StringComparison.OrdinalIgnoreCase);

	// Android killed the WebView renderer (low memory / WebView update): build a fresh WebView and reload.
	private void OnRendererGone()
	{
		Dispatcher.Dispatch(() =>
		{
			try
			{
				var url = _currentUrl ?? InstitutionConfig.HomeUrl;
				var old = _web ?? Browser;
				old.Navigating -= OnNavigating;
				old.Navigated -= OnNavigated;
				var fresh = new WebView { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill };
				fresh.Navigating += OnNavigating;
				fresh.Navigated += OnNavigated;
				Refresher.Content = fresh;
				_web = fresh;
				try { old.Handler?.DisconnectHandler(); } catch { }
				Load(url);
			}
			catch
			{
				ShowOffline(_currentUrl);
			}
		});
	}

	// ------------------------------------------------------------------ navigation

	private void ToggleMore()
	{
		ContactView.IsVisible = false;
		MoreSheet.IsVisible = !MoreSheet.IsVisible;
		if (MoreSheet.IsVisible)
			ShowTab(Tab.More);
	}

	private void OnCloseMore(object? sender, TappedEventArgs e) => MoreSheet.IsVisible = false;

	private void Go(string url, Tab tab)
	{
		MoreSheet.IsVisible = false;
		ContactView.IsVisible = false;
		if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && !IsInApp(uri))
		{
			OpenExternal(uri); // tel:, mailto:, other sites
			return;
		}

		OfflineView.IsVisible = false;
		ShowTab(tab);
		Load(url);
	}

	private void ShowContact()
	{
		MoreSheet.IsVisible = false;
		OfflineView.IsVisible = false;
		ContactView.IsVisible = true;
		ShowTab(Tab.Contact);
	}

	private void OnCallOffice(object? sender, TappedEventArgs e) => OpenExternal(new Uri(InstitutionConfig.OfficePhoneTel));
	private void OnCallMobile(object? sender, TappedEventArgs e) => OpenExternal(new Uri(InstitutionConfig.MobilePhoneTel));
	private void OnWhatsApp(object? sender, TappedEventArgs e) => OpenExternal(new Uri(InstitutionConfig.WhatsAppUrl));

	private static bool IsInApp(Uri uri) =>
		(uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp) && InstitutionConfig.IsAllowedHost(uri.Host);

	private void OnNavigating(object? sender, WebNavigatingEventArgs e)
	{
		if (Uri.TryCreate(e.Url, UriKind.Absolute, out var uri))
		{
			var scheme = uri.Scheme.ToLowerInvariant();
			if (scheme is "http" or "https")
			{
				if (!InstitutionConfig.IsAllowedHost(uri.Host))
				{
					e.Cancel = true; // other websites open in the phone's browser / apps
					OpenExternal(uri);
					return;
				}

				if (scheme == "http")
				{
					// cleartext is disabled: upgrade our own links to https
					e.Cancel = true;
					var https = new UriBuilder(uri) { Scheme = Uri.UriSchemeHttps, Port = -1 }.Uri.ToString();
					Dispatcher.Dispatch(() => Load(https));
					return;
				}
			}
			else if (scheme is not ("about" or "data" or "blob" or "javascript" or "file"))
			{
				e.Cancel = true; // tel:, mailto:, sms:, whatsapp:, intent:, market: ...
				OpenExternal(uri);
				return;
			}
		}

		// a new page is starting: arm the failsafe (also covers links tapped inside the page)
		if (!e.Cancel && (e.NavigationEvent == WebNavigationEvent.NewPage || e.NavigationEvent == WebNavigationEvent.Refresh)
			&& !string.Equals(e.Url, _loadingUrl, StringComparison.OrdinalIgnoreCase))
			StartLoadWatch(e.Url);

		// while pull-to-refresh is spinning, its own spinner is enough
		var show = !Refresher.IsRefreshing;
		LoadingIndicator.IsVisible = show;
		LoadingIndicator.IsRunning = show;
	}

	private static async void OpenExternal(Uri uri)
	{
		try
		{
			if (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
				await Microsoft.Maui.ApplicationModel.Browser.Default.OpenAsync(uri, BrowserLaunchMode.External);
			else
				await Launcher.Default.OpenAsync(uri);
		}
		catch
		{
			// no app can handle this link
		}
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
		OfflineView.IsVisible = false;
		Load(url);

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
		SetProgress(100);
		if (Refresher.IsRefreshing)
			Refresher.IsRefreshing = false; // finished or failed: stop the pull-to-refresh spinner

		if (e.Result == WebNavigationResult.Cancel)
			return;

		if (e.Result is WebNavigationResult.Failure or WebNavigationResult.Timeout)
		{
			_loadId++; // failsafe no longer needed: the retry page is shown now
			ShowOffline(e.Url);
			return;
		}

		if (_loadError)
			return; // TLS error page stays (the WebView reports an empty page as "finished")

		_contentShown = true;
		_loadId++;

		OfflineView.IsVisible = false;
		_failedUrl = null;
		_currentUrl = e.Url;

		// First guess from the URL, so the bar switches immediately; the injected script confirms from the page.
		SetPanelMode(IsPanelPath(e.Url));
		if (!_panel && !MoreSheet.IsVisible)
		{
			var tab = TabFor(e.Url);
			if (tab == Tab.Contact)
				ShowContact();
			else
			{
				ContactView.IsVisible = false;
				ShowTab(tab);
			}
		}

		try
		{
			await Web.EvaluateJavaScriptAsync(PageScript);
		}
		catch
		{
		}
	}

	// Called from the page script (background thread) when portal mode or the URL changes.
	private void OnPageState(bool panelInDom, string? url)
	{
		try
		{
			Dispatcher.Dispatch(() =>
			{
				if (OfflineView.IsVisible)
					return;
				if (!string.IsNullOrWhiteSpace(url))
					_currentUrl = url;
				SetPanelMode(panelInDom || IsPanelPath(_currentUrl));
				if (!_panel && !MoreSheet.IsVisible && !ContactView.IsVisible)
					ShowTab(TabFor(_currentUrl));
			});
		}
		catch
		{
		}
	}

	private void SetPanelMode(bool panel)
	{
		if (panel && !_panel)
			_panelHome = Key(_currentUrl);
		else if (!panel)
			_panelHome = null;

		_panel = panel;
		if (panel)
			MoreSheet.IsVisible = false;
		if (TabBarBorder.IsVisible == panel)
		{
			TabBarBorder.IsVisible = !panel; // the Auto row collapses, so the WebView takes the freed space
			if (panel)
			{
				ContactView.IsVisible = false;
				MoreSheet.IsVisible = false;
			}
		}
	}

	private static bool IsPanelPath(string? url)
	{
		if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var u))
			return false;
		if (!u.Host.Equals(HybridHost, StringComparison.OrdinalIgnoreCase))
			return false;

		var p = u.AbsolutePath.TrimEnd('/');
		foreach (var pub in PublicPaths)
		{
			if (p.Equals(pub, StringComparison.OrdinalIgnoreCase))
				return false;
		}
		// framework/static files are never pages
		return !p.StartsWith("/_", StringComparison.Ordinal);
	}

	private static string? Key(string? url)
	{
		if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var u))
			return null;
		return (u.Host + u.AbsolutePath.TrimEnd('/')).ToLowerInvariant();
	}

	protected override bool OnBackButtonPressed()
	{
		if (MoreSheet.IsVisible)
		{
			MoreSheet.IsVisible = false;
			return true;
		}

		if (ContactView.IsVisible)
		{
			ContactView.IsVisible = false;
			ShowTab(Tab.Home);
			return true;
		}

		if (OfflineView.IsVisible)
		{
			if (Web.CanGoBack)
			{
				OfflineView.IsVisible = false;
				Web.GoBack();
				return true;
			}
			return base.OnBackButtonPressed();
		}

		// On the portal's home (dashboard) going "back" would land on the login form or a pre-login page,
		// so leave the app instead (it stays logged in).
		if (_panel && _panelHome is not null && _panelHome == Key(_currentUrl))
			return base.OnBackButtonPressed();

		if (Web.CanGoBack)
		{
			Web.GoBack();
			return true;
		}

		return base.OnBackButtonPressed();
	}

	// ------------------------------------------------------------------ offline / error page

	private void ShowOffline(string? url, bool timedOut = false, bool ssl = false)
	{
		_failedUrl = !string.IsNullOrWhiteSpace(url) ? url : _currentUrl ?? InstitutionConfig.HomeUrl;
		LoadingIndicator.IsVisible = false;
		LoadingIndicator.IsRunning = false;
		SetProgress(100);
		MoreSheet.IsVisible = false;
		if (Refresher.IsRefreshing)
			Refresher.IsRefreshing = false;

		bool offline;
		try { offline = Connectivity.Current.NetworkAccess == NetworkAccess.None; } catch { offline = false; }

		if (ssl)
		{
			OfflineTitle.Text = "নিরাপদ সংযোগ করা যায়নি";
			OfflineMessage.Text = "সার্ভারের সাথে নিরাপদ (HTTPS) সংযোগ স্থাপন করা যায়নি। ফোনের তারিখ/সময় ঠিক আছে কিনা দেখে আবার চেষ্টা করুন।\nA secure connection could not be made. Please check the device date/time and try again.";
		}
		else if (timedOut && !offline)
		{
			OfflineTitle.Text = "পেজ লোড হতে দেরি হচ্ছে";
			OfflineMessage.Text = "সংযোগ ধীর বা সার্ভার সাড়া দিচ্ছে না। আবার চেষ্টা করুন।\nThe page is taking too long to load. Please check your connection and tap Retry.";
		}
		else if (offline)
		{
			OfflineTitle.Text = "ইন্টারনেট সংযোগ নেই";
			OfflineMessage.Text = "অনুগ্রহ করে মোবাইল ডেটা বা Wi-Fi চালু করে আবার চেষ্টা করুন।\nNo internet connection. Please check your connection and try again.";
		}
		else
		{
			OfflineTitle.Text = "পেজটি লোড করা যায়নি";
			OfflineMessage.Text = "সার্ভারের সাথে সংযোগ করা যাচ্ছে না। একটু পরে আবার চেষ্টা করুন।\nThe page could not be loaded. Please try again.";
		}
		OfflineView.IsVisible = true;
	}

	private void OnRetryClicked(object? sender, EventArgs e) => Retry();

	private void Retry()
	{
		var url = _failedUrl ?? _currentUrl ?? InstitutionConfig.HomeUrl;

		OfflineView.IsVisible = false;
		LoadingIndicator.IsVisible = true;
		LoadingIndicator.IsRunning = true;
		Load(url);
	}

	private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
	{
		if (e.NetworkAccess == NetworkAccess.None)
			return;
		try
		{
			Dispatcher.Dispatch(() =>
			{
				if (OfflineView.IsVisible)
					Retry(); // connection is back: reload automatically
			});
		}
		catch
		{
		}
	}

	// ------------------------------------------------------------------ tab bar

	private static void Wire(Label label, Action action)
	{
		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) => action();
		label.GestureRecognizers.Add(tap);
	}

	private static void WireTab(Border tab, Action action)
	{
		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) => action();
		tab.GestureRecognizers.Add(tap);
	}

	// Font Awesome glyphs (square vector icons — Size sets both axes, AspectFit, never stretched)
	private const string GlyphHome = "\uf015";      // house
	private const string GlyphLogin = "\uf2f6";     // right-to-bracket
	private const string GlyphContact = "\uf2b9";   // address-book (distinct from phone cards)
	private const string GlyphMore = "\uf0c9";      // bars

	private void ShowTab(Tab tab)
	{
		_tab = tab;
		MarkTab(TabHome, LblHome, IconHome, GlyphHome, tab == Tab.Home);
		MarkTab(TabLogin, LblLogin, IconLogin, GlyphLogin, tab == Tab.Login);
		MarkTab(TabContact, LblContact, IconContact, GlyphContact, tab == Tab.Contact);
		MarkTab(TabMore, LblMore, IconMore, GlyphMore, tab == Tab.More);
	}

	private static void MarkTab(Border tab, Label label, Image icon, string glyph, bool on)
	{
		var green = Color.FromArgb("#0B6E2E");
		label.TextColor = on ? green : Colors.White;
		label.FontAttributes = on ? FontAttributes.Bold : FontAttributes.None;
		tab.BackgroundColor = on ? Colors.White : Colors.Transparent;
		icon.Aspect = Aspect.AspectFit;
		icon.WidthRequest = 22;
		icon.HeightRequest = 22;
		icon.Source = new FontImageSource
		{
			FontFamily = "FASolid",
			Glyph = glyph,
			Size = 22,
			Color = on ? green : Colors.White
		};
	}

	private static Tab TabFor(string? url)
	{
		if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var u))
			return Tab.Home;

		if (!u.Host.Equals(HybridHost, StringComparison.OrdinalIgnoreCase))
			return Tab.More; // sikkhaloy.com website etc. (opened from "আরো")

		var path = u.AbsolutePath.TrimEnd('/');
		if (path.Equals("/login", StringComparison.OrdinalIgnoreCase))
			return Tab.Login;

		if (u.Fragment is "#features" or "#solutions" or "#customers")
			return Tab.More;
		if (u.Fragment == "#contact")
			return Tab.Contact;

		return Tab.Home;
	}
}
