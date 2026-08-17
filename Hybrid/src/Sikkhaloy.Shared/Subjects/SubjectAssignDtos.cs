namespace Sikkhaloy.Shared.Subjects;

public sealed class ClassSubjectRowDto
{
    public int SubjectID { get; set; }
    public string SubjectName { get; set; } = "";
    public bool Selected { get; set; }
    public string? SubjectType { get; set; }
}

public sealed class ClassSubjectChoice
{
    public int SubjectID { get; set; }
    public string SubjectType { get; set; } = "";
}

public sealed class SaveClassSubjectsRequest
{
    public int ClassID { get; set; }
    public int SubjectGroupID { get; set; }
    public List<ClassSubjectChoice> Items { get; set; } = [];
}

public sealed class SubjectAssignResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
}
