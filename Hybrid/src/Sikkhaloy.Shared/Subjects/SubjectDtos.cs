namespace Sikkhaloy.Shared.Subjects;

public sealed class SubjectDto
{
    public int SubjectID { get; set; }
    public string SubjectName { get; set; } = "";
    public int? SN { get; set; }
}

public sealed class SaveSubjectRequest
{
    public string SubjectName { get; set; } = "";
}

public sealed class SubjectSerialItem
{
    public int SubjectID { get; set; }
    public int? SN { get; set; }
}

public sealed class SaveSubjectSerialsRequest
{
    public List<SubjectSerialItem> Items { get; set; } = [];
}

public sealed class SubjectResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public int SubjectID { get; set; }
}
