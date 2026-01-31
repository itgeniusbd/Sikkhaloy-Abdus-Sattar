using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Committee
{
    public partial class DonationBulkEdit : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                UpdateResultCount();
            }
        }

        protected void DonationsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == System.Web.UI.WebControls.DataControlRowType.DataRow)
            {
                // Check if this row's donation is paid
                var isPaid = DataBinder.Eval(e.Row.DataItem, "IsPaid");
                if (isPaid != null && Convert.ToInt32(isPaid) == 1)
                {
                    // Disable checkbox for paid donations
                    CheckBox chk = e.Row.FindControl("SelectCheckBox") as CheckBox;
                    if (chk != null)
                    {
                        chk.Enabled = false;
                        chk.ToolTip = "Cannot edit paid donation";
                    }
                }
            }
        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            DonationsGridView.DataBind();
            UpdateResultCount();
        }

        protected void ClearFiltersButton_Click(object sender, EventArgs e)
        {
            MemberTypeDropDownList.SelectedIndex = 0;
            SearchNameTextBox.Text = "";
            SearchPhoneTextBox.Text = "";
            SelectedDonorIdHiddenField.Value = "";
            CategoryDropDownList.SelectedIndex = 0;
            StatusDropDownList.SelectedIndex = 0;
            DonationsGridView.DataBind();
            UpdateResultCount();
        }

        private void UpdateResultCount()
        {
            try
            {
                DonationsGridView.DataBind();
                ResultCountLabel.Text = $"Total: {DonationsGridView.Rows.Count} donation(s) found";
            }
            catch
            {
                ResultCountLabel.Text = "";
            }
        }

        [System.Web.Services.WebMethod]
        public static string SearchDonorsByName(string searchText)
        {
            List<DonorInfo> donors = new List<DonorInfo>();
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            
            SqlCommand cmd = new SqlCommand(@"SELECT TOP 10 CommitteeMemberId, MemberName, ISNULL(SmsNumber, '') AS SmsNumber 
                FROM CommitteeMember 
                WHERE SchoolID = @SchoolID AND MemberName LIKE '%' + @SearchText + '%'
                ORDER BY MemberName", con);
            
            cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
            cmd.Parameters.AddWithValue("@SearchText", searchText);

            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                donors.Add(new DonorInfo
                {
                    CommitteeMemberId = dr["CommitteeMemberId"].ToString(),
                    MemberName = dr["MemberName"].ToString(),
                    SmsNumber = dr["SmsNumber"].ToString()
                });
            }
            con.Close();

            return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(donors);
        }

        [System.Web.Services.WebMethod]
        public static string SearchDonorsByPhone(string searchText)
        {
            List<DonorInfo> donors = new List<DonorInfo>();
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            
            SqlCommand cmd = new SqlCommand(@"SELECT TOP 10 CommitteeMemberId, MemberName, ISNULL(SmsNumber, '') AS SmsNumber 
                FROM CommitteeMember 
                WHERE SchoolID = @SchoolID AND SmsNumber LIKE '%' + @SearchText + '%'
                ORDER BY MemberName", con);
            
            cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
            cmd.Parameters.AddWithValue("@SearchText", searchText);

            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                donors.Add(new DonorInfo
                {
                    CommitteeMemberId = dr["CommitteeMemberId"].ToString(),
                    MemberName = dr["MemberName"].ToString(),
                    SmsNumber = dr["SmsNumber"].ToString()
                });
            }
            con.Close();

            return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(donors);
        }

        private class DonorInfo
        {
            public string CommitteeMemberId { get; set; }
            public string MemberName { get; set; }
            public string SmsNumber { get; set; }
        }

        protected void BulkDeleteButton_Click(object sender, EventArgs e)
        {
            int deleteCount = 0;
            int skipCount = 0;
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

            try
            {
                foreach (GridViewRow row in DonationsGridView.Rows)
                {
                    CheckBox chk = row.FindControl("SelectCheckBox") as CheckBox;
                    if (chk != null && chk.Checked && chk.Enabled)
                    {
                        int donationId = Convert.ToInt32(DonationsGridView.DataKeys[row.RowIndex]["CommitteeDonationId"]);

                        SqlCommand cmd = new SqlCommand(@"DELETE FROM CommitteeDonation 
                            WHERE CommitteeDonationId = @DonationId 
                            AND PaidAmount = 0 
                            AND SchoolID = @SchoolID", con);
                        
                        cmd.Parameters.AddWithValue("@DonationId", donationId);
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());

                        if (con.State != ConnectionState.Open) con.Open();
                        int result = cmd.ExecuteNonQuery();
                        
                        if (result > 0)
                            deleteCount++;
                        else
                            skipCount++;
                    }
                }

                DonationsGridView.DataBind();
                UpdateResultCount();

                string message = deleteCount + " donation(s) deleted successfully!";
                if (skipCount > 0)
                {
                    message += " (" + skipCount + " already paid - skipped)";
                }

                ScriptManager.RegisterStartupScript(this, this.GetType(), "success",
                    $"$.notify('{message}', 'success');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "error",
                    $"$.notify('Error: {ex.Message}', 'error');", true);
            }
            finally
            {
                if (con.State == System.Data.ConnectionState.Open)
                    con.Close();
            }
        }

        protected void BulkUpdateButton_Click(object sender, EventArgs e)
        {
            int updateCount = 0;
            int skipCount = 0;
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

            try
            {
                foreach (GridViewRow row in DonationsGridView.Rows)
                {
                    CheckBox chk = row.FindControl("SelectCheckBox") as CheckBox;
                    if (chk != null && chk.Checked && chk.Enabled)
                    {
                        int donationId = Convert.ToInt32(DonationsGridView.DataKeys[row.RowIndex]["CommitteeDonationId"]);
                        TextBox amountTextBox = row.FindControl("AmountTextBox") as TextBox;
                        TextBox dateTextBox = row.FindControl("PromiseDateTextBox") as TextBox;

                        if (amountTextBox != null && dateTextBox != null)
                        {
                            try
                            {
                                decimal amount = decimal.Parse(amountTextBox.Text);
                                DateTime promiseDate = DateTime.Parse(dateTextBox.Text);

                                SqlCommand cmd = new SqlCommand(@"UPDATE CommitteeDonation 
                                    SET Amount = @Amount, 
                                        PromiseDate = @PromiseDate 
                                    WHERE CommitteeDonationId = @DonationId 
                                    AND PaidAmount = 0 
                                    AND SchoolID = @SchoolID", con);

                                cmd.Parameters.AddWithValue("@Amount", amount);
                                cmd.Parameters.AddWithValue("@PromiseDate", promiseDate);
                                cmd.Parameters.AddWithValue("@DonationId", donationId);
                                cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());

                                if (con.State != ConnectionState.Open) con.Open();
                                int result = cmd.ExecuteNonQuery();

                                if (result > 0)
                                    updateCount++;
                                else
                                    skipCount++;
                            }
                            catch
                            {
                                skipCount++;
                            }
                        }
                    }
                }

                DonationsGridView.DataBind();
                UpdateResultCount();

                string message = updateCount + " donation(s) updated successfully!";
                if (skipCount > 0)
                {
                    message += " (" + skipCount + " skipped)";
                }

                ScriptManager.RegisterStartupScript(this, this.GetType(), "success",
                    $"$.notify('{message}', 'success');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "error",
                    $"$.notify('Error: {ex.Message}', 'error');", true);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }
    }
}
