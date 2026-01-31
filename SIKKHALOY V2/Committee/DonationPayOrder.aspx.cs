using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Committee
{
    public partial class DonationPayOrder : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CreateMonthlyDonationGridView(12);
            }
        }

        protected void MemberTypeDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MemberTypeDropDownList.SelectedIndex > 0)
            {
                MembersGridView.DataBind();
                TotalMembersLabel.Text = GetMemberCount().ToString();
                LoadTemplateAmount();
            }
        }

        protected void DonationCategoryDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTemplateAmount();
        }

        private void LoadTemplateAmount()
        {
            if (MemberTypeDropDownList.SelectedIndex > 0 && DonationCategoryDropDownList.SelectedIndex > 0)
            {
                try
                {
                    SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
                    
                    // First check if CommitteeDonationTemplate table exists
                    SqlCommand checkTableCmd = new SqlCommand(@"
                        IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CommitteeDonationTemplate')
                            SELECT 1
                        ELSE
                            SELECT 0", con);
                    
                    con.Open();
                    int tableExists = Convert.ToInt32(checkTableCmd.ExecuteScalar());
                    con.Close();
                    
                    if (tableExists == 1)
                    {
                        SqlCommand cmd = new SqlCommand(@"SELECT Amount FROM CommitteeDonationTemplate 
                            WHERE SchoolID = @SchoolID 
                            AND CommitteeMemberTypeId = @MemberTypeId 
                            AND CommitteeDonationCategoryId = @CategoryId", con);

                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                        cmd.Parameters.AddWithValue("@MemberTypeId", MemberTypeDropDownList.SelectedValue);
                        cmd.Parameters.AddWithValue("@CategoryId", DonationCategoryDropDownList.SelectedValue);

                        con.Open();
                        object result = cmd.ExecuteScalar();
                        con.Close();

                        if (result != null)
                        {
                            TemplateAmountLabel.Visible = true;
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "setTemplateAmount",
                                $"document.getElementById('templateAmountValue').innerText = '{result}';", true);
                        }
                        else
                        {
                            TemplateAmountLabel.Visible = false;
                        }
                    }
                    else
                    {
                        // Template feature not available, hide label
                        TemplateAmountLabel.Visible = false;
                    }
                }
                catch (Exception)
                {
                    // If any error occurs, just hide the template feature
                    TemplateAmountLabel.Visible = false;
                }
            }
        }

        private int GetMemberCount()
        {
            int count = 0;
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM CommitteeMember WHERE SchoolID = @SchoolID AND CommitteeMemberTypeId = @CommitteeMemberTypeId", con);
            cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            cmd.Parameters.AddWithValue("@CommitteeMemberTypeId", MemberTypeDropDownList.SelectedValue);

            con.Open();
            count = Convert.ToInt32(cmd.ExecuteScalar());
            con.Close();

            return count;
        }

        private void CreateMonthlyDonationGridView(int monthCount)
        {
            ArrayList monthData = new ArrayList();
            monthData.Add(new { MonthCount = monthCount });

            MonthlyDonationGridView.DataSource = monthData;
            MonthlyDonationGridView.DataBind();
        }

        protected void MonthlyDonationGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GridView MonthDetailsGridView = (GridView)e.Row.FindControl("MonthDetailsGridView");
                Label MonthCountLabel = (Label)e.Row.FindControl("MonthCountLabel");

                int monthCount = Convert.ToInt32(MonthCountLabel.Text);
                ArrayList values = new ArrayList();
                for (int i = 0; i < monthCount; i++)
                {
                    values.Add(i + 1);
                }

                MonthDetailsGridView.DataSource = values;
                MonthDetailsGridView.DataBind();
            }
        }

        protected void PayOrderButton_Click(object sender, EventArgs e)
        {
            if (DonationCategoryDropDownList.SelectedIndex == 0)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert",
                    "alert('Please select a donation category.');", true);
                return;
            }

            double successCount = 0;
            double failCount = 0;
            double duplicateCount = 0;

            try
            {
                foreach (GridViewRow memberRow in MembersGridView.Rows)
                {
                    CheckBox SingleMemberCheckBox = memberRow.FindControl("SingleMemberCheckBox") as CheckBox;
                    if (SingleMemberCheckBox != null && SingleMemberCheckBox.Checked)
                    {
                        int committeeMemberId = Convert.ToInt32(MembersGridView.DataKeys[memberRow.RowIndex]["CommitteeMemberId"]);

                        foreach (GridViewRow monthlyRow in MonthlyDonationGridView.Rows)
                        {
                            CheckBox AddMonthlyDonationCheckBox = monthlyRow.FindControl("AddMonthlyDonationCheckBox") as CheckBox;

                            if (AddMonthlyDonationCheckBox != null && AddMonthlyDonationCheckBox.Checked)
                            {
                                GridView MonthDetailsGridView = monthlyRow.FindControl("MonthDetailsGridView") as GridView;

                                if (MonthDetailsGridView != null)
                                {
                                    foreach (GridViewRow monthRow in MonthDetailsGridView.Rows)
                                    {
                                        CheckBox MonthCheckBox = monthRow.FindControl("MonthCheckBox") as CheckBox;

                                        if (MonthCheckBox != null && MonthCheckBox.Checked)
                                        {
                                            TextBox PayForTextBox = monthRow.FindControl("PayForTextBox") as TextBox;
                                            TextBox AmountTextBox = monthRow.FindControl("AmountTextBox") as TextBox;
                                            TextBox PromiseDateTextBox = monthRow.FindControl("PromiseDateTextBox") as TextBox;
                                            TextBox DescriptionTextBox = monthRow.FindControl("DescriptionTextBox") as TextBox;

                                            if (PayForTextBox != null && AmountTextBox != null && 
                                                !string.IsNullOrWhiteSpace(PayForTextBox.Text) && 
                                                !string.IsNullOrWhiteSpace(AmountTextBox.Text))
                                            {
                                                double amount;
                                                if (double.TryParse(AmountTextBox.Text.Trim(), out amount))
                                                {
                                                    // Check for duplicate
                                                    if (IsDuplicateDonation(committeeMemberId, DonationCategoryDropDownList.SelectedValue, PayForTextBox.Text.Trim()))
                                                    {
                                                        duplicateCount++;
                                                        continue;
                                                    }

                                                    try
                                                    {
                                                        PayOrderSQL.InsertParameters["CommitteeMemberId"].DefaultValue = committeeMemberId.ToString();
                                                        PayOrderSQL.InsertParameters["Amount"].DefaultValue = amount.ToString();
                                                        PayOrderSQL.InsertParameters["Description"].DefaultValue = 
                                                            string.IsNullOrWhiteSpace(DescriptionTextBox?.Text) ? 
                                                            PayForTextBox.Text : 
                                                            DescriptionTextBox.Text;
                                                        PayOrderSQL.InsertParameters["PromiseDate"].DefaultValue = 
                                                            string.IsNullOrWhiteSpace(PromiseDateTextBox?.Text) ? 
                                                            DBNull.Value.ToString() : 
                                                            PromiseDateTextBox.Text;

                                                        PayOrderSQL.Insert();
                                                        successCount++;
                                                    }
                                                    catch
                                                    {
                                                        failCount++;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                string message = successCount.ToString() + " Donation Pay Order(s) created successfully";
                if (duplicateCount > 0)
                {
                    message += ", " + duplicateCount.ToString() + " duplicate(s) skipped";
                }
                if (failCount > 0)
                {
                    message += ", " + failCount.ToString() + " failed";
                }

                ScriptManager.RegisterStartupScript(this, this.GetType(), "PayOrderSuccess",
                    $"alert('{message}'); window.location.href = 'Donations.aspx';", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "PayOrderError",
                    $"alert('Error creating pay orders: {ex.Message}');", true);
            }
        }

        private bool IsDuplicateDonation(int committeeMemberId, string categoryId, string description)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            SqlCommand cmd = new SqlCommand(@"SELECT COUNT(*) FROM CommitteeDonation 
                WHERE CommitteeMemberId = @CommitteeMemberId 
                AND CommitteeDonationCategoryId = @CategoryId 
                AND Description = @Description 
                AND SchoolID = @SchoolID", con);
            
            cmd.Parameters.AddWithValue("@CommitteeMemberId", committeeMemberId);
            cmd.Parameters.AddWithValue("@CategoryId", categoryId);
            cmd.Parameters.AddWithValue("@Description", description);
            cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());

            con.Open();
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            con.Close();

            return count > 0;
        }

        [WebMethod(EnableSession = true)]
        public static string GetMonth(string prefix)
        {
            List<EduMonthYear> months = new List<EduMonthYear>();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"WITH months(date) AS (
                        SELECT StartDate FROM Education_Year WHERE (EducationYearID = @EducationYearID) 
                        UNION ALL 
                        SELECT DATEADD(month, 1, date) FROM months 
                        WHERE DATEADD(month, 1, date) <= (SELECT EndDate FROM Education_Year WHERE (EducationYearID = @EducationYearID))
                    ) 
                    SELECT FORMAT(Date, 'MMM yyyy') as Month_Year, FORMAT(date, 'MMMM') AS [Month] 
                    FROM months 
                    WHERE FORMAT(date, 'MMMM') LIKE @Search + '%'";

                    cmd.Parameters.AddWithValue("@Search", prefix);
                    cmd.Parameters.AddWithValue("@EducationYearID", HttpContext.Current.Session["Edu_Year"].ToString());
                    cmd.Connection = con;

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        months.Add(new EduMonthYear
                        {
                            Month = dr["Month"].ToString(),
                            MonthYearValue = dr["Month_Year"].ToString()
                        });
                    }
                    con.Close();
                }
            }

            var json = new JavaScriptSerializer().Serialize(months);
            return json;
        }

        [WebMethod(EnableSession = true)]
        public static string GetDonationMonths()
        {
            List<EduMonthYear> months = new List<EduMonthYear>();
            int year = DateTime.Now.Year;

            try
            {
                // Try to get the year from Education_Year if session is available
                if (HttpContext.Current != null && HttpContext.Current.Session != null && HttpContext.Current.Session["Edu_Year"] != null)
                {
                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("SELECT StartDate FROM Education_Year WHERE EducationYearID = @EducationYearID", con))
                        {
                            cmd.Parameters.AddWithValue("@EducationYearID", HttpContext.Current.Session["Edu_Year"].ToString());
                            con.Open();
                            object result = cmd.ExecuteScalar();

                            if (result != null && result != DBNull.Value)
                            {
                                year = Convert.ToDateTime(result).Year;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Fallback to current year
                year = DateTime.Now.Year;
            }

            // Generate exactly 12 months starting from January 1st
            DateTime loopDate = new DateTime(year, 1, 1);

            for (int i = 0; i < 12; i++)
            {
                months.Add(new EduMonthYear
                {
                    MonthName = loopDate.ToString("MMMM yyyy"),
                    MonthYearValue = loopDate.ToString("MMM yyyy")
                });
                loopDate = loopDate.AddMonths(1);
            }

            return new JavaScriptSerializer().Serialize(months);
        }

        class EduMonthYear
        {
            public string Month { get; set; }
            public string MonthYearValue { get; set; }
            public string MonthName { get; set; }
        }
    }
}
