namespace Sikkhaloy.Shared.Students;

public sealed class StudentFeedbackRecipientDto
{
    public int RegistrationID { get; set; }
    public string Name { get; set; } = "";
    public string Designation { get; set; } = "";
    public string Category { get; set; } = "";
}

public sealed class StudentFeedbackReplyDto
{
    public int ReplyID { get; set; }
    public int FeedbackID { get; set; }
    public int FromRegistrationID { get; set; }
    public string FromName { get; set; } = "";
    public string FromDesignation { get; set; } = "";
    public string FromCategory { get; set; } = "";
    public string Body { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public sealed class StudentFeedbackItemDto
{
    public int FeedbackID { get; set; }
    public string Kind { get; set; } = "";
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public string Status { get; set; } = "Pending";
    public int ToRegistrationID { get; set; }
    public string ToName { get; set; } = "";
    public string ToDesignation { get; set; } = "";
    public string ToCategory { get; set; } = "";
    public int AssignedByRegistrationID { get; set; }
    public string AssignedByName { get; set; } = "";
    public string AssignedByDesignation { get; set; } = "";
    public int ProcessingByRegistrationID { get; set; }
    public string ProcessingByName { get; set; } = "";
    public string ProcessingByDesignation { get; set; } = "";
    public DateTime? ProcessingAt { get; set; }
    public int SolvedByRegistrationID { get; set; }
    public string SolvedByName { get; set; } = "";
    public string SolvedByDesignation { get; set; } = "";
    public DateTime? SolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string StudentName { get; set; } = "";
    public string StudentCode { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string SectionName { get; set; } = "";
    public int ReplyCount { get; set; }
    public int UnreadReplyCount { get; set; }
    public List<StudentFeedbackReplyDto> Replies { get; set; } = [];
}

public sealed class SubmitStudentFeedbackRequest
{
    public string Kind { get; set; } = "Complaint";
    public int ToRegistrationID { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
}

public sealed class StudentFeedbackStatusRequest
{
    public string Status { get; set; } = "";
}

public sealed class AssignStudentFeedbackRequest
{
    public int ToRegistrationID { get; set; }
}

public sealed class StudentFeedbackReplyRequest
{
    public string Body { get; set; } = "";
}

public sealed class StudentFeedbackResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public int FeedbackID { get; set; }
    public int ReplyID { get; set; }
}

public sealed class StudentFeedbackUnreadDto
{
    public int Count { get; set; }
    public bool IsMainAdmin { get; set; }
}
