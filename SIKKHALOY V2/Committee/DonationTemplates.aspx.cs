using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Services;
using System.Web.UI;

namespace EDUCATION.COM.Committee
{
    public partial class DonationTemplates : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CreateTemplateTableIfNotExists();
            }
        }

        protected void AddTemplateButton_Click(object sender, EventArgs e)
        {
            try
            {
                TemplatesSQL.Insert();
                TemplatesGridView.DataBind();
                
                MemberTypeDropDownList.SelectedIndex = 0;
                CategoryDropDownList.SelectedIndex = 0;
                AmountTextBox.Text = "";

                ScriptManager.RegisterStartupScript(this, this.GetType(), "success",
                    "$.notify('Template added successfully!', 'success');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "error",
                    $"$.notify('Error: {ex.Message}', 'error');", true);
            }
        }

        private void CreateTemplateTableIfNotExists()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            string createTableQuery = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CommitteeDonationTemplate]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[CommitteeDonationTemplate](
                        [DonationTemplateId] [int] IDENTITY(1,1) NOT NULL,
                        [SchoolID] [int] NOT NULL,
                        [RegistrationID] [int] NOT NULL,
                        [CommitteeMemberTypeId] [int] NOT NULL,
                        [CommitteeDonationCategoryId] [int] NOT NULL,
                        [Amount] [decimal](18, 2) NOT NULL,
                        [CreatedDate] [datetime] NOT NULL DEFAULT (getdate()),
                        CONSTRAINT [PK_CommitteeDonationTemplate] PRIMARY KEY CLUSTERED ([DonationTemplateId] ASC)
                    )
                END";

            SqlCommand cmd = new SqlCommand(createTableQuery, con);
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
        }

        [WebMethod]
        public static void CreateTemplateTable()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            string createTableQuery = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CommitteeDonationTemplate]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[CommitteeDonationTemplate](
                        [DonationTemplateId] [int] IDENTITY(1,1) NOT NULL,
                        [SchoolID] [int] NOT NULL,
                        [RegistrationID] [int] NOT NULL,
                        [CommitteeMemberTypeId] [int] NOT NULL,
                        [CommitteeDonationCategoryId] [int] NOT NULL,
                        [Amount] [decimal](18, 2) NOT NULL,
                        [CreatedDate] [datetime] NOT NULL DEFAULT (getdate()),
                        CONSTRAINT [PK_CommitteeDonationTemplate] PRIMARY KEY CLUSTERED ([DonationTemplateId] ASC)
                    )
                END";

            SqlCommand cmd = new SqlCommand(createTableQuery, con);
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
        }
    }
}
