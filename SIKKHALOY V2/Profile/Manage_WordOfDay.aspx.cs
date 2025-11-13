using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace EDUCATION.COM.Profile
{
    public partial class Manage_WordOfDay : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
      {
            if (!IsPostBack)
       {
           // Check if user is logged in
                if (Session["SchoolID"] == null)
     {
  Response.Redirect("~/Login.aspx");
         }
      }
   }

protected void AddWordButton_Click(object sender, EventArgs e)
      {
  try
        {
     string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
        
      using (SqlConnection con = new SqlConnection(connectionString))
          {
        string query = @"INSERT INTO WordOfTheDay 
       (EnglishWord, BengaliMeaning, PartOfSpeech, ExampleSentence, Pronunciation, IsActive) 
                 VALUES 
   (@EnglishWord, @BengaliMeaning, @PartOfSpeech, @ExampleSentence, @Pronunciation, 1)";
        
     using (SqlCommand cmd = new SqlCommand(query, con))
    {
            cmd.Parameters.AddWithValue("@EnglishWord", EnglishWordTextBox.Text.Trim());
     cmd.Parameters.AddWithValue("@BengaliMeaning", BengaliMeaningTextBox.Text.Trim());
    cmd.Parameters.AddWithValue("@PartOfSpeech", 
     string.IsNullOrEmpty(PartOfSpeechDropDown.SelectedValue) ? (object)DBNull.Value : PartOfSpeechDropDown.SelectedValue);
      cmd.Parameters.AddWithValue("@ExampleSentence", 
   string.IsNullOrEmpty(ExampleSentenceTextBox.Text) ? (object)DBNull.Value : ExampleSentenceTextBox.Text.Trim());
    cmd.Parameters.AddWithValue("@Pronunciation", 
       string.IsNullOrEmpty(PronunciationTextBox.Text) ? (object)DBNull.Value : PronunciationTextBox.Text.Trim());
      
         con.Open();
     int result = cmd.ExecuteNonQuery();
       
       if (result > 0)
          {
       MessageLabel.Text = "<span class='text-success'><i class='fa fa-check-circle'></i> Word added successfully!</span>";
               MessageLabel.ForeColor = System.Drawing.Color.LightGreen;
       
         // Clear form fields
     ClearForm();
      
    // Refresh GridView
  WordsGridView.DataBind();
        }
   else
    {
            MessageLabel.Text = "<span class='text-danger'><i class='fa fa-exclamation-circle'></i> Failed to add word.</span>";
      MessageLabel.ForeColor = System.Drawing.Color.Red;
  }
         }
         }
            }
            catch (Exception ex)
 {
     MessageLabel.Text = string.Format("<span class='text-danger'><i class='fa fa-exclamation-triangle'></i> Error: {0}</span>", ex.Message);
                MessageLabel.ForeColor = System.Drawing.Color.Red;
   }
        }
        
        private void ClearForm()
        {
            EnglishWordTextBox.Text = string.Empty;
  BengaliMeaningTextBox.Text = string.Empty;
      PartOfSpeechDropDown.SelectedIndex = 0;
        ExampleSentenceTextBox.Text = string.Empty;
PronunciationTextBox.Text = string.Empty;
        }
    }
}
