namespace Sikkhaloy.Shared.Support;

public sealed class SupportTitleDto
{
    public int SupportTitleID { get; set; }
    public string SupportTitle { get; set; } = "";
}

public sealed class SupportTicketDto
{
    public int SupportID { get; set; }
    public string Subject { get; set; } = "";
    public string Message { get; set; } = "";
    public bool IsRead { get; set; }
    public DateTime? SentDate { get; set; }
}

public sealed class SupportPageDto
{
    public List<SupportTitleDto> Titles { get; set; } = [];
    public List<SupportTicketDto> Tickets { get; set; } = [];
}

public sealed class SubmitSupportRequest
{
    public int SupportTitleID { get; set; }
    public string Message { get; set; } = "";
}

public sealed class SupportResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
}
