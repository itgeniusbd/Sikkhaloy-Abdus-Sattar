using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using Education;

namespace EDUCATION.COM.Admission.New_Student_Admission
{
    public partial class Admission_New_Student : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;

            if (!IsPostBack)
            {
                try
                {
                    if (Session["Edu_Year"] == null && EducationYearDropDownList.Items.Count > 0)
                    {
                        Session["Edu_Year"] = EducationYearDropDownList.SelectedValue;
                    }

                    // Dropdowns will be hidden via CSS initially
                    // GroupDropDownList.Visible = false;
                    // SectionDropDownList.Visible = false;
                    // ShiftDropDownList.Visible = false;

                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand("SELECT TOP 1 ID FROM Student WHERE SchoolID = @SchoolID ORDER BY StudentID DESC", con);
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                        var lastID = cmd.ExecuteScalar();
                        if (lastID != null)
                        {
                            LastIDLabel.Text = "Last Entry ID: " + lastID.ToString();
                        }
                    }
                }
                catch { }
            }
        }

        protected void view()
        {
            // CSS will handle visibility, not server-side Visible property
            /*
            DataView GroupDV = new DataView();
            GroupDV = (DataView)GroupSQL.Select(DataSourceSelectArguments.Empty);
            if (GroupDV.Count < 1)
            {
                GroupDropDownList.Visible = false;
            }
            else
            {
                GroupDropDownList.Visible = true;
            }

            DataView SectionDV = new DataView();
            SectionDV = (DataView)SectionSQL.Select(DataSourceSelectArguments.Empty);
            if (SectionDV.Count < 1)
            {
                SectionDropDownList.Visible = false;
            }
            else
            {
                SectionDropDownList.Visible = true;
            }

            DataView ShiftDV = new DataView();
            ShiftDV = (DataView)ShiftSQL.Select(DataSourceSelectArguments.Empty);
            if (ShiftDV.Count < 1)
            {
                ShiftDropDownList.Visible = false;
            }
            else
            {
                ShiftDropDownList.Visible = true;
            }
            */
        }

        private int GetLastIdentity(string tableName)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand($"SELECT IDENT_CURRENT('{tableName}')", con);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "ValidationError",
                    "alert('Please fill in all required fields correctly.');", true);
                return;
            }

            try
            {
                if (Session["SchoolID"] == null)
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "SessionError",
                        "alert('Session expired. Please login again.');", true);
                    return;
                }

                if (Session["Edu_Year"] == null)
                {
                    Session["Edu_Year"] = EducationYearDropDownList.SelectedValue;
                }

                if (ClassDropDownList.SelectedValue == "0")
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "ClassError",
                        "alert('Please select a class.');", true);
                    return;
                }

                // Check for duplicate ID before proceeding
                if (IsIDDuplicate(IDTextBox.Text.Trim()))
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "DuplicateIDError",
     $"alert('⚠️ Student ID \"{IDTextBox.Text.Trim()}\" already exists!\\n\\nPlease use a different ID.');", true);
                    IDTextBox.Focus();
    return;
}

  StudentImageSQL.Insert();
    int ImageID = GetLastIdentity("Student_Image");

         StudentInfoSQL.InsertParameters["StudentImageID"].DefaultValue = ImageID.ToString();
    StudentInfoSQL.Insert();
                int StudentID = GetLastIdentity("Student");

       if (GroupDropDownList.SelectedValue == "%")
 {
         Session["GroupID"] = "0";
            }
          else
          {
           Session["GroupID"] = GroupDropDownList.SelectedValue;
         }

 if (SectionDropDownList.SelectedValue == "%")
      {
           Session["SectionID"] = "0";
       }
     else
      {
  Session["SectionID"] = SectionDropDownList.SelectedValue;
       }

    if (ShiftDropDownList.SelectedValue == "%")
      {
          Session["ShiftID"] = "0";
      }
     else
          {
      Session["ShiftID"] = ShiftDropDownList.SelectedValue;
       }

       StudentClassSQL.InsertParameters["StudentID"].DefaultValue = StudentID.ToString();
       StudentClassSQL.Insert();
      int StudentClassID = GetLastIdentity("StudentsClass");
           Session["StudentClassID"] = StudentClassID;

        foreach (GridViewRow row in GroupGridView.Rows)
       {
    CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
   if (SubjectCheckBox != null && SubjectCheckBox.Checked)
          {
   RadioButtonList SubjectTypeRadioButtonList = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");
           if (SubjectTypeRadioButtonList != null && !string.IsNullOrEmpty(SubjectTypeRadioButtonList.SelectedValue))
{
      StudentRecordSQL.InsertParameters["StudentID"].DefaultValue = StudentID.ToString();
 StudentRecordSQL.InsertParameters["SubjectID"].DefaultValue = GroupGridView.DataKeys[row.RowIndex].Values["SubjectID"].ToString();
     StudentRecordSQL.InsertParameters["SubjectType"].DefaultValue = SubjectTypeRadioButtonList.SelectedValue;
     StudentRecordSQL.Insert();
             }
      }
   }

  // Send admission SMS if checkbox is checked
       if (SendAdmissionSMSCheckBox.Checked && !string.IsNullOrEmpty(SMSPhoneNoTextBox.Text))
       {
        SendAdmissionSMS(StudentNameTextBox.Text, IDTextBox.Text, ClassDropDownList.SelectedItem.Text,
               RollNumberTextBox.Text, SMSPhoneNoTextBox.Text, StudentID.ToString(), false);
       }

       if (PrintCheckBox.Checked)
   {
        Response.Redirect($"Admission_Form.aspx?Student={StudentID}&StudentClass={StudentClassID}", false);
   Context.ApplicationInstance.CompleteRequest();
        }
else if (BanglaPrintCheckBox.Checked)
    {
       Response.Redirect($"Form_Bangla.aspx?Student={StudentID}&StudentClass={StudentClassID}", false);
     Context.ApplicationInstance.CompleteRequest();
   }
 else
    {
        ClientScript.RegisterStartupScript(this.GetType(), "Success",
  "alert('Admission completed successfully! ID: " + IDTextBox.Text + "'); window.location='Admission_New_Student.aspx';", true);
      }
   }
          catch (SqlException sqlEx)
            {
    // Check if it's a unique constraint violation
    if (sqlEx.Number == 2627 || sqlEx.Number == 2601) // Unique constraint violation
       {
         ClientScript.RegisterStartupScript(this.GetType(), "DuplicateError",
            $"alert('⚠️ Student ID \"{IDTextBox.Text.Trim()}\" already exists!\\n\\nThis ID is already registered in the system.');", true);
  }
           else
             {
          ClientScript.RegisterStartupScript(this.GetType(), "SQLError",
 $"alert('Database Error: {sqlEx.Message.Replace("'", "\\'")}');", true);
    }
            }
        catch (Exception ex)
         {
                ClientScript.RegisterStartupScript(this.GetType(), "Error",
           $"alert('Error: {ex.Message.Replace("'", "\\'")}');", true);
        }
        }

    protected void GoPayorderButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
        {
                ClientScript.RegisterStartupScript(this.GetType(), "ValidationError",
           "alert('Please fill in all required fields correctly.');", true);
       return;
  }

try
    {
        if (Session["SchoolID"] == null)
         {
     ClientScript.RegisterStartupScript(this.GetType(), "SessionError",
    "alert('Session expired. Please login again.');", true);
       return;
            }

     if (Session["Edu_Year"] == null)
  {
           Session["Edu_Year"] = EducationYearDropDownList.SelectedValue;
      }

       if (ClassDropDownList.SelectedValue == "0")
       {
       ClientScript.RegisterStartupScript(this.GetType(), "ClassError",
         "alert('Please select a class.');", true);
  return;
      }

         // Check for duplicate ID before proceeding
     if (IsIDDuplicate(IDTextBox.Text.Trim()))
              {
        ClientScript.RegisterStartupScript(this.GetType(), "DuplicateIDError",
              $"alert('⚠️ Student ID \"{IDTextBox.Text.Trim()}\" already exists!\\n\\nPlease use a different ID.');", true);
      IDTextBox.Focus();
       return;
      }

     StudentImageSQL.Insert();
   int ImageID = GetLastIdentity("Student_Image");

      StudentInfoSQL.InsertParameters["StudentImageID"].DefaultValue = ImageID.ToString();
         StudentInfoSQL.Insert();
             int StudentID = GetLastIdentity("Student");

        if (GroupDropDownList.SelectedValue == "%")
        {
          Session["GroupID"] = "0";
      }
   else
      {
            Session["GroupID"] = GroupDropDownList.SelectedValue;
             }

 if (SectionDropDownList.SelectedValue == "%")
      {
            Session["SectionID"] = "0";
     }
   else
     {
            Session["SectionID"] = SectionDropDownList.SelectedValue;
            }

       if (ShiftDropDownList.SelectedValue == "%")
         {
          Session["ShiftID"] = "0";
       }
        else
      {
    Session["ShiftID"] = ShiftDropDownList.SelectedValue;
       }

     StudentClassSQL.InsertParameters["StudentID"].DefaultValue = StudentID.ToString();
    StudentClassSQL.Insert();
    int StudentClassID = GetLastIdentity("StudentsClass");
        Session["StudentClassID"] = StudentClassID;

          foreach (GridViewRow row in GroupGridView.Rows)
     {
  CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
if (SubjectCheckBox != null && SubjectCheckBox.Checked)
       {
           RadioButtonList SubjectTypeRadioButtonList = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");
       if (SubjectTypeRadioButtonList != null && !string.IsNullOrEmpty(SubjectTypeRadioButtonList.SelectedValue))
    {
                StudentRecordSQL.InsertParameters["StudentID"].DefaultValue = StudentID.ToString();
     StudentRecordSQL.InsertParameters["SubjectID"].DefaultValue = GroupGridView.DataKeys[row.RowIndex].Values["SubjectID"].ToString();
    StudentRecordSQL.InsertParameters["SubjectType"].DefaultValue = SubjectTypeRadioButtonList.SelectedValue;
           StudentRecordSQL.Insert();
            }
   }
        }

           // Send admission SMS if checkbox is checked
    if (SendAdmissionSMSCheckBox.Checked && !string.IsNullOrEmpty(SMSPhoneNoTextBox.Text))
 {
      SendAdmissionSMS(StudentNameTextBox.Text, IDTextBox.Text, ClassDropDownList.SelectedItem.Text,
   RollNumberTextBox.Text, SMSPhoneNoTextBox.Text, StudentID.ToString(), false);
     }

           Response.Cookies["Class"].Value = ClassDropDownList.SelectedItem.Text;
      Response.Cookies["RollNo"].Value = RollNumberTextBox.Text;
   Response.Cookies["Admission_Year"].Value = Session["Edu_Year"].ToString();
   Response.Cookies["Admission_Year"].Expires = DateTime.Now.AddDays(1);

   Response.Redirect($"Payments.aspx?Student={StudentID}&Class={ClassDropDownList.SelectedValue}&StudentClass={StudentClassID}", false);
 Context.ApplicationInstance.CompleteRequest();
         }
            catch (SqlException sqlEx)
            {
                // Check if it's a unique constraint violation
           if (sqlEx.Number == 2627 || sqlEx.Number == 2601) // Unique constraint violation
{
      ClientScript.RegisterStartupScript(this.GetType(), "DuplicateError",
      $"alert('⚠️ Student ID \"{IDTextBox.Text.Trim()}\" already exists!\\n\\nThis ID is already registered in the system.');", true);
   }
    else
    {
              ClientScript.RegisterStartupScript(this.GetType(), "SQLError",
    $"alert('Database Error: {sqlEx.Message.Replace("'", "\\'")}');", true);
             }
            }
 catch (Exception ex)
{
    ClientScript.RegisterStartupScript(this.GetType(), "Error",
  $"alert('Error: {ex.Message.Replace("'", "\\'")}');", true);
   }
        }

        private void SendAdmissionSMS(string studentName, string studentID, string className, string rollNo, string phone, string studentIDforDB, bool isBangla)
        {
            try
            {
                string Text;

                // Try to get admission template
                string admissionTemplate = GetSMSTemplate("Admission", isBangla ? "AdmissionWelcome" : "AdmissionConfirm");

                if (!string.IsNullOrEmpty(admissionTemplate))
                {
                    // Use template with placeholders
                    Text = admissionTemplate
                    .Replace("{StudentName}", studentName)
                          .Replace("{ID}", studentID)
                   .Replace("{Class}", className)
                     .Replace("{RollNo}", rollNo)
                      .Replace("{AdmissionDate}", DateTime.Now.ToString("dd MMM yyyy"))
                 .Replace("{SchoolName}", Session["School_Name"].ToString());
                }
                else
                {
                    // Default message (fallback)
                    if (isBangla)
                    {
                        // Bangla SMS format
                        Text = $"অভিনন্দন! {studentName} আপনি {className} শ্রেণীতে অনলাইনে ভর্তি হয়েছেন। আইডি: {studentID}, রোল: {rollNo}। ভবিষ্যতের জন্য এই তথ্য সংরক্ষণ করুন। ধন্যবাদ - {Session["School_Name"]}";
                    }
                    else
                    {
                        // English SMS format
                        Text = $"Congratulation!! {studentName} You have been Online admitted into class: {className}. ID: {studentID}. Roll No: {rollNo} Please save this information for future. Regards: {Session["School_Name"]}";
                    }
                }

                SMS_Class SMS = new SMS_Class(Session["SchoolID"].ToString());
                int TotalSMS = SMS.SMS_Conut(Text);
                int SMSBalance = SMS.SMSBalance;

                if (SMSBalance >= TotalSMS)
                {
                    if (SMS.SMS_GetBalance() >= TotalSMS)
                    {
                        Get_Validation IsValid = SMS.SMS_Validation(phone, Text);

                        if (IsValid.Validation)
                        {
                            Guid SMS_Send_ID = SMS.SMS_Send(phone, Text, "Admission");

                            if (SMS_Send_ID != Guid.Empty)
                            {
                                using (SqlConnection con = new SqlConnection(connectionString))
                                {
                                    con.Open();
                                    SqlCommand cmd = new SqlCommand(@"
          INSERT INTO SMS_OtherInfo(SMS_Send_ID, SchoolID, StudentID, TeacherID, EducationYearID) 
      VALUES (@SMS_Send_ID, @SchoolID, @StudentID, @TeacherID, @EducationYearID)", con);

                                    cmd.Parameters.AddWithValue("@SMS_Send_ID", SMS_Send_ID);
                                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                                    cmd.Parameters.AddWithValue("@StudentID", studentIDforDB);
                                    cmd.Parameters.AddWithValue("@TeacherID", "");
                                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"]);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SMS Error: " + ex.Message);
            }
        }

        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Group"] = "%";
            Session["Shift"] = "%";
            Session["Section"] = "%";

            GroupDropDownList.DataBind();
            ShiftDropDownList.DataBind();
            SectionDropDownList.DataBind();
            // view(); // Removed - CSS will handle visibility
            
            // Register script to show/hide dropdowns after postback with delay for data binding
            string script = "setTimeout(function() { console.log('Calling toggleAcademicDropdowns from server script'); toggleAcademicDropdowns(); }, 500);";
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowDropdowns", script, true);
        }

        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            SectionDropDownList.DataBind();
            ShiftDropDownList.DataBind();
            // view(); // Removed - CSS will handle visibility
            
            string script = "setTimeout(function() { console.log('Calling toggleAcademicDropdowns from server script'); toggleAcademicDropdowns(); }, 500);";
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowDropdowns", script, true);
        }

        protected void SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShiftDropDownList.DataBind();
            // view(); // Removed - CSS will handle visibility
            
            string script = "setTimeout(function() { console.log('Calling toggleAcademicDropdowns from server script'); toggleAcademicDropdowns(); }, 500);";
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowDropdowns", script, true);
        }

        protected void ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            GroupGridView.DataBind();
        }

        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ ALL ]", "%"));
            if (IsPostBack)
                GroupDropDownList.Items.FindByValue(Session["Group"].ToString()).Selected = true;
        }

        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ ALL ]", "%"));
            if (IsPostBack)
                SectionDropDownList.Items.FindByValue(Session["Section"].ToString()).Selected = true;
        }

        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            ShiftDropDownList.Items.Insert(0, new ListItem("[ ALL ]", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }

        [WebMethod]
        public static string GetAllID(string ids)
        {
            try
            {
                List<string> IDList = new List<string>();
                string connString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                
                // Get SchoolID from session - need to access through HttpContext
                string schoolID = HttpContext.Current.Session["SchoolID"]?.ToString();
 
                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT TOP 10 ID FROM Student WHERE ID LIKE @ID + '%' AND SchoolID = @SchoolID ORDER BY ID", con);
                    cmd.Parameters.AddWithValue("@ID", ids);
                    cmd.Parameters.AddWithValue("@SchoolID", schoolID ?? "");
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        IDList.Add(reader["ID"].ToString());
                    }
                }
                return new JavaScriptSerializer().Serialize(IDList);
            }
            catch
            {
                return "[]";
            }
        }

        // New WebMethod to check if ID already exists
        [WebMethod]
        public static string CheckIDExists(string studentID)
        {
            try
   {
         string connString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
         string schoolID = HttpContext.Current.Session["SchoolID"]?.ToString();

     using (SqlConnection con = new SqlConnection(connString))
         {
        con.Open();
              SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Student WHERE ID = @ID AND SchoolID = @SchoolID", con);
           cmd.Parameters.AddWithValue("@ID", studentID.Trim());
     cmd.Parameters.AddWithValue("@SchoolID", schoolID ?? "");
          
   int count = Convert.ToInt32(cmd.ExecuteScalar());
          
   return new JavaScriptSerializer().Serialize(new { exists = count > 0, count = count });
    }
            }
            catch (Exception ex)
          {
   return new JavaScriptSerializer().Serialize(new { exists = false, error = ex.Message });
       }
        }

      // Server-side method to check duplicate ID before insertion
private bool IsIDDuplicate(string studentID)
        {
    try
    {
        using (SqlConnection con = new SqlConnection(connectionString))
    {
            con.Open();
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Student WHERE ID = @ID AND SchoolID = @SchoolID", con);
 cmd.Parameters.AddWithValue("@ID", studentID.Trim());
   cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
            
            int count = Convert.ToInt32(cmd.ExecuteScalar());
          return count > 0;
        }
    }
    catch
    {
        return false; // If error, assume not duplicate and let database constraint handle it
    }
}

/// <summary>
/// Get SMS Template from database by category and type
/// </summary>
private string GetSMSTemplate(string category, string templateType)
{
  try
    {
        using (SqlConnection tempCon = new SqlConnection(connectionString))
  {
  tempCon.Open();

          // First check if SMS_Template table exists
            SqlCommand checkTableCmd = new SqlCommand(@"
  IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
     WHERE TABLE_NAME = 'SMS_Template')
                    SELECT 1
     ELSE
          SELECT 0", tempCon);

       int tableExists = (int)checkTableCmd.ExecuteScalar();

  if (tableExists == 0)
            {
  return string.Empty;
 }

    // Check if TemplateCategory column exists
            SqlCommand checkColumnCmd = new SqlCommand(@"
   IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
   WHERE TABLE_NAME = 'SMS_Template' AND COLUMN_NAME = 'TemplateCategory')
           SELECT 1
 ELSE
  SELECT 0", tempCon);

   int columnExists = (int)checkColumnCmd.ExecuteScalar();

  string selectQuery;
            if (columnExists == 1)
 {
         selectQuery = @"SELECT TOP 1 MessageTemplate 
        FROM SMS_Template 
         WHERE SchoolID = @SchoolID 
         AND TemplateCategory = @TemplateCategory
    AND TemplateType = @TemplateType 
    AND IsActive = 1 
         ORDER BY CreatedDate DESC";
    }
       else
            {
    selectQuery = @"SELECT TOP 1 MessageTemplate 
    FROM SMS_Template 
        WHERE SchoolID = @SchoolID 
   AND TemplateType = @TemplateType 
             AND IsActive = 1 
       ORDER BY CreatedDate DESC";
       }

            SqlCommand cmd = new SqlCommand(selectQuery, tempCon);
         cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
            if (columnExists == 1)
     {
      cmd.Parameters.AddWithValue("@TemplateCategory", category);
   }
     cmd.Parameters.AddWithValue("@TemplateType", templateType);

  object result = cmd.ExecuteScalar();
  return result != null ? result.ToString() : string.Empty;
        }
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine("Template Error: " + ex.Message);
        return string.Empty;
    }
}
    }
}
