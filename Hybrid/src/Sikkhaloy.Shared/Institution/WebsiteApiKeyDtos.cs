namespace Sikkhaloy.Shared.Institution;

public sealed class WebsiteApiKeyDto
{
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public bool HasKey { get; set; }
    public string? ApiKey { get; set; }
    public string? AllowedHosts { get; set; }
    public string? UpdatedAtUtc { get; set; }
}

public sealed class WebsiteApiKeySaveRequest
{
    public string? AllowedHosts { get; set; }
    public bool Regenerate { get; set; } = true;
}

public sealed class WebsiteApiKeyResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public WebsiteApiKeyDto? Data { get; set; }
}
