namespace SradcApp;

public static class InstitutionConfig
{
	public const string AppName = "সানারপাড় রওশন আরা ডিগ্রি কলেজ";
	public const string AppNameEn = "Shanarpar Rowshan Ara Degree College";
	public const string ShortName = "SRADC";
	public const string HomeUrl = "https://sradc.edu.bd/";
	public const string LoginUrl = "https://hybrid.sikkhaloy.com/login";
	public const string Website = "https://sradc.edu.bd/";

	// Contact details from sradc.edu.bd/contact ("Need Help?")
	public const string PhoneDisplay = "01309112476";
	public const string PhoneTel = "tel:01309112476";
	public const string WhatsAppDisplay = "01309112476";
	public const string WhatsAppUrl = "https://wa.me/8801309112476";
	public const string Address = "Shanarpar, Siddhirganj, Narayanganj, Code #2509, EIIN #112476";
	public const string Email = "rowshanaracollege7@gmail.com";
	public const string EmailUri = "mailto:rowshanaracollege7@gmail.com";

	/// <summary>
	/// Hosts that open inside the app. Everything else (facebook, youtube, other sites) opens in the
	/// phone's browser/apps. The college website and sikkhaloy.com (+ all sub-domains, e.g. the
	/// hybrid.sikkhaloy.com portal used for login / online payment) are allowed.
	/// </summary>
	public static readonly string[] AllowedHostSuffixes =
	[
		"sradc.edu.bd",
		"sikkhaloy.com",
		// online fee payment gateways: kept in-app so the payment returns to the portal session
		"sslcommerz.com", "bka.sh", "bkash.com", "mynagad.com", "nagad.com.bd",
		"shurjopayment.com", "aamarpay.com", "portwallet.com",
		// WhatsApp deep links stay outside (wa.me is opened externally)
	];

	/// <summary>More sheet: sections of sradc.edu.bd (from the site's menu). Contact opens the native Contact tab.</summary>
	public static readonly MenuLink[] MoreItems =
	[
		new("আমাদের সম্পর্কে", "https://sradc.edu.bd/p/glance", "\uf05a"),          // circle-info
		new("ইতিহাস", "https://sradc.edu.bd/p/history", "\uf1da"),                  // clock-rotate-left
		new("অর্জন", "https://sradc.edu.bd/p/achievement", "\uf091"),               // trophy
		new("সভাপতির বাণী", "https://sradc.edu.bd/p/chairman-message", "\uf27a"),   // message
		new("অধ্যক্ষের বাণী", "https://sradc.edu.bd/p/principal-message", "\uf27a"),
		new("প্রশাসন", "https://sradc.edu.bd/p/admin-body", "\uf0c0"),              // users
		new("শিক্ষক ও কর্মচারী", "https://sradc.edu.bd/teachers", "\uf51c"),        // chalkboard-user
		new("একাডেমিক নিয়মাবলি", "https://sradc.edu.bd/p/rules-regulations", "\uf02d"), // book
		new("ফলাফল", "https://sradc.edu.bd/p/results", "\uf080"),                   // chart-bar
		new("ভর্তি তথ্য", "https://sradc.edu.bd/p/admission-info", "\uf19d"),        // graduation-cap
		new("অনলাইনে আবেদন", "https://sradc.edu.bd/p/apply-online", "\uf044"),      // pen-to-square
		new("সুবিধাসমূহ", "https://sradc.edu.bd/p/facilities", "\uf1ad"),          // building
		new("নোটিশ", "https://sradc.edu.bd/notices", "\uf0a1"),                     // bullhorn
		new("ইভেন্ট", "https://sradc.edu.bd/events", "\uf073"),                     // calendar-days
		new("ফটো গ্যালারি", "https://sradc.edu.bd/gallery", "\uf03e"),              // image
		new("ভিডিও গ্যালারি", "https://sradc.edu.bd/videos", "\uf03d"),             // video
		new("ক্যারিয়ার", "https://sradc.edu.bd/career", "\uf0b1"),                 // briefcase
		new("যোগাযোগ", "app:contact", "\uf2b9"), // special: opens the Contact tab
	];

	public static bool IsAllowedHost(string? host)
	{
		if (string.IsNullOrEmpty(host))
			return false;

		foreach (var s in AllowedHostSuffixes)
		{
			if (host.Equals(s, StringComparison.OrdinalIgnoreCase)
				|| host.EndsWith("." + s, StringComparison.OrdinalIgnoreCase))
				return true;
		}
		return false;
	}
}

public sealed class MenuLink
{
	public MenuLink(string title, string url, string glyph)
	{
		Title = title;
		Url = url;
		Glyph = glyph;
	}

	public string Title { get; }
	public string Url { get; }
	public string Glyph { get; }
}
