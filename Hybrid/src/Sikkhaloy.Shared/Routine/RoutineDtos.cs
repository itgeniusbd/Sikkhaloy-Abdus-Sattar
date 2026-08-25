namespace Sikkhaloy.Shared.Routine;

public sealed class RoutineResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
    public int Count { get; set; }
}

public sealed class RoutineNameDto
{
    public int RoutineInfoID { get; set; }
    public string Name { get; set; } = "";
    public bool Used { get; set; }
}

public sealed class SaveRoutineNameRequest
{
    public int RoutineInfoID { get; set; }
    public string Name { get; set; } = "";
}

public sealed class RoutinePeriodDraft
{
    public string Period { get; set; } = "";
    public string StartTime { get; set; } = "";
    public string EndTime { get; set; } = "";
    public string Duration { get; set; } = "";
    public bool OffTime { get; set; }
}

public sealed class CreateClassRoutineRequest
{
    public int RoutineInfoID { get; set; }
    public List<string> Days { get; set; } = [];
    public List<RoutinePeriodDraft> Periods { get; set; } = [];
}

public sealed class RoutinePeriodDto
{
    public int RoutineTimeID { get; set; }
    public string Period { get; set; } = "";
    public string StartTime { get; set; } = "";
    public string EndTime { get; set; } = "";
    public string TimeRange { get; set; } = "";
    public string Duration { get; set; } = "";
    public bool OffTime { get; set; }
}

public sealed class RoutineOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public sealed class RoutineCellDto
{
    public int RoutineTimeID { get; set; }
    public int RoutineForClassID { get; set; }
    public int SubjectID { get; set; }
    public int TeacherID { get; set; }
    public string SubjectName { get; set; } = "";
    public string TeacherName { get; set; } = "";
    public string StartTime { get; set; } = "";
    public string EndTime { get; set; } = "";
    public bool OffTime { get; set; }
    public List<RoutineOptionDto> Teachers { get; set; } = [];
}

public sealed class RoutineDayRowDto
{
    public int RoutineDayID { get; set; }
    public string Day { get; set; } = "";
    public List<RoutineCellDto> Cells { get; set; } = [];
}

public sealed class ClassRoutineSheetDto
{
    public List<RoutineNameDto> Routines { get; set; } = [];
    public List<RoutinePeriodDto> Periods { get; set; } = [];
    public List<RoutineDayRowDto> Days { get; set; } = [];
    public List<RoutineOptionDto> Subjects { get; set; } = [];
    public string Title { get; set; } = "";
}

public sealed class AssignRoutineCell
{
    public string Day { get; set; } = "";
    public int RoutineTimeID { get; set; }
    public int RoutineForClassID { get; set; }
    public int SubjectID { get; set; }
    public int TeacherID { get; set; }
    public string StartTime { get; set; } = "";
    public string EndTime { get; set; } = "";
}

public sealed class AssignRoutineRequest
{
    public int ClassID { get; set; }
    public int RoutineInfoID { get; set; }
    public int GroupID { get; set; }
    public int SectionID { get; set; }
    public int ShiftID { get; set; }
    public List<AssignRoutineCell> Cells { get; set; } = [];
}

public sealed class ExamRoutineListItemDto
{
    public int RoutineID { get; set; }
    public string Name { get; set; } = "";
    public DateTime CreatedDate { get; set; }
}

public sealed class ExamRoutineCellDto
{
    public int ColumnIndex { get; set; }
    public int SubjectID { get; set; }
    public string SubjectText { get; set; } = "";
}

public sealed class ExamRoutineRowDto
{
    public string ExamDate { get; set; } = "";
    public string DayName { get; set; } = "";
    public string StartTime { get; set; } = "";
    public string EndTime { get; set; } = "";
    public string Duration { get; set; } = "";
    public List<ExamRoutineCellDto> Cells { get; set; } = [];
}

public sealed class ExamRoutineColumnDto
{
    public int ColumnIndex { get; set; }
    public int ClassID { get; set; }
    public string ClassName { get; set; } = "";
    public List<RoutineOptionDto> Subjects { get; set; } = [];
}

public sealed class ExamRoutineSheetDto
{
    public int RoutineID { get; set; }
    public string RoutineName { get; set; } = "";
    public List<ExamRoutineListItemDto> Saved { get; set; } = [];
    public List<RoutineOptionDto> Classes { get; set; } = [];
    public List<ExamRoutineColumnDto> Columns { get; set; } = [];
    public List<ExamRoutineRowDto> Rows { get; set; } = [];
}

public sealed class SaveExamRoutineRequest
{
    public int RoutineID { get; set; }
    public string RoutineName { get; set; } = "";
    public List<ExamRoutineColumnDto> Columns { get; set; } = [];
    public List<ExamRoutineRowDto> Rows { get; set; } = [];
}
