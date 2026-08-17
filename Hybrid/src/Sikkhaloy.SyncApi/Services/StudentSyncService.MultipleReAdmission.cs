using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Students;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class StudentSyncService
{
    public async Task<IReadOnlyList<ReAdmissionExamDto>> ListReAdmissionExamsAsync(
        SessionSnapshot session, int educationYearId, int classId, bool cumulative, CancellationToken cancellationToken)
    {
        if (educationYearId <= 0 || classId <= 0)
            return [];

        var sql = cumulative
            ? @"
SELECT DISTINCT n.CumulativeNameID AS ExamID, n.CumulativeResultName AS Name
FROM dbo.Exam_Cumulative_Setting AS s
INNER JOIN dbo.Exam_Cumulative_Name AS n ON s.CumulativeNameID = n.CumulativeNameID
WHERE s.SchoolID = @SchoolID AND s.EducationYearID = @EducationYearID AND s.ClassID = @ClassID
ORDER BY n.CumulativeResultName"
            : @"
SELECT DISTINCT e.ExamID, e.ExamName AS Name
FROM dbo.Exam_Publish_Setting AS p
INNER JOIN dbo.Exam_Name AS e ON p.ExamID = e.ExamID
WHERE p.SchoolID = @SchoolID AND p.EducationYearID = @EducationYearID AND p.ClassID = @ClassID
ORDER BY e.ExamName";

        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(cancellationToken);
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@EducationYearID", educationYearId);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            var items = new List<ReAdmissionExamDto>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                items.Add(new ReAdmissionExamDto
                {
                    ExamID = Convert.ToInt32(reader["ExamID"]),
                    Name = reader["Name"]?.ToString() ?? ""
                });
            }

            return items;
        }
        catch (Exception)
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<ReAdmissionPositionDto>> ListReAdmissionPositionsAsync(
        SessionSnapshot session,
        int educationYearId,
        int classId,
        int examId,
        bool cumulative,
        bool sectionWise,
        CancellationToken cancellationToken)
    {
        if (educationYearId <= 0 || classId <= 0 || examId <= 0)
            return [];

        var column = sectionWise ? "Position_InExam_Subsection" : "Position_InExam_Class";
        var sql = cumulative
            ? $@"
SELECT StudentID, StudentClassID, {column} AS Position
FROM dbo.Exam_Cumulative_Student
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID
  AND CumulativeNameID = @ExamID"
            : $@"
SELECT StudentID, StudentClassID, {column} AS Position
FROM dbo.Exam_Result_of_Student
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID
  AND ExamID = @ExamID";

        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(cancellationToken);
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@EducationYearID", educationYearId);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            var items = new List<ReAdmissionPositionDto>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                items.Add(new ReAdmissionPositionDto
                {
                    StudentID = Convert.ToInt32(reader["StudentID"]),
                    StudentClassID = Convert.ToInt32(reader["StudentClassID"]),
                    Position = reader["Position"] is DBNull or null ? null : Convert.ToString(reader["Position"])
                });
            }

            return items;
        }
        catch (Exception)
        {
            return [];
        }
    }

    public async Task<BulkReAdmissionResult> FinishBulkReAdmissionAsync(
        SessionSnapshot session, BulkReAdmissionRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.Students.Count == 0)
            return BulkFail("mreadm.needStudents");
        if (request.ToEducationYearID <= 0)
            return BulkFail("readm.needTarget");
        if (request.ClassID <= 0)
            return BulkFail("readm.needClass");

        var result = new BulkReAdmissionResult { Succeeded = true };
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);

        foreach (var item in request.Students)
        {
            if (item.StudentID <= 0 || item.FromStudentClassID <= 0)
            {
                result.Skipped++;
                continue;
            }

            await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
            try
            {
                if (!await OwnsStudentClassAsync(con, tx, session.SchoolID, item.StudentID, item.FromStudentClassID, cancellationToken)
                    || await StudentClassExistsAsync(con, tx, session.SchoolID, item.StudentID, request.ToEducationYearID, cancellationToken))
                {
                    await tx.RollbackAsync(cancellationToken);
                    result.Skipped++;
                    continue;
                }

                var sectionId = request.SectionID > 0 ? request.SectionID : 0;
                var shiftId = request.ShiftID > 0 ? request.ShiftID : 0;
                var groupId = request.SubjectGroupID > 0 ? request.SubjectGroupID : 0;
                var rollNo = string.IsNullOrWhiteSpace(item.RollNo) ? (object)DBNull.Value : item.RollNo.Trim();

                const string insertSql = @"
INSERT INTO dbo.StudentsClass
    (SchoolID, RegistrationID, StudentID, ClassID, SectionID, ShiftID, SubjectGroupID, RollNo, EducationYearID, Date, Is_New)
VALUES
    (@SchoolID, @RegistrationID, @StudentID, @ClassID, @SectionID, @ShiftID, @SubjectGroupID, @RollNo, @EducationYearID, GETDATE(), 0);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                int studentClassId;
                await using (var insert = new SqlCommand(insertSql, con, tx))
                {
                    insert.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                    insert.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                    insert.Parameters.AddWithValue("@StudentID", item.StudentID);
                    insert.Parameters.AddWithValue("@ClassID", request.ClassID);
                    insert.Parameters.AddWithValue("@SectionID", sectionId);
                    insert.Parameters.AddWithValue("@ShiftID", shiftId);
                    insert.Parameters.AddWithValue("@SubjectGroupID", groupId);
                    insert.Parameters.AddWithValue("@RollNo", rollNo);
                    insert.Parameters.AddWithValue("@EducationYearID", request.ToEducationYearID);
                    studentClassId = Convert.ToInt32(await insert.ExecuteScalarAsync(cancellationToken));
                }

                foreach (var subject in request.Subjects ?? [])
                {
                    if (subject.SubjectID <= 0)
                        continue;
                    const string recordSql = @"
IF NOT EXISTS (
    SELECT 1 FROM dbo.StudentRecord
    WHERE StudentID = @StudentID AND SchoolID = @SchoolID
      AND EducationYearID = @EducationYearID
      AND StudentClassID = @StudentClassID AND SubjectID = @SubjectID)
INSERT INTO dbo.StudentRecord
    (SchoolID, RegistrationID, StudentID, StudentClassID, SubjectID, EducationYearID, Date, SubjectType)
VALUES
    (@SchoolID, @RegistrationID, @StudentID, @StudentClassID, @SubjectID, @EducationYearID, GETDATE(), @SubjectType)";
                    await using var record = new SqlCommand(recordSql, con, tx);
                    record.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                    record.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                    record.Parameters.AddWithValue("@StudentID", item.StudentID);
                    record.Parameters.AddWithValue("@StudentClassID", studentClassId);
                    record.Parameters.AddWithValue("@SubjectID", subject.SubjectID);
                    record.Parameters.AddWithValue("@EducationYearID", request.ToEducationYearID);
                    record.Parameters.AddWithValue("@SubjectType",
                        string.IsNullOrWhiteSpace(subject.SubjectType) ? "Compulsory" : subject.SubjectType.Trim());
                    await record.ExecuteNonQueryAsync(cancellationToken);
                }

                const string statusSql = @"
UPDATE dbo.StudentsClass
SET Class_Status = N'Re-Admitted'
WHERE StudentClassID = @StudentClassID AND SchoolID = @SchoolID AND StudentID = @StudentID";
                await using (var status = new SqlCommand(statusSql, con, tx))
                {
                    status.Parameters.AddWithValue("@StudentClassID", item.FromStudentClassID);
                    status.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                    status.Parameters.AddWithValue("@StudentID", item.StudentID);
                    await status.ExecuteNonQueryAsync(cancellationToken);
                }

                await tx.CommitAsync(cancellationToken);
                result.Admitted++;
            }
            catch (Exception)
            {
                await tx.RollbackAsync(cancellationToken);
                result.Skipped++;
            }
        }

        if (result.Admitted == 0 && result.Skipped > 0)
        {
            result.Succeeded = false;
            result.Error = "mreadm.sameSession";
            return result;
        }

        foreach (var item in request.Students)
        {
            try
            {
                var student = await LoadStudentDtoAsync(con, session, item.StudentID, request.ToEducationYearID, cancellationToken);
                if (student is null)
                    continue;
                result.Students.Add(student);
                await TryLogChangeAsync(con, session, student.LocalId, item.StudentID, "Update", session.DeviceId, cancellationToken);
            }
            catch (Exception)
            {
            }
        }

        return result;
    }

    private static BulkReAdmissionResult BulkFail(string error) =>
        new() { Succeeded = false, Error = error };
}
