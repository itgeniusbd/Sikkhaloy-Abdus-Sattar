using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
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
                    -- Currently running?
                    (SELECT COUNT(*) FROM msdb.dbo.sysjobactivity a2
                     WHERE a2.job_id = j.job_id AND a2.start_execution_date IS NOT NULL
                       AND a2.stop_execution_date IS NULL)  AS IsRunning
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
            foreach (GridViewRow row in Payment_GridView.Rows)
            {
                var Invoice_CheckBox = row.FindControl("Invoice_CheckBox") as CheckBox;
                var Total_Student_Label = row.FindControl("Total_Student_Label") as Label;
                var Committee_Count_Label = row.FindControl("Committee_Count_Label") as Label;
                var PerStudent_Label = row.FindControl("PerStudent_Label") as Label;
                var Fixed_Label = row.FindControl("Fixed_Label") as Label;
                var Discount_TextBox = row.FindControl("Discount_TextBox") as TextBox;

                double Amount = 0;
                double TotalStudent = Convert.ToDouble(Total_Student_Label.Text);
                double CommitteeCount = Committee_Count_Label != null ? Convert.ToDouble(Committee_Count_Label.Text) : 0;
                double TotalBillableCount = TotalStudent + CommitteeCount; // Student + Committee
                double PerStudent = Convert.ToDouble(PerStudent_Label.Text);
                double Fixed = Convert.ToDouble(Fixed_Label.Text);
                double Discount = Convert.ToDouble(Discount_TextBox.Text);
                DateTime Issue = Convert.ToDateTime(sIssueDate_TextBox.Text);

                if (Invoice_CheckBox.Checked)
                {
                    if (Fixed == 0)
                    {
                        Amount = TotalBillableCount * PerStudent; // Changed: Use total billable count
                        PayOrderSQL.InsertParameters["UnitPrice"].DefaultValue = PerStudent.ToString();
                    }
                    else
                    {
                        Amount = Fixed;
                        PayOrderSQL.InsertParameters["UnitPrice"].DefaultValue = null;
                    }

                    PayOrderSQL.InsertParameters["EndDate"].DefaultValue = Issue.AddDays(15).ToString();
                    PayOrderSQL.InsertParameters["SchoolID"].DefaultValue = Payment_GridView.DataKeys[row.DataItemIndex]["SchoolID"].ToString();
                    PayOrderSQL.InsertParameters["TotalAmount"].DefaultValue = Amount.ToString();
                    PayOrderSQL.InsertParameters["Discount"].DefaultValue = Discount_TextBox.Text;
                    PayOrderSQL.InsertParameters["Unit"].DefaultValue = TotalBillableCount.ToString(); // Changed: Use total billable count
                    PayOrderSQL.Insert();
                }
            }

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Record Inserted Successfully')", true);
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