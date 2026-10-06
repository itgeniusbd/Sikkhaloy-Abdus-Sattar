using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Students;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class SmsOfficeService
{
    private const int GuardianContactDefaultTake = 60;
    private const int GuardianContactMaxTake = 200;

    /// <summary>
    /// "অভিভাবকের সাথে যোগাযোগ" page: active students of the current session of this school only.
    /// No class = all classes ("সকল ক্লাস"); optional class (+ group / section / shift) and/or a search
    /// on ID / roll / name / father's name. Sorted by class order, section, roll, ID.
    /// Reads only the columns the cards show; paged with OFFSET/FETCH (Total = all matches).
    /// </summary>
    public async Task<GuardianContactPageDto> GetGuardianContactsAsync(
        SessionSnapshot session, int classId, int groupId, int sectionId, int shiftId, string? q,
        int skip, int take, CancellationToken ct)
    {
        var search = GuardianSearchText(q);
        if (session.SchoolID <= 0 || session.EducationYearID <= 0)
            return new GuardianContactPageDto();
        if (skip < 0) skip = 0;
        if (take <= 0) take = GuardianContactDefaultTake;
        if (take > GuardianContactMaxTake) take = GuardianContactMaxTake;

        // C# raw strings drop the final newline, so every appended fragment starts with "\n".
        var sql = """
SELECT st.StudentID, st.ID, sc.RollNo, st.StudentsName, st.FathersName, st.SMSPhoneNo,
       cc.Class, cs.Section, COUNT(*) OVER () AS TotalRows
FROM dbo.StudentsClass AS sc
INNER JOIN dbo.Student AS st ON st.StudentID = sc.StudentID
INNER JOIN dbo.CreateClass AS cc ON cc.ClassID = sc.ClassID
LEFT JOIN dbo.CreateSection AS cs ON cs.SectionID = TRY_CONVERT(int, sc.SectionID)
WHERE st.Status = N'Active'
  AND sc.SchoolID = @SchoolID
  AND sc.EducationYearID = @EducationYearID
""";
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand { Connection = con };
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        if (classId > 0)
        {
            sql += "\n" + """
  AND sc.ClassID = @ClassID
  AND (@SectionID = 0 OR sc.SectionID = CONVERT(nvarchar(50), @SectionID))
  AND (@SubjectGroupID = 0 OR sc.SubjectGroupID = CONVERT(nvarchar(50), @SubjectGroupID))
  AND (@ShiftID = 0 OR sc.ShiftID = CONVERT(nvarchar(50), @ShiftID))
""";
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@SectionID", Math.Max(0, sectionId));
            cmd.Parameters.AddWithValue("@SubjectGroupID", Math.Max(0, groupId));
            cmd.Parameters.AddWithValue("@ShiftID", Math.Max(0, shiftId));
        }
        if (search.Length > 0)
        {
            sql += "\n" + """
  AND (st.ID = @Q OR st.ID LIKE @QStart OR sc.RollNo = @Q
       OR st.StudentsName LIKE @QAny OR st.FathersName LIKE @QAny)
""";
            var like = GuardianEscapeLike(search);
            cmd.Parameters.Add("@Q", System.Data.SqlDbType.NVarChar, 100).Value = search;
            cmd.Parameters.Add("@QStart", System.Data.SqlDbType.NVarChar, 210).Value = like + "%";
            cmd.Parameters.Add("@QAny", System.Data.SqlDbType.NVarChar, 210).Value = "%" + like + "%";
        }
        sql += "\n" + """
ORDER BY ISNULL(cc.SN, 2147483647), cc.Class, cc.ClassID, ISNULL(cs.Section, N''),
         CASE WHEN TRY_CONVERT(float, sc.RollNo) IS NULL THEN 1 ELSE 0 END,
         TRY_CONVERT(float, sc.RollNo), sc.RollNo,
         CASE WHEN TRY_CONVERT(bigint, st.ID) IS NULL THEN 1 ELSE 0 END, TRY_CONVERT(bigint, st.ID), st.ID, st.StudentID
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
""";
        cmd.Parameters.AddWithValue("@Skip", skip);
        cmd.Parameters.AddWithValue("@Take", take);
        cmd.CommandText = sql;

        var page = new GuardianContactPageDto();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            page.Total = Convert.ToInt32(reader["TotalRows"]);
            page.Rows.Add(new GuardianContactDto
            {
                StudentID = Convert.ToInt32(reader["StudentID"]),
                ID = reader["ID"]?.ToString()?.Trim() ?? "",
                RollNo = reader["RollNo"] is DBNull ? null : reader["RollNo"]?.ToString()?.Trim(),
                Name = reader["StudentsName"]?.ToString()?.Trim() ?? "",
                FatherName = reader["FathersName"] is DBNull ? null : reader["FathersName"]?.ToString()?.Trim(),
                ClassName = reader["Class"] is DBNull ? null : reader["Class"]?.ToString()?.Trim(),
                SectionName = reader["Section"] is DBNull ? null : reader["Section"]?.ToString()?.Trim(),
                Phone = reader["SMSPhoneNo"] is DBNull ? null : reader["SMSPhoneNo"]?.ToString()?.Trim()
            });
        }
        return page;
    }

    private static string GuardianSearchText(string? q)
    {
        var text = (q ?? "").Trim();
        if (text.Length == 0)
            return "";
        var chars = text.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (chars[i] is >= '\u09E6' and <= '\u09EF')   // Bangla digits -> 0-9 (IDs / rolls are stored in ASCII)
                chars[i] = (char)('0' + (chars[i] - '\u09E6'));
        }
        text = new string(chars);
        return text.Length > 100 ? text[..100] : text;
    }

    private static string GuardianEscapeLike(string value) =>
        value.Replace("[", "[[]", StringComparison.Ordinal)
             .Replace("%", "[%]", StringComparison.Ordinal)
             .Replace("_", "[_]", StringComparison.Ordinal);
}
