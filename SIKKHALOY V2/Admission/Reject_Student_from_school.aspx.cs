using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ADMISSION_REGISTER
{
    public partial class Reject_Student_from_school : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                RejectButton.Visible = false;
                ActiveButton.Visible = false;
                PayorderRadioButtonList.Visible = false;

                // Add client-side onclick to prevent double-click
                ConfirmActiveButton.Attributes.Add("onclick", 
  "if(typeof Page_ClientValidate === 'function' && !Page_ClientValidate('ActiveStudent')) return false; " +
   "this.disabled=true; this.value='Processing...'; " + 
      ClientScript.GetPostBackEventReference(ConfirmActiveButton, "") + "; return false;");

                // Check if page reloaded after activation with student ID
                if (!string.IsNullOrEmpty(Request.QueryString["id"]) && Request.QueryString["refresh"] == "1")
                {
                    IDTextBox.Text = Request.QueryString["id"];
                    // Trigger find button click programmatically
                    StudentInfoFormView.DataBind();
                }
            }
            else
            {
                // Check if activation was successful and trigger redirect
                if (ViewState["ActivationSuccess"] != null && ViewState["ActivationSuccess"].ToString() == "true")
                {
                    string studentID = ViewState["StudentID"]?.ToString() ?? "";
                    ViewState["ActivationSuccess"] = null;
                    ViewState["StudentID"] = null;

                    // Redirect to reload page with student ID
                    Response.Redirect(Request.Url.AbsolutePath + "?id=" + studentID + "&refresh=1", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
        }

        protected void StudentInfoFormView_DataBound(object sender, EventArgs e)
        {
            // Check if FormView has data and update button visibility
            if (StudentInfoFormView.DataItem != null)
            {
                DataRowView row = (DataRowView)StudentInfoFormView.DataItem;
                string status = row["Status"]?.ToString();

                // Store student data in ViewState AND hidden fields
   ViewState["CurrentStudentID"] = StudentInfoFormView.DataKey["StudentID"];
                ViewState["CurrentStudentClassID"] = StudentInfoFormView.DataKey["StudentClassID"];
                ViewState["CurrentStudentIDText"] = row["ID"];
                ViewState["CurrentDeactivateTime"] = StudentInfoFormView.DataKey["DeactivateTime"];

                hdnStudentID.Value = StudentInfoFormView.DataKey["StudentID"]?.ToString() ?? "";
                hdnStudentClassID.Value = StudentInfoFormView.DataKey["StudentClassID"]?.ToString() ?? "";
  hdnStudentIDText.Value = row["ID"]?.ToString() ?? "";
                hdnDeactivateTime.Value = StudentInfoFormView.DataKey["DeactivateTime"]?.ToString() ?? "";

                System.Diagnostics.Debug.WriteLine($"DataBound - Student: {hdnStudentIDText.Value}, Status: {status}");

                if (status == "Rejected")
                {
                    RejectButton.Visible = false;
                    PayorderRadioButtonList.Visible = false;
ActiveButton.Visible = true;  // Show Active button for rejected students
     ActiveStudentPanel.Visible = false; // Don't show panel automatically
                }
                else if (status == "Active")
  {
         RejectButton.Visible = true;
    PayorderRadioButtonList.Visible = true;
        ActiveButton.Visible = false;  // Hide Active button for active students
            ActiveStudentPanel.Visible = false;
        }
 else
        {
            RejectButton.Visible = false;
PayorderRadioButtonList.Visible = false;
ActiveButton.Visible = false;
     ActiveStudentPanel.Visible = false;
        }
        
        // Force UpdatePanel refresh to show/hide buttons
        UpdatePanelStudentInfo.Update();
    }
  else
    {
        RejectButton.Visible = false;
        PayorderRadioButtonList.Visible = false;
        ActiveButton.Visible = false;
     ActiveStudentPanel.Visible = false;
      
        // Clear all
     ViewState["CurrentStudentID"] = null;
        ViewState["CurrentStudentClassID"] = null;
        ViewState["CurrentStudentIDText"] = null;
   ViewState["CurrentDeactivateTime"] = null;
    
        hdnStudentID.Value = "";
        hdnStudentClassID.Value = "";
 hdnStudentIDText.Value = "";
        hdnDeactivateTime.Value = "";
        
     // Force UpdatePanel refresh
      UpdatePanelStudentInfo.Update();
    }
        }

        protected void ActiveSectionSQL_Selecting(object sender, SqlDataSourceSelectingEventArgs e)
        {
            // Ensure ClassID parameter is set correctly
            if (ActiveClassDropDown.SelectedValue == "0")
            {
                e.Cancel = true;
            }
        }

        protected void ActiveGroupSQL_Selecting(object sender, SqlDataSourceSelectingEventArgs e)
        {
            // Ensure ClassID parameter is set correctly
            if (ActiveClassDropDown.SelectedValue == "0")
            {
                e.Cancel = true;
            }
        }

        protected void StatusSQL_Selected(object sender, SqlDataSourceStatusEventArgs e)
        {
            // Set count of students in the label
            if (e.AffectedRows > 0)
            {
                CountStudentLabel.Text = $"Total {e.AffectedRows} student(s) found with TC.";
            }
            else
            {
                CountStudentLabel.Text = "No students found with TC.";
            }
        }

        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Refresh the GridView when class selection changes
            StatusGridView.DataBind();
        }

        protected void FindButton_Click(object sender, EventArgs e)
        {
            // Store the searched ID in ViewState before binding
            ViewState["SearchedStudentID"] = IDTextBox.Text;
    
            // Force rebind of FormView to trigger DataBound event
            StudentInfoFormView.DataBind();
        }

        [WebMethod]
        public static string GetAllID(string ids)
        {
            try
            {
                string schoolID = HttpContext.Current.Session["SchoolID"]?.ToString();
                if (string.IsNullOrEmpty(schoolID))
                {
                    return "[]";
                }

                List<string> studentIDs = new List<string>();

                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString()))
                {
                    string query = @"SELECT TOP 10 ID FROM Student 
      WHERE SchoolID = @SchoolID 
       AND ID LIKE @SearchText + '%'
           ORDER BY ID";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@SchoolID", schoolID);
                    cmd.Parameters.AddWithValue("@SearchText", ids ?? "");

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        studentIDs.Add(reader["ID"].ToString());
                    }
                    reader.Close();
                }

                return Newtonsoft.Json.JsonConvert.SerializeObject(studentIDs);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetAllID Error: " + ex.Message);
                return "[]";
            }
        }

        [WebMethod]
        public static string GetSections(string schoolID, string classID)
        {
            try
            {
                List<object> sections = new List<object>();

                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString()))
                {
                    string query = @"SELECT SectionID, Section 
          FROM CreateSection 
          WHERE SchoolID = @SchoolID AND ClassID = @ClassID 
              ORDER BY Section";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@SchoolID", schoolID);
                    cmd.Parameters.AddWithValue("@ClassID", classID);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        sections.Add(new
                        {
                            SectionID = reader["SectionID"].ToString(),
                            Section = reader["Section"].ToString()
                        });
                    }
                    reader.Close();
                }

                return Newtonsoft.Json.JsonConvert.SerializeObject(sections);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetSections Error: " + ex.Message);
                return "[]";
            }
        }

        [WebMethod]
        public static string GetGroups(string schoolID, string classID)
        {
            try
            {
                List<object> groups = new List<object>();

                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString()))
                {
                    string query = @"SELECT SubjectGroupID, SubjectGroup 
       FROM CreateSubjectGroup 
   WHERE SchoolID = @SchoolID AND ClassID = @ClassID 
     ORDER BY SubjectGroup";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@SchoolID", schoolID);
                    cmd.Parameters.AddWithValue("@ClassID", classID);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        groups.Add(new
                        {
                            SubjectGroupID = reader["SubjectGroupID"].ToString(),
                            SubjectGroup = reader["SubjectGroup"].ToString()
                        });
                    }
                    reader.Close();
                }

                return Newtonsoft.Json.JsonConvert.SerializeObject(groups);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetGroups Error: " + ex.Message);
                return "[]";
            }
        }

        protected void RejectButton_Click(object sender, EventArgs e)
        {
            Reject_StudentInfoSQL.UpdateParameters["StudentID"].DefaultValue = StudentInfoFormView.DataKey["StudentID"].ToString();
            Reject_StudentInfoSQL.Update();

  StatusGridView.DataBind();
            StudentInfoFormView.DataBind();

     //Log
            ActDeActLogSQL.InsertParameters["StudentClassID"].DefaultValue = StudentInfoFormView.DataKey["StudentClassID"].ToString();
    ActDeActLogSQL.InsertParameters["StudentID"].DefaultValue = StudentInfoFormView.DataKey["StudentID"].ToString();
       ActDeActLogSQL.InsertParameters["Status"].DefaultValue = "Active";
       ActDeActLogSQL.InsertParameters["time"].DefaultValue = StudentInfoFormView.DataKey["ActiveTime"].ToString();
        ActDeActLogSQL.Insert();

     SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
   Membership.DeleteUser(Session["SchoolID"].ToString() + IDTextBox.Text.Trim(), true);

            SqlCommand UpdateCmd = new SqlCommand("DELETE FROM Registration WHERE (UserName = @UserName) AND (SchoolID = @SchoolID)", con);//Delete user from Registration table
            UpdateCmd.Parameters.AddWithValue("@UserName", Session["SchoolID"].ToString() + IDTextBox.Text);
 UpdateCmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());

   con.Open();
          UpdateCmd.ExecuteNonQuery();
       con.Close();

    if (PayorderRadioButtonList.SelectedIndex == 0)
            {
     PayOrderDeleteSQL.Delete();
      }
            else
        {
       PayOrderDeleteSQL.DeleteParameters["EndDate"].DefaultValue = DateTime.Today.ToShortDateString();
     PayOrderDeleteSQL.Delete();
                PayOrderDeleteSQL.Update();
            }

        // Hide reject button and show active button
   RejectButton.Visible = false;
       PayorderRadioButtonList.Visible = false;
            ActiveButton.Visible = true;

      Device_DataUpdateSQL.InsertParameters["UpdateType"].DefaultValue = "TC Student";
     Device_DataUpdateSQL.InsertParameters["UpdateDescription"].DefaultValue = "Deactivate Student";
  Device_DataUpdateSQL.Insert();

          Response.Redirect("Print_TC.aspx?Student=" + StudentInfoFormView.DataKey["StudentID"].ToString() + "&S_Class=" + StudentInfoFormView.DataKey["StudentClassID"].ToString());
   }

        protected void ConfirmActiveButton_Click(object sender, EventArgs e)
 {
     try
  {
System.Diagnostics.Debug.WriteLine("=== ConfirmActiveButton_Click START ===");

  // Validate page first
   if (!Page.IsValid)
   {
   System.Diagnostics.Debug.WriteLine("Page validation failed!");
 return;
   }

// Try to get from hidden fields FIRST (most reliable for postback)
 string studentID = hdnStudentID.Value;
string currentStudentClassID = hdnStudentClassID.Value;
  string currentStudentID = hdnStudentIDText.Value;
 string deactivateTime = hdnDeactivateTime.Value;

  System.Diagnostics.Debug.WriteLine($"From Hidden Fields: StudentID={studentID}, ClassID={currentStudentClassID}, ID={currentStudentID}");

 // Fallback to ViewState if hidden fields are empty
 if (string.IsNullOrEmpty(studentID))
{
    System.Diagnostics.Debug.WriteLine("Hidden fields empty, trying ViewState...");
  studentID = ViewState["CurrentStudentID"]?.ToString();
      currentStudentClassID = ViewState["CurrentStudentClassID"]?.ToString();
    currentStudentID = ViewState["CurrentStudentIDText"]?.ToString();
        deactivateTime = ViewState["CurrentDeactivateTime"]?.ToString();
   System.Diagnostics.Debug.WriteLine($"From ViewState: StudentID={studentID}, ClassID={currentStudentClassID}, ID={currentStudentID}");
   }

  // Validate student data is available
   if (string.IsNullOrEmpty(studentID) || 
    string.IsNullOrEmpty(currentStudentClassID) || 
  string.IsNullOrEmpty(currentStudentID))
    {
 System.Diagnostics.Debug.WriteLine("VALIDATION FAILED - No student data found!");
     
        // Show error message to user
     ScriptManager.RegisterStartupScript(this, GetType(), "noStudentError", 
 "alert('Student information not found. Please search for a student first.');", true);
      return;
        }

System.Diagnostics.Debug.WriteLine("Validation PASSED - Proceeding with activation...");

     // Validate class and session selection
 if (ActiveClassDropDown.SelectedValue == "0" || string.IsNullOrEmpty(ActiveClassDropDown.SelectedValue))
           {
   System.Diagnostics.Debug.WriteLine("Class validation failed!");
   ScriptManager.RegisterStartupScript(this, GetType(), "validationError", 
   "alert('Please select a class!');", true);
     return;
     }

       if (ActiveSessionDropDown.SelectedValue == "0" || string.IsNullOrEmpty(ActiveSessionDropDown.SelectedValue))
         {
    System.Diagnostics.Debug.WriteLine("Session validation failed!");
     ScriptManager.RegisterStartupScript(this, GetType(), "validationError", 
       "alert('Please select a session year!');", true);
             return;
       }

    string schoolID = Session["SchoolID"].ToString();
             string registrationID = Session["RegistrationID"].ToString();

       // Get selected values - Convert "0" or null to 0 (not DBNull)
 int newClassID = int.Parse(ActiveClassDropDown.SelectedValue);
        int newEducationYearID = int.Parse(ActiveSessionDropDown.SelectedValue);

     // Convert to 0 instead of null for Section, Group, Shift
        int sectionID = (ActiveSectionDropDown.SelectedValue == "0" || string.IsNullOrEmpty(ActiveSectionDropDown.SelectedValue)) ? 0 : int.Parse(ActiveSectionDropDown.SelectedValue);
          int groupID = (ActiveGroupDropDown.SelectedValue == "0" || string.IsNullOrEmpty(ActiveGroupDropDown.SelectedValue)) ? 0 : int.Parse(ActiveGroupDropDown.SelectedValue);
          int shiftID = (ActiveShiftDropDown.SelectedValue == "0" || string.IsNullOrEmpty(ActiveShiftDropDown.SelectedValue)) ? 0 : int.Parse(ActiveShiftDropDown.SelectedValue);

     System.Diagnostics.Debug.WriteLine($"Selected - Class: {newClassID}, Session: {newEducationYearID}, Section: {sectionID}, Group: {groupID}, Shift: {shiftID}");

  bool success = false;
             string errorMessage = "";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString()))
        {
      con.Open();
             SqlTransaction transaction = con.BeginTransaction();

    try
        {
           // Step 1: Update Student Status to Active
      SqlCommand cmdActivate = new SqlCommand(@"
UPDATE Student 
SET Status = 'Active', 
    ActiveTime = GETDATE(), 
    ActiveDate = GETDATE() 
WHERE StudentID = @StudentID 
  AND SchoolID = @SchoolID", con, transaction);

   cmdActivate.Parameters.AddWithValue("@StudentID", studentID);
         cmdActivate.Parameters.AddWithValue("@SchoolID", schoolID);
 int rowsAffected = cmdActivate.ExecuteNonQuery();
      System.Diagnostics.Debug.WriteLine($"Student activated: {rowsAffected} rows affected");

    // Step 2: Check if student already has ANY active record in the SELECTED session
  string checkSessionSQL = @"
SELECT StudentClassID, ClassID
FROM StudentsClass 
WHERE StudentID = @StudentID 
  AND SchoolID = @SchoolID 
  AND EducationYearID = @EducationYearID
  AND Class_Status IS NULL";

          SqlCommand cmdCheckSession = new SqlCommand(checkSessionSQL, con, transaction);
    cmdCheckSession.Parameters.AddWithValue("@StudentID", studentID);
 cmdCheckSession.Parameters.AddWithValue("@SchoolID", schoolID);
      cmdCheckSession.Parameters.AddWithValue("@EducationYearID", newEducationYearID);

    SqlDataReader reader = cmdCheckSession.ExecuteReader();
bool hasActiveRecordInSession = false;
object existingStudentClassID = null;
int existingClassID = 0;

if (reader.Read())
{
    hasActiveRecordInSession = true;
    existingStudentClassID = reader["StudentClassID"];
    existingClassID = reader["ClassID"] != DBNull.Value ? Convert.ToInt32(reader["ClassID"]) : 0;
  System.Diagnostics.Debug.WriteLine($"Found existing active record in session: StudentClassID={existingStudentClassID}, ClassID={existingClassID}");
}
reader.Close();

if (hasActiveRecordInSession)
{
    // Student has active record in this session
    if (existingClassID == newClassID)
    {
        // Same class - just update section, group, shift
        System.Diagnostics.Debug.WriteLine("Same session, same class - updating only Section/Group/Shift");
        string updateSameClassSQL = @"
UPDATE StudentsClass 
SET SectionID = @SectionID,
    SubjectGroupID = @SubjectGroupID,
    ShiftID = @ShiftID
WHERE StudentClassID = @StudentClassID";

 SqlCommand cmdUpdateSameClass = new SqlCommand(updateSameClassSQL, con, transaction);
  cmdUpdateSameClass.Parameters.AddWithValue("@SectionID", sectionID);
        cmdUpdateSameClass.Parameters.AddWithValue("@SubjectGroupID", groupID);
        cmdUpdateSameClass.Parameters.AddWithValue("@ShiftID", shiftID);
        cmdUpdateSameClass.Parameters.AddWithValue("@StudentClassID", existingStudentClassID);
        cmdUpdateSameClass.ExecuteNonQuery();
    }
  else
    {
        // Different class in same session - update ClassID + Section/Group/Shift
        System.Diagnostics.Debug.WriteLine($"Same session, different class (from {existingClassID} to {newClassID}) - updating ClassID");
        string updateDifferentClassSQL = @"
UPDATE StudentsClass 
SET ClassID = @ClassID,
 SectionID = @SectionID,
    SubjectGroupID = @SubjectGroupID,
    ShiftID = @ShiftID
WHERE StudentClassID = @StudentClassID";

   SqlCommand cmdUpdateDiffClass = new SqlCommand(updateDifferentClassSQL, con, transaction);
    cmdUpdateDiffClass.Parameters.AddWithValue("@ClassID", newClassID);
        cmdUpdateDiffClass.Parameters.AddWithValue("@SectionID", sectionID);
   cmdUpdateDiffClass.Parameters.AddWithValue("@SubjectGroupID", groupID);
   cmdUpdateDiffClass.Parameters.AddWithValue("@ShiftID", shiftID);
        cmdUpdateDiffClass.Parameters.AddWithValue("@StudentClassID", existingStudentClassID);
    cmdUpdateDiffClass.ExecuteNonQuery();
    }
}
else
{
    // No active record in selected session
    System.Diagnostics.Debug.WriteLine("No active record in selected session");
    
    // Mark ALL active records in OTHER sessions as 'Re-Admitted'
    string markOtherSessionsSQL = @"
UPDATE StudentsClass 
SET Class_Status = 'Re-Admitted'
WHERE StudentID = @StudentID 
  AND SchoolID = @SchoolID 
  AND EducationYearID <> @EducationYearID
  AND Class_Status IS NULL";

    SqlCommand cmdMarkOther = new SqlCommand(markOtherSessionsSQL, con, transaction);
    cmdMarkOther.Parameters.AddWithValue("@StudentID", studentID);
    cmdMarkOther.Parameters.AddWithValue("@SchoolID", schoolID);
    cmdMarkOther.Parameters.AddWithValue("@EducationYearID", newEducationYearID);
    rowsAffected = cmdMarkOther.ExecuteNonQuery();
    System.Diagnostics.Debug.WriteLine($"Marked old session records: {rowsAffected} rows affected");

    // Insert new class record for new session
    System.Diagnostics.Debug.WriteLine("Inserting new class record for new session");
    string insertNewRecordSQL = @"
INSERT INTO StudentsClass 
(SchoolID, RegistrationID, StudentID, ClassID, SectionID, SubjectGroupID, ShiftID, EducationYearID, Date, Class_Status)
VALUES 
(@SchoolID, @RegistrationID, @StudentID, @ClassID, @SectionID, @SubjectGroupID, @ShiftID, @EducationYearID, GETDATE(), NULL)";

    SqlCommand cmdInsertNew = new SqlCommand(insertNewRecordSQL, con, transaction);
    cmdInsertNew.Parameters.AddWithValue("@SchoolID", schoolID);
    cmdInsertNew.Parameters.AddWithValue("@RegistrationID", registrationID);
    cmdInsertNew.Parameters.AddWithValue("@StudentID", studentID);
    cmdInsertNew.Parameters.AddWithValue("@ClassID", newClassID);
    cmdInsertNew.Parameters.AddWithValue("@SectionID", sectionID);
    cmdInsertNew.Parameters.AddWithValue("@SubjectGroupID", groupID);
    cmdInsertNew.Parameters.AddWithValue("@ShiftID", shiftID);
    cmdInsertNew.Parameters.AddWithValue("@EducationYearID", newEducationYearID);
    cmdInsertNew.ExecuteNonQuery();
}

          // Step 3: Log the activation
   ActDeActLogSQL.InsertParameters["StudentClassID"].DefaultValue = currentStudentClassID;
      ActDeActLogSQL.InsertParameters["StudentID"].DefaultValue = studentID;
     ActDeActLogSQL.InsertParameters["Status"].DefaultValue = "Rejected";
     ActDeActLogSQL.InsertParameters["time"].DefaultValue = deactivateTime ?? DateTime.Now.ToString();
   ActDeActLogSQL.Insert();

   // Step 4: Update device data
      Device_DataUpdateSQL.InsertParameters["UpdateType"].DefaultValue = "Active Student";
    Device_DataUpdateSQL.InsertParameters["UpdateDescription"].DefaultValue = "Activate Student";
          Device_DataUpdateSQL.Insert();

       // Commit transaction
 transaction.Commit();
success = true;
 System.Diagnostics.Debug.WriteLine("Transaction committed successfully!");
 }
      catch (Exception ex)
    {
    transaction.Rollback();
      errorMessage = ex.Message;
   success = false;
    System.Diagnostics.Debug.WriteLine($"Transaction failed: {ex.Message}");
         }
     }

             if (success)
         {
      System.Diagnostics.Debug.WriteLine("Activation successful - showing success message");
 
      // Hide panel
            ActiveStudentPanel.Visible = false;
      
            // Reset all dropdowns
    ActiveClassDropDown.SelectedValue = "0";
    ActiveSectionDropDown.SelectedValue = "0";
    ActiveGroupDropDown.SelectedValue = "0";
    ActiveShiftDropDown.SelectedValue = "0";
    ActiveSessionDropDown.SelectedValue = "0";
   
    // Refresh student info to show updated status (Active instead of Rejected)
    IDTextBox.Text = currentStudentID;
    StudentInfoFormView.DataBind();
    
    // Also refresh the TC List GridView if visible
    StatusGridView.DataBind();
    
    // Update both UpdatePanels
    UpdatePanelActiveStudent.Update();
    UpdatePanelStudentInfo.Update();
    
    // Show success message - WITHOUT triggering another postback
    ScriptManager.RegisterStartupScript(this, GetType(), "ActivationSuccess", 
        "alert('✓ Student activated successfully!');", true);
 }
else
    {
  System.Diagnostics.Debug.WriteLine($"Activation failed: {errorMessage}");
        // Show error
       ScriptManager.RegisterStartupScript(this, GetType(), "ActivationError", 
     $"alert('Error activating student: {errorMessage.Replace("'", "\\'")}');", true);
     }
       }
       catch (Exception ex)
        {
  // Handle any unexpected errors
   System.Diagnostics.Debug.WriteLine($"Unexpected error: {ex.Message}\n{ex.StackTrace}");
      ScriptManager.RegisterStartupScript(this, GetType(), "UnexpectedError", 
   $"alert('An error occurred: {ex.Message.Replace("'", "\\'")}');", true);
      }
   }

        protected void ActiveButton_Click(object sender, EventArgs e)
        {
   // Simply show the ActiveStudentPanel
            ActiveStudentPanel.Visible = true;
 
            // Reset all dropdowns to default values
            ActiveClassDropDown.SelectedValue = "0";
          ActiveSectionDropDown.SelectedValue = "0";
            ActiveGroupDropDown.SelectedValue = "0";
        ActiveShiftDropDown.SelectedValue = "0";
            ActiveSessionDropDown.SelectedValue = "0";
        
    // Clear all items except the default ones
    while (ActiveSectionDropDown.Items.Count > 1)
    {
        ActiveSectionDropDown.Items.RemoveAt(1);
    }
    
    while (ActiveGroupDropDown.Items.Count > 1)
    {
        ActiveGroupDropDown.Items.RemoveAt(1);
    }
    
    while (ActiveShiftDropDown.Items.Count > 1)
    {
  ActiveShiftDropDown.Items.RemoveAt(1);
    }
    
    // Bind all dropdowns to get fresh data
    ActiveClassDropDown.DataBind();
            ActiveSectionDropDown.DataBind();
            ActiveGroupDropDown.DataBind();
  ActiveShiftDropDown.DataBind();
ActiveSessionDropDown.DataBind();
 
      // Initially hide optional fields until class is selected
            UpdateActiveFormVisibility();
   
    // Update the UpdatePanel
            UpdatePanelActiveStudent.Update();
   
       System.Diagnostics.Debug.WriteLine("ActiveButton clicked - Panel shown and dropdowns reset");
        }

        protected void CancelActiveButton_Click(object sender, EventArgs e)
        {
 // Hide the panel
   ActiveStudentPanel.Visible = false;
  
  // Update the UpdatePanel
    UpdatePanelActiveStudent.Update();
  }

 private void UpdateActiveFormVisibility()
    {
         // Show/hide optional fields based on class selection
      // Check if class is selected
 bool hasClass = ActiveClassDropDown.SelectedValue != "0" && !string.IsNullOrEmpty(ActiveClassDropDown.SelectedValue);
   
    if (!hasClass)
    {
        // No class selected - hide all optional field containers
        divSection.Visible = false;
 divGroup.Visible = false;
        divShift.Visible = false;
return;
    }

    // Class is selected - check if there are sections, groups, shifts available
    
    // Check Sections
    // Clear items except the first one (default "[ No Section ]") before databind
    while (ActiveSectionDropDown.Items.Count > 1)
    {
  ActiveSectionDropDown.Items.RemoveAt(1);
  }
    ActiveSectionDropDown.DataBind();
    bool hasSections = ActiveSectionDropDown.Items.Count > 1; // More than just "[ No Section ]"
    divSection.Visible = hasSections;
    
  // Check Groups
    // Clear items except the first one (default "[ No Group ]") before databind
    while (ActiveGroupDropDown.Items.Count > 1)
    {
        ActiveGroupDropDown.Items.RemoveAt(1);
    }
    ActiveGroupDropDown.DataBind();
    bool hasGroups = ActiveGroupDropDown.Items.Count > 1; // More than just "[ No Group ]"
    divGroup.Visible = hasGroups;
    
    // Check Shifts
    // Clear items except the first one (default "[ No Shift ]") before databind
    while (ActiveShiftDropDown.Items.Count > 1)
    {
        ActiveShiftDropDown.Items.RemoveAt(1);
    }
    // Shifts are school-wide, not class-specific, so check if there are any shifts
    ActiveShiftDropDown.DataBind();
    bool hasShifts = ActiveShiftDropDown.Items.Count > 1; // More than just "[ No Shift ]"
    divShift.Visible = hasShifts;

    System.Diagnostics.Debug.WriteLine($"Visibility - Sections: {hasSections}, Groups: {hasGroups}, Shifts: {hasShifts}");
        }

        protected void ActiveClassDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
          // Reset dependent dropdowns when class changes
   ActiveSectionDropDown.SelectedValue = "0";
    ActiveGroupDropDown.SelectedValue = "0";
    
    // Clear existing items except the default one before rebinding
  while (ActiveSectionDropDown.Items.Count > 1)
    {
        ActiveSectionDropDown.Items.RemoveAt(1);
}
    
    while (ActiveGroupDropDown.Items.Count > 1)
    {
 ActiveGroupDropDown.Items.RemoveAt(1);
    }
    
    // Rebind dependent dropdowns to get data for new class
    ActiveSectionDropDown.DataBind();
    ActiveGroupDropDown.DataBind();

    // Update visibility based on availability
    UpdateActiveFormVisibility();
    
    // Update the UpdatePanel
    UpdatePanelActiveStudent.Update();
    
    System.Diagnostics.Debug.WriteLine($"Class changed to: {ActiveClassDropDown.SelectedValue}");
        }
    }
}