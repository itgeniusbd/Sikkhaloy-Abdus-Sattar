using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Attendances.Online_Display
{
    public partial class DeviceDisplay : Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            var schoolId = ResolveSchoolId();
            if (schoolId <= 0)
                return;

            var educationYearId = ResolveEducationYearId(schoolId);
            ApplyEducationYearParameter(StudentScheduleSummarySQL, educationYearId);
            ApplyEducationYearParameter(Student_Entry_LogSQL, educationYearId);
            ApplyEducationYearParameter(Student_Exit_LogSQL, educationYearId);
        }

        private int ResolveSchoolId()
        {
            int schoolId;
            if (int.TryParse(Request.QueryString["SchoolID"], out schoolId) && schoolId > 0)
                return schoolId;

            return 0;
        }

        private int ResolveEducationYearId(int schoolId)
        {
            const string sql = @"
SELECT TOP 1 scPick.EducationYearID
FROM Attendance_Schedule_AssignStudent assPick
INNER JOIN Student sPick ON assPick.StudentID = sPick.StudentID AND sPick.Status = N'Active'
INNER JOIN StudentsClass scPick ON scPick.StudentID = sPick.StudentID AND scPick.SchoolID = assPick.SchoolID
WHERE assPick.SchoolID = @SchoolID
GROUP BY scPick.EducationYearID
ORDER BY COUNT(DISTINCT assPick.StudentID) DESC, scPick.EducationYearID DESC;

SELECT TOP 1 EducationYearID
FROM Education_Year
WHERE SchoolID = @SchoolID AND Status = N'True'
ORDER BY SN DESC, EducationYearID DESC;

SELECT TOP 1 EducationYearID
FROM Education_Year
WHERE SchoolID = @SchoolID
ORDER BY SN DESC, EducationYearID DESC;";

            var connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(connStr))
                return 0;

            using (var con = new SqlConnection(connStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@SchoolID", SqlDbType.Int).Value = schoolId;
                con.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read() && reader[0] != DBNull.Value)
                        return Convert.ToInt32(reader[0]);

                    if (reader.NextResult() && reader.Read() && reader[0] != DBNull.Value)
                        return Convert.ToInt32(reader[0]);

                    if (reader.NextResult() && reader.Read() && reader[0] != DBNull.Value)
                        return Convert.ToInt32(reader[0]);
                }
            }

            return 0;
        }

        private static void ApplyEducationYearParameter(SqlDataSource source, int educationYearId)
        {
            if (source == null)
                return;

            var value = Math.Max(educationYearId, 0).ToString();
            var param = source.SelectParameters["EducationYearID"];
            if (param == null)
            {
                source.SelectParameters.Add("EducationYearID", DbType.Int32, value);
                return;
            }

            param.DefaultValue = value;
        }
    }
}
