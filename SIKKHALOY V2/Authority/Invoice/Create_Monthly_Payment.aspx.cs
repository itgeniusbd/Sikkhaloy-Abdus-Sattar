using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority.Invoice
{
    public partial class Create_Monthly_Payment : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Auto-select the latest month so user can see data immediately
                Month_DropDownList.DataBind();
                if (Month_DropDownList.Items.Count > 1)
                {
                    // Items are ordered DESC, so index 1 is the latest month (index 0 is placeholder)
                    Month_DropDownList.SelectedIndex = 1;
                }
                LoadJobStatus();
            }
        }

        private void LoadJobStatus()
        {
            const string jobName = "Auto_Generate_Monthly_Invoice";
            string cs = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            // Query msdb to get job details + last run outcome + next run
            string sql = @"
                SELECT
                    j.name                          AS JobName,
                    j.enabled                       AS Enabled,
                    -- Last run date/time
                    CASE WHEN h.run_date IS NULL THEN NULL
                         ELSE CONVERT(datetime,
                              STUFF(STUFF(CAST(h.run_date AS VARCHAR(8)),7,0,'-'),5,0,'-')
                              + ' ' +
                              STUFF(STUFF(RIGHT('000000'+CAST(h.run_time AS VARCHAR(6)),6),5,0,':'),3,0,':'))
                    END                             AS LastRunDateTime,
                    -- Last run status: 0=Failed,1=Succeeded,2=Retry,3=Cancelled,5=Unknown
                    h.run_status                    AS LastRunStatus,
                    h.message                       AS LastRunMessage,
                    -- Next scheduled run
                    CASE WHEN s.next_run_date = 0 THEN NULL
                         ELSE CONVERT(datetime,
                              STUFF(STUFF(CAST(s.next_run_date AS VARCHAR(8)),7,0,'-'),5,0,'-')
                              + ' ' +
                              STUFF(STUFF(RIGHT('000000'+CAST(s.next_run_time AS VARCHAR(6)),6),5,0,':'),3,0,':'))
                    END                             AS NextRunDateTime,
                    -- Currently running? (sysjobactivity requires extra permission; use 0 as safe fallback)
                    0 AS IsRunning
                FROM msdb.dbo.sysjobs j
                LEFT JOIN (
                    SELECT job_id, run_date, run_time, run_status, message
                    FROM msdb.dbo.sysjobhistory
                    WHERE step_id = 0
                      AND instance_id = (
                          SELECT MAX(instance_id) FROM msdb.dbo.sysjobhistory h2
                          WHERE h2.job_id = (SELECT job_id FROM msdb.dbo.sysjobs WHERE name = @JobName)
                            AND h2.step_id = 0)
                ) h ON j.job_id = h.job_id
                LEFT JOIN (
                    SELECT job_id, next_run_date, next_run_time
                    FROM msdb.dbo.sysjobschedules
                    WHERE schedule_id = (
                        SELECT TOP 1 schedule_id FROM msdb.dbo.sysjobschedules js2
                        WHERE js2.job_id = (SELECT job_id FROM msdb.dbo.sysjobs WHERE name = @JobName))
                ) s ON j.job_id = s.job_id
                WHERE j.name = @JobName";

            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@JobName", jobName);
                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            JobNameLabel.Text = dr["JobName"].ToString();

                            bool enabled = Convert.ToBoolean(dr["Enabled"]);
                            JobEnabledLabel.Text = enabled
                                ? "<span class='badge badge-success'>Yes</span>"
                                : "<span class='badge badge-danger'>No</span>";

                            if (JobDisabledWarningLabel != null)
                            {
                                JobDisabledWarningLabel.Visible = !enabled;
                                JobDisabledWarningLabel.Text = enabled
                                    ? string.Empty
                                    : "<i class='fa fa-exclamation-triangle'></i> <strong>সতর্কতা:</strong> Job বর্তমানে <strong>Disabled</strong> — মাসের ১ তারিখে অটো ইনভয়েস চলবে না। <strong>Job Enable করুন</strong> বাটনে ক্লিক করুন।";
                            }
                            if (EnableJobBtn != null)
                                EnableJobBtn.Visible = !enabled;

                            // Last run
                            JobLastRunLabel.Text = dr["LastRunDateTime"] == DBNull.Value
                                ? "<span class='text-muted'>কখনো চলেনি</span>"
                                : Convert.ToDateTime(dr["LastRunDateTime"]).ToString("d MMM yyyy hh:mm tt");

                            // Last run status
                            if (dr["LastRunStatus"] != DBNull.Value)
                            {
                                int status = Convert.ToInt32(dr["LastRunStatus"]);
                                switch (status)
                                {
                                    case 1: JobLastStatusLabel.Text = "<span class='badge badge-success'>Succeeded</span>"; break;
                                    case 0: JobLastStatusLabel.Text = "<span class='badge badge-danger'>Failed</span>"; break;
                                    case 3: JobLastStatusLabel.Text = "<span class='badge badge-warning'>Cancelled</span>"; break;
                                    case 2: JobLastStatusLabel.Text = "<span class='badge badge-info'>Retry</span>"; break;
                                    default: JobLastStatusLabel.Text = "<span class='badge badge-secondary'>Unknown</span>"; break;
                                }
                            }
                            else
                            {
                                JobLastStatusLabel.Text = "<span class='text-muted'>—</span>";
                            }

                            // Next run
                            JobNextRunLabel.Text = dr["NextRunDateTime"] == DBNull.Value
                                ? "<span class='text-muted'>Schedule নেই</span>"
                                : Convert.ToDateTime(dr["NextRunDateTime"]).ToString("d MMM yyyy hh:mm tt");

                            // Currently running?
                            int isRunning = Convert.ToInt32(dr["IsRunning"]);
                            JobCurrentStateLabel.Text = isRunning > 0
                                ? "<span class='badge badge-primary'><i class='fa fa-spinner fa-spin'></i> Running</span>"
                                : "<span class='badge badge-secondary'>Idle</span>";
                        }
                        else
                        {
                            JobNameLabel.Text = jobName;
                            JobEnabledLabel.Text = "<span class='badge badge-danger'>Not Found</span>";
                            JobLastRunLabel.Text = JobLastStatusLabel.Text = JobNextRunLabel.Text = "—";
                            JobCurrentStateLabel.Text = "<span class='badge badge-danger'>Job পাওয়া যায়নি</span>";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                JobErrorLabel.Text = "⚠ Job status লোড করতে সমস্যা: " + ex.Message;
                JobErrorLabel.Visible = true;
            }
        }

        protected void RefreshJobStatusBtn_Click(object sender, EventArgs e)
        {
            LoadJobStatus();
        }

        protected void RunAutoGenerateBtn_Click(object sender, EventArgs e)
        {
            AutoGenerateMsgLabel.Visible = false;

            if (string.IsNullOrWhiteSpace(Month_DropDownList.SelectedValue))
            {
                AutoGenerateMsgLabel.Text = "<span class='text-danger'>Service Charge ট্যাবে মাস নির্বাচন করুন।</span>";
                AutoGenerateMsgLabel.Visible = true;
                return;
            }

            DateTime targetMonth;
            if (!DateTime.TryParse(Month_DropDownList.SelectedValue, out targetMonth))
            {
                AutoGenerateMsgLabel.Text = "<span class='text-danger'>মাস সঠিক নয়।</span>";
                AutoGenerateMsgLabel.Visible = true;
                return;
            }

            DateTime monthEnd = new DateTime(targetMonth.Year, targetMonth.Month,
                DateTime.DaysInMonth(targetMonth.Year, targetMonth.Month));

            int registrationId = 1;
            if (Session["RegistrationID"] != null)
                int.TryParse(Session["RegistrationID"].ToString(), out registrationId);

            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                string countMessage = RunStudentCountGeneration(connStr, monthEnd);

                using (SqlConnection con = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand("AAP_Auto_Generate_Monthly_Invoice", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 180;
                    cmd.Parameters.AddWithValue("@TargetMonth", monthEnd);
                    cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                AutoGenerateMsgLabel.Text = "<span class='text-success'><i class='fa fa-check'></i> Student Count + Invoice সম্পন্ন। "
                    + HttpUtility.HtmlEncode(countMessage) + " Paid Invoice থেকে যাচাই করুন।</span>";
                AutoGenerateMsgLabel.Visible = true;
                Payment_GridView.DataBind();
            }
            catch (Exception ex)
            {
                AutoGenerateMsgLabel.Text = "<span class='text-danger'>Auto generate ত্রুটি: " + HttpUtility.HtmlEncode(ex.Message) + "</span>";
                AutoGenerateMsgLabel.Visible = true;
            }
        }

        protected void EnableJobBtn_Click(object sender, EventArgs e)
        {
            AutoGenerateMsgLabel.Visible = false;
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand("EXEC msdb.dbo.sp_update_job @job_name = @JobName, @enabled = 1", con))
                {
                    cmd.Parameters.AddWithValue("@JobName", "Auto_Generate_Monthly_Invoice");
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                AutoGenerateMsgLabel.Text = "<span class='text-success'><i class='fa fa-check'></i> SQL Agent Job সক্রিয় করা হয়েছে।</span>";
                AutoGenerateMsgLabel.Visible = true;
                LoadJobStatus();
            }
            catch (Exception ex)
            {
                AutoGenerateMsgLabel.Text = "<span class='text-danger'>Job enable করতে পারেনি: " + HttpUtility.HtmlEncode(ex.Message) + "</span>";
                AutoGenerateMsgLabel.Visible = true;
            }
        }

        private static string RunStudentCountGeneration(string connStr, DateTime monthEnd)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("sp_Generate_Monthly_Student_Count", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 180;
                cmd.Parameters.AddWithValue("@TargetMonth", monthEnd);

                SqlParameter countParam = new SqlParameter("@GeneratedCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
                SqlParameter msgParam = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(countParam);
                cmd.Parameters.Add(msgParam);

                con.Open();
                cmd.ExecuteNonQuery();

                return msgParam.Value != DBNull.Value ? msgParam.Value.ToString() : "Student count completed";
            }
        }

        protected void Ins_LinkButton_Command(object sender, CommandEventArgs e)
        {
            DetailsSQL.SelectParameters["SchoolID"].DefaultValue = e.CommandName.ToString();
            Institution_Label.Text = e.CommandArgument.ToString();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Pop", "openModal();", true);
        }

        protected void CategoryButton_Click(object sender, EventArgs e)
        {
            InvoiceCategorySQL.Insert();
            Category_TextBox.Text = "";
        }

        protected void Monthly_Button_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Month_DropDownList.SelectedValue))
            {
                ShowInvoiceResult("মাস নির্বাচন করুন।", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(sIssueDate_TextBox.Text))
            {
                ShowInvoiceResult("Issue Date দিন।", true);
                return;
            }

            DateTime issueDate;
            if (!DateTime.TryParse(sIssueDate_TextBox.Text.Trim(), out issueDate))
            {
                ShowInvoiceResult("Issue Date সঠিক নয়।", true);
                return;
            }

            DateTime monthEnd;
            if (!DateTime.TryParse(Month_DropDownList.SelectedValue, out monthEnd))
            {
                ShowInvoiceResult("মাস সঠিক নয়।", true);
                return;
            }
            monthEnd = new DateTime(monthEnd.Year, monthEnd.Month, DateTime.DaysInMonth(monthEnd.Year, monthEnd.Month));

            int inserted = 0;
            int skippedExists = 0;
            var errors = new List<string>();
            var skippedDetails = new List<string>();
            string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            int registrationId = 1;
            if (Session["RegistrationID"] != null)
                int.TryParse(Session["RegistrationID"].ToString(), out registrationId);

            const string insertSql = @"
IF NOT EXISTS (
    SELECT InvoiceID FROM AAP_Invoice
    WHERE SchoolID = @SchoolID
      AND InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = N'Service Charge')
      AND EOMONTH(MonthName) = EOMONTH(@MonthName)
      AND IsPaid = 0
)
BEGIN
    INSERT INTO AAP_Invoice(RegistrationID, InvoiceCategoryID, SchoolID, IssuDate, EndDate, Invoice_For, TotalAmount, Discount, MonthName, Invoice_SN, Unit, UnitPrice)
    VALUES (@RegistrationID,
            (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = N'Service Charge'),
            @SchoolID, @IssuDate, @EndDate, @Invoice_For, @TotalAmount, @Discount, @MonthName,
            dbo.Invoice_SerialNumber(@SchoolID), @Unit, @UnitPrice);
    SELECT 1;
END
ELSE
    SELECT 0;";

            const string existingSql = @"
SELECT TOP 1 InvoiceID, Invoice_For, IsPaid,
       CAST(TotalAmount - PaidAmount - Discount AS DECIMAL(18,2)) AS Due
FROM AAP_Invoice
WHERE SchoolID = @SchoolID
  AND InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = N'Service Charge')
  AND EOMONTH(MonthName) = EOMONTH(@MonthName)
ORDER BY InvoiceID DESC";

            foreach (GridViewRow row in Payment_GridView.Rows)
            {
                var invoiceCheckBox = row.FindControl("Invoice_CheckBox") as CheckBox;
                if (invoiceCheckBox == null || !invoiceCheckBox.Checked)
                    continue;

                int schoolId = Convert.ToInt32(Payment_GridView.DataKeys[row.RowIndex]["SchoolID"]);
                var totalStudentLabel = row.FindControl("Total_Student_Label") as Label;
                var committeeCountLabel = row.FindControl("Committee_Count_Label") as Label;
                var perStudentLabel = row.FindControl("PerStudent_Label") as Label;
                var fixedLabel = row.FindControl("Fixed_Label") as Label;
                var discountTextBox = row.FindControl("Discount_TextBox") as TextBox;

                try
                {
                    double totalStudent = SafeToDouble(totalStudentLabel != null ? totalStudentLabel.Text : null);
                    double committeeCount = SafeToDouble(committeeCountLabel != null ? committeeCountLabel.Text : null);
                    double totalBillableCount = totalStudent + committeeCount;
                    double perStudent = SafeToDouble(perStudentLabel != null ? perStudentLabel.Text : null);
                    double fixedAmount = SafeToDouble(fixedLabel != null ? fixedLabel.Text : null);
                    double discount = SafeToDouble(discountTextBox != null ? discountTextBox.Text : null);

                    double amount;
                    object unitPrice;
                    if (fixedAmount > 0)
                    {
                        amount = fixedAmount;
                        unitPrice = DBNull.Value;
                    }
                    else
                    {
                        amount = totalBillableCount * perStudent;
                        unitPrice = perStudent;
                    }

                    using (SqlConnection con = new SqlConnection(connStr))
                    using (SqlCommand cmd = new SqlCommand(insertSql, con))
                    {
                        cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
                        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                        cmd.Parameters.AddWithValue("@IssuDate", issueDate.Date);
                        cmd.Parameters.AddWithValue("@EndDate", issueDate.Date.AddDays(15));
                        cmd.Parameters.AddWithValue("@Invoice_For", Month_DropDownList.SelectedItem.Text);
                        cmd.Parameters.AddWithValue("@TotalAmount", amount);
                        cmd.Parameters.AddWithValue("@Discount", discount);
                        cmd.Parameters.AddWithValue("@MonthName", monthEnd);
                        cmd.Parameters.AddWithValue("@Unit", totalBillableCount);
                        cmd.Parameters.AddWithValue("@UnitPrice", unitPrice ?? DBNull.Value);

                        con.Open();
                        int result = Convert.ToInt32(cmd.ExecuteScalar());
                        if (result == 1)
                        {
                            inserted++;
                        }
                        else
                        {
                            skippedExists++;
                            skippedDetails.Add(GetSkippedInvoiceReason(con, existingSql, schoolId, monthEnd));
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(schoolId + ": " + ex.Message);
                }
            }

            var msg = new StringBuilder();
            msg.Append("তৈরি হয়েছে: ").Append(inserted);
            if (skippedExists > 0)
            {
                msg.Append("\\nবকেয়া ইনভয়েস আছে (স্কিপ): ").Append(skippedExists);
                if (skippedDetails.Count > 0)
                    msg.Append("\\n").Append(string.Join("\\n", skippedDetails));
            }
            if (errors.Count > 0)
                msg.Append("\\nত্রুটি: ").Append(string.Join("\\n", errors));
            if (inserted == 0 && skippedExists == 0 && errors.Count == 0)
                msg.Append("\\nকোনো প্রতিষ্ঠান সিলেক্ট করা হয়নি। All চেকবক্স চেক করে আবার চেষ্টা করুন।");

            ShowInvoiceResult(msg.ToString(), inserted == 0 && errors.Count > 0);
        }

        private static string GetSkippedInvoiceReason(SqlConnection con, string existingSql, int schoolId, DateTime monthEnd)
        {
            using (SqlCommand lookup = new SqlCommand(existingSql, con))
            {
                lookup.Parameters.AddWithValue("@SchoolID", schoolId);
                lookup.Parameters.AddWithValue("@MonthName", monthEnd);
                using (SqlDataReader reader = lookup.ExecuteReader())
                {
                    if (!reader.Read())
                        return schoolId + ": অজানা কারণে স্কিপ";

                    string invoiceFor = reader["Invoice_For"]?.ToString() ?? "";
                    bool isPaid = reader["IsPaid"] != DBNull.Value && Convert.ToBoolean(reader["IsPaid"]);
                    string due = reader["Due"]?.ToString() ?? "0";
                    string invoiceId = reader["InvoiceID"]?.ToString() ?? "";

                    if (isPaid)
                        return schoolId + ": পেইড ইনভয়েস #" + invoiceId + " (" + invoiceFor + ") — Due Invoice-এ দেখায় না";
                    return schoolId + ": বকেয়া ইনভয়েস #" + invoiceId + " (" + invoiceFor + "), Due=" + due + " টাকা";
                }
            }
        }

        private static double SafeToDouble(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            double result;
            return double.TryParse(value.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out result)
                ? result
                : 0;
        }

        private void ShowInvoiceResult(string message, bool isError)
        {
            string safeMessage = message.Replace("'", "\\'");
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "invoiceResult",
                "alert('" + safeMessage + "');", true);
        }


        protected void SMS_Paid_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // This functionality is no longer needed as invoices are auto-generated
            // Kept for backward compatibility
        }

        protected void SMS_Invoice_Button_Click(object sender, EventArgs e)
        {
            // This functionality is no longer needed as invoices are auto-generated
            // Kept for backward compatibility
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('SMS invoices are now generated automatically when recharging from Institution Details page.')", true);
        }

        protected void OtherInvoice_Button_Click(object sender, EventArgs e)
        {
            OthersInvoiceSQL.Insert();
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Record Inserted Successfully')", true);
        }

        protected void GenerateCountButton_Click(object sender, EventArgs e)
        {
            try
            {
                GenerateStatusLabel.Visible = false;

                if (string.IsNullOrWhiteSpace(GenerateMonth_TextBox.Text))
                {
                    ShowGenerateStatus("Please select a month", "alert-danger");
                    return;
                }

                // Parse the selected month - support multiple formats
                DateTime selectedMonth;
                string[] formats = { 
                    "MM yyyy", "MMM yyyy", "MMMM yyyy",  // March 2026, Mar 2026
                    "dd MMM yyyy", "d MMM yyyy",         // 01 Mar 2026
                    "yyyy-MM-dd", "MM/dd/yyyy"           // 2026-03-01
                };
                
                if (!DateTime.TryParseExact(GenerateMonth_TextBox.Text.Trim(), formats,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out selectedMonth))
                {
                    // Try standard DateTime.Parse as fallback
                    if (!DateTime.TryParse(GenerateMonth_TextBox.Text.Trim(), out selectedMonth))
                    {
                        ShowGenerateStatus("Invalid month format. Please use format: March 2026 or Mar 2026", "alert-danger");
                        return;
                    }
                }

                // Get the last day of the month (EOMONTH)
                DateTime monthEnd = new DateTime(selectedMonth.Year, selectedMonth.Month, 
                    DateTime.DaysInMonth(selectedMonth.Year, selectedMonth.Month));

                string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Call the stored procedure
                    using (SqlCommand cmd = new SqlCommand("sp_Generate_Monthly_Student_Count", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 120; // 2 minutes timeout for large data
                        
                        cmd.Parameters.AddWithValue("@TargetMonth", monthEnd);
                        
                        SqlParameter countParam = new SqlParameter("@GeneratedCount", SqlDbType.Int);
                        countParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(countParam);
                        
                        SqlParameter msgParam = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 500);
                        msgParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(msgParam);
                        
                        cmd.ExecuteNonQuery();
                        
                        int generatedCount = countParam.Value != DBNull.Value ? Convert.ToInt32(countParam.Value) : 0;
                        string errorMessage = msgParam.Value != DBNull.Value ? msgParam.Value.ToString() : "Unknown error";
                        
                        // Refresh the month dropdown
                        MonthSQL.DataBind();
                        Month_DropDownList.DataBind();
                        
                        // Show result
                        if (errorMessage.StartsWith("Success") || errorMessage.Contains("already exists"))
                        {
                            ShowGenerateStatus(errorMessage, "alert-success");
                            
                            // Try to auto-select the month
                            try
                            {
                                string monthValue = monthEnd.ToString("yyyy-MM-dd");
                                if (Month_DropDownList.Items.FindByValue(monthValue) != null)
                                {
                                    Month_DropDownList.SelectedValue = monthValue;
                                }
                            }
                            catch { }
                        }
                        else
                        {
                            ShowGenerateStatus("Error: " + errorMessage, "alert-danger");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowGenerateStatus($"Error: {ex.Message}", "alert-danger");
            }
        }

        private void ShowGenerateStatus(string message, string cssClass)
        {
            GenerateStatusLabel.Text = message;
            GenerateStatusLabel.CssClass = $"alert {cssClass}";
            GenerateStatusLabel.Visible = true;
        }

        protected void GraceSubmit_Button_Click(object sender, EventArgs e)
        {
            var graceSchoolDdl = FindDeepControl(Page, "GraceSchool_DDL") as DropDownList;
            var graceUntilTb = FindDeepControl(Page, "GraceUntil_TextBox") as TextBox;
            var graceMsgLbl = FindDeepControl(Page, "GraceMsg_Label") as Label;
            var graceListGv = FindDeepControl(Page, "GraceList_GridView") as GridView;

            if (graceSchoolDdl == null || graceUntilTb == null) return;

            int schoolId;
            if (!int.TryParse(graceSchoolDdl.SelectedValue, out schoolId) || schoolId == 0) return;

            DateTime graceUntil;
            string[] formats = { "dd MMM yyyy", "d MMM yyyy", "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };
            if (!DateTime.TryParseExact(graceUntilTb.Text.Trim(), formats,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out graceUntil))
            {
                if (!DateTime.TryParse(graceUntilTb.Text.Trim(), out graceUntil))
                {
                    if (graceMsgLbl != null) graceMsgLbl.Text = "<span class='text-danger'>তারিখ সঠিক নয়।</span>";
                    return;
                }
            }

            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (var con = new SqlConnection(connStr))
                {
                    con.Open();
                    using (var cmd = new SqlCommand(
                        "UPDATE SchoolInfo SET AccessGraceUntil = @Grace WHERE SchoolID = @SID", con))
                    {
                        cmd.Parameters.AddWithValue("@Grace", graceUntil.Date);
                        cmd.Parameters.AddWithValue("@SID", schoolId);
                        cmd.ExecuteNonQuery();
                    }
                }

                if (graceMsgLbl != null) graceMsgLbl.Text = "<span class='text-success'><i class='fa fa-check'></i> Grace period সফলভাবে সেট হয়েছে।</span>";
                graceUntilTb.Text = "";
                graceSchoolDdl.SelectedValue = "0";
                if (graceListGv != null) graceListGv.DataBind();
            }
            catch (Exception ex)
            {
                if (graceMsgLbl != null) graceMsgLbl.Text = "<span class='text-danger'>Error: " + ex.Message + "</span>";
            }
        }

        protected void CancelGrace_Command(object sender, CommandEventArgs e)
        {
            int schoolId;
            if (!int.TryParse(e.CommandArgument.ToString(), out schoolId)) return;

            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (var con = new SqlConnection(connStr))
                {
                    con.Open();
                    using (var cmd = new SqlCommand(
                        "UPDATE SchoolInfo SET AccessGraceUntil = NULL WHERE SchoolID = @SID", con))
                    {
                        cmd.Parameters.AddWithValue("@SID", schoolId);
                        cmd.ExecuteNonQuery();
                    }
                }
                var graceListGv = FindDeepControl(Page, "GraceList_GridView") as GridView;
                if (graceListGv != null) graceListGv.DataBind();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CancelGrace error: " + ex.Message);
            }
        }

        private static Control FindDeepControl(Control parent, string id)
        {
            foreach (Control c in parent.Controls)
            {
                if (c.ID == id) return c;
                var found = FindDeepControl(c, id);
                if (found != null) return found;
            }
            return null;
        }

    }
}