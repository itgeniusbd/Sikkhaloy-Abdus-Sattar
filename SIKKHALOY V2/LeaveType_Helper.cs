using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace Education
{
    public class LeaveType_Helper
    {
        private static readonly string[] DefaultLeaveTypes =
        {
            "অসুস্থতার জন্য",
            "ব্যাক্তিগত কারনে",
            "ফ্যামেলি প্রয়োজনে",
            "মেডিক্যাল",
            "সাময়িক",
            "সাপ্তাহিক",
            "মাসিক",
            "অন্যান্ন"
        };

        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

        public List<string> GetLeaveTypes(int schoolId)
        {
            var types = new List<string>();
            if (schoolId <= 0 || !TableExists())
                return new List<string>(DefaultLeaveTypes);

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT LeaveTypeName
                      FROM Attendance_Leave_Type
                      WHERE SchoolID = @SchoolID AND IsActive = 1
                      ORDER BY SortOrder, LeaveTypeName", con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string name = dr["LeaveTypeName"].ToString().Trim();
                            if (!string.IsNullOrWhiteSpace(name))
                                types.Add(name);
                        }
                    }
                }
            }
            catch { }

            if (types.Count == 0)
                return new List<string>(DefaultLeaveTypes);

            return types;
        }

        public void BindDropDownList(DropDownList dropDown, int schoolId, bool includeBlankOption = true)
        {
            if (dropDown == null) return;

            dropDown.Items.Clear();
            if (includeBlankOption)
                dropDown.Items.Add(new ListItem("-- Select --", ""));

            foreach (string leaveType in GetLeaveTypes(schoolId))
                dropDown.Items.Add(new ListItem(leaveType, leaveType));
        }

        public bool AddLeaveType(int schoolId, string leaveTypeName, int registrationId)
        {
            leaveTypeName = (leaveTypeName ?? string.Empty).Trim();
            if (schoolId <= 0 || string.IsNullOrWhiteSpace(leaveTypeName) || !TableExists())
                return false;

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    @"IF NOT EXISTS (
                          SELECT 1 FROM Attendance_Leave_Type
                          WHERE SchoolID = @SchoolID AND LeaveTypeName = @LeaveTypeName
                      )
                      INSERT INTO Attendance_Leave_Type (SchoolID, LeaveTypeName, SortOrder)
                      SELECT @SchoolID, @LeaveTypeName,
                             ISNULL(MAX(SortOrder), 0) + 1
                      FROM Attendance_Leave_Type
                      WHERE SchoolID = @SchoolID", con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                    cmd.Parameters.AddWithValue("@LeaveTypeName", leaveTypeName);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteLeaveType(int schoolId, int leaveTypeId)
        {
            if (schoolId <= 0 || leaveTypeId <= 0 || !TableExists())
                return false;

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Attendance_Leave_Type WHERE LeaveTypeID = @LeaveTypeID AND SchoolID = @SchoolID", con))
                {
                    cmd.Parameters.AddWithValue("@LeaveTypeID", leaveTypeId);
                    cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool TableExists()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Attendance_Leave_Type]') AND type = N'U'", con))
                {
                    con.Open();
                    return cmd.ExecuteScalar() != null;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
