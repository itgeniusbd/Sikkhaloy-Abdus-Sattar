using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority.Reference
{
    public partial class Referral_Management : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

        private static readonly string[] UiDateFormats = {
            "dd MMM yyyy", "d MMM yyyy", "dd MMMM yyyy", "d MMMM yyyy",
            "dd-MMM-yyyy", "d-MMM-yyyy", "dd/MM/yyyy", "d/M/yyyy",
            "yyyy-MM-dd", "dd-MM-yyyy"
        };

        protected string FormatUiDate(object value)
        {
            if (value == null || value == DBNull.Value) return "";
            return Convert.ToDateTime(value).ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        }

        protected bool IsExpired(object value)
        {
            if (value == null || value == DBNull.Value) return false;
            return Convert.ToDateTime(value).Date < DateTime.Today;
        }

        protected bool IsActiveExpiry(object value)
        {
            if (value == null || value == DBNull.Value) return false;
            return Convert.ToDateTime(value).Date >= DateTime.Today;
        }

        private static bool TryParseUiDate(string text, out DateTime date)
        {
            date = default(DateTime);
            if (string.IsNullOrWhiteSpace(text)) return false;
            text = text.Trim();

            // Truncated full-month leftovers like "31 Decembe" from old picker format
            if (text.EndsWith("Decembe", StringComparison.OrdinalIgnoreCase))
                text = text.Substring(0, text.Length - "Decembe".Length) + "Dec";

            var cultures = new[]
            {
                CultureInfo.InvariantCulture,
                CultureInfo.GetCultureInfo("en-GB"),
                CultureInfo.GetCultureInfo("en-US"),
                CultureInfo.CurrentCulture
            };

            foreach (var culture in cultures)
            {
                if (DateTime.TryParseExact(text, UiDateFormats, culture, DateTimeStyles.AllowWhiteSpaces, out date))
                    return true;
                if (DateTime.TryParse(text, culture, DateTimeStyles.AllowWhiteSpaces, out date))
                    return true;
            }
            return false;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadReferrers();
            }
        }

        // Load the list of referrers
        private void LoadReferrers()
        {
            string sql = @"
                SELECT 
                    r.ReferenceID,
                    r.Reference_SN,
                    r.Reference_Name,
                    r.Reference_Phone,
                    r.Address,
                    r.Marketing_StartDate,
                    COUNT(DISTINCT rs.Reference_School_ID) AS TotalSchools,
                    ISNULL(SUM(DISTINCT rp.Commission_Amount), 0) AS TotalCommission,
                    ISNULL((SELECT SUM(Amount) FROM AAP_Reference_PaymentRecord pr WHERE pr.ReferenceID = r.ReferenceID), 0) AS PaidAmount,
                    ISNULL(SUM(DISTINCT rp.Commission_Amount), 0) - ISNULL((SELECT SUM(Amount) FROM AAP_Reference_PaymentRecord pr WHERE pr.ReferenceID = r.ReferenceID), 0) AS DueAmount
                FROM AAP_Reference r
                LEFT JOIN AAP_Reference_School rs ON r.ReferenceID = rs.ReferenceID
                LEFT JOIN AAP_Reference_Commission rp ON r.ReferenceID = rp.ReferenceID                GROUP BY r.ReferenceID, r.Reference_SN, r.Reference_Name, r.Reference_Phone, r.Address, r.Marketing_StartDate
                ORDER BY r.Reference_SN";

            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlDataAdapter da = new SqlDataAdapter(sql, con))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                ReferrerGridView.DataSource = dt;
                ReferrerGridView.DataBind();
            }
        }

        // Save new referrer or update existing one
        protected void SaveRefButton_Click(object sender, EventArgs e)
        {
            RefMsgLabel.Text = "";
            if (string.IsNullOrWhiteSpace(RefNameTextBox.Text))
            {
                RefMsgLabel.CssClass = "text-danger font-weight-bold";
                RefMsgLabel.Text = "Please enter referrer name.";
                return;
            }

            int editID = int.Parse(EditReferenceIDHidden.Value);
            DateTime? startDate = null;
            DateTime sd;
            if (DateTime.TryParse(RefStartDateTextBox.Text.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out sd)) startDate = sd;

            using (SqlConnection con = new SqlConnection(ConnStr))
            {
                con.Open();
                if (editID == 0)
                {
                    // Insert new referrer
                    string sql = @"INSERT INTO AAP_Reference(Reference_Name, Reference_Phone, Address, Marketing_StartDate)
                                   VALUES (@Name, @Phone, @Address, @StartDate)";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@Name", RefNameTextBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@Phone", RefPhoneTextBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@Address", RefAddressTextBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@StartDate", startDate.HasValue ? (object)startDate.Value : DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                    RefMsgLabel.CssClass = "text-success font-weight-bold";
                    RefMsgLabel.Text = "Referrer added successfully.";
                }
                else
                {
                    // Update existing referrer
                    string sql = @"UPDATE AAP_Reference SET Reference_Name=@Name, Reference_Phone=@Phone, 
                                   Address=@Address, Marketing_StartDate=@StartDate 
                                   WHERE ReferenceID=@ID";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@Name", RefNameTextBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@Phone", RefPhoneTextBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@Address", RefAddressTextBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@StartDate", startDate.HasValue ? (object)startDate.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@ID", editID);
                        cmd.ExecuteNonQuery();
                    }
                    RefMsgLabel.CssClass = "text-success font-weight-bold";
                    RefMsgLabel.Text = "Referrer info updated successfully.";
                }
            }

            ClearRefForm();
            LoadReferrers();
        }

        private void ClearRefForm()
        {
            RefNameTextBox.Text = "";
            RefPhoneTextBox.Text = "";
            RefAddressTextBox.Text = "";
            RefStartDateTextBox.Text = "";
            EditReferenceIDHidden.Value = "0";
            SaveRefButton.Text = "Save Referrer";
        }

        // Show the assign panel when a referrer is selected
        protected void ReferrerGridView_SelectedIndexChanged(object sender, EventArgs e)
        {
            int refID = (int)ReferrerGridView.SelectedDataKey["ReferenceID"];
            string name = ReferrerGridView.SelectedRow.Cells[2].Text;
            ShowAssignPanel(refID, name);
        }

        protected void ReferrerGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditRef")
            {
                int refID = int.Parse(e.CommandArgument.ToString());
                LoadRefForEdit(refID);
            }
        }

        private void LoadRefForEdit(int refID)
        {
            string sql = "SELECT * FROM AAP_Reference WHERE ReferenceID=@ID";
            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@ID", refID);
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        RefNameTextBox.Text = dr["Reference_Name"].ToString();
                        RefPhoneTextBox.Text = dr["Reference_Phone"].ToString();
                        RefAddressTextBox.Text = dr["Address"].ToString();
                        RefStartDateTextBox.Text = dr["Marketing_StartDate"] != DBNull.Value
                            ? ((DateTime)dr["Marketing_StartDate"]).ToString("dd MMM yyyy") : "";
                        EditReferenceIDHidden.Value = refID.ToString();
                        SaveRefButton.Text = "Update";
                    }
                }
            }
        }

        private void ShowAssignPanel(int refID, string refName)
        {
            AssignPanel.Visible = true;
            SelectedRefNameLabel.Text = refName;
            ViewState["CurrentReferenceID"] = refID;
            ViewState["CurrentReferenceName"] = refName;
            LoadAssignedSchools(refID);
            // Reset institution search selection
            InsSearchTextBox.Text = "";
            SelectedSchoolIDHidden.Value = "0";
            SelectedSchoolNameHidden.Value = "";
            SelectedInsPanel.Visible = false;
            searchResultDiv.Visible = false;
        }

        // Search all institutions (with or without invoice)
        protected void InsSearchTextBox_TextChanged(object sender, EventArgs e)
        {
            string keyword = InsSearchTextBox.Text.Trim();
            SelectedSchoolIDHidden.Value = "0";
            SelectedSchoolNameHidden.Value = "";
            SelectedInsPanel.Visible = false;

            if (keyword.Length < 1)
            {
                searchResultDiv.Visible = false;
                SearchResultRepeater.DataSource = null;
                SearchResultRepeater.DataBind();
                return;
            }

            int refID = ViewState["CurrentReferenceID"] != null ? (int)ViewState["CurrentReferenceID"] : 0;

            // All SchoolInfo rows — no AAP_Invoice join. Newest / starts-with first.
            string sql = @"
                SELECT TOP 50
                    s.SchoolID,
                    s.SchoolName,
                    s.Phone,
                    CASE WHEN EXISTS (SELECT 1 FROM AAP_Invoice i WHERE i.SchoolID = s.SchoolID) THEN 1 ELSE 0 END AS HasInvoice
                FROM SchoolInfo s
                WHERE (
                        s.SchoolName LIKE @Keyword
                     OR ISNULL(s.Phone, '') LIKE @Keyword
                     OR ISNULL(s.UserName, '') LIKE @Keyword
                     OR CAST(s.SchoolID AS NVARCHAR(20)) LIKE @Keyword
                     OR CAST(ISNULL(s.School_SN, 0) AS NVARCHAR(20)) LIKE @Keyword
                  )
                  AND (@RefID = 0 OR s.SchoolID NOT IN (
                        SELECT SchoolID FROM AAP_Reference_School WHERE ReferenceID = @RefID
                  ))
                ORDER BY
                    CASE WHEN s.SchoolName LIKE @StartsWith THEN 0 ELSE 1 END,
                    s.SchoolID DESC";

            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlDataAdapter da = new SqlDataAdapter(sql, con))
            {
                da.SelectCommand.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");
                da.SelectCommand.Parameters.AddWithValue("@StartsWith", keyword + "%");
                da.SelectCommand.Parameters.AddWithValue("@RefID", refID);
                DataTable dt = new DataTable();
                da.Fill(dt);
                SearchResultRepeater.DataSource = dt;
                SearchResultRepeater.DataBind();
                searchResultDiv.Visible = dt.Rows.Count > 0;
            }
        }

        protected void SearchResultRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "SelectSchool")
            {
                int schoolID;
                if (!int.TryParse(e.CommandArgument.ToString(), out schoolID) || schoolID <= 0)
                    return;

                string schoolName = "";
                using (SqlConnection con = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand("SELECT SchoolName FROM SchoolInfo WHERE SchoolID=@ID", con))
                {
                    cmd.Parameters.AddWithValue("@ID", schoolID);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        schoolName = result.ToString();
                }

                if (string.IsNullOrEmpty(schoolName))
                    return;

                SelectedSchoolIDHidden.Value = schoolID.ToString();
                SelectedSchoolNameHidden.Value = schoolName;
                InsSearchTextBox.Text = schoolName;
                SelectedInsNameLabel.Text = schoolID + " — " + schoolName;
                SelectedInsPanel.Visible = true;
                searchResultDiv.Visible = false;
                SearchResultRepeater.DataSource = null;
                SearchResultRepeater.DataBind();
            }
        }

        // Assign institution to selected referrer
        protected void AssignInsButton_Click(object sender, EventArgs e)
        {
            AssignMsgLabel.Text = "";

            int refID = ViewState["CurrentReferenceID"] != null ? (int)ViewState["CurrentReferenceID"] : 0;
            int schoolID = 0;
            int.TryParse(SelectedSchoolIDHidden.Value, out schoolID);

            if (refID == 0 || schoolID == 0)
            {
                AssignMsgLabel.CssClass = "text-danger font-weight-bold";
                AssignMsgLabel.Text = "Please select referrer and institution.";
                return;
            }

            double pct = 0;
            if (!double.TryParse(CommissionPctTextBox.Text.Trim(), out pct) || pct <= 0)
            {
                AssignMsgLabel.CssClass = "text-danger font-weight-bold";
                AssignMsgLabel.Text = "Please enter valid commission percentage.";
                return;
            }

            DateTime? signupDate = null;
            DateTime sd;
            if (TryParseUiDate(SignupDateTextBox.Text, out sd)) signupDate = sd;

            DateTime? expireDate = null;
            DateTime ed;
            if (TryParseUiDate(CommExpireDateTextBox.Text, out ed)) expireDate = ed;

            // If user typed an expiry but it could not be parsed, stop with a clear error
            if (!string.IsNullOrWhiteSpace(CommExpireDateTextBox.Text) && !expireDate.HasValue)
            {
                AssignMsgLabel.CssClass = "text-danger font-weight-bold";
                AssignMsgLabel.Text = "Invalid Expiry Date. Use format like 31 Dec 2026.";
                return;
            }
            if (!string.IsNullOrWhiteSpace(SignupDateTextBox.Text) && !signupDate.HasValue)
            {
                AssignMsgLabel.CssClass = "text-danger font-weight-bold";
                AssignMsgLabel.Text = "Invalid Signup Date. Use format like 01 Jul 2024.";
                return;
            }

            using (SqlConnection con = new SqlConnection(ConnStr))
            {
                con.Open();
                // Check duplicate assignment
                string checkSql = "SELECT COUNT(*) FROM AAP_Reference_School WHERE SchoolID=@SchoolID AND ReferenceID=@RefID";
                using (SqlCommand chk = new SqlCommand(checkSql, con))
                {
                    chk.Parameters.AddWithValue("@SchoolID", schoolID);
                    chk.Parameters.AddWithValue("@RefID", refID);
                    int exists = (int)chk.ExecuteScalar();
                    if (exists > 0)
                    {
                        AssignMsgLabel.CssClass = "text-warning font-weight-bold";
                        AssignMsgLabel.Text = "This institution is already assigned to this referrer.";
                        return;
                    }
                }

                string sql = @"INSERT INTO AAP_Reference_School(SchoolID, ReferenceID, Percentage, School_SignUp_Date, End_Reference_Date)
                               VALUES(@SchoolID, @RefID, @Pct, @Signup, @Expire)";
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", schoolID);
                    cmd.Parameters.AddWithValue("@RefID", refID);
                    cmd.Parameters.AddWithValue("@Pct", pct);
                    cmd.Parameters.AddWithValue("@Signup", signupDate.HasValue ? (object)signupDate.Value.Date : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Expire", expireDate.HasValue ? (object)expireDate.Value.Date : DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }

            AssignMsgLabel.CssClass = "text-success font-weight-bold";
            AssignMsgLabel.Text = "Institution assigned successfully.";
            CommissionPctTextBox.Text = "";
            SignupDateTextBox.Text = "";
            CommExpireDateTextBox.Text = "";
            SelectedSchoolIDHidden.Value = "0";
            SelectedSchoolNameHidden.Value = "";
            InsSearchTextBox.Text = "";
            SelectedInsPanel.Visible = false;

            LoadAssignedSchools(refID);
            LoadReferrers();
        }

        // Load assigned institutions list
        private void LoadAssignedSchools(int refID)
        {
            string sql = @"
                SELECT 
                    rs.Reference_School_ID,
                    s.SchoolName,
                    s.Phone,
                    rs.Percentage,
                    rs.School_SignUp_Date,
                    rs.End_Reference_Date,
                    ISNULL((
                        SELECT SUM(rc.Commission_Amount) 
                        FROM AAP_Reference_Commission rc 
                        WHERE rc.Reference_School_ID = rs.Reference_School_ID
                    ), 0) AS TotalCommission,
                    ISNULL((
                        SELECT SUM(pr.Amount) 
                        FROM AAP_Reference_PaymentRecord pr 
                        WHERE pr.Reference_School_ID = rs.Reference_School_ID
                    ), 0) AS PaidCommission
                FROM AAP_Reference_School rs
                INNER JOIN SchoolInfo s ON rs.SchoolID = s.SchoolID
                WHERE rs.ReferenceID = @RefID
                ORDER BY rs.Reference_School_ID DESC";

            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlDataAdapter da = new SqlDataAdapter(sql, con))
            {
                da.SelectCommand.Parameters.AddWithValue("@RefID", refID);
                DataTable dt = new DataTable();
                da.Fill(dt);
                AssignedSchoolsGridView.DataSource = dt;
                AssignedSchoolsGridView.DataBind();
            }
        }

        // Delete / Edit assigned institution
        protected void AssignedSchoolsGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteAssign")
            {
                int rsID = int.Parse(e.CommandArgument.ToString());
                using (SqlConnection con = new SqlConnection(ConnStr))
                {
                    con.Open();
                    string sql = "DELETE FROM AAP_Reference_School WHERE Reference_School_ID=@ID";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@ID", rsID);
                        cmd.ExecuteNonQuery();
                    }
                }
                int refID = (int)ViewState["CurrentReferenceID"];
                LoadAssignedSchools(refID);
                LoadReferrers();
            }
        }

        protected void AssignedSchoolsGridView_RowEditing(object sender, GridViewEditEventArgs e)
        {
            AssignedSchoolsGridView.EditIndex = e.NewEditIndex;
            int refID = (int)ViewState["CurrentReferenceID"];
            LoadAssignedSchools(refID);
        }

        protected void AssignedSchoolsGridView_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            AssignedSchoolsGridView.EditIndex = -1;
            int refID = (int)ViewState["CurrentReferenceID"];
            LoadAssignedSchools(refID);
        }

        protected void AssignedSchoolsGridView_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int rsID = (int)AssignedSchoolsGridView.DataKeys[e.RowIndex]["Reference_School_ID"];
            GridViewRow row = AssignedSchoolsGridView.Rows[e.RowIndex];

            var pctBox = (TextBox)row.FindControl("EditPctTextBox");
            var signupBox = (TextBox)row.FindControl("EditSignupTextBox");
            var expireBox = (TextBox)row.FindControl("EditExpireTextBox");

            if (pctBox == null || signupBox == null || expireBox == null)
            {
                e.Cancel = true;
                AssignMsgLabel.CssClass = "text-danger font-weight-bold";
                AssignMsgLabel.Text = "Edit controls not found. Please try again.";
                return;
            }

            double pct = 0;
            double.TryParse(pctBox.Text.Trim(), out pct);

            DateTime? signupDate = null;
            DateTime sd;
            if (TryParseUiDate(signupBox.Text, out sd)) signupDate = sd;

            DateTime? expireDate = null;
            DateTime ed;
            if (TryParseUiDate(expireBox.Text, out ed)) expireDate = ed;

            if (!string.IsNullOrWhiteSpace(expireBox.Text) && !expireDate.HasValue)
            {
                e.Cancel = true;
                AssignMsgLabel.CssClass = "text-danger font-weight-bold";
                AssignMsgLabel.Text = "Invalid Expiry Date. Use format like 31 Dec 2026.";
                return;
            }
            if (!string.IsNullOrWhiteSpace(signupBox.Text) && !signupDate.HasValue)
            {
                e.Cancel = true;
                AssignMsgLabel.CssClass = "text-danger font-weight-bold";
                AssignMsgLabel.Text = "Invalid Signup Date. Use format like 01 Jul 2024.";
                return;
            }

            using (SqlConnection con = new SqlConnection(ConnStr))
            {
                con.Open();
                string sql = @"UPDATE AAP_Reference_School 
                               SET Percentage=@Pct, School_SignUp_Date=@Signup, End_Reference_Date=@Expire
                               WHERE Reference_School_ID=@ID";
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Pct", pct);
                    cmd.Parameters.AddWithValue("@Signup", signupDate.HasValue ? (object)signupDate.Value.Date : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Expire", expireDate.HasValue ? (object)expireDate.Value.Date : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ID", rsID);
                    cmd.ExecuteNonQuery();
                }
            }

            AssignedSchoolsGridView.EditIndex = -1;
            int refID = (int)ViewState["CurrentReferenceID"];
            LoadAssignedSchools(refID);
            AssignMsgLabel.CssClass = "text-success font-weight-bold";
            AssignMsgLabel.Text = "Assignment updated successfully.";
        }
    }
}
