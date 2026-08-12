using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority
{
    public partial class SignUp_Institution : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadReferrers();
                UpdateExpiryPreview();
            }
        }

        private void LoadReferrers()
        {
            ReferrerDropDownList.Items.Clear();
            ReferrerDropDownList.Items.Add(new ListItem("[ No Referrer ]", "0"));

            string sql = "SELECT ReferenceID, Reference_Name, Reference_Phone FROM AAP_Reference ORDER BY Reference_Name";
            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlDataAdapter da = new SqlDataAdapter(sql, con))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                foreach (DataRow row in dt.Rows)
                {
                    string text = row["Reference_Name"].ToString();
                    if (row["Reference_Phone"] != DBNull.Value && !string.IsNullOrWhiteSpace(row["Reference_Phone"].ToString()))
                        text += " (" + row["Reference_Phone"] + ")";
                    ReferrerDropDownList.Items.Add(new ListItem(text, row["ReferenceID"].ToString()));
                }
            }
        }

        private int GetSelectedDurationYears()
        {
            int years;
            if (int.TryParse(ReferrerDurationRadio.SelectedValue, out years) && (years == 2 || years == 3 || years == 5))
                return years;
            return 2;
        }

        private void UpdateExpiryPreview()
        {
            int years = GetSelectedDurationYears();
            DateTime expiry = DateTime.Today.AddYears(years);
            ReferrerExpiryPreviewLabel.Text = expiry.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)
                + " (" + years + " years from today)";
        }

        protected void InstitutionCW_CreatedUser(object sender, EventArgs e)
        {
            string[] userName = { InstitutionCW.UserName };
            string[] role = { "Admin" };

            Roles.AddUsersToRoles(userName, role);
            ViewState["Password"] = InstitutionCW.Password;
            ViewState["PasswordAnswer"] = InstitutionCW.Answer;
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            var con = new SqlConnection(ConnStr);

            SchoolInfoSQL.InsertParameters["UserName"].DefaultValue = InstitutionCW.UserName;
            SchoolInfoSQL.Insert();

            RegistrationSQL.InsertParameters["UserName"].DefaultValue = InstitutionCW.UserName;
            RegistrationSQL.Insert();

            AdminSQL.Insert();

            var schoolInfoCmd = new SqlCommand("Select IDENT_CURRENT('SchoolInfo')", con);
            var registrationIdCmd = new SqlCommand("Select IDENT_CURRENT('Registration')", con);

            con.Open();
            var schoolId = schoolInfoCmd.ExecuteScalar().ToString();
            var registrationId = registrationIdCmd.ExecuteScalar().ToString();
            con.Close();

            LIT_SQL.InsertParameters["SchoolID"].DefaultValue = schoolId;
            LIT_SQL.InsertParameters["RegistrationID"].DefaultValue = registrationId;
            LIT_SQL.InsertParameters["UserName"].DefaultValue = InstitutionCW.UserName;
            LIT_SQL.InsertParameters["Password"].DefaultValue = ViewState["Password"].ToString();
            LIT_SQL.InsertParameters["PasswordAnswer"].DefaultValue = ViewState["PasswordAnswer"].ToString();
            LIT_SQL.Insert();

            Edu_YearSQL.InsertParameters["SchoolID"].DefaultValue = schoolId;
            Edu_YearSQL.InsertParameters["RegistrationID"].DefaultValue = registrationId;
            Edu_YearSQL.Insert();

            SMS_SQL.InsertParameters["SchoolID"].DefaultValue = schoolId;
            SMS_SQL.Insert();

            AssignReferrerIfSelected(schoolId);

            InstitutionCW.ActiveStepIndex = 2;
        }

        private void AssignReferrerIfSelected(string schoolId)
        {
            int refID;
            if (!int.TryParse(ReferrerDropDownList.SelectedValue, out refID) || refID <= 0)
                return;

            int schoolID;
            if (!int.TryParse(schoolId, out schoolID) || schoolID <= 0)
                return;

            decimal pct = 0;
            decimal.TryParse(ReferrerCommissionTextBox.Text.Trim(), out pct);
            if (pct < 0) pct = 0;

            int years = GetSelectedDurationYears();
            DateTime signupDate = DateTime.Today;
            DateTime expiryDate = signupDate.AddYears(years);

            string sql = @"
                IF NOT EXISTS (SELECT 1 FROM AAP_Reference_School WHERE SchoolID = @SchoolID AND ReferenceID = @RefID)
                BEGIN
                    INSERT INTO AAP_Reference_School(SchoolID, ReferenceID, Percentage, School_SignUp_Date, End_Reference_Date)
                    VALUES (@SchoolID, @RefID, @Pct, @Signup, @Expire)
                END";

            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@SchoolID", schoolID);
                cmd.Parameters.AddWithValue("@RefID", refID);
                cmd.Parameters.AddWithValue("@Pct", pct);
                cmd.Parameters.AddWithValue("@Signup", signupDate);
                cmd.Parameters.AddWithValue("@Expire", expiryDate);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
