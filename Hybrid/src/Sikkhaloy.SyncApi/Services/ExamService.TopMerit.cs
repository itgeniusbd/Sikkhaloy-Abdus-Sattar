using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Exam;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class ExamService
{
    public async Task<ExamTopMeritSheetDto> GetTopMeritAsync(SessionSnapshot session, int examId, string? mode, CancellationToken ct)
    {
        var bySection = string.Equals(mode, "section", StringComparison.OrdinalIgnoreCase);
        var dto = new ExamTopMeritSheetDto { Mode = bySection ? "section" : "class" };
        if (examId <= 0) return dto;

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        dto.ExamName = await ScalarTextAsync(con, """
SELECT ExamName FROM Exam_Name
WHERE ExamID = @ExamID AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID
""", session, ct, c => c.Parameters.AddWithValue("@ExamID", examId));
        dto.EducationYear = await ScalarTextAsync(con, """
SELECT EducationYear FROM Education_Year
WHERE EducationYearID = @EducationYearID AND SchoolID = @SchoolID
""", session, ct);

        var sql = bySection ? """
SELECT
    cc.Class AS ClassName,
    cc.ClassID,
    ISNULL(cc.SN, 999) AS ClassSN,
    ISNULL(cs.Section, N'') AS SectionName,
    ISNULL(csg.SubjectGroup, N'') AS GroupName,
    s.ID,
    sc.RollNo,
    s.StudentsName,
    s.FathersName,
    ers.TotalExamObtainedMark_ofStudent AS TotalMark,
    ers.Average,
    ers.Student_Grade AS Grade,
    ers.Student_Point AS Point,
    TRY_CAST(ers.Position_InExam_Subsection AS INT) AS MeritPosition
FROM Exam_Result_of_Student ers
INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
INNER JOIN Student s ON sc.StudentID = s.StudentID
INNER JOIN CreateClass cc ON sc.ClassID = cc.ClassID
LEFT JOIN CreateSection cs ON sc.SectionID = cs.SectionID
LEFT JOIN CreateSubjectGroup csg ON sc.SubjectGroupID = csg.SubjectGroupID
WHERE ers.SchoolID = @SchoolID
  AND ers.EducationYearID = @EducationYearID
  AND ers.ExamID = @ExamID
  AND TRY_CAST(ers.Position_InExam_Subsection AS INT) BETWEEN 1 AND 3
ORDER BY ClassSN, cc.ClassID, SectionName, GroupName, MeritPosition
""" : """
SELECT
    cc.Class AS ClassName,
    cc.ClassID,
    ISNULL(cc.SN, 999) AS ClassSN,
    CAST(N'' AS nvarchar(100)) AS SectionName,
    CAST(N'' AS nvarchar(100)) AS GroupName,
    s.ID,
    sc.RollNo,
    s.StudentsName,
    s.FathersName,
    ers.TotalExamObtainedMark_ofStudent AS TotalMark,
    ers.Average,
    ers.Student_Grade AS Grade,
    ers.Student_Point AS Point,
    TRY_CAST(ers.Position_InExam_Class AS INT) AS MeritPosition
FROM Exam_Result_of_Student ers
INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
INNER JOIN Student s ON sc.StudentID = s.StudentID
INNER JOIN CreateClass cc ON sc.ClassID = cc.ClassID
WHERE ers.SchoolID = @SchoolID
  AND ers.EducationYearID = @EducationYearID
  AND ers.ExamID = @ExamID
  AND TRY_CAST(ers.Position_InExam_Class AS INT) BETWEEN 1 AND 3
ORDER BY ClassSN, cc.ClassID, MeritPosition
""";

        await using var cmd = new SqlCommand(sql, con);
        AddSession(cmd, session);
        cmd.Parameters.AddWithValue("@ExamID", examId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        ExamTopMeritGroupDto? current = null;
        var currentKey = "";
        while (await reader.ReadAsync(ct))
        {
            var position = ToInt(reader["MeritPosition"]);
            if (position is < 1 or > 3) continue;
            var className = Text(reader["ClassName"]);
            var sectionName = Text(reader["SectionName"]);
            var groupName = Text(reader["GroupName"]);
            var key = className + "\n" + sectionName + "\n" + groupName;
            if (current is null || currentKey != key)
            {
                current = new ExamTopMeritGroupDto
                {
                    ClassName = className,
                    SectionName = sectionName,
                    GroupName = groupName
                };
                currentKey = key;
                dto.Groups.Add(current);
            }
            current.Students.Add(new ExamTopMeritStudentDto
            {
                Position = position,
                ID = Text(reader["ID"]),
                RollNo = Text(reader["RollNo"]),
                Name = Text(reader["StudentsName"]),
                FatherName = Text(reader["FathersName"]),
                Total = MarkText(reader["TotalMark"]),
                Average = MarkText(reader["Average"]),
                Grade = Text(reader["Grade"]),
                Point = PointText(reader["Point"])
            });
        }
        return dto;
    }
}
