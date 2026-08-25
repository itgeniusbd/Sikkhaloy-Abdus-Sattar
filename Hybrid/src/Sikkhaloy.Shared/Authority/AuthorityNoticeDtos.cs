namespace Sikkhaloy.Shared.Authority;

public sealed class AuthNoticeDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Notice { get; set; } = "";
    public DateTime? ShowDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? InsertDate { get; set; }
}

public sealed class AuthNoticeSaveRequest
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Notice { get; set; } = "";
    public string From { get; set; } = "";
    public string To { get; set; } = "";
}

public sealed class AuthNoticeIdRequest
{
    public int Id { get; set; }
}

public sealed class AuthUnreadDto
{
    public int Count { get; set; }
}

public sealed class AuthMessagePageDto
{
    public int Unread { get; set; }
    public List<AuthSupportRowDto> Support { get; set; } = [];
    public List<AuthContactRowDto> Contact { get; set; } = [];
}

public sealed class AuthSupportRowDto
{
    public int Id { get; set; }
    public string SchoolName { get; set; } = "";
    public string Name { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTime? SentDate { get; set; }
    public bool IsRead { get; set; }
}

public sealed class AuthContactRowDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Mobile { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTime? SentDate { get; set; }
    public bool IsRead { get; set; }
}

public sealed class AuthMessageReadRequest
{
    public string Kind { get; set; } = "";
    public int Id { get; set; }
}
