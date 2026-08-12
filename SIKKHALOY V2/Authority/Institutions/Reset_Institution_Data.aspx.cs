using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority.Institutions
{
    public partial class Reset_Institution_Data : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSchools();
            }
        }

        private void LoadSchools()
        {
            SchoolDropDown.Items.Clear();
            SchoolDropDown.Items.Add(new ListItem("[ SELECT INSTITUTION ]", "0"));

            string sql = @"SELECT SchoolID, CAST(SchoolID AS NVARCHAR(20)) + N' - ' + SchoolName AS DisplayText
                           FROM SchoolInfo
                           ORDER BY SchoolID DESC";
            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlDataAdapter da = new SqlDataAdapter(sql, con))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                foreach (DataRow row in dt.Rows)
                {
                    SchoolDropDown.Items.Add(new ListItem(row["DisplayText"].ToString(), row["SchoolID"].ToString()));
                }
            }
        }

        protected void SchoolDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            MsgLabel.Text = "";
            FullConfirmTextBox.Text = "";
            SessionConfirmTextBox.Text = "";
            PurgeConfirmIdTextBox.Text = "";
            PurgeConfirmWordTextBox.Text = "";

            int schoolID;
            if (!int.TryParse(SchoolDropDown.SelectedValue, out schoolID) || schoolID <= 0)
            {
                ActionPanel.Visible = false;
                SchoolInfoLabel.Text = "";
                return;
            }

            ActionPanel.Visible = true;
            SchoolInfoLabel.Text = "Selected SchoolID: " + schoolID;
            LoadSessions(schoolID);
        }

        private void LoadSessions(int schoolID)
        {
            SessionDropDown.Items.Clear();
            SessionDropDown.Items.Add(new ListItem("[ SELECT SESSION ]", "0"));

            string sql = @"SELECT EducationYearID,
                                  CAST(EducationYear AS NVARCHAR(20)) + N' (ID: ' + CAST(EducationYearID AS NVARCHAR(20)) + N')' AS DisplayText
                           FROM Education_Year
                           WHERE SchoolID = @SchoolID
                           ORDER BY EducationYearID DESC";
            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@SchoolID", schoolID);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SessionDropDown.Items.Add(new ListItem(
                            reader["DisplayText"].ToString(),
                            reader["EducationYearID"].ToString()));
                    }
                }
            }
        }
    }
}
