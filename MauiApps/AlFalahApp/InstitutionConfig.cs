namespace AlFalahApp;

public static class InstitutionConfig
{
	public const string AppName = "আল-ফালাহ সেবা";
	public const string HomeUrl = "https://alfalah.qrsheba.com/";
	public const string LoginUrl = "https://alfalah.qrsheba.com/login";

	public static readonly MenuLink[] MoreItems =
	[
		new("আমাদের সম্পর্কে", "https://alfalah.qrsheba.com/#about"),
		new("প্রকল্প", "https://alfalah.qrsheba.com/projects"),
		new("নোটিশ", "https://alfalah.qrsheba.com/#notices"),
		new("সদস্য", "https://alfalah.qrsheba.com/members"),
		new("পরিচালনা পর্ষদ", "https://alfalah.qrsheba.com/committee"),
		new("উপদেষ্টা", "https://alfalah.qrsheba.com/advisors"),
		new("সদস্য হোন", "https://alfalah.qrsheba.com/#join"),
		new("যোগাযোগ", "https://alfalah.qrsheba.com/#contact"),
	];
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
