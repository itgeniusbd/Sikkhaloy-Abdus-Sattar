using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace EDUCATION.COM.Admission.New_Student_Admission
{
    public partial class Admission_Form : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Check if Student and Student_Class query parameters exist
                if (!string.IsNullOrEmpty(Request.QueryString["Student"]) && 
                    !string.IsNullOrEmpty(Request.QueryString["Student_Class"]))
                {
                    // Student data loaded via query string - show footer
                    ShowFooterSection();
                }
            }
        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            // Find controls in the content placeholder
            TextBox studentIDBox = FindControlRecursive(this, "StudentIDTextBox") as TextBox;
            Label msgLabel = FindControlRecursive(this, "MessageLabel") as Label;

            if (studentIDBox == null)
            {
                Response.Write("<script>alert('StudentIDTextBox not found!');</script>");
                return;
            }

            if (msgLabel == null)
            {
                Response.Write("<script>alert('MessageLabel not found!');</script>");
                return;
            }

            string studentID = studentIDBox.Text.Trim();

            if (string.IsNullOrEmpty(studentID))
            {
                msgLabel.Text = "Please enter Student ID";
                return;
            }

            if (Session["SchoolID"] == null)
            {
                msgLabel.Text = "Session expired! Please login again.";
                return;
            }

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Get student and class info
                    string query = @"SELECT TOP 1 
                                        s.StudentID, 
                                        sc.StudentClassID,
                                        s.ID,
                                        s.StudentsName
                                    FROM Student s
                                    INNER JOIN StudentsClass sc ON s.StudentID = sc.StudentID 
                                    WHERE s.ID = @ID 
                                    AND s.SchoolID = @SchoolID 
                                    ORDER BY sc.StudentClassID DESC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ID", studentID);
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int studentIDValue = Convert.ToInt32(reader["StudentID"]);
                                int studentClassID = Convert.ToInt32(reader["StudentClassID"]);

                                Response.Redirect($"Admission_Form.aspx?Student={studentIDValue}&Student_Class={studentClassID}", false);
                            }
                            else
                            {
                                msgLabel.Text = $"No student found with ID '{studentID}'!";
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                msgLabel.Text = "Database Error: " + sqlEx.Message;
            }
            catch (Exception ex)
            {
                msgLabel.Text = "Error: " + ex.Message;
            }
        }

        // Method to show footer section
        private void ShowFooterSection()
        {
            // Use JavaScript to show the footer
            string script = @"
                <script type='text/javascript'>
                    window.onload = function() {
                        var footer = document.getElementById('signatureSection');
                        if (footer) {
                            footer.style.display = 'flex';
                        }
                    };
                </script>
            ";
            ClientScript.RegisterStartupScript(this.GetType(), "ShowFooter", script, false);
        }

        // Helper method to find controls recursively
        private Control FindControlRecursive(Control root, string id)
        {
            if (root.ID == id)
                return root;

            foreach (Control control in root.Controls)
            {
                Control foundControl = FindControlRecursive(control, id);
                if (foundControl != null)
                    return foundControl;
            }

            return null;
        }
    }
}