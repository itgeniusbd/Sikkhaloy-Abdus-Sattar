using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority.Invoice
{
    public partial class Online_Payment_Report : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Default: current month
                FromDateTextBox.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("dd MMM yyyy");
                ToDateTextBox.Text   = DateTime.Now.ToString("dd MMM yyyy");
                LoadReport();
            }
        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        protected void ClearButton_Click(object sender, EventArgs e)
        {
            TypeDropDownList.SelectedValue   = "All";
            SchoolDropDownList.SelectedValue = "0";
            MethodDropDownList.SelectedValue = "";
            FromDateTextBox.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("dd MMM yyyy");
            ToDateTextBox.Text   = DateTime.Now.ToString("dd MMM yyyy");
            LoadReport();
        }

        private void LoadReport()
        {
            DateTime fromDate = DateTime.Today.AddDays(1 - DateTime.Today.Day); // first of month
            DateTime toDate   = DateTime.Today;

            if (!string.IsNullOrWhiteSpace(FromDateTextBox.Text))
                DateTime.TryParse(FromDateTextBox.Text, out fromDate);
            if (!string.IsNullOrWhiteSpace(ToDateTextBox.Text))
                DateTime.TryParse(ToDateTextBox.Text, out toDate);

            // Ensure toDate includes end of day
            toDate = toDate.Date.AddDays(1).AddSeconds(-1);

            DateRangeLabel.Text = string.Format("{0:d MMM yyyy} — {1:d MMM yyyy}",
                fromDate, toDate.Date);

            int schoolId = 0;
            int.TryParse(SchoolDropDownList.SelectedValue, out schoolId);
            string method = MethodDropDownList.SelectedValue;
            string type   = TypeDropDownList.SelectedValue; // All | Online | Offline

            string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // Detail: UNION of Online + Offline
                string detailSql = @"
                    SELECT *
                    FROM (
                        SELECT
                            p.SchoolID,
                            ISNULL(s.SchoolName, 'Unknown')   AS SchoolName,
                            p.Amount,
                            ISNULL(p.SP_Method, 'ShurjoPay')  AS PayMethod,
                            'Online'                           AS CollectionType,
                            'ShurjoPay'                        AS CollectedBy,
                            ISNULL(p.SP_TrxID, p.SP_OrderID)  AS Reference,
                            p.PaymentDate
                        FROM AAP_Invoice_OnlinePayment p
                        LEFT JOIN SchoolInfo s ON p.SchoolID = s.SchoolID
                        WHERE p.PaymentDate BETWEEN @From AND @To
                          AND (@SchoolID = 0 OR p.SchoolID = @SchoolID)
                          AND (@Method = '' OR p.SP_Method LIKE '%' + @Method + '%')
                          AND (@Type = 'All' OR @Type = 'Online')

                        UNION ALL

                        SELECT
                            r.SchoolID,
                            ISNULL(s.SchoolName, 'Unknown')            AS SchoolName,
                            r.TotalAmount                              AS Amount,
                            ISNULL(r.Payment_Method, 'Cash')           AS PayMethod,
                            'Offline'                                  AS CollectionType,
                            ISNULL(r.Collected_By, r.PaymentBy)        AS CollectedBy,
                            CAST(r.InvoiceReceipt_SN AS NVARCHAR(50))  AS Reference,
                            r.PaidDate                                 AS PaymentDate
                        FROM AAP_Invoice_Receipt r
                        LEFT JOIN SchoolInfo s ON r.SchoolID = s.SchoolID
                        WHERE r.PaidDate BETWEEN @From AND @To
                          AND (@SchoolID = 0 OR r.SchoolID = @SchoolID)
                          AND (@Method = '' OR ISNULL(r.Payment_Method,'') LIKE '%' + @Method + '%')
                          AND (@Type = 'All' OR @Type = 'Offline')
                          AND ISNULL(r.Collected_By, '') NOT LIKE '%ShurjoPay%'
                    ) AS Combined
                    ORDER BY PaymentDate DESC";

                using (SqlCommand cmd = new SqlCommand(detailSql, conn))
                {
                    AddParams(cmd, fromDate, toDate, schoolId, method, type);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    ReportGridView.DataSource = dt;
                    ReportGridView.DataBind();
                }

                // Summary
                string sumSql = @"
                    SELECT
                        ISNULL(SUM(Amount), 0)   AS TotalAmount,
                        COUNT(*)                 AS TotalCount,
                        COUNT(DISTINCT SchoolID) AS InstitutionCount,
                        ISNULL(SUM(CASE WHEN CollectionType='Online'  THEN Amount ELSE 0 END), 0) AS OnlineAmount,
                        ISNULL(SUM(CASE WHEN CollectionType='Offline' THEN Amount ELSE 0 END), 0) AS OfflineAmount,
                        ISNULL(SUM(CASE WHEN CollectionType='Online'  THEN 1 ELSE 0 END), 0) AS OnlineCount,
                        ISNULL(SUM(CASE WHEN CollectionType='Offline' THEN 1 ELSE 0 END), 0) AS OfflineCount
                    FROM (
                        SELECT SchoolID, Amount, 'Online' AS CollectionType
                        FROM AAP_Invoice_OnlinePayment
                        WHERE PaymentDate BETWEEN @From AND @To
                          AND (@SchoolID = 0 OR SchoolID = @SchoolID)
                          AND (@Method = '' OR SP_Method LIKE '%' + @Method + '%')
                          AND (@Type = 'All' OR @Type = 'Online')

                        UNION ALL

                        SELECT SchoolID, TotalAmount AS Amount, 'Offline' AS CollectionType
                        FROM AAP_Invoice_Receipt
                        WHERE PaidDate BETWEEN @From AND @To
                          AND (@SchoolID = 0 OR SchoolID = @SchoolID)
                          AND (@Method = '' OR ISNULL(Payment_Method,'') LIKE '%' + @Method + '%')
                          AND (@Type = 'All' OR @Type = 'Offline')
                          AND ISNULL(Collected_By, '') NOT LIKE '%ShurjoPay%'
                    ) AS T";

                using (SqlCommand cmd2 = new SqlCommand(sumSql, conn))
                {
                    AddParams(cmd2, fromDate, toDate, schoolId, method, type);
                    using (SqlDataReader rdr = cmd2.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            TotalAmountLabel.Text      = Convert.ToDecimal(rdr["TotalAmount"]).ToString("N2");
                            TotalCountLabel.Text       = Convert.ToInt32(rdr["TotalCount"]).ToString("N0");
                            InstitutionCountLabel.Text = Convert.ToInt32(rdr["InstitutionCount"]).ToString();
                            OnlineAmountLabel.Text     = Convert.ToDecimal(rdr["OnlineAmount"]).ToString("N2");
                            OfflineAmountLabel.Text    = Convert.ToDecimal(rdr["OfflineAmount"]).ToString("N2");
                            OnlineCountLabel.Text      = Convert.ToInt32(rdr["OnlineCount"]).ToString();
                            OfflineCountLabel.Text     = Convert.ToInt32(rdr["OfflineCount"]).ToString();
                        }
                    }
                }
            }
        }

        private static void AddParams(SqlCommand cmd, DateTime from, DateTime to, int schoolId, string method, string type)
        {
            cmd.Parameters.AddWithValue("@From",     from);
            cmd.Parameters.AddWithValue("@To",       to);
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            cmd.Parameters.AddWithValue("@Method",   method);
            cmd.Parameters.AddWithValue("@Type",     type);
        }

        public string GetMethodBadge(string method)
        {
            if (string.IsNullOrEmpty(method)) return "badge-other";
            string m = method.ToLower();
            if (m.Contains("nagad"))  return "badge-nagad";
            if (m.Contains("bkash")) return "badge-bkash";
            if (m.Contains("card"))   return "badge-card";
            if (m.Contains("cash") || m.Contains("rocket") || m.Contains("dbbl")) return "badge-cash";
            return "badge-other";
        }
    }
}
