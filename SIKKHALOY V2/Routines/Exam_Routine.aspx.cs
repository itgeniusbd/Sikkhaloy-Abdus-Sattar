using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Newtonsoft.Json;

namespace EDUCATION.COM.Routines
{
    public partial class Exam_Routine_Bangla : System.Web.UI.Page
    {
        // Properties to track counts
        private int ClassColumnCount
        {
            get { return ViewState["ClassColumnCount"] != null ? (int)ViewState["ClassColumnCount"] : 1; }
            set { ViewState["ClassColumnCount"] = value; }
        }

        private int RowCount
        {
            get { return ViewState["RowCount"] != null ? (int)ViewState["RowCount"] : 1; }
            set { ViewState["RowCount"] = value; }
        }

        // Store selected class IDs
        private Dictionary<int, int> SelectedClassIds
        {
            get
            {
                if (ViewState["SelectedClassIds"] == null)
                    ViewState["SelectedClassIds"] = new Dictionary<int, int>();
                return (Dictionary<int, int>)ViewState["SelectedClassIds"];
            }
            set { ViewState["SelectedClassIds"] = value; }
        }

        // Store loaded cell data for pre-selecting during ItemDataBound
        private Dictionary<string, CellData> LoadedCellData
        {
            get
            {
                if (ViewState["LoadedCellData"] == null)
                    ViewState["LoadedCellData"] = new Dictionary<string, CellData>();
                return (Dictionary<string, CellData>)ViewState["LoadedCellData"];
            }
            set { ViewState["LoadedCellData"] = value; }
        }

        // Helper class to store cell data
        [Serializable]
        public class CellData
        {
            public int SubjectID { get; set; }
            public string SubjectText { get; set; }
            public string TimeText { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // **CRITICAL FIX: Save class values FIRST on every postback**
            if (IsPostBack)
            {
                SaveSelectedClassValues();
            }

            if (!IsPostBack)
            {
                ClassColumnCount = 1;
                RowCount = 1;

                // **NEW: Set default class selection for first column**
                // This ensures subject dropdown appears on first load
                SelectedClassIds[1] = 0; // Will be replaced when user selects a class

                LoadDefaultRoutine();
                GenerateClassHeaders();

            }
            else
            {
                // Restore from hidden fields if available
                if (!string.IsNullOrEmpty(ClassColumnCountHF.Value))
                {
                    int tempClassCount;
                    if (int.TryParse(ClassColumnCountHF.Value, out tempClassCount))
                    {
                        ClassColumnCount = tempClassCount;
                    }
                }

                if (!string.IsNullOrEmpty(RowCountHF.Value))
                {
                    int tempRowCount;
                    if (int.TryParse(RowCountHF.Value, out tempRowCount))
                    {
                        RowCount = tempRowCount;
                    }
                }

                // Regenerate headers after restoring state
                GenerateClassHeaders();
            }

            // Update labels
            ClassColumnCountLabel.Text = "বর্তমান: " + ClassColumnCount;
            RowCountLabel.Text = "বর্তমান: " + RowCount;
        }

        private void SaveSelectedClassValues()
        {
            // Clear existing
            SelectedClassIds.Clear();

            // Read from form data
            for (int i = 1; i <= ClassColumnCount; i++)
            {
                string dropdownName = $"ClassDropdown{i}";
                string selectedValue = Request.Form[dropdownName];

                if (!string.IsNullOrEmpty(selectedValue))
                {
                    int classId;
                    if (int.TryParse(selectedValue, out classId) && classId > 0)
                    {
                        SelectedClassIds[i] = classId;
                    }
                }
            }
        }

        private void GenerateClassHeaders()
        {
            StringBuilder headerHtml = new StringBuilder();

            // Load available classes from database
            string connString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            DataTable classesTable = new DataTable();

            using (SqlConnection con = new SqlConnection(connString))
            {
                string query = "SELECT ClassID, Class FROM CreateClass WHERE SchoolID = @SchoolID ORDER BY SN";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(classesTable);
            }

            // Generate header columns
            for (int i = 1; i <= ClassColumnCount; i++)
            {
                headerHtml.Append("<th class='period-header'>");

                // Get previously selected value to show class name
                int selectedValue = 0;
                string selectedClassName = "";
                if (SelectedClassIds.ContainsKey(i))
                {
                    selectedValue = SelectedClassIds[i];
                    // Find class name
                    foreach (DataRow row in classesTable.Rows)
                    {
                        if (selectedValue > 0 && selectedValue.ToString() == row["ClassID"].ToString())
                        {
                            selectedClassName = row["Class"].ToString();
                            break;
                        }
                    }
                }

                // **FIXED: Always show "শ্রেণী" label with class name**
                headerHtml.Append("<div style='display: flex; align-items: center; justify-content: center; gap: 10px; flex-wrap: wrap;'>");

                // Show "শ্রেণী: ClassName" or just "শ্রেণী" if no class selected
                if (!string.IsNullOrEmpty(selectedClassName))
                {
                    headerHtml.Append("<span style='font-weight: bold;'>শ্রেণী: " + selectedClassName + "</span>");
                }
                else
                {
                    headerHtml.Append("<span style='font-weight: bold;'>শ্রেণী</span>");
                }

                headerHtml.Append("<select id='ClassDropdown" + i + "' name='ClassDropdown" + i + "' class='form-control-routine class-dropdown' style='width: auto; min-width: 150px;'>");
                headerHtml.Append("<option value=''>নির্বাচন করুন</option>");

                foreach (DataRow row in classesTable.Rows)
                {
                    string selected = "";
                    if (selectedValue > 0 && selectedValue.ToString() == row["ClassID"].ToString())
                    {
                        selected = " selected='selected'";
                    }
                    headerHtml.Append("<option value='" + row["ClassID"] + "'" + selected + ">" + row["Class"] + "</option>");
                }

                headerHtml.Append("</select>");
                headerHtml.Append("</div>"); // Close flex container

                headerHtml.Append("</th>");
            }

            // Set to Literal control instead of placeholder
            ClassHeaderLiteral.Text = headerHtml.ToString();
        }

        private string GetBengaliDayName(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Saturday: return "শনিবার";
                case DayOfWeek.Sunday: return "রবিবার";
                case DayOfWeek.Monday: return "সোমবার";
                case DayOfWeek.Tuesday: return "মঙ্গলবার";
                case DayOfWeek.Wednesday: return "বুধবার";
                case DayOfWeek.Thursday: return "বৃহস্পতিবার";
                case DayOfWeek.Friday: return "শুক্রবার";
                default: return "";
            }
        }

        private void LoadDefaultRoutine()
        {
            DataTable dtRows = new DataTable();
            dtRows.Columns.Add("RowIndex", typeof(int));
            dtRows.Columns.Add("ExamDate", typeof(string));  // **CHANGED: Use string to control format**
            dtRows.Columns.Add("DayName", typeof(string));
            dtRows.Columns.Add("StartTime", typeof(string));
            dtRows.Columns.Add("EndTime", typeof(string));
            dtRows.Columns.Add("Duration", typeof(string));
            dtRows.Columns.Add("ExamTime", typeof(string));
            dtRows.Columns.Add("RoutineID", typeof(int));

            DateTime startDate = DateTime.Today;

            for (int i = 0; i < RowCount; i++)
            {
                DataRow newRow = dtRows.NewRow();
                DateTime currentDate = startDate.AddDays(i);

                newRow["RowIndex"] = i;
                // **CRITICAL: Format date as dd/MM/yyyy string**
                newRow["ExamDate"] = currentDate.ToString("dd/MM/yyyy");
                newRow["DayName"] = GetBengaliDayName(currentDate.DayOfWeek);
                newRow["StartTime"] = "10:00 AM";
                newRow["EndTime"] = "01:00 PM";
                newRow["Duration"] = "৩ ঘন্টা";
                newRow["ExamTime"] = "10:00 AM";
                newRow["RoutineID"] = 0;
                dtRows.Rows.Add(newRow);
            }

            RoutineRepeater.DataSource = dtRows;
            RoutineRepeater.DataBind();
        }

        private void LoadRoutineFromDatabase(int routineId)
        {
            string connString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connString))
            {
                con.Open();

                // 1. Load main routine data
                string mainQuery = @"
     SELECT * FROM Exam_Routine_SavedData 
  WHERE RoutineID = @RoutineID AND SchoolID = @SchoolID";

                SqlCommand cmdMain = new SqlCommand(mainQuery, con);
                cmdMain.Parameters.AddWithValue("@RoutineID", routineId);
                cmdMain.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 0);

                SqlDataReader reader = cmdMain.ExecuteReader();

                if (reader.Read())
                {
                    // Load metadata
                    ClassColumnCount = Convert.ToInt32(reader["ClassColumnCount"]);
                    RowCount = Convert.ToInt32(reader["RowCount"]);

                    ClassColumnCountHF.Value = ClassColumnCount.ToString();
                    RowCountHF.Value = RowCount.ToString();

                    // Set LoadedRoutineIdHF to mark routine as loaded
                    LoadedRoutineIdHF.Value = routineId.ToString();

                    // Set Routine Name
                    string routineName = reader["RoutineName"].ToString();
                    RoutineNameLabel.Text = routineName;
                    RoutineNameTextBox.Text = routineName;
                }
                reader.Close();

                // 2. Load class columns
                string classQuery = @"
        SELECT ColumnIndex, ClassID 
        FROM Exam_Routine_ClassColumns 
          WHERE RoutineID = @RoutineID 
     ORDER BY ColumnIndex";

                SqlCommand cmdClasses = new SqlCommand(classQuery, con);
                cmdClasses.Parameters.AddWithValue("@RoutineID", routineId);

                SelectedClassIds.Clear();
                SqlDataReader classReader = cmdClasses.ExecuteReader();
                while (classReader.Read())
                {
                    int columnIndex = Convert.ToInt32(classReader["ColumnIndex"]);
                    int classId = Convert.ToInt32(classReader["ClassID"]);
                    SelectedClassIds[columnIndex] = classId;
                }
                classReader.Close();

                // 3. Load row data (dates) - FIXED: Proper date formatting
                string rowQuery = @"
  SELECT RowIndex, ExamDate, DayName, ExamTime 
 FROM Exam_Routine_Rows 
   WHERE RoutineID = @RoutineID 
ORDER BY RowIndex";

                SqlCommand cmdRows = new SqlCommand(rowQuery, con);
                cmdRows.Parameters.AddWithValue("@RoutineID", routineId);

                DataTable dtRows = new DataTable();
                dtRows.Columns.Add("RowIndex", typeof(int));
                dtRows.Columns.Add("ExamDate", typeof(string));  // **CHANGED: Use string to control format**
                dtRows.Columns.Add("DayName", typeof(string));
                dtRows.Columns.Add("StartTime", typeof(string));  // NEW
                dtRows.Columns.Add("EndTime", typeof(string));    // NEW
                dtRows.Columns.Add("Duration", typeof(string));   // NEW
                dtRows.Columns.Add("ExamTime", typeof(string));// LEGACY
                dtRows.Columns.Add("RoutineID", typeof(int));

                SqlDataReader rowReader = cmdRows.ExecuteReader();

                // **CRITICAL FIX: Keep track of saved dates to fill missing ones sequentially**
                var savedDates = new Dictionary<int, DateTime?>();
                while (rowReader.Read())
                {
                    int rowIndex = Convert.ToInt32(rowReader["RowIndex"]);
                    DateTime? examDate = null;

                    if (rowReader["ExamDate"] != DBNull.Value)
                    {
                        examDate = Convert.ToDateTime(rowReader["ExamDate"]);
                    }

                    savedDates[rowIndex] = examDate;
                }
                rowReader.Close();
                

                // **NEW: Fill in missing dates sequentially from current date**
                DateTime currentDate = DateTime.Today;
                for (int i = 0; i < RowCount; i++)
                {
                    if (!savedDates.ContainsKey(i) || !savedDates[i].HasValue)
                    {
                        savedDates[i] = currentDate;
                        currentDate = currentDate.AddDays(1);
                    }
                    else
                    {
                        currentDate = savedDates[i].Value.AddDays(1);
                    }
                }

                // **NEW: Re-read and create rows with proper dates**
                cmdRows = new SqlCommand(rowQuery, con);
                cmdRows.Parameters.AddWithValue("@RoutineID", routineId);
                rowReader = cmdRows.ExecuteReader();

                var rowData = new Dictionary<int, Tuple<string, string>>();
                while (rowReader.Read())
                {
                    int rowIndex = Convert.ToInt32(rowReader["RowIndex"]);
                    string dayName = rowReader["DayName"].ToString();
                    string examTime = rowReader["ExamTime"].ToString();
                    rowData[rowIndex] = new Tuple<string, string>(dayName, examTime);
                }
                rowReader.Close();

                // **CRITICAL: Create DataTable with all rows**
                for (int i = 0; i < RowCount; i++)
                {
                    DataRow newRow = dtRows.NewRow();
                    newRow["RowIndex"] = i;
                    newRow["RoutineID"] = routineId;

                    // Set date - **CRITICAL: Format as dd/MM/yyyy string**
                    if (savedDates.ContainsKey(i) && savedDates[i].HasValue)
                    {
                        newRow["ExamDate"] = savedDates[i].Value.ToString("dd/MM/yyyy");
                        newRow["DayName"] = GetBengaliDayName(savedDates[i].Value.DayOfWeek);
                    }
                    else
                    {
                        newRow["ExamDate"] = "";
                        newRow["DayName"] = "";
                    }

                    // Set time
                    if (rowData.ContainsKey(i))
                    {
                        newRow["DayName"] = rowData[i].Item1; // Override with saved day name
                        newRow["ExamTime"] = rowData[i].Item2;

                        // Parse ExamTime to get StartTime and EndTime
                        string examTimeStr = rowData[i].Item2;
                        if (!string.IsNullOrEmpty(examTimeStr) && examTimeStr.Contains("-"))
                        {
                            var timeParts = examTimeStr.Split(new[] { '-' }, 2);
                            newRow["StartTime"] = timeParts[0].Trim();
                            newRow["EndTime"] = timeParts.Length > 1 ? timeParts[1].Trim() : "01:00 PM";
                        }
                        else
                        {
                            newRow["StartTime"] = "10:00 AM";
                            newRow["EndTime"] = "01:00 PM";
                        }
                    }
                    else
                    {
                        newRow["ExamTime"] = "10:00 AM - 01:00 PM";
                        newRow["StartTime"] = "10:00 AM";
                        newRow["EndTime"] = "01:00 PM";
                    }

                    newRow["Duration"] = "৩ ঘন্টা";
                    dtRows.Rows.Add(newRow);
                }

                // 4. Load cell data
                string cellQuery = @"
          SELECT RowIndex, ColumnIndex, SubjectID, SubjectText, TimeText 
         FROM Exam_Routine_CellData 
 WHERE RoutineID = @RoutineID";

                SqlCommand cmdCells = new SqlCommand(cellQuery, con);
                cmdCells.Parameters.AddWithValue("@RoutineID", routineId);

                // Clear previous loaded cell data
                LoadedCellData.Clear();

                SqlDataReader cellReader = cmdCells.ExecuteReader();
                while (cellReader.Read())
                {
                    int rowIndex = Convert.ToInt32(cellReader["RowIndex"]);
                    int columnIndex = Convert.ToInt32(cellReader["ColumnIndex"]);

                    // Store in dictionary with key "rowIndex_columnIndex"
                    string cellKey = $"{rowIndex}_{columnIndex}";
                    LoadedCellData[cellKey] = new CellData
                    {
                        SubjectID = cellReader["SubjectID"] != DBNull.Value ? Convert.ToInt32(cellReader["SubjectID"]) : 0,
                        SubjectText = cellReader["SubjectText"]?.ToString() ?? "",
                        TimeText = cellReader["TimeText"]?.ToString() ?? ""
                    };
                }
                cellReader.Close();

                // Generate UI
                GenerateClassHeaders();
                RoutineRepeater.DataSource = dtRows;
                RoutineRepeater.DataBind();

                // Update labels
                ClassColumnCountLabel.Text = "বরতমান: " + ClassColumnCount;
                RowCountLabel.Text = "বরতমান: " + RowCount;

                // Force UpdatePanel refresh
                MainUpdatePanel.Update();
            }
        }

        protected void RoutineRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                StringBuilder columnsHtml = new StringBuilder();
                Literal classColumnsLiteral = (Literal)e.Item.FindControl("ClassColumnsLiteral");

                // Generate columns for each class
                for (int i = 1; i <= ClassColumnCount; i++)
                {
                    int selectedClassId = 0;
                    if (SelectedClassIds.ContainsKey(i))
                    {
                        selectedClassId = SelectedClassIds[i];
                    }

                    columnsHtml.Append("<td class='editable-cell'>");

                    // **CRITICAL FIX: Always generate dropdown HTML, even if no previous selection**

                    // Check if we have loaded cell data for this cell
                    string cellKey = $"{e.Item.ItemIndex}_{i}";
                    CellData loadedCell = null;
                    if (LoadedCellData.ContainsKey(cellKey))
                    {
                        loadedCell = LoadedCellData[cellKey];
                    }

                    // Get previously selected subject from form or loaded data
                    string selectedSubjectText = "";
                    int previouslySelectedSubjectId = 0;
                    string subjectFieldName = "Subject" + i + "Dropdown_" + e.Item.ItemIndex;
                    string previouslySelected = Request.Form[subjectFieldName];

                    // Priority: Form data > Loaded data
                    if (!string.IsNullOrEmpty(previouslySelected) && int.TryParse(previouslySelected, out int subId) && subId > 0)
                    {
                        previouslySelectedSubjectId = subId;
                        if (selectedClassId > 0)
                        {
                            DataTable subjectsTable = GetSubjectsForClass(selectedClassId);
                            foreach (DataRow row in subjectsTable.Rows)
                            {
                                if (row["SubjectID"].ToString() == previouslySelected)
                                {
                                    selectedSubjectText = row["SubjectName"].ToString();
                                    break;
                                }
                            }
                        }
                    }
                    else if (loadedCell != null && loadedCell.SubjectID > 0)
                    {
                        // Use loaded data
                        previouslySelectedSubjectId = loadedCell.SubjectID;
                        if (selectedClassId > 0)
                        {
                            DataTable subjectsTable = GetSubjectsForClass(selectedClassId);
                            foreach (DataRow row in subjectsTable.Rows)
                            {
                                if (row["SubjectID"].ToString() == loadedCell.SubjectID.ToString())
                                {
                                    selectedSubjectText = row["SubjectName"].ToString();
                                    break;
                                }
                            }
                        }
                    }

                    // **ALWAYS generate dropdown - removed the if condition**

                    // Subject container with data attribute for print
                    columnsHtml.Append("<div class='subject-name' data-subject='" + selectedSubjectText + "'>");

                    // **CRITICAL: Subject dropdown generation**
                    string dropdownId = $"Subject{i}Dropdown_{e.Item.ItemIndex}";
                    columnsHtml.Append($"<select id='{dropdownId}' name='{dropdownId}' class='form-control-routine' style='margin-bottom: 5px;'>");
                    columnsHtml.Append("<option value=''>বিষয় নির্বাচন করুন</option>");

                    // Load subjects if class is selected
                    if (selectedClassId > 0)
                    {
                        DataTable subjectsTable = GetSubjectsForClass(selectedClassId);
                        foreach (DataRow row in subjectsTable.Rows)
                        {
                            string selected = "";
                            // Check if this subject should be selected
                            if (previouslySelectedSubjectId > 0 && previouslySelectedSubjectId.ToString() == row["SubjectID"].ToString())
                            {
                                selected = " selected='selected'";
                            }
                            columnsHtml.Append("<option value='" + row["SubjectID"] + "'" + selected + ">" + row["SubjectName"] + "</option>");
                        }
                    }
                    columnsHtml.Append("</select>");

                    // Subject textbox
                    string textboxFieldName = "Subject" + i + "TextBox_" + e.Item.ItemIndex;
                    string textboxValue = Request.Form[textboxFieldName] ?? "";

                    // Use loaded data if no form data
                    if (string.IsNullOrEmpty(textboxValue) && loadedCell != null)
                    {
                        textboxValue = loadedCell.SubjectText ?? "";
                    }

                    columnsHtml.Append("<input type='text' id='Subject" + i + "TextBox_" + e.Item.ItemIndex + "' name='Subject" + i + "TextBox_" + e.Item.ItemIndex + "' class='form-control-routine subject-textbox' value='" + textboxValue + "' placeholder='অতিরিক্ত তথ্য' />");

                    // Display selected subject text (visible on screen & print)
                    if (!string.IsNullOrEmpty(selectedSubjectText))
                    {
                        columnsHtml.Append("<span class='subject-display-text'>" + selectedSubjectText + "</span>");
                    }

                    columnsHtml.Append("</div>");

                    // **NEW: Add time textbox (without time picker) for manual entry**
                    string timeFieldName = "Time" + i + "TextBox_" + e.Item.ItemIndex;
                    string timeValue = Request.Form[timeFieldName] ?? "";

                    // Use loaded data if no form data
                    if (string.IsNullOrEmpty(timeValue) && loadedCell != null)
                    {
                        timeValue = loadedCell.TimeText ?? "";
                    }

                    columnsHtml.Append("<div class='subject-time-manual'>");
                    columnsHtml.Append("<input type='text' id='Time" + i + "TextBox_" + e.Item.ItemIndex + "' name='Time" + i + "TextBox_" + e.Item.ItemIndex + "' class='form-control-routine time-manual-input' value='" + timeValue + "' placeholder='সময় (যেমন: 8:20 PM)' />");
                    columnsHtml.Append("</div>");

                    columnsHtml.Append("</td>");
                }

                // **CRITICAL: Set the literal text**
                string finalHtml = columnsHtml.ToString();
                classColumnsLiteral.Text = finalHtml;
            }
        }

        protected void AddClassColumnButton_Click(object sender, EventArgs e)
        {
            ClassColumnCount++;
            ClassColumnCountHF.Value = ClassColumnCount.ToString();
            GenerateClassHeaders();
            LoadDefaultRoutine();
        }

        protected void RemoveClassColumnButton_Click(object sender, EventArgs e)
        {
            if (ClassColumnCount > 1)
            {
                ClassColumnCount--;
                ClassColumnCountHF.Value = ClassColumnCount.ToString();

                // Remove from selected classes if exists
                if (SelectedClassIds.ContainsKey(ClassColumnCount + 1))
                {
                    SelectedClassIds.Remove(ClassColumnCount + 1);
                }

                GenerateClassHeaders();
                LoadDefaultRoutine();
            }
        }

        protected void AddRowButton_Click(object sender, EventArgs e)
        {
            RowCount++;
            RowCountHF.Value = RowCount.ToString();
            LoadDefaultRoutine();
        }

        protected void RemoveRowButton_Click(object sender, EventArgs e)
        {
            if (RowCount > 1)
            {
                RowCount--;
                RowCountHF.Value = RowCount.ToString();
                LoadDefaultRoutine();
            }
        }

        private DataTable GetSubjectsForClass(int classId)
        {
            string connString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connString))
            {
                string query = @"
     SELECT DISTINCT s.SubjectID, s.SubjectName 
   FROM Subject s
   INNER JOIN SubjectForGroup sfg ON s.SubjectID = sfg.SubjectID
WHERE sfg.ClassID = @ClassID AND sfg.SchoolID = @SchoolID
  ORDER BY s.SubjectName";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ClassID", classId);
                cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 0);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            return dt;
        }

        private void LoadRoutineDropdown()
        {
            string connString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connString))
            {
                string query = @"
 SELECT RoutineID, RoutineName 
          FROM Exam_Routine_SavedData 
           WHERE SchoolID = @SchoolID 
    ORDER BY RoutineName";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                RoutineListDropDown.DataSourceID = null; // Clear DataSourceID
                RoutineListDropDown.DataSource = dt;
                RoutineListDropDown.DataTextField = "RoutineName";
                RoutineListDropDown.DataValueField = "RoutineID";
                RoutineListDropDown.DataBind();

                RoutineListDropDown.Items.Insert(0, new ListItem("-- নতুন রুটিন --", "0"));
            }
        }

        protected void LoadRoutineButton_Click(object sender, EventArgs e)
        {
            int routineId;
            if (int.TryParse(RoutineListDropDown.SelectedValue, out routineId) && routineId > 0)
            {
                LoadRoutineFromDatabase(routineId);
            }
        }

        protected void SaveRoutineButton_Click(object sender, EventArgs e)
        {
            string routineName = RoutineNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(routineName))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('দয়া করে রুটিনের nome লিখুন');", true);
                return;
            }

            // Check if this is an UPDATE or INSERT
            int loadedRoutineId = 0;
            if (!string.IsNullOrEmpty(LoadedRoutineIdHF.Value))
            {
                int.TryParse(LoadedRoutineIdHF.Value, out loadedRoutineId);
            }

            if (loadedRoutineId > 0)
            {
                // UPDATE existing routine
                UpdateRoutineInDatabase(loadedRoutineId, routineName);
            }
            else
            {
                // INSERT new routine
                SaveRoutineToDatabase(routineName);
            }
        }

        private void SaveRoutineToDatabase(string routineName)
        {
            string connString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connString))
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();

                try
                {
                    // 1. Insert main routine data with [RowCount]
                    string mainInsert = @"
INSERT INTO Exam_Routine_SavedData (RoutineName, ClassColumnCount, [RowCount], SchoolID, CreatedDate)
    VALUES (@RoutineName, @ClassColumnCount, @RowCount, @SchoolID, GETDATE());
SELECT SCOPE_IDENTITY();";

                    SqlCommand cmdMain = new SqlCommand(mainInsert, con, transaction);
                    cmdMain.Parameters.AddWithValue("@RoutineName", routineName);
                    cmdMain.Parameters.AddWithValue("@ClassColumnCount", ClassColumnCount);
                    cmdMain.Parameters.AddWithValue("@RowCount", RowCount);
                    cmdMain.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);

                    int newRoutineId = Convert.ToInt32(cmdMain.ExecuteScalar());

                    // 2. Insert class columns
                    foreach (var kvp in SelectedClassIds)
                    {
                        string classInsert = @"
INSERT INTO Exam_Routine_ClassColumns (RoutineID, ColumnIndex, ClassID)
  VALUES (@RoutineID, @ColumnIndex, @ClassID)";

                        SqlCommand cmdClass = new SqlCommand(classInsert, con, transaction);
                        cmdClass.Parameters.AddWithValue("@RoutineID", newRoutineId);
                        cmdClass.Parameters.AddWithValue("@ColumnIndex", kvp.Key);
                        cmdClass.Parameters.AddWithValue("@ClassID", kvp.Value);
                        cmdClass.ExecuteNonQuery();
                    }

                    // 3. Insert row data
                    foreach (RepeaterItem item in RoutineRepeater.Items)
                    {
                        if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                        {
                            TextBox examDateTextBox = (TextBox)item.FindControl("ExamDateTextBox");
                            TextBox dayNameTextBox = (TextBox)item.FindControl("DayNameTextBox");
                            TextBox startTimeTextBox = (TextBox)item.FindControl("StartTimeTextBox");
                            TextBox endTimeTextBox = (TextBox)item.FindControl("EndTimeTextBox");

                            string examDateStr = examDateTextBox?.Text ?? "";
                            string dayName = dayNameTextBox?.Text ?? "";
                            string startTime = startTimeTextBox?.Text ?? "";
                            string endTime = endTimeTextBox?.Text ?? "";

                            // Combine start and end time for ExamTime field (backward compatibility)
                            string examTime = string.IsNullOrEmpty(startTime) && string.IsNullOrEmpty(endTime)
                     ? ""
                       : $"{startTime} - {endTime}";

                            DateTime? examDate = null;
                            if (!string.IsNullOrEmpty(examDateStr))
                            {
                                // **CRITICAL: Parse date from dd/MM/yyyy format**
                                if (DateTime.TryParseExact(examDateStr, "dd/MM/yyyy",
                                 System.Globalization.CultureInfo.InvariantCulture,
                                      System.Globalization.DateTimeStyles.None, out DateTime tempDate))
                                {
                                    examDate = tempDate;
                                }
                                else if (DateTime.TryParse(examDateStr, out tempDate))
                                {
                                    examDate = tempDate;
                                }
                            }

                            string rowInsert = @"
     INSERT INTO Exam_Routine_Rows (RoutineID, RowIndex, ExamDate, DayName, ExamTime)
     VALUES (@RoutineID, @RowIndex, @ExamDate, @DayName, @ExamTime)";

                            SqlCommand cmdRow = new SqlCommand(rowInsert, con, transaction);
                            cmdRow.Parameters.AddWithValue("@RoutineID", newRoutineId);
                            cmdRow.Parameters.AddWithValue("@RowIndex", item.ItemIndex);
                            cmdRow.Parameters.AddWithValue("@ExamDate", (object)examDate ?? DBNull.Value);
                            cmdRow.Parameters.AddWithValue("@DayName", dayName);
                            cmdRow.Parameters.AddWithValue("@ExamTime", examTime);
                            cmdRow.ExecuteNonQuery();
                        }
                    }

                    // 4. Insert cell data
                    foreach (RepeaterItem item in RoutineRepeater.Items)
                    {
                        if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                        {
                            for (int colIndex = 1; colIndex <= ClassColumnCount; colIndex++)
                            {
                                string subjectDropdownName = "Subject" + colIndex + "Dropdown_" + item.ItemIndex;
                                string subjectTextboxName = "Subject" + colIndex + "TextBox_" + item.ItemIndex;
                                string timeTextboxName = "Time" + colIndex + "TextBox_" + item.ItemIndex;

                                string subjectIdStr = Request.Form[subjectDropdownName];
                                string subjectText = Request.Form[subjectTextboxName] ?? "";
                                string timeText = Request.Form[timeTextboxName] ?? "";

                                int subjectId = 0;
                                if (!string.IsNullOrEmpty(subjectIdStr))
                                {
                                    int.TryParse(subjectIdStr, out subjectId);
                                }

                                string cellInsert = @"
    INSERT INTO Exam_Routine_CellData (RoutineID, RowIndex, ColumnIndex, SubjectID, SubjectText, TimeText)
  VALUES (@RoutineID, @RowIndex, @ColumnIndex, @SubjectID, @SubjectText, @TimeText)";

                                SqlCommand cmdCell = new SqlCommand(cellInsert, con, transaction);
                                cmdCell.Parameters.AddWithValue("@RoutineID", newRoutineId);
                                cmdCell.Parameters.AddWithValue("@RowIndex", item.ItemIndex);
                                cmdCell.Parameters.AddWithValue("@ColumnIndex", colIndex);
                                cmdCell.Parameters.AddWithValue("@SubjectID", subjectId > 0 ? (object)subjectId : DBNull.Value);
                                cmdCell.Parameters.AddWithValue("@SubjectText", subjectText);
                                cmdCell.Parameters.AddWithValue("@TimeText", timeText);
                                cmdCell.ExecuteNonQuery();
                            }
                        }
                    }

                    transaction.Commit();

                    // Update UI
                    LoadedRoutineIdHF.Value = newRoutineId.ToString();
                    RoutineNameLabel.Text = routineName;
                    LoadRoutineDropdown();

                    ScriptManager.RegisterStartupScript(this, GetType(), "success", "alert('রুটিন সফলভাবে সংরক্ষণ করা হয়েছে');", true);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    string safeMessage = ex.Message.Replace("'", "\\'").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ");
                    ScriptManager.RegisterStartupScript(this, GetType(), "error", $"alert('Error: {safeMessage}');", true);
                }
            }
        }

        private void UpdateRoutineInDatabase(int routineId, string routineName)
        {
            string connString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connString))
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();

                try
                {
                    // 1. Update main routine data with [RowCount]
                    string mainUpdate = @"
UPDATE Exam_Routine_SavedData 
SET RoutineName = @RoutineName, 
    ClassColumnCount = @ClassColumnCount, 
    [RowCount] = @RowCount,
    ModifiedDate = GETDATE()
WHERE RoutineID = @RoutineID AND SchoolID = @SchoolID";

                    SqlCommand cmdMain = new SqlCommand(mainUpdate, con, transaction);
                    cmdMain.Parameters.AddWithValue("@RoutineName", routineName);
                    cmdMain.Parameters.AddWithValue("@ClassColumnCount", ClassColumnCount);
                    cmdMain.Parameters.AddWithValue("@RowCount", RowCount);
                    cmdMain.Parameters.AddWithValue("@RoutineID", routineId);
                    cmdMain.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                    cmdMain.ExecuteNonQuery();

                    // 2. Delete and re-insert class columns
                    string deleteClasses = "DELETE FROM Exam_Routine_ClassColumns WHERE RoutineID = @RoutineID";
                    SqlCommand cmdDeleteClasses = new SqlCommand(deleteClasses, con, transaction);
                    cmdDeleteClasses.Parameters.AddWithValue("@RoutineID", routineId);
                    cmdDeleteClasses.ExecuteNonQuery();

                    foreach (var kvp in SelectedClassIds)
                    {
                        string classInsert = @"
INSERT INTO Exam_Routine_ClassColumns (RoutineID, ColumnIndex, ClassID)
  VALUES (@RoutineID, @ColumnIndex, @ClassID)";

                        SqlCommand cmdClass = new SqlCommand(classInsert, con, transaction);
                        cmdClass.Parameters.AddWithValue("@RoutineID", routineId);
                        cmdClass.Parameters.AddWithValue("@ColumnIndex", kvp.Key);
                        cmdClass.Parameters.AddWithValue("@ClassID", kvp.Value);
                        cmdClass.ExecuteNonQuery();
                    }

                    // 3. Delete and re-insert row data
                    string deleteRows = "DELETE FROM Exam_Routine_Rows WHERE RoutineID = @RoutineID";
                    SqlCommand cmdDeleteRows = new SqlCommand(deleteRows, con, transaction);
                    cmdDeleteRows.Parameters.AddWithValue("@RoutineID", routineId);
                    cmdDeleteRows.ExecuteNonQuery();

                    foreach (RepeaterItem item in RoutineRepeater.Items)
                    {
                        if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                        {
                            TextBox examDateTextBox = (TextBox)item.FindControl("ExamDateTextBox");
                            TextBox dayNameTextBox = (TextBox)item.FindControl("DayNameTextBox");
                            TextBox startTimeTextBox = (TextBox)item.FindControl("StartTimeTextBox");
                            TextBox endTimeTextBox = (TextBox)item.FindControl("EndTimeTextBox");

                            string examDateStr = examDateTextBox?.Text ?? "";
                            string dayName = dayNameTextBox?.Text ?? "";
                            string startTime = startTimeTextBox?.Text ?? "";
                            string endTime = endTimeTextBox?.Text ?? "";

                            // Combine start and end time for ExamTime field (backward compatibility)
                            string examTime = string.IsNullOrEmpty(startTime) && string.IsNullOrEmpty(endTime)
                           ? ""
                     : $"{startTime} - {endTime}";

                            DateTime? examDate = null;
                            if (!string.IsNullOrEmpty(examDateStr))
                            {
                                // **CRITICAL: Parse date from dd/MM/yyyy format**
                                if (DateTime.TryParseExact(examDateStr, "dd/MM/yyyy",
                                   System.Globalization.CultureInfo.InvariantCulture,
                              System.Globalization.DateTimeStyles.None, out DateTime tempDate))
                                {
                                    examDate = tempDate;
                                }
                                else if (DateTime.TryParse(examDateStr, out tempDate))
                                {
                                    examDate = tempDate;
                                }
                            }

                            string rowInsert = @"
           INSERT INTO Exam_Routine_Rows (RoutineID, RowIndex, ExamDate, DayName, ExamTime)
  VALUES (@RoutineID, @RowIndex, @ExamDate, @DayName, @ExamTime)";

                            SqlCommand cmdRow = new SqlCommand(rowInsert, con, transaction);
                            cmdRow.Parameters.AddWithValue("@RoutineID", routineId);
                            cmdRow.Parameters.AddWithValue("@RowIndex", item.ItemIndex);
                            cmdRow.Parameters.AddWithValue("@ExamDate", (object)examDate ?? DBNull.Value);
                            cmdRow.Parameters.AddWithValue("@DayName", dayName);
                            cmdRow.Parameters.AddWithValue("@ExamTime", examTime);
                            cmdRow.ExecuteNonQuery();
                        }
                    }

                    // 4. Delete and re-insert cell data
                    string deleteCells = "DELETE FROM Exam_Routine_CellData WHERE RoutineID = @RoutineID";
                    SqlCommand cmdDeleteCells = new SqlCommand(deleteCells, con, transaction);
                    cmdDeleteCells.Parameters.AddWithValue("@RoutineID", routineId);
                    cmdDeleteCells.ExecuteNonQuery();

                    foreach (RepeaterItem item in RoutineRepeater.Items)
                    {
                        if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                        {
                            for (int colIndex = 1; colIndex <= ClassColumnCount; colIndex++)
                            {
                                string subjectDropdownName = "Subject" + colIndex + "Dropdown_" + item.ItemIndex;
                                string subjectTextboxName = "Subject" + colIndex + "TextBox_" + item.ItemIndex;
                                string timeTextboxName = "Time" + colIndex + "TextBox_" + item.ItemIndex;

                                string subjectIdStr = Request.Form[subjectDropdownName];
                                string subjectText = Request.Form[subjectTextboxName] ?? "";
                                string timeText = Request.Form[timeTextboxName] ?? "";

                                int subjectId = 0;
                                if (!string.IsNullOrEmpty(subjectIdStr))
                                {
                                    int.TryParse(subjectIdStr, out subjectId);
                                }

                                string cellInsert = @"
       INSERT INTO Exam_Routine_CellData (RoutineID, RowIndex, ColumnIndex, SubjectID, SubjectText, TimeText)
  VALUES (@RoutineID, @RowIndex, @ColumnIndex, @SubjectID, @SubjectText, @TimeText)";

                                SqlCommand cmdCell = new SqlCommand(cellInsert, con, transaction);
                                cmdCell.Parameters.AddWithValue("@RoutineID", routineId);
                                cmdCell.Parameters.AddWithValue("@RowIndex", item.ItemIndex);
                                cmdCell.Parameters.AddWithValue("@ColumnIndex", colIndex);
                                cmdCell.Parameters.AddWithValue("@SubjectID", subjectId > 0 ? (object)subjectId : DBNull.Value);
                                cmdCell.Parameters.AddWithValue("@SubjectText", subjectText);
                                cmdCell.Parameters.AddWithValue("@TimeText", timeText);
                                cmdCell.ExecuteNonQuery();
                            }
                        }
                    }

                    transaction.Commit();

                    // Update UI
                    RoutineNameLabel.Text = routineName;
                    LoadRoutineDropdown();

                    ScriptManager.RegisterStartupScript(this, GetType(), "success", "alert('রুটিন সফলভাবে আপডেট হয়েছে');", true);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    string safeMessage = ex.Message.Replace("'", "\\'").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ");
                    ScriptManager.RegisterStartupScript(this, GetType(), "error", $"alert('Error: {safeMessage}');", true);
                }
            }
        }

        protected void DeleteRoutineButton_Click(object sender, EventArgs e)
        {
            int routineId;
            if (int.TryParse(RoutineListDropDown.SelectedValue, out routineId) && routineId > 0)
            {
                string connString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                SqlTransaction transaction = null; // Define transaction here to access in catch
                bool transactionCommitted = false; // Flag to track transaction status

                using (SqlConnection con = new SqlConnection(connString))
                {
                    try
                    {
                        con.Open();
                        transaction = con.BeginTransaction();

                        // Delete in correct order (child tables first)
                        string deleteCells = "DELETE FROM Exam_Routine_CellData WHERE RoutineID = @RoutineID";
                        SqlCommand cmdCells = new SqlCommand(deleteCells, con, transaction);
                        cmdCells.Parameters.AddWithValue("@RoutineID", routineId);
                        cmdCells.ExecuteNonQuery();

                        string deleteRows = "DELETE FROM Exam_Routine_Rows WHERE RoutineID = @RoutineID";
                        SqlCommand cmdRows = new SqlCommand(deleteRows, con, transaction);
                        cmdRows.Parameters.AddWithValue("@RoutineID", routineId);
                        cmdRows.ExecuteNonQuery();

                        string deleteClasses = "DELETE FROM Exam_Routine_ClassColumns WHERE RoutineID = @RoutineID";
                        SqlCommand cmdClasses = new SqlCommand(deleteClasses, con, transaction);
                        cmdClasses.Parameters.AddWithValue("@RoutineID", routineId);
                        cmdClasses.ExecuteNonQuery();

                        string deleteMain = "DELETE FROM Exam_Routine_SavedData WHERE RoutineID = @RoutineID AND SchoolID = @SchoolID";
                        SqlCommand cmdMain = new SqlCommand(deleteMain, con, transaction);
                        cmdMain.Parameters.AddWithValue("@RoutineID", routineId);
                        cmdMain.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                        cmdMain.ExecuteNonQuery();

                        transaction.Commit();
                        transactionCommitted = true; // Mark as committed

                        // Reset UI
                        LoadedRoutineIdHF.Value = "";
                        RoutineNameTextBox.Text = "";
                        RoutineNameLabel.Text = "";
                        LoadRoutineDropdown();

                        // Reset to default
                        ClassColumnCount = 1;
                        RowCount = 1;
                        SelectedClassIds.Clear();
                        LoadedCellData.Clear();
                        LoadDefaultRoutine();

                        ScriptManager.RegisterStartupScript(this, GetType(), "success", "alert('রুটিন সফলভাবে মুছে ফেলা হয়েছে');", true);
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null && !transactionCommitted)
                        {
                            transaction.Rollback();
                        }
                        ScriptManager.RegisterStartupScript(this, GetType(), "error", $"alert('Error: {ex.Message}');", true);
                    }
                }
            }
        }

        protected void RefreshSubjectsButton_Click(object sender, EventArgs e)
        {
            // **CRITICAL: Save class values FIRST before anything else**
            SaveSelectedClassValues();

            // Save subject and time selections from form
            var cellSelections = new Dictionary<string, CellData>();
            for (int rowIdx = 0; rowIdx < RowCount; rowIdx++)
            {
                for (int colIdx = 1; colIdx <= ClassColumnCount; colIdx++)
                {
                    string subjectDropdownName = $"Subject{colIdx}Dropdown_{rowIdx}";
                    string subjectTextboxName = $"Subject{colIdx}TextBox_{rowIdx}";
                    string timeTextboxName = $"Time{colIdx}TextBox_{rowIdx}";

                    string subjectIdStr = Request.Form[subjectDropdownName];
                    string subjectText = Request.Form[subjectTextboxName] ?? "";
                    string timeText = Request.Form[timeTextboxName] ?? "";

                    int subjectId = 0;
                    if (!string.IsNullOrEmpty(subjectIdStr))
                    {
                        int.TryParse(subjectIdStr, out subjectId);
                    }

                    string cellKey = $"{rowIdx}_{colIdx}";
                    cellSelections[cellKey] = new CellData
                    {
                        SubjectID = subjectId,
                        SubjectText = subjectText,
                        TimeText = timeText
                    };
                }
            }

            // Store in LoadedCellData for ItemDataBound
            LoadedCellData = cellSelections;

            // Save all current form data before re-binding
            var currentData = new List<Dictionary<string, string>>();
            foreach (RepeaterItem item in RoutineRepeater.Items)
            {
                if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                {
                    var rowData = new Dictionary<string, string>();

                    // Get TextBox controls
                    TextBox dateTextBox = (TextBox)item.FindControl("ExamDateTextBox");
                    TextBox dayTextBox = (TextBox)item.FindControl("DayNameTextBox");
                    TextBox timeTextBox = (TextBox)item.FindControl("ExamTimeTextBox");

                    rowData["ExamDate"] = dateTextBox != null ? dateTextBox.Text : "";
                    rowData["DayName"] = dayTextBox != null ? dayTextBox.Text : "";
                    rowData["ExamTime"] = timeTextBox != null ? timeTextBox.Text : "";
                    currentData.Add(rowData);
                }
            }

            // Create DataTable for rebinding
            DataTable dtRows = new DataTable();
            dtRows.Columns.Add("RowIndex", typeof(int));
            dtRows.Columns.Add("ExamDate", typeof(string));
            dtRows.Columns.Add("DayName", typeof(string));
            dtRows.Columns.Add("ExamTime", typeof(string));

            for (int i = 0; i < currentData.Count; i++)
            {
                DataRow newRow = dtRows.NewRow();
                newRow["RowIndex"] = i;

                // **CRITICAL: Keep date in dd/MM/yyyy format as string**
                newRow["ExamDate"] = currentData[i]["ExamDate"];
                newRow["DayName"] = currentData[i]["DayName"];
                newRow["ExamTime"] = currentData[i]["ExamTime"];
                dtRows.Rows.Add(newRow);
            }

            // **CRITICAL: Regenerate headers BEFORE binding**
            GenerateClassHeaders();

            // **CRITICAL: Rebind repeater to trigger ItemDataBound**
            RoutineRepeater.DataSource = dtRows;
            RoutineRepeater.DataBind();

            // Force UpdatePanel refresh
            MainUpdatePanel.Update();
        }
    }
}