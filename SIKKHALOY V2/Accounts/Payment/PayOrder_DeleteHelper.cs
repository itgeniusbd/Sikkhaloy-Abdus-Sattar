using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace EDUCATION.COM.ACCOUNTS.Payment
{
    public static class PayOrder_DeleteHelper
    {
        public const int DefaultBatchSize = 50;

        public static int DeleteBatch(int schoolId, string commaSeparatedIds)
        {
            var payOrderIds = ParseIds(commaSeparatedIds);
            if (payOrderIds.Length == 0 || schoolId <= 0)
                return 0;

            string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            string idsParam = string.Join(",", payOrderIds);

            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                using (SqlTransaction tx = con.BeginTransaction())
                {
                    ExecuteDelete(con, tx, schoolId, idsParam,
                        "DELETE FROM Income_Discount_Record WHERE SchoolID = @SchoolID AND PayOrderID IN (SELECT id FROM dbo.In_Function_Parameter(@IDs))");

                    ExecuteDelete(con, tx, schoolId, idsParam,
                        @"IF OBJECT_ID(N'dbo.Attendance_Monthly_Report', N'U') IS NOT NULL
                          DELETE FROM Attendance_Monthly_Report
                          WHERE SchoolID = @SchoolID
                            AND PayOrderID IN (SELECT id FROM dbo.In_Function_Parameter(@IDs))");

                    int deleted = ExecuteDelete(con, tx, schoolId, idsParam,
                        @"DELETE FROM Income_PayOrder
                          WHERE SchoolID = @SchoolID
                            AND PaidAmount <= 0
                            AND PayOrderID IN (SELECT id FROM dbo.In_Function_Parameter(@IDs))");

                    tx.Commit();
                    return deleted;
                }
            }
        }

        public static string[] ParseIds(string commaSeparatedIds)
        {
            if (string.IsNullOrWhiteSpace(commaSeparatedIds))
                return new string[0];

            return commaSeparatedIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim())
                .Where(id => id.All(char.IsDigit))
                .Distinct()
                .ToArray();
        }

        private static int ExecuteDelete(SqlConnection con, SqlTransaction tx, int schoolId, string idsParam, string sql)
        {
            using (SqlCommand cmd = new SqlCommand(sql, con, tx))
            {
                cmd.CommandTimeout = 90;
                cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                cmd.Parameters.AddWithValue("@IDs", idsParam);
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
