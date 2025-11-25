using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Script.Serialization;
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

                // Check if page reloaded after activation with student ID
                if (!string.IsNullOrEmpty(Request.QueryString["id"]) && Request.QueryString["refresh"] == "1")
                {
                    IDTextBox.Text = Request.QueryString["id"];
                    // Trigger find button click programmatically
                    StudentInfoFormView.DataBind();
                }
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

            RejectButton.Visible = false;
            ActiveButton.Visible = true;

            Device_DataUpdateSQL.InsertParameters["UpdateType"].DefaultValue = "TC Student";
            Device_DataUpdateSQL.InsertParameters["UpdateDescription"].DefaultValue = "Deactivate Student";
            Device_DataUpdateSQL.Insert();

            Response.Redirect("Print_TC.aspx?Student=" + StudentInfoFormView.DataKey["StudentID"].ToString() + "&S_Class=" + StudentInfoFormView.DataKey["StudentClassID"].ToString());
        }

        protected void ActiveClassDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Refresh section and group dropdowns when class changes
            DropDownList sectionDDL = (DropDownList)ModalUpdatePanel.FindControl("ActiveSectionDropDown");
            DropDownList groupDDL = (DropDownList)ModalUpdatePanel.FindControl("ActiveGroupDropDown");

            // Ensure SqlDataSource parameters receive the selected ClassID explicitly
            SqlDataSource sectionDS = (SqlDataSource)ModalUpdatePanel.FindControl("ActiveSectionSQL");
            SqlDataSource groupDS = (SqlDataSource)ModalUpdatePanel.FindControl("ActiveGroupSQL");
            DropDownList classDDL = (DropDownList)ModalUpdatePanel.FindControl("ActiveClassDropDown");
            string classId = classDDL != null ? classDDL.SelectedValue : "0";

            if (sectionDS != null)
            {
                if (sectionDS.SelectParameters["ClassID"] != null)
                    sectionDS.SelectParameters["ClassID"].DefaultValue = classId;
            }
            if (groupDS != null)
            {
                if (groupDS.SelectParameters["ClassID"] != null)
                    groupDS.SelectParameters["ClassID"].DefaultValue = classId;
            }

            if (sectionDDL != null) sectionDDL.DataBind();
            if (groupDDL != null) groupDDL.DataBind();
        }

        protected void ConfirmActiveButton_Click(object sender, EventArgs e)
        {
            // Validate class and session selection
            if (ActiveClassDropDown.SelectedValue == "0")
            {
                // Show error using ScriptManager
                ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "alert('Please select a class!');", true);
                return;
            }

            if (ActiveSessionDropDown.SelectedValue == "0")
            {
                // Show error using ScriptManager
                ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "alert('Please select a session year!');", true);
                return;
            }

            // Get student info from FormView
            string studentID = StudentInfoFormView.DataKey["StudentID"].ToString();
            string currentStudentClassID = StudentInfoFormView.DataKey["StudentClassID"].ToString();
            int currentClassID = Convert.ToInt32(StudentInfoFormView.DataKey["ClassID"]);
            int currentEducationYearID = Convert.ToInt32(StudentInfoFormView.DataKey["EducationYearID"]);
            string schoolID = Session["SchoolID"].ToString();
            string registrationID = Session["RegistrationID"].ToString();

            // Get selected values - Convert "0" or null to 0 (not DBNull)
            int newClassID = int.Parse(ActiveClassDropDown.SelectedValue);
            int newEducationYearID = int.Parse(ActiveSessionDropDown.SelectedValue);

            // Convert to 0 instead of null for Section, Group, Shift
            int sectionID = (ActiveSectionDropDown.SelectedValue == "0" || string.IsNullOrEmpty(ActiveSectionDropDown.SelectedValue)) ? 0 : int.Parse(ActiveSectionDropDown.SelectedValue);
            int groupID = (ActiveGroupDropDown.SelectedValue == "0" || string.IsNullOrEmpty(ActiveGroupDropDown.SelectedValue)) ? 0 : int.Parse(ActiveGroupDropDown.SelectedValue);
            int shiftID = (ActiveShiftDropDown.SelectedValue == "0" || string.IsNullOrEmpty(ActiveShiftDropDown.SelectedValue)) ? 0 : int.Parse(ActiveShiftDropDown.SelectedValue);

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
                    cmdActivate.ExecuteNonQuery();

                    // Step 2: Check if same class and same session
                    bool isSameClassAndSession = (currentClassID == newClassID && currentEducationYearID == newEducationYearID);

                    if (isSameClassAndSession)
                    {
                        // Same class & session - just update the current record (optional: update section/group/shift)
                        string updateCurrentSQL = @"
       UPDATE StudentsClass 
     SET SectionID = @SectionID,
  SubjectGroupID = @SubjectGroupID,
     ShiftID = @ShiftID
           WHERE StudentClassID = @StudentClassID";

                        SqlCommand cmdUpdateCurrent = new SqlCommand(updateCurrentSQL, con, transaction);
                        cmdUpdateCurrent.Parameters.AddWithValue("@SectionID", sectionID);
                        cmdUpdateCurrent.Parameters.AddWithValue("@SubjectGroupID", groupID);
                        cmdUpdateCurrent.Parameters.AddWithValue("@ShiftID", shiftID);
                        cmdUpdateCurrent.Parameters.AddWithValue("@StudentClassID", currentStudentClassID);
                        cmdUpdateCurrent.ExecuteNonQuery();
                    }
                    else
                    {
                        // Different class OR different session
                        // Step 2a: Mark old StudentsClass record as 'Re-Admitted'
                        string markOldRecordSQL = @"
    UPDATE StudentsClass 
   SET Class_Status = 'Re-Admitted'
         WHERE StudentClassID = @StudentClassID";

                        SqlCommand cmdMarkOld = new SqlCommand(markOldRecordSQL, con, transaction);
                        cmdMarkOld.Parameters.AddWithValue("@StudentClassID", currentStudentClassID);
                        cmdMarkOld.ExecuteNonQuery();

                        // Step 2b: Check if student already has a record for the NEW session
                        string checkNewSessionSQL = @"
 SELECT StudentClassID 
     FROM StudentsClass 
 WHERE StudentID = @StudentID 
 AND SchoolID = @SchoolID 
AND EducationYearID = @EducationYearID
   AND ClassID = @ClassID
  AND Class_Status IS NULL";

                        SqlCommand cmdCheckNew = new SqlCommand(checkNewSessionSQL, con, transaction);
                        cmdCheckNew.Parameters.AddWithValue("@StudentID", studentID);
                        cmdCheckNew.Parameters.AddWithValue("@SchoolID", schoolID);
                        cmdCheckNew.Parameters.AddWithValue("@EducationYearID", newEducationYearID);
                        cmdCheckNew.Parameters.AddWithValue("@ClassID", newClassID);

                        object existingNewClassID = cmdCheckNew.ExecuteScalar();

                        if (existingNewClassID != null)
                        {
                            // Update existing record for new session
                            string updateNewRecordSQL = @"
    UPDATE StudentsClass 
         SET SectionID = @SectionID,
      SubjectGroupID = @SubjectGroupID,
        ShiftID = @ShiftID,
  Class_Status = NULL
          WHERE StudentClassID = @StudentClassID";

                            SqlCommand cmdUpdateNew = new SqlCommand(updateNewRecordSQL, con, transaction);
                            cmdUpdateNew.Parameters.AddWithValue("@SectionID", sectionID);
                            cmdUpdateNew.Parameters.AddWithValue("@SubjectGroupID", groupID);
                            cmdUpdateNew.Parameters.AddWithValue("@ShiftID", shiftID);
                            cmdUpdateNew.Parameters.AddWithValue("@StudentClassID", existingNewClassID);
                            cmdUpdateNew.ExecuteNonQuery();
                        }
                        else
                        {
                            // Insert new class record for new session/class
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
                    }

                    // Step 3: Log the activation
                    ActDeActLogSQL.InsertParameters["StudentClassID"].DefaultValue = currentStudentClassID;
                    ActDeActLogSQL.InsertParameters["StudentID"].DefaultValue = studentID;
                    ActDeActLogSQL.InsertParameters["Status"].DefaultValue = "Rejected";
                    ActDeActLogSQL.InsertParameters["time"].DefaultValue = StudentInfoFormView.DataKey["DeactivateTime"].ToString();
                    ActDeActLogSQL.Insert();

                    // Step 4: Update device data
                    Device_DataUpdateSQL.InsertParameters["UpdateType"].DefaultValue = "Active Student";
                    Device_DataUpdateSQL.InsertParameters["UpdateDescription"].DefaultValue = "Activate Student";
                    Device_DataUpdateSQL.Insert();

                    // Commit transaction
                    transaction.Commit();

                    // Store current student ID to reload after page refresh
                    string currentStudentID = StudentInfoFormView.DataKey["ID"].ToString();

                    // Refresh grid data (TC List tab)
                    StatusGridView.DataBind();

                    // Hide modal and reload page with student ID to auto-find
                    string script = $@"
     hideActiveModal(); 
   
      // Show success message
 alert('✓ Student activated successfully!\n\nRefreshing to show updated status...');
 
  // Reload page with student ID parameter
       setTimeout(function() {{
   window.location.href = '{Request.Url.AbsolutePath}?id={currentStudentID}&refresh=1';
       }}, 500);
  ";
                    ScriptManager.RegisterStartupScript(this, GetType(), "hideModal", script, true);

                    // Update button visibility (will be set correctly by StudentInfoFormView_DataBound)
                    RejectButton.Visible = true;
                    ActiveButton.Visible = false;
                    PayorderRadioButtonList.Visible = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    // Show error message
                    ScriptManager.RegisterStartupScript(this, GetType(), "showalert", $"alert('Error activating student: {ex.Message}');", true);
                }
            }
        }

        protected void StatusSQL_Selected(object sender, SqlDataSourceStatusEventArgs e)
        {
            CountStudentLabel.Text = "Total: " + e.AffectedRows.ToString() + " Student(s)";
        }

        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            TiIDTextBox.Text = "";
        }

        protected void StudentInfoFormView_DataBound(object sender, EventArgs e)
        {
            if (StudentInfoFormView.DataItemCount > 0)
            {
                string Status = StudentInfoFormView.DataKey["Status"].ToString();

                PlaceHolder ph = (PlaceHolder)StudentInfoFormView.FindControl("PayOrderLinkPlaceHolder");

                if (Status == "Active")
                {
                    RejectButton.Visible = true;
                    ActiveButton.Visible = false;
                    PayorderRadioButtonList.Visible = true;

                    // Add Pay Order link dynamically
                    if (ph != null)
                    {
                        string studentID = StudentInfoFormView.DataKey["StudentID"].ToString();
                        string classID = StudentInfoFormView.DataKey["ClassID"].ToString();
                        string studentClassID = StudentInfoFormView.DataKey["StudentClassID"].ToString();
                        string yearID = StudentInfoFormView.DataKey["EducationYearID"].ToString();

                        string payOrderUrl = $"~/Admission/New_Student_Admission/Pay_Order.aspx?Student={studentID}&Class={classID}&StudentClass={studentClassID}&Year={yearID}";

                        ph.Controls.Add(new LiteralControl($@"
      <a href='{ResolveUrl(payOrderUrl)}' target='_blank' class='btn btn-warning ml-2'>
          <i class='fa fa-money'></i> Go to Pay Order
  </a>
              "));
                    }
                }
                else
                {
                    ActiveButton.Visible = true;
                    RejectButton.Visible = false;
                    PayorderRadioButtonList.Visible = false;
                }
            }
        }


        [WebMethod]
        public static string GetAllID(string ids)
        {
            List<string> StudentId = new List<string>();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT top(3) ID FROM Student WHERE SchoolID = @SchoolID AND ID like @ID + '%'";
                    cmd.Parameters.AddWithValue("@ID", ids);
                    cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                    cmd.Connection = con;

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        StudentId.Add(dr["ID"].ToString());
                    }
                    con.Close();

                    var json = new JavaScriptSerializer().Serialize(StudentId);
                    return json;
                }
            }
        }

        protected void ActiveSectionSQL_Selecting(object sender, SqlDataSourceSelectingEventArgs e)
        {
            // Ensure @ClassID is always present (default0) to avoid SQL error on first load
            if (e.Command.Parameters.Contains("@ClassID"))
            {
                var classDDL = (DropDownList)ModalUpdatePanel.FindControl("ActiveClassDropDown");
                e.Command.Parameters["@ClassID"].Value = classDDL != null && !string.IsNullOrEmpty(classDDL.SelectedValue) ? classDDL.SelectedValue : "0";
            }
        }

        protected void ActiveGroupSQL_Selecting(object sender, SqlDataSourceSelectingEventArgs e)
        {
            if (e.Command.Parameters.Contains("@ClassID"))
            {
                var classDDL = (DropDownList)ModalUpdatePanel.FindControl("ActiveClassDropDown");
                e.Command.Parameters["@ClassID"].Value = classDDL != null && !string.IsNullOrEmpty(classDDL.SelectedValue) ? classDDL.SelectedValue : "0";
            }
        }

    }
}