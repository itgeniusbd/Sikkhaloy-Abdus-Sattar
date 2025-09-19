using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

namespace EDUCATION.COM.Exam.Result
{
    public partial class Bangla_Result_DirectPrint : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;

            try
            {
                if (!IsPostBack)
                {
                    GroupDropDownList.Visible = false;
                    SectionDropDownList.Visible = false;
                    ShiftDropDownList.Visible = false;
                }
            }
            catch { }
        }

        protected void view()
        {
            DataView GroupDV = (DataView)GroupSQL.Select(DataSourceSelectArguments.Empty);
            GroupDropDownList.Visible = GroupDV.Count > 0;

            DataView SectionDV = (DataView)SectionSQL.Select(DataSourceSelectArguments.Empty);
            SectionDropDownList.Visible = SectionDV.Count > 0;

            DataView ShiftDV = (DataView)ShiftSQL.Select(DataSourceSelectArguments.Empty);
            ShiftDropDownList.Visible = ShiftDV.Count > 0;
        }

        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Group"] = "%";
            Session["Shift"] = "%";
            Session["Section"] = "%";

            GroupDropDownList.DataBind();
            ShiftDropDownList.DataBind();
            SectionDropDownList.DataBind();
            ExamDropDownList.DataBind();
            view();

            ResultPanel.Visible = false;
        }

        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
            ResultPanel.Visible = false;
        }

        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ ALL ]", "%"));
            if (IsPostBack)
            {
                var groupValue = Session["Group"]?.ToString();
                if (!string.IsNullOrEmpty(groupValue) && GroupDropDownList.Items.FindByValue(groupValue) != null)
                    GroupDropDownList.Items.FindByValue(groupValue).Selected = true;
            }
        }

        protected void SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
            ResultPanel.Visible = false;
        }

        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ ALL ]", "%"));
            if (IsPostBack)
            {
                var sectionValue = Session["Section"]?.ToString();
                if (!string.IsNullOrEmpty(sectionValue) && SectionDropDownList.Items.FindByValue(sectionValue) != null)
                    SectionDropDownList.Items.FindByValue(sectionValue).Selected = true;
            }
        }

        protected void ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
            ResultPanel.Visible = false;
        }

        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            ShiftDropDownList.Items.Insert(0, new ListItem("[ ALL ]", "%"));
            if (IsPostBack)
            {
                var shiftValue = Session["Shift"]?.ToString();
                if (!string.IsNullOrEmpty(shiftValue) && ShiftDropDownList.Items.FindByValue(shiftValue) != null)
                    ShiftDropDownList.Items.FindByValue(shiftValue).Selected = true;
            }
        }

        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
            ResultPanel.Visible = false;
        }

        protected void ExamDropDownList_DataBound(object sender, EventArgs e)
        {
            ExamDropDownList.Items.Insert(0, new ListItem("[ SELECT ]", "0"));
        }

        protected void LoadResultsButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Debug using simple console.log with safe string handling
                string debugScript = "console.log('=== DEBUG INFO ===');" +
                                    "console.log('Class: " + ClassDropDownList.SelectedValue + "');" +
                                    "console.log('Exam: " + ExamDropDownList.SelectedValue + "');" +
                                    "console.log('Section: " + SectionDropDownList.SelectedValue + "');" +
                                    "console.log('Group: " + GroupDropDownList.SelectedValue + "');" +
                                    "console.log('Shift: " + ShiftDropDownList.SelectedValue + "');";

                Page.ClientScript.RegisterStartupScript(typeof(Page), "debug", debugScript, true);

                if (ExamDropDownList.SelectedValue != "0" && ClassDropDownList.SelectedValue != "0")
                {
                    LoadResults();
                }
                else
                {
                    Page.ClientScript.RegisterStartupScript(typeof(Page), "alert",
                        "alert('Please select both Class and Exam');", true);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message.Replace("'", "\\'").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ");
                string errorScript = "console.error('LoadResults Error: " + errorMsg + "');";
                Page.ClientScript.RegisterStartupScript(typeof(Page), "error", errorScript, true);
            }
        }

        private void LoadResults()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                try
                {
                    con.Open();

                    // Debug session values using safe string handling
                    string schoolId = Session["SchoolID"]?.ToString() ?? "NULL";
                    string eduYear = Session["Edu_Year"]?.ToString() ?? "NULL";

                    string sessionScript = "console.log('=== SESSION VALUES ===');" +
                                         "console.log('SchoolID: " + schoolId + "');" +
                                         "console.log('EducationYearID: " + eduYear + "');";
                    Page.ClientScript.RegisterStartupScript(typeof(Page), "sessionDebug", sessionScript, true);

                    // Fixed query with SchoolID included
                    string query = @"
                        SELECT DISTINCT
                            ers.StudentResultID,
                            ers.TotalExamObtainedMark_ofStudent,
                            ers.Student_Grade,
                            ers.Student_Point,
                            ers.Average,
                            ers.ObtainedPercentage_ofStudent,
                            ers.TotalMark_ofStudent,
                            ers.Position_InExam_Class,
                            ers.Position_InExam_Subsection,
                            CASE 
                                WHEN ers.Student_Grade = 'F' THEN 'Fail'
                                ELSE 'Pass'
                            END as PassStatus_ofStudent,
                            s.StudentsName,
                            s.StudentID as ID,
                            sc.RollNo,
                            cc.Class as ClassName,
                            ISNULL(cs.Section, '') as SectionName,
                            ISNULL(csh.Shift, '') as ShiftName,
                            ISNULL(csg.SubjectGroup, '') as GroupName,
                            en.ExamName,
                            -- Include SchoolID for logo handler
                            ers.SchoolID,
                            -- Default values for missing columns
                            NULL as Image,
                            'Imperial Ideal School & College' as SchoolName,
                            '761,Tulatulisohera Rd,Kalulkotil, Narayangonj' as Address,
                            'Phone: 01906-265260, 01789-752002' as Phone,
                            NULL as SchoolLogo,
                            -- Attendance default values
                            0 as WorkingDays,
                            0 as TotalPresent,
                            0 as TotalAbsent,
                            0 as TotalLate,
                            0 as TotalLeave,
                            0 as TotalBunk,
                            0 as TotalLateAbs
                        FROM Exam_Result_of_Student ers
                        INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
                        INNER JOIN Student s ON sc.StudentID = s.StudentID
                        INNER JOIN CreateClass cc ON sc.ClassID = cc.ClassID
                        INNER JOIN Exam_Name en ON ers.ExamID = en.ExamID
                        LEFT JOIN CreateSection cs ON sc.SectionID = cs.SectionID
                        LEFT JOIN CreateShift csh ON sc.ShiftID = csh.ShiftID
                        LEFT JOIN CreateSubjectGroup csg ON sc.SubjectGroupID = csg.SubjectGroupID
                        WHERE ers.ExamID = @ExamID
                        AND sc.ClassID = @ClassID
                        AND sc.SectionID LIKE @SectionID
                        AND sc.ShiftID LIKE @ShiftID
                        AND sc.SubjectGroupID LIKE @GroupID
                        AND ers.SchoolID = @SchoolID
                        AND ers.EducationYearID = @EducationYearID
                        ORDER BY sc.RollNo";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@SectionID", SectionDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@ShiftID", ShiftDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@GroupID", GroupDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"]);

                    DataTable dt = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);

                    // Debug: Show row count with safe string handling
                    string resultScript = "console.log('=== QUERY RESULT ===');" +
                                        "console.log('Found " + dt.Rows.Count + " students');";
                    Page.ClientScript.RegisterStartupScript(typeof(Page), "debug2", resultScript, true);

                    if (dt.Rows.Count > 0)
                    {
                        ResultRepeater.DataSource = dt;
                        ResultRepeater.DataBind();
                        ResultPanel.Visible = true;

                        // Success message
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "success",
                            "console.log('Results loaded successfully!'); alert('Results loaded successfully!');", true);
                    }
                    else
                    {
                        ResultPanel.Visible = false;
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "nodata",
                            "alert('No results found for the selected criteria');", true);
                    }
                }
                catch (Exception ex)
                {
                    // Handle error with detailed message and safe string handling
                    ResultPanel.Visible = false;
                    string errorMsg = ex.Message.Replace("'", "\\'").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ");
                    string errorScript = "console.error('Database Error: " + errorMsg + "'); alert('Database Error occurred. Check console for details.');";
                    Page.ClientScript.RegisterStartupScript(typeof(Page), "dberror", errorScript, true);
                }
            }
        }

        protected void ResultRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                DataRowView row = (DataRowView)e.Item.DataItem;
                string studentResultID = row["StudentResultID"].ToString();

                // Find the nested repeater for subjects
                Repeater subjectRepeater = (Repeater)e.Item.FindControl("SubjectRepeater");
                
                if (subjectRepeater != null)
                {
                    // Load subject data for this student
                    DataTable subjectData = GetSubjectData(studentResultID);
                    subjectRepeater.DataSource = subjectData;
                    subjectRepeater.DataBind();
                }

                // Find and bind the grading system repeater
                Repeater gradingSystemRepeater = (Repeater)e.Item.FindControl("GradingSystemRepeater");
                
                if (gradingSystemRepeater != null)
                {
                    // Load grading system for this school
                    DataTable gradingData = GetGradingSystemForDisplay();
                    gradingSystemRepeater.DataSource = gradingData;
                    gradingSystemRepeater.DataBind();
                    
                    // Debug: Log grading system info
                    string gradingScript = "console.log('Grading System loaded: " + gradingData.Rows.Count + " grade levels');";
                    Page.ClientScript.RegisterStartupScript(typeof(Page), "gradingDebug" + studentResultID, gradingScript, true);
                }
            }
        }

        private DataTable GetSubjectData(string studentResultID)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    
                    // First check if we should use simple or complex query
                    bool hasSubExams = CheckIfExamHasSubExams(studentResultID);
                    
                    if (!hasSubExams)
                    {
                        // Use simple query for better performance
                        return GetSubjectDataSimple(studentResultID);
                    }
                    
                    // Continue with complex query only if sub-exams exist
                    DataTable gradingSystem = GetGradingSystem();
                    
                    // Get the exam ID and class ID for dynamic sub-exam detection
                    string examID = GetExamIDFromStudentResult(studentResultID);
                    string classID = GetCurrentClassID();
                    DataTable subExams = GetSubExamNames(examID);
                    
                    // Build dynamic sub-exam columns only if sub-exams exist for this class
                    string subExamColumns = "";
                    if (subExams.Rows.Count > 0)
                    {
                        for (int i = 0; i < subExams.Rows.Count; i++)
                        {
                            string subExamID = SafeGetString(subExams.Rows[i], "SubExamID");
                            string columnName = "SubExam" + (i + 1) + "Mark";
                            
                            subExamColumns += @"
                                ISNULL((SELECT TOP 1 eom.MarksObtained 
                                       FROM Exam_Obtain_Marks eom 
                                       INNER JOIN StudentsClass sc ON eom.StudentClassID = sc.StudentClassID
                                       WHERE eom.StudentClassID = ersMain.StudentClassID 
                                       AND eom.SubjectID = ers.SubjectID 
                                       AND eom.ExamID = ersMain.ExamID
                                       AND eom.SubExamID = " + subExamID + @"
                                       AND sc.ClassID = @ClassID), 0) as " + columnName + ",";
                        }
                    }
                    else
                    {
                        // Fallback to generic detection with class filtering
                        subExamColumns = @"
                            ISNULL((SELECT TOP 1 eom.MarksObtained 
                                   FROM Exam_Obtain_Marks eom 
                                   INNER JOIN Exam_SubExam_Name esn ON eom.SubExamID = esn.SubExamID
                                   INNER JOIN StudentsClass sc ON eom.StudentClassID = sc.StudentClassID
                                   WHERE eom.StudentClassID = ersMain.StudentClassID 
                                   AND eom.SubjectID = ers.SubjectID 
                                   AND eom.ExamID = ersMain.ExamID
                                   AND sc.ClassID = @ClassID
                                   AND esn.Sub_ExamSN = 1), 0) as MidtermMark,
                                   
                            ISNULL((SELECT TOP 1 eom.MarksObtained 
                                   FROM Exam_Obtain_Marks eom 
                                   INNER JOIN Exam_SubExam_Name esn ON eom.SubExamID = esn.SubExamID
                                   INNER JOIN StudentsClass sc ON eom.StudentClassID = sc.StudentClassID
                                   WHERE eom.StudentClassID = ersMain.StudentClassID 
                                   AND eom.SubjectID = ers.SubjectID 
                                   AND eom.ExamID = ersMain.ExamID
                                   AND sc.ClassID = @ClassID
                                   AND esn.Sub_ExamSN = 2), 0) as PeriodicalMark,";
                    }
                    
                    // Optimized query with class filtering
                    string query = @"
                        SELECT 
                            ISNULL(sub.SubjectName, '') as SubjectName,
                            sub.SubjectID,
                            ISNULL(sub.SN, 999) as SubjectSN,
                            ISNULL(ers.ObtainedMark_ofSubject, 0) as ObtainedMark_ofSubject,
                            ISNULL(ers.TotalMark_ofSubject, 0) as FullMark,
                            ISNULL(ers.SubjectGrades, '') as SubjectGrades,
                            ISNULL(ers.SubjectPoint, 0) as SubjectPoint,
                            ISNULL(ers.PassStatus_Subject, 'Pass') as PassStatus_InSubject,
                            " + subExamColumns + @"
                            ISNULL(ers.HighestMark_InSubject_Class, 0) as HighestMark_InSubject_Class,
                            ISNULL(ers.Position_InSubject_Class, 0) as Position_InSubject_Class,
                            ISNULL(ers.SubjectAbsenceStatus, '') as SubjectAbsenceStatus,
                            ISNULL(ers.IS_Add_InExam, 1) as IS_Add_InExam,
                            ISNULL(ers.PassMark_Subject, 33) as PassMark
                        FROM Exam_Result_of_Subject ers WITH (NOLOCK)
                        INNER JOIN Subject sub WITH (NOLOCK) ON ers.SubjectID = sub.SubjectID
                        INNER JOIN Exam_Result_of_Student ersMain WITH (NOLOCK) ON ers.StudentResultID = ersMain.StudentResultID
                        INNER JOIN StudentsClass sc WITH (NOLOCK) ON ersMain.StudentClassID = sc.StudentClassID
                        WHERE ers.StudentResultID = @StudentResultID
                        AND ISNULL(ers.IS_Add_InExam, 1) = 1
                        AND sc.ClassID = @ClassID
                        ORDER BY ISNULL(sub.SN, 999), sub.SubjectName";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.CommandTimeout = 30;
                    cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);
                    cmd.Parameters.AddWithValue("@ClassID", classID);
                    
                    DataTable dt = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);

                    // Apply dynamic grading if grades are missing
                    foreach (DataRow row in dt.Rows)
                    {
                        string currentGrade = SafeGetString(row, "SubjectGrades");
                        decimal currentPoint = SafeGetDecimal(row, "SubjectPoint");
                        
                        if (string.IsNullOrEmpty(currentGrade) || currentPoint == 0)
                        {
                            ApplyDynamicGrading(row, gradingSystem);
                        }
                    }

                    // Debug logging with class info
                    if (dt.Rows.Count > 0)
                    {
                        string subExamInfo = "";
                        if (subExams.Rows.Count > 0)
                        {
                            foreach (DataRow subExamRow in subExams.Rows)
                            {
                                subExamInfo += SafeGetString(subExamRow, "SubExamName") + ", ";
                            }
                            subExamInfo = subExamInfo.TrimEnd(',', ' ');
                        }
                        else
                        {
                            subExamInfo = "Midterm, Periodical";
                        }
                        
                        string infoScript = "console.log('Class " + classID + " - Sub-exams: " + subExamInfo + "');";
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "classSpecificSubExamInfo" + studentResultID, infoScript, true);
                        
                        string successScript = "console.log('Class-specific subject data loaded - " + dt.Rows.Count + " subjects found');";
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "classSpecificSubjectSuccess" + studentResultID, successScript, true);
                    }

                    return dt;
                }
                catch (Exception ex)
                {
                    // Log error and fallback to simple query
                    string errorMsg = ex.Message.Replace("'", "\\'").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ");
                    string errorScript = "console.error('Class-specific subject query failed, trying simple: " + errorMsg + "');";
                    Page.ClientScript.RegisterStartupScript(typeof(Page), "classSpecificSubjectError" + studentResultID, errorScript, true);
                    
                    // Fallback to simple query on error
                    return GetSubjectDataSimple(studentResultID);
                }
            }
        }

        private DataTable GetSubjectDataSimple(string studentResultID)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    
                    // Simple query without sub-exam complexity
                    string query = @"
                        SELECT 
                            ISNULL(sub.SubjectName, '') as SubjectName,
                            sub.SubjectID,
                            ISNULL(sub.SN, 999) as SubjectSN,
                            ISNULL(ers.ObtainedMark_ofSubject, 0) as ObtainedMark_ofSubject,
                            ISNULL(ers.TotalMark_ofSubject, 0) as FullMark,
                            ISNULL(ers.SubjectGrades, '') as SubjectGrades,
                            ISNULL(ers.SubjectPoint, 0) as SubjectPoint,
                            ISNULL(ers.PassStatus_Subject, 'Pass') as PassStatus_InSubject,
                            ISNULL(ers.IS_Add_InExam, 1) as IS_Add_InExam
                        FROM Exam_Result_of_Subject ers
                        INNER JOIN Subject sub ON ers.SubjectID = sub.SubjectID
                        WHERE ers.StudentResultID = @StudentResultID
                        AND ISNULL(ers.IS_Add_InExam, 1) = 1
                        ORDER BY ISNULL(sub.SN, 999), sub.SubjectName";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.CommandTimeout = 15; // 15 second timeout
                    cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);
                    
                    DataTable dt = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);

                    // Apply dynamic grading if needed
                    DataTable gradingSystem = GetGradingSystem();
                    foreach (DataRow row in dt.Rows)
                    {
                        string currentGrade = SafeGetString(row, "SubjectGrades");
                        decimal currentPoint = SafeGetDecimal(row, "SubjectPoint");
                        
                        if (string.IsNullOrEmpty(currentGrade) || currentPoint == 0)
                        {
                            ApplyDynamicGrading(row, gradingSystem);
                        }
                    }

                    // Log success
                    string successScript = "console.log('Simple subject data loaded - " + dt.Rows.Count + " subjects found');";
                    Page.ClientScript.RegisterStartupScript(typeof(Page), "simpleSubjectSuccess" + studentResultID, successScript, true);

                    return dt;
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.Replace("'", "\\'").Replace("\"", "\\\"");
                    string errorScript = "console.error('Simple subject data error: " + errorMsg + "');";
                    Page.ClientScript.RegisterStartupScript(typeof(Page), "simpleSubjectError" + studentResultID, errorScript, true);
                    return new DataTable();
                }
            }
        }

        private void ApplyDynamicGrading(DataRow row, DataTable gradingSystem)
        {
            try
            {
                decimal obtainedMarks = Convert.ToDecimal(row["ObtainedMark_ofSubject"]);
                decimal fullMarks = Convert.ToDecimal(row["FullMark"]);
                decimal percentage = fullMarks > 0 ? (obtainedMarks / fullMarks) * 100 : 0;

                // Find the appropriate grade
                string grade = "F";
                decimal point = 0.0m;
                string status = "Fail";

                foreach (DataRow gradeRow in gradingSystem.Rows)
                {
                    decimal maxPercentage = Convert.ToDecimal(gradeRow["MaxPercentage"]);
                    decimal minPercentage = Convert.ToDecimal(gradeRow["MinPercentage"]);

                    if (percentage >= minPercentage && percentage <= maxPercentage)
                    {
                        grade = gradeRow["Grades"].ToString();
                        point = Convert.ToDecimal(gradeRow["Point"]);
                        status = grade == "F" ? "Fail" : "Pass";
                        break;
                    }
                }

                // Update the row with calculated values
                row["SubjectGrades"] = grade;
                row["SubjectPoint"] = point;
                row["PassStatus_InSubject"] = status;
            }
            catch
            {
                // If calculation fails, use default values
                row["SubjectGrades"] = "F";
                row["SubjectPoint"] = 0.0;
                row["PassStatus_InSubject"] = "Fail";
            }
        }

        private DataTable GetGradingSystem()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    
                    // Get grading system based on school and exam
                    string query = @"
                        SELECT 
                            Grades,
                            MaxPercentage,
                            MinPercentage,
                            Point,
                            Comments
                        FROM Exam_Grading_System 
                        WHERE SchoolID = @SchoolID 
                        AND EducationYearID = @EducationYearID
                        ORDER BY MaxPercentage DESC";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"]);
                    
                    DataTable dt = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);

                    // If no custom grading system found, use default Bangladesh system
                    if (dt.Rows.Count == 0)
                    {
                        dt = GetDefaultGradingSystem();
                    }

                    return dt;
                }
                catch
                {
                    return GetDefaultGradingSystem();
                }
            }
        }

        private DataTable GetDefaultGradingSystem()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Grades", typeof(string));
            dt.Columns.Add("MaxPercentage", typeof(decimal));
            dt.Columns.Add("MinPercentage", typeof(decimal));
            dt.Columns.Add("Point", typeof(decimal));
            dt.Columns.Add("Comments", typeof(string));

            // Default Bangladesh Grading System
            dt.Rows.Add("A+", 100, 80, 5.00, "Outstanding");
            dt.Rows.Add("A", 79, 70, 4.00, "Excellent");
            dt.Rows.Add("A-", 69, 60, 3.50, "Very Good");
            dt.Rows.Add("B", 59, 50, 3.00, "Good");
            dt.Rows.Add("C", 49, 40, 2.00, "Satisfactory");
            dt.Rows.Add("D", 39, 33, 1.00, "Acceptable");
            dt.Rows.Add("F", 32, 0, 0.00, "Fail");

            return dt;
        }

        private void ApplyDynamicGrading(DataTable subjectData, DataTable gradingSystem)
        {
            foreach (DataRow row in subjectData.Rows)
            {
                try
                {
                    decimal obtainedMarks = Convert.ToDecimal(row["ObtainedMark_ofSubject"]);
                    decimal percentage = obtainedMarks; // Assuming marks are already in percentage

                    // Find the appropriate grade
                    string grade = "F";
                    decimal point = 0.0m;
                    string status = "Fail";

                    foreach (DataRow gradeRow in gradingSystem.Rows)
                    {
                        decimal maxPercentage = Convert.ToDecimal(gradeRow["MaxPercentage"]);
                        decimal minPercentage = Convert.ToDecimal(gradeRow["MinPercentage"]);

                        if (percentage >= minPercentage && percentage <= maxPercentage)
                        {
                            grade = gradeRow["Grades"].ToString();
                            point = Convert.ToDecimal(gradeRow["Point"]);
                            status = grade == "F" ? "Fail" : "Pass";
                            break;
                        }
                    }

                    // Update the row with calculated values
                    row["SubjectGrades"] = grade;
                    row["SubjectPoint"] = point;
                    row["ObtainedPoint"] = point;
                    row["PassStatus_InSubject"] = status;
                    row["PassStatus_InSubExam"] = status;
                }
                catch
                {
                    // If calculation fails, use default values
                    row["SubjectGrades"] = "F";
                    row["SubjectPoint"] = 0.0;
                    row["ObtainedPoint"] = 0.0;
                    row["PassStatus_InSubject"] = "Fail";
                    row["PassStatus_InSubExam"] = "Fail";
                }
            }
        }

        // Public method to get grading system for display in grade scale box
        public DataTable GetGradingSystemForDisplay()
        {
            return GetGradingSystem();
        }

        // Helper methods for data binding - Public methods for ASPX access
        public string GetSchoolName()
        {
            return "Imperial Ideal School & College";
        }

        public string GetSchoolAddress()
        {
            return "761,Tulatulisohera Rd,Kalulkotil, Narayangonj | Phone: 01906-265260, 01789-752002 | Idealedu8@gmail.com";
        }

        public string GetExamName()
        {
            return ExamDropDownList.SelectedItem?.Text + " - " + DateTime.Now.Year;
        }

        public string GetTotalMarks(object dataItem)
        {
            // Calculate total marks from subjects
            DataRowView row = (DataRowView)dataItem;
            return GetSubjectMarksTotal(row["StudentResultID"].ToString());
        }

        public string GetFullMarks(object dataItem)
        {
            // Calculate full marks from subjects
            DataRowView row = (DataRowView)dataItem;
            return GetSubjectFullMarksTotal(row["StudentResultID"].ToString());
        }

        public string GetResult(object dataItem)
        {
            DataRowView row = (DataRowView)dataItem;
            return row["PassStatus_ofStudent"].ToString() == "Pass" ? "উত্তীর্ণ" : "অনুত্তীর্ণ";
        }

        private string GetSubjectMarksTotal(string studentResultID)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    string query = @"
                        SELECT SUM(CAST(ISNULL(ObtainedMark_ofSubject, 0) AS FLOAT)) as TotalMarks
                        FROM Exam_Result_of_Subject 
                        WHERE StudentResultID = @StudentResultID";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);

                    object result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "0";
                }
                catch
                {
                    return "0";
                }
            }
        }

        private string GetSubjectFullMarksTotal(string studentResultID)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    string query = @"
                        SELECT SUM(CAST(ISNULL(s.FullMark, 0) AS FLOAT)) as FullMarks
                        FROM Exam_Result_of_Subject ers
                        INNER JOIN Subject s ON ers.SubjectID = s.SubjectID
                        WHERE ers.StudentResultID = @StudentResultID";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);

                    object result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "0";
                }
                catch
                {
                    return "0";
                }
            }
        }

        // Method to get result comment based on student grade
        public string GetResultComment(string studentGrade, decimal gpa)
        {
            if (string.IsNullOrEmpty(studentGrade))
                return "Good";
                
            switch (studentGrade.ToUpper())
            {
                case "A+":
                    return "Excellent";
                case "A":
                    return "Very Good";
                case "A-":
                    return "Good";
                case "B":
                    return "Satisfactory";
                case "C":
                    return "Average";
                case "D":
                    return "Below Average";
                case "F":
                    return "Fail";
                default:
                    return gpa >= 4.0m ? "Excellent" : "Good";
            }
        }

        // Method to get dynamic sub-exam names - Simplified and more effective approach
        private DataTable GetSubExamNames(string examID)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    
                    // Get current class ID from session or exam data
                    string classID = GetCurrentClassID();
                    
                    // Step 1: Get all sub-exams with their total marks for this class and exam
                    string analysisQuery = @"
                        SELECT 
                            esn.SubExamID,
                            esn.SubExamName,
                            esn.Sub_ExamSN,
                            COUNT(eom.MarksObtained) as StudentCount,
                            SUM(CASE WHEN eom.MarksObtained > 0 THEN 1 ELSE 0 END) as StudentsWithMarks,
                            AVG(CAST(eom.MarksObtained AS FLOAT)) as AvgMarks
                        FROM Exam_SubExam_Name esn
                        INNER JOIN Exam_Obtain_Marks eom ON esn.SubExamID = eom.SubExamID
                        INNER JOIN StudentsClass sc ON eom.StudentClassID = sc.StudentClassID
                        WHERE eom.ExamID = @ExamID
                        AND sc.ClassID = @ClassID
                        AND esn.SchoolID = @SchoolID
                        AND esn.EducationYearID = @EducationYearID
                        GROUP BY esn.SubExamID, esn.SubExamName, esn.Sub_ExamSN
                        ORDER BY esn.Sub_ExamSN";

                    SqlCommand cmd = new SqlCommand(analysisQuery, con);
                    cmd.CommandTimeout = 15;
                    cmd.Parameters.AddWithValue("@ExamID", examID);
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"].ToString());
                    cmd.Parameters.AddWithValue("@ClassID", classID);

                    DataTable allSubExams = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(allSubExams);

                    // Step 2: Log analysis for debugging
                    string analysisScript = "console.log('=== SUB-EXAM ANALYSIS FOR CLASS " + classID + " ===');";
                    foreach (DataRow row in allSubExams.Rows)
                    {
                        string name = SafeGetString(row, "SubExamName");
                        string studentCount = SafeGetString(row, "StudentCount");
                        string studentsWithMarks = SafeGetString(row, "StudentsWithMarks");
                        string avgMarks = Math.Round(SafeGetDecimal(row, "AvgMarks"), 1).ToString();
                        
                        analysisScript += "console.log('" + name + ": " + studentCount + " students, " + studentsWithMarks + " with marks, avg=" + avgMarks + "');";
                    }
                    Page.ClientScript.RegisterStartupScript(typeof(Page), "subExamAnalysis", analysisScript, true);

                    // Step 3: Filter meaningful sub-exams
                    DataTable meaningfulSubExams = allSubExams.Clone();
                    
                    foreach (DataRow row in allSubExams.Rows)
                    {
                        int studentsWithMarks = SafeGetString(row, "StudentsWithMarks") != "" ? Convert.ToInt32(row["StudentsWithMarks"]) : 0;
                        decimal avgMarks = SafeGetDecimal(row, "AvgMarks");
                        
                        // A sub-exam is meaningful if:
                        // 1. At least 5 students have marks > 0
                        // 2. Average mark is > 5 (not just token marks)
                        if (studentsWithMarks >= 5 && avgMarks > 5)
                        {
                            meaningfulSubExams.ImportRow(row);
                        }
                    }

                    // Step 4: If no meaningful sub-exams, take top 2 by usage
                    if (meaningfulSubExams.Rows.Count == 0)
                    {
                        string fallbackScript = "console.log('No meaningful sub-exams found, using top 2 by student count');";
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "subExamFallback", fallbackScript, true);
                        
                        for (int i = 0; i < Math.Min(2, allSubExams.Rows.Count); i++)
                        {
                            meaningfulSubExams.ImportRow(allSubExams.Rows[i]);
                        }
                    }

                    // Step 5: Limit to maximum 3 sub-exams for better layout
                    if (meaningfulSubExams.Rows.Count > 3)
                    {
                        DataTable limitedSubExams = meaningfulSubExams.Clone();
                        for (int i = 0; i < 3; i++)
                        {
                            limitedSubExams.ImportRow(meaningfulSubExams.Rows[i]);
                        }
                        meaningfulSubExams = limitedSubExams;
                        
                        string limitScript = "console.log('Limited to top 3 meaningful sub-exams');";
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "subExamLimit", limitScript, true);
                    }

                    // Step 6: Log final selection
                    if (meaningfulSubExams.Rows.Count > 0)
                    {
                        string selectedList = "";
                        foreach (DataRow row in meaningfulSubExams.Rows)
                        {
                            selectedList += SafeGetString(row, "SubExamName") + ", ";
                        }
                        selectedList = selectedList.TrimEnd(',', ' ');
                        
                        string selectionScript = "console.log('SELECTED SUB-EXAMS: " + selectedList + "');";
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "finalSubExamSelection", selectionScript, true);
                    }

                    return meaningfulSubExams;
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.Replace("'", "\\'").Replace("\"", "\\\"");
                    string errorScript = "console.error('Sub-exam analysis error: " + errorMsg + "');";
                    Page.ClientScript.RegisterStartupScript(typeof(Page), "subExamAnalysisError", errorScript, true);
                    return new DataTable();
                }
            }
        }

        // Check if exam has meaningful sub-exams - Simplified version with better logging
        private bool CheckIfExamHasSubExams(string studentResultID)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    
                    // Get current class ID and exam ID
                    string classID = GetCurrentClassID();
                    string examID = GetExamIDFromStudentResult(studentResultID);
                    
                    // Get actual sub-exam data to analyze
                    DataTable subExams = GetSubExamNames(examID);
                    
                    // Log the decision process
                    bool hasSubExams = subExams.Rows.Count > 1;
                    string decisionScript = "console.log('SUB-EXAM DECISION: " + subExams.Rows.Count + " sub-exams found, Using " + (hasSubExams ? "DETAILED" : "SIMPLE") + " table');";
                    Page.ClientScript.RegisterStartupScript(typeof(Page), "subExamDecision" + studentResultID, decisionScript, true);
                    
                    return hasSubExams;
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.Replace("'", "\\'").Replace("\"", "\\\"");
                    string errorScript = "console.error('Sub-exam decision error: " + errorMsg + "');";
                    Page.ClientScript.RegisterStartupScript(typeof(Page), "subExamDecisionError" + studentResultID, errorScript, true);
                    
                    return false; // Default to simple table on error
                }
            }
        }

        // Generate detailed subject table (with sub-exams) - Enhanced with better filtering
        private string GenerateDetailedSubjectTable(string studentResultID, string studentGrade, decimal studentPoint)
        {
            DataTable subjects = GetSubjectData(studentResultID);
            string resultComment = GetResultComment(studentGrade, studentPoint);
            
            if (subjects.Rows.Count == 0)
            {
                return "<p>No subject data found</p>";
            }
            
            // Get dynamic sub-exam names with better filtering
            string examID = GetExamIDFromStudentResult(studentResultID);
            DataTable subExams = GetSubExamNames(examID);
            
            // If we have too many sub-exams (more than 4), limit to most important ones
            if (subExams.Rows.Count > 4)
            {
                // Keep only first 2 most used sub-exams
                DataTable limitedSubExams = subExams.Clone();
                for (int i = 0; i < Math.Min(2, subExams.Rows.Count); i++)
                {
                    limitedSubExams.ImportRow(subExams.Rows[i]);
                }
                subExams = limitedSubExams;
                
                string limitScript = "console.log('Limited sub-exams to first 2 most used ones');";
                Page.ClientScript.RegisterStartupScript(typeof(Page), "limitSubExams", limitScript, true);
            }
            
            // Build dynamic header
            string subExamHeaders = "";
            if (subExams.Rows.Count > 0)
            {
                foreach (DataRow subExamRow in subExams.Rows)
                {
                    string subExamName = SafeGetString(subExamRow, "SubExamName");
                    // Shorten long sub-exam names for better display
                    if (subExamName.Length > 10)
                    {
                        if (subExamName.Contains("Midterm") || subExamName.Contains("মধ্য"))
                            subExamName = "Midterm";
                        else if (subExamName.Contains("Periodical") || subExamName.Contains("পর্যায়"))
                            subExamName = "Periodical";
                        else if (subExamName.Contains("Final") || subExamName.Contains("চূড়ান্ত"))
                            subExamName = "Final";
                        else
                            subExamName = subExamName.Substring(0, 8) + "..";
                    }
                    subExamHeaders += "<th>" + subExamName + "</th>";
                }
            }
            else
            {
                // Fallback to default headers
                subExamHeaders = "<th>Midterm</th><th>Periodical</th>";
            }
            
            string html = @"
                <table class=""marks-table"">
                    <tr>
                        <th rowspan=""2"">বিষয়সমূহ</th>
                        <th colspan=""" + Math.Max(subExams.Rows.Count, 2) + @""">প্রাপ্ত নাম্বার</th>
                        <th rowspan=""2"">মোট</th>
                        <th rowspan=""2"">গ্রেড</th>
                        <th rowspan=""2"">পয়েন্ট</th>
                        <th rowspan=""" + (subjects.Rows.Count + 2) + @""" class=""vertical-text"">" + resultComment + @"</th>
                    </tr>
                    <tr>
                        " + subExamHeaders + @"
                    </tr>";

            foreach (DataRow row in subjects.Rows)
            {
                // Safe null handling for all fields
                string subjectName = SafeGetString(row, "SubjectName");
                string obtainedMark = SafeGetString(row, "ObtainedMark_ofSubject");
                string subjectGrades = SafeGetString(row, "SubjectGrades");
                decimal subjectPoint = SafeGetDecimal(row, "SubjectPoint");
                string passStatus = SafeGetString(row, "PassStatus_InSubject");
                
                if (passStatus == "") passStatus = "Pass"; // Default to Pass if empty
                string rowClass = passStatus == "Fail" ? "failed-row" : "";
                
                // Build sub-exam marks dynamically based on filtered sub-exams
                string subExamMarks = "";
                if (subExams.Rows.Count > 0)
                {
                    // Get marks for each filtered sub-exam
                    for (int i = 1; i <= subExams.Rows.Count; i++)
                    {
                        string columnName = "SubExam" + i + "Mark";
                        string mark = SafeGetString(row, columnName);
                        if (mark == "") mark = "0";
                        subExamMarks += "<td>" + mark + "</td>";
                    }
                }
                else
                {
                    // Fallback to midterm and periodical
                    string midtermMark = SafeGetString(row, "MidtermMark");
                    string periodicalMark = SafeGetString(row, "PeriodicalMark");
                    if (midtermMark == "") midtermMark = "0";
                    if (periodicalMark == "") periodicalMark = "0";
                    subExamMarks = "<td>" + midtermMark + "</td><td>" + periodicalMark + "</td>";
                }
                
                html += @"
                    <tr class=""" + rowClass + @""">
                        <td style=""text-align: left; padding-left: 12px;"">" + subjectName + @"</td>
                        " + subExamMarks + @"
                        <td>" + obtainedMark + @"</td>
                        <td>" + subjectGrades + @"</td>
                        <td>" + subjectPoint.ToString("F1") + @"</td>
                    </tr>";
            }

            html += "</table>";
            return html;
        }

        // Helper method to get current class ID with better debugging
        private string GetCurrentClassID()
        {
            try
            {
                string classID = "";
                
                // First try to get from ClassDropDownList
                if (ClassDropDownList != null && !string.IsNullOrEmpty(ClassDropDownList.SelectedValue) && ClassDropDownList.SelectedValue != "0")
                {
                    classID = ClassDropDownList.SelectedValue;
                    string dropdownScript = "console.log('ClassID from DropDown: " + classID + "');";
                    Page.ClientScript.RegisterStartupScript(typeof(Page), "classIDFromDropdown", dropdownScript, true);
                    return classID;
                }
                
                // Fallback to session if available
                if (Session["ClassID"] != null)
                {
                    classID = Session["ClassID"].ToString();
                    string sessionScript = "console.log('ClassID from Session: " + classID + "');";
                    Page.ClientScript.RegisterStartupScript(typeof(Page), "classIDFromSession", sessionScript, true);
                    return classID;
                }
                
                // Default fallback
                classID = "1";
                string fallbackScript = "console.log('ClassID fallback to default: " + classID + "');";
                Page.ClientScript.RegisterStartupScript(typeof(Page), "classIDFallback", fallbackScript, true);
                return classID;
            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message.Replace("'", "\\'").Replace("\"", "\\\"");
                string errorScript = "console.error('GetCurrentClassID error: " + errorMsg + "');";
                Page.ClientScript.RegisterStartupScript(typeof(Page), "classIDError", errorScript, true);
                return "1";
            }
        }

        // Helper method to get ExamID from StudentResultID - Missing method
        private string GetExamIDFromStudentResult(string studentResultID)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    string query = "SELECT ExamID FROM Exam_Result_of_Student WHERE StudentResultID = @StudentResultID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.CommandTimeout = 15;
                    cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);
                    
                    object result = cmd.ExecuteScalar();
                    string examID = result != null && result != DBNull.Value ? result.ToString() : "";
                    
                    if (!string.IsNullOrEmpty(examID))
                    {
                        string debugScript = "console.log('ExamID from StudentResult: " + examID + "');";
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "examIDFromResult", debugScript, true);
                    }
                    
                    return examID;
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.Replace("'", "\\'").Replace("\"", "\\\"");
                    string errorScript = "console.error('GetExamIDFromStudentResult error: " + errorMsg + "');";
                    Page.ClientScript.RegisterStartupScript(typeof(Page), "examIDError", errorScript, true);
                    return "";
                }
            }
        }

        // Public method called from ASPX for generating subject marks table
        public string GenerateSubjectMarksTable(string studentResultID, string studentGrade, decimal studentPoint)
        {
            try
            {
                // Check if exam has meaningful sub-exams
                bool hasSubExams = CheckIfExamHasSubExams(studentResultID);
                
                if (hasSubExams)
                {
                    return GenerateDetailedSubjectTable(studentResultID, studentGrade, studentPoint);
                }
                else
                {
                    return GenerateSimpleSubjectTable(studentResultID, studentGrade, studentPoint);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message.Replace("'", "\\'").Replace("\"", "\\\"");
                string errorScript = "console.error('GenerateSubjectMarksTable error: " + errorMsg + "');";
                Page.ClientScript.RegisterStartupScript(typeof(Page), "subjectTableError", errorScript, true);
                return "<p>Error loading subject table</p>";
            }
        }

        // Generate simple subject table (without sub-exams)
        private string GenerateSimpleSubjectTable(string studentResultID, string studentGrade, decimal studentPoint)
        {
            DataTable subjects = GetSubjectDataSimple(studentResultID);
            string resultComment = GetResultComment(studentGrade, studentPoint);
            
            if (subjects.Rows.Count == 0)
            {
                return "<p>No subject data found</p>";
            }
            
            string html = @"
                <div class=""marks-heading"">বিষয়ভিত্তিক ফলাফল</div>
                <table class=""marks-table"">
                    <tr>
                        <th>বিষয়সমূহ</th>
                        <th>প্রাপ্ত নাম্বার</th>
                        <th>পূর্ণ নাম্বার</th>
                        <th>গ্রেড</th>
                        <th>পয়েন্ট</th>
                        <th rowspan=""" + (subjects.Rows.Count + 1) + @""" class=""vertical-text"">" + resultComment + @"</th>
                    </tr>";

            foreach (DataRow row in subjects.Rows)
            {
                string subjectName = SafeGetString(row, "SubjectName");
                string obtainedMark = SafeGetString(row, "ObtainedMark_ofSubject");
                string fullMark = SafeGetString(row, "FullMark");
                string subjectGrades = SafeGetString(row, "SubjectGrades");
                decimal subjectPoint = SafeGetDecimal(row, "SubjectPoint");
                string passStatus = SafeGetString(row, "PassStatus_InSubject");
                
                if (passStatus == "") passStatus = "Pass";
                string rowClass = passStatus == "Fail" ? "failed-row" : "";
                
                html += @"
                    <tr class=""" + rowClass + @""">
                        <td style=""text-align: left; padding-left: 12px;"">" + subjectName + @"</td>
                        <td>" + obtainedMark + @"</td>
                        <td>" + fullMark + @"</td>
                        <td>" + subjectGrades + @"</td>
                        <td>" + subjectPoint.ToString("F1") + @"</td>
                    </tr>";
            }

            html += "</table>";
            return html;
        }

        // Helper methods for safe data access
        private string SafeGetString(DataRow row, string columnName)
        {
            if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                return row[columnName].ToString();
            return string.Empty;
        }

        private decimal SafeGetDecimal(DataRow row, string columnName)
        {
            if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
            {
                decimal value;
                if (decimal.TryParse(row[columnName].ToString(), out value))
                    return value;
            }
            return 0m;
        }
    }
}
