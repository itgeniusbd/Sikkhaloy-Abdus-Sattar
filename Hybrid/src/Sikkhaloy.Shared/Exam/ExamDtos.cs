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
    public List<ExamOptionDto> Schedules { get; set; } = [];
}

public sealed class ExamPublishSubjectDto
{
    public int SubjectID { get; set; }
    public string SubjectName { get; set; } = "";
    public bool AddInExam { get; set; } = true;
    public double CountableMark { get; set; }
    public double DistFullMark { get; set; }
}

public sealed class ExamPublishSubExamDto
{
    public int SubjectID { get; set; }
    public int SubExamID { get; set; }
    public string SubjectName { get; set; } = "";
    public string SubExamName { get; set; } = "";
    public double AddPercentage { get; set; } = 100;
}

public class ExamPublishSettingDto
{
    public string PositionFormat { get; set; } = "Point";
    public bool HideSecPosition { get; set; }
    public bool HideClassPosition { get; set; }
    public bool HideFullMark { get; set; }
    public bool HidePassMark { get; set; }
    public int OptionalMode { get; set; }
    public double OptionalPercent { get; set; }
    public bool FailOptional { get; set; }
    public bool AddOptionalInFull { get; set; }
    public bool GradeOnGpa { get; set; } = true;
    public bool GradeAsItIs { get; set; }
    public bool SameCountable { get; set; } = true;
    public double CountableMark { get; set; }
    public bool SubExamFail { get; set; }
    public bool EqualSubExam { get; set; } = true;
    public DateTime? AttendanceFrom { get; set; }
    public DateTime? AttendanceTo { get; set; }
    public int ScheduleID { get; set; }
    public List<ExamPublishSubjectDto> Subjects { get; set; } = [];
    public List<ExamPublishSubExamDto> SubExams { get; set; } = [];
}

public sealed class ExamPublishRequest : ExamPublishSettingDto
{
    public int ClassID { get; set; }
    public int ExamID { get; set; }
}

public sealed class ExamDeleteResultRequest
{
    public int ExamID { get; set; }
    public int ClassID { get; set; }
    public int SubjectID { get; set; }
    public int SubExamID { get; set; }
}

public sealed class ExamMeritSubjectMarkDto
{
    public string Name { get; set; } = "";
    public string Mark { get; set; } = "";
    public string PassStatus { get; set; } = "";
    public string SubjectType { get; set; } = "";
}

public sealed class ExamMeritRowDto
{
    public int StudentResultID { get; set; }
    public int SubjectID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string RollNo { get; set; } = "";
    public double Total { get; set; }
    public string Grade { get; set; } = "";
    public double Point { get; set; }
    public string Average { get; set; } = "";
    public string PositionClass { get; set; } = "";
    public string PositionSection { get; set; } = "";
    public string PassStatus { get; set; } = "";
    public string SubjectName { get; set; } = "";
    public List<ExamMeritSubjectMarkDto> Marks { get; set; } = [];
}

public sealed class ExamMeritListDto
{
    public string Title { get; set; } = "";
    public List<ExamMeritRowDto> Rows { get; set; } = [];
}

public sealed class ExamGradeBandViewDto
{
    public string Marks { get; set; } = "";
    public string Grade { get; set; } = "";
    public double Point { get; set; }
    public string Comments { get; set; } = "";
}

public sealed class ExamCardSubMarkDto
{
    public int SubExamID { get; set; }
    public string SubExamName { get; set; } = "";
    public string Obtained { get; set; } = "";
    public double FullMark { get; set; }
    public double PassMark { get; set; }
}

public sealed class ExamCardSubjectDto
{
    public int SubjectID { get; set; }
    public string SubjectName { get; set; } = "";
    public string Obtained { get; set; } = "";
    public double FullMark { get; set; }
    public double PassMark { get; set; }
    public string Grade { get; set; } = "";
    public double Point { get; set; }
    public string PassStatus { get; set; } = "";
    public string PositionClass { get; set; } = "";
    public string PositionSection { get; set; } = "";
    public string HighestClass { get; set; } = "";
    public string HighestSection { get; set; } = "";
    public bool Failed { get; set; }
    public List<ExamCardSubMarkDto> Subs { get; set; } = [];
}

public class ExamCardStudentDto
{
    public int StudentResultID { get; set; }
    public int StudentID { get; set; }
    public int StudentClassID { get; set; }
    public string StudentsName { get; set; } = "";
    public string ID { get; set; } = "";
    public string RollNo { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string SectionName { get; set; } = "";
    public string ShiftName { get; set; } = "";
    public string GroupName { get; set; } = "";
    public string ExamName { get; set; } = "";
    public double Total { get; set; }
    public double FullTotal { get; set; }
    public string Grade { get; set; } = "";
    public double Point { get; set; }
    public string Average { get; set; } = "";
    public string Percentage { get; set; } = "";
    public string PositionClass { get; set; } = "";
    public string PositionSection { get; set; } = "";
    public string PassStatus { get; set; } = "";
    public string Comment { get; set; } = "";
    public string WorkingDays { get; set; } = "";
    public string PresentDays { get; set; } = "";
    public string AbsentDays { get; set; } = "";
    public string LateAbsDays { get; set; } = "";
    public string LeaveDays { get; set; } = "";
    public string LateDays { get; set; } = "";
    public List<ExamCardSubjectDto> Subjects { get; set; } = [];
}

public sealed class ExamResultCardSheetDto
{
    public string SchoolName { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public bool HideFullMark { get; set; }
    public bool HidePassMark { get; set; }
    public bool HideClassPosition { get; set; }
    public bool HideSecPosition { get; set; }
    public bool HasSections { get; set; }
    public bool HasSubExams { get; set; }
    public List<string> SubExamNames { get; set; } = [];
    public List<ExamGradeBandViewDto> Grades { get; set; } = [];
    public List<ExamCardStudentDto> Students { get; set; } = [];
}

public sealed class ExamStatRowDto
{
    public string Label { get; set; } = "";
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public sealed class ExamSubjectStatDto
{
    public string SubjectName { get; set; } = "";
    public List<ExamStatRowDto> Grades { get; set; } = [];
}

public sealed class ExamFailStudentDto
{
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string RollNo { get; set; } = "";
    public string FailedSubjects { get; set; } = "";
    public int FailCount { get; set; }
}

public sealed class SaveCumulativeNameRequest
{
    public string Name { get; set; } = "";
}

public sealed class CumulativeExamChoiceDto
{
    public int ExamID { get; set; }
    public string ExamName { get; set; } = "";
    public bool Selected { get; set; }
    public double Percentage { get; set; }
    public bool EnableFail { get; set; }
}

public class CumulativePublishSettingDto : ExamPublishSettingDto
{
    public int GradeNameID { get; set; }
    public bool HideSubExam { get; set; } = true;
    public List<CumulativeExamChoiceDto> Exams { get; set; } = [];
}

public sealed class CumulativePublishRequest : CumulativePublishSettingDto
{
    public int ClassID { get; set; }
    public int CumulativeNameID { get; set; }
}

public sealed class CumulativeExamColDto
{
    public int ExamID { get; set; }
    public string ExamName { get; set; } = "";
    public string Percentage { get; set; } = "";
}

public sealed class CumulativeCardExamMarkDto
{
    public int ExamID { get; set; }
    public string FullMark { get; set; } = "-";
    public string PassMark { get; set; } = "-";
    public string Obtained { get; set; } = "-";
}

public sealed class CumulativeCardSubjectDto
{
    public int SubjectID { get; set; }
    public string SubjectName { get; set; } = "";
    public List<CumulativeCardExamMarkDto> Exams { get; set; } = [];
    public string CuFull { get; set; } = "";
    public string CuObtained { get; set; } = "";
    public string Grade { get; set; } = "";
    public string Point { get; set; } = "";
    public string PositionClass { get; set; } = "";
    public string PositionSection { get; set; } = "";
    public string HighestClass { get; set; } = "";
    public string HighestSection { get; set; } = "";
}

public sealed class CumulativeCardStudentDto : ExamCardStudentDto
{
    public string HighestClass { get; set; } = "";
    public string HighestSection { get; set; } = "";
    public List<CumulativeCardSubjectDto> CuSubjects { get; set; } = [];
}

public sealed class CumulativeResultCardSheetDto
{
    public string SchoolName { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public bool HideClassPosition { get; set; }
    public bool HideSecPosition { get; set; }
    public bool HasSections { get; set; }
    public List<CumulativeExamColDto> Exams { get; set; } = [];
    public List<ExamGradeBandViewDto> Grades { get; set; } = [];
    public List<CumulativeCardStudentDto> Students { get; set; } = [];
}

public sealed class ExamAnalyticalDto
{
    public string SchoolName { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string ExamName { get; set; } = "";
    public int TotalStudents { get; set; }
    public int Passed { get; set; }
    public int Failed { get; set; }
    public List<ExamStatRowDto> GradeDistribution { get; set; } = [];
    public List<ExamSubjectStatDto> SubjectStats { get; set; } = [];
    public List<ExamStatRowDto> UnsuccessfulSummary { get; set; } = [];
    public List<ExamFailStudentDto> UnsuccessfulStudents { get; set; } = [];
}

public sealed class RandomSeatRequest
{
    public List<int> ClassIds { get; set; } = [];
    public string? GroupId { get; set; }
    public string? SectionId { get; set; }
    public string? ShiftId { get; set; }
}

public sealed class ExamPrintStudentDto
{
    public int StudentClassID { get; set; }
    public int ClassID { get; set; }
    public int StudentID { get; set; }
    public string ID { get; set; } = "";
    public string StudentsName { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string SectionName { get; set; } = "";
    public string ShiftName { get; set; } = "";
    public string GroupName { get; set; } = "";
    public string RollNo { get; set; } = "";
    public string SeatNo { get; set; } = "";
    public string EducationYear { get; set; } = "";
}

public sealed class ExamSeatPlanSheetDto
{
    public string SchoolName { get; set; } = "";
    public string ExamName { get; set; } = "";
    public List<ExamPrintStudentDto> Students { get; set; } = [];
}

public sealed class ExamAdmitCardSheetDto
{
    public string SchoolName { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public string ExamName { get; set; } = "";
    public string EducationYear { get; set; } = "";
    public string? TeacherSignDataUrl { get; set; }
    public string? PrincipalSignDataUrl { get; set; }
    public List<ExamPrintStudentDto> Students { get; set; } = [];
}

public sealed class SaveExamSignRequest
{
    public string Kind { get; set; } = "";
    public string Image { get; set; } = "";
}
