namespace HybridSikkhaloyApp;

public static class InstitutionConfig
{
	public const string AppName = "হাইব্রিড শিক্ষালয়";
	public const string AppNameEn = "Hybrid Sikkhaloy";
	public const string HomeUrl = "https://hybrid.sikkhaloy.com/";
	public const string LoginUrl = "https://hybrid.sikkhaloy.com/login";
	public const string Website = "https://sikkhaloy.com/";

	// Support numbers from hybrid.sikkhaloy.com/#contact (IT Genius)
	public const string SupportBrand = "IT GENIUS";
	public const string OfficePhoneDisplay = "09638 66 99 66";
	public const string OfficePhoneTel = "tel:09638669966";
	public const string MobilePhoneDisplay = "01739 144 141";
	public const string MobilePhoneTel = "tel:+8801739144141";
	public const string WhatsAppDisplay = "01739 144 141";
	public const string WhatsAppUrl = "https://wa.me/8801739144141";
	public const string SupportHours = "10:00 AM to 5:00 PM support available";
	public const string SupportHoursBn = "সকাল ১০টা থেকে বিকেল ৫টা পর্যন্ত সাপোর্ট";

	/// <summary>
	/// Hosts that open inside the app. Everything else (facebook, youtube, other sites) opens in the
	/// phone's browser/apps. sikkhaloy.com and all its sub-domains are allowed (hybrid., hybridapi., www.).
	/// </summary>
	public static readonly string[] AllowedHostSuffixes =
	[
		"sikkhaloy.com",
		// online fee payment gateways: kept in-app so the payment returns to the portal session
		"sslcommerz.com", "bka.sh", "bkash.com", "mynagad.com", "nagad.com.bd",
		"shurjopayment.com", "aamarpay.com", "portwallet.com",
		// WhatsApp deep links stay outside (wa.me is opened externally)
	];

	/// <summary>More sheet: site sections only. Contact opens the native Contact tab.</summary>
	public static readonly MenuLink[] MoreItems =
	[
		new("ফিচারসমূহ", "https://hybrid.sikkhaloy.com/#features"),
		new("সমাধান", "https://hybrid.sikkhaloy.com/#solutions"),
		new("আমাদের গ্রাহক", "https://hybrid.sikkhaloy.com/#customers"),
		new("যোগাযোগ", "app:contact"), // special: opens the Contact tab
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
	public MenuLink(string title, string url)
	{
		Title = title;
		Url = url;
	}

	public string Title { get; }
	public string Url { get; }
}
