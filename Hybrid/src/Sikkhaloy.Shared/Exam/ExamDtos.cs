namespace Sikkhaloy.Shared.Exam;

public sealed class ExamResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public int Id { get; set; }
    public int Count { get; set; }
}

public sealed class ExamOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public sealed class ExamNameDto
{
    public int ExamID { get; set; }
    public string ExamName { get; set; } = "";
    public DateTime? Period_StartDate { get; set; }
    public DateTime? Period_EndDate { get; set; }
}

public sealed class SaveExamNameRequest
{
    public string ExamName { get; set; } = "";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public sealed class SubExamDto
{
    public int SubExamID { get; set; }
    public string SubExamName { get; set; } = "";
    public int Sub_ExamSN { get; set; }
}

public sealed class SaveSubExamRequest
{
    public string SubExamName { get; set; } = "";
    public int Serial { get; set; }
}

public sealed class GradeBandDto
{
    public int GradingID { get; set; }
    public double MaxPercentage { get; set; }
    public double MinPercentage { get; set; }
    public string Grades { get; set; } = "";
    public double Point { get; set; }
    public string? Comments { get; set; }
}

public sealed class GradeSystemDto
{
    public int GradeNameID { get; set; }
    public string GradeName { get; set; } = "";
    public List<GradeBandDto> Bands { get; set; } = [];
}

public sealed class SaveGradeSystemRequest
{
    public string GradeName { get; set; } = "";
    public List<GradeBandDto> Bands { get; set; } = [];
}

public sealed class PassMarkRowDto
{
    public int ExamFullMarksID { get; set; }
    public string SubjectName { get; set; } = "";
    public string? SubExamName { get; set; }
    public double FullMarks { get; set; }
    public double Sub_PassMarks { get; set; }
}

public sealed class SavePassMarksRequest
{
    public int ClassID { get; set; }
    public int ExamID { get; set; }
    public List<PassMarkRowDto> Rows { get; set; } = [];
}

public sealed class DistSubMarkDto
{
    public int SubExamID { get; set; }
    public string SubExamName { get; set; } = "";
    public bool Selected { get; set; }
    public double? FullMarks { get; set; }
}

public sealed class DistSubjectDto
{
    public int SubjectID { get; set; }
    public string SubjectName { get; set; } = "";
    public bool UseSubExam { get; set; }
    public double? FullMarks { get; set; }
    public List<DistSubMarkDto> SubExams { get; set; } = [];
}

public sealed class DistSheetDto
{
    public int GradeNameID { get; set; }
    public List<ExamOptionDto> Grades { get; set; } = [];
    public List<DistSubjectDto> Subjects { get; set; } = [];
}

public sealed class SaveDistributionRequest
{
    public int ClassID { get; set; }
    public int ExamID { get; set; }
    public int GradeNameID { get; set; }
    public List<DistSubjectDto> Subjects { get; set; } = [];
}

public sealed class CopyDistributionRequest
{
    public int ClassID { get; set; }
    public int FromExamID { get; set; }
    public int ToExamID { get; set; }
}

public sealed class CollectPaperDto
{
    public bool HasMarks { get; set; }
    public string ExamName { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string SubjectName { get; set; } = "";
    public List<string> Columns { get; set; } = [];
    public List<CollectStudentDto> Students { get; set; } = [];
}

public sealed class CollectStudentDto
{
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string FathersName { get; set; } = "";
    public string RollNo { get; set; } = "";
}

public sealed class InputSubMarkDto
{
    public int SubExamID { get; set; }
    public string SubExamName { get; set; } = "";
    public double FullMark { get; set; }
    public double PassMark { get; set; }
    public double PassPercentage { get; set; }
    public double? MarksObtained { get; set; }
    public bool Absent { get; set; }
}

public sealed class InputStudentDto
{
    public int StudentID { get; set; }
    public int StudentClassID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string RollNo { get; set; } = "";
    public double? MarksObtained { get; set; }
    public bool Absent { get; set; }
    public List<InputSubMarkDto> Subs { get; set; } = [];
}

public sealed class InputSheetDto
{
    public bool HasSubExams { get; set; }
    public double FullMark { get; set; }
    public double PassMark { get; set; }
    public double PassPercentage { get; set; }
    public List<InputSubMarkDto> SubHeaders { get; set; } = [];
    public List<InputStudentDto> Students { get; set; } = [];
}

public sealed class SaveInputMarksRequest
{
    public int ClassID { get; set; }
    public int ExamID { get; set; }
    public int SubjectID { get; set; }
    public int SubExamID { get; set; }
    public bool AllSubExams { get; set; }
    public List<InputStudentDto> Students { get; set; } = [];
}

public sealed class MarksCheckSubDto
{
    public string SubExamName { get; set; } = "";
    public int TotalStudent { get; set; }
}

public sealed class MarksCheckRowDto
{
    public int SubjectID { get; set; }
    public string SubjectName { get; set; } = "";
    public int TotalStudent { get; set; }
    public List<MarksCheckSubDto> Subs { get; set; } = [];
}

public sealed class ExamControlRowDto
{
    public int ClassID { get; set; }
    public string ClassName { get; set; } = "";
    public DateTime? LastPublished { get; set; }
    public bool MarksLocked { get; set; }
    public bool Published { get; set; }
}

public sealed class SaveExamControlRequest
{
    public int ExamID { get; set; }
    public bool Cumulative { get; set; }
    public List<ExamControlRowDto> Rows { get; set; } = [];
}

public sealed class SaveGradeCommentRequest
{
    public string? Comments { get; set; }
}

public sealed class ExamFilterDto
{
    public List<ExamOptionDto> Classes { get; set; } = [];
    public List<ExamOptionDto> Exams { get; set; } = [];
    public List<ExamOptionDto> SubExams { get; set; } = [];
    public List<ExamOptionDto> Groups { get; set; } = [];
    public List<ExamOptionDto> Sections { get; set; } = [];
    public List<ExamOptionDto> Shifts { get; set; } = [];
    public List<ExamOptionDto> Subjects { get; set; } = [];
    public List<ExamOptionDto> Grades { get; set; } = [];
    public List<ExamOptionDto> CumulativeExams { get; set; } = [];
    public List<ExamOptionDto> CopyToExams { get; set; } = [];
}
