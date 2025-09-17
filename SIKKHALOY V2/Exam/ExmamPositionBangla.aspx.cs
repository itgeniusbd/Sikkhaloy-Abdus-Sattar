using Education;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Exam
{
    public partial class ExmamPositionBangla : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;

            if (!IsPostBack)
            {
                GroupDropDownList.Visible = false;
                SectionDropDownList.Visible = false;
                ShiftDropDownList.Visible = false;
            }
        }
        protected void view()
        {
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

            if (ExamDropDownList.SelectedIndex > 0)
            {
                string name = ExamDropDownList.SelectedItem.Text + " এর মেধা তালিকা";

                name += "  শ্রেনি:  " + ClassDropDownList.SelectedItem.Text;

                if (SectionDropDownList.SelectedIndex != 0)
                {
                    name += " , শাখা:  " + SectionDropDownList.SelectedItem.Text;
                }
                if (GroupDropDownList.SelectedIndex != 0)
                {
                    name += " , গ্রুপ:  " + GroupDropDownList.SelectedItem.Text;
                }
                if (ShiftDropDownList.SelectedIndex != 0)
                {
                    name += " , শিফট:  " + ShiftDropDownList.SelectedItem.Text;
                }
                CGSSLabel.Text = name;
            }
            else
            {
                CGSSLabel.Text = "";
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

            view(); // শুধু label update করবে, GridView হবে খালি
            StudentsGridView.DataSource = null;
            StudentsGridView.DataBind();
           
        }

        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShiftDropDownList.DataBind();
            SectionDropDownList.DataBind();
            ExamDropDownList.DataBind();

            view();
            StudentsGridView.DataSource = null;
            StudentsGridView.DataBind();


        }

        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ সব গ্রুপ ]", "%"));
            if (IsPostBack)
                GroupDropDownList.Items.FindByValue(Session["Group"].ToString()).Selected = true;
        }

        protected void SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {

            ShiftDropDownList.DataBind();
            GroupDropDownList.DataBind();
            ExamDropDownList.DataBind();

            view();
            StudentsGridView.DataSource = null;
            StudentsGridView.DataBind();
        }

        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ সব শাখা ]", "%"));
            if (IsPostBack)
                SectionDropDownList.Items.FindByValue(Session["Section"].ToString()).Selected = true;
        }

        protected void ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            view();
            StudentsGridView.DataSource = null;
            StudentsGridView.DataBind();
        }

        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            ShiftDropDownList.Items.Insert(0, new ListItem("[ সব শিফট ]", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }
        //End DDL
        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
            LoadSubjectsAndResults();  // <-- এখানে কল দিন
        }


        private void LoadSubjectsAndResults()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                con.Open();
                string examID = ExamDropDownList.SelectedValue;
                int classID = Convert.ToInt32(ClassDropDownList.SelectedValue); // Assuming you have a Class dropdown
                // Step 1: Get distinct subjects
                DataTable subjectList = new DataTable();
                using (SqlCommand cmdSubjects = new SqlCommand(@"
    SELECT DISTINCT sub.SubjectName
    FROM Subject sub
    INNER JOIN Exam_Result_of_Subject subR ON sub.SubjectID = subR.SubjectID
    INNER JOIN Exam_Result_of_Student ers ON subR.StudentResultID = ers.StudentResultID
    INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
    WHERE ers.ExamID = @ExamID
      AND sc.ClassID = @ClassID
    ORDER BY sub.SubjectName", con))
                {
                    cmdSubjects.Parameters.AddWithValue("@ExamID", examID);
                    cmdSubjects.Parameters.AddWithValue("@ClassID", classID); // Pass the class ID here

                    new SqlDataAdapter(cmdSubjects).Fill(subjectList);
                }

                if (subjectList.Rows.Count == 0)
                {
                    StudentsGridView.DataSource = null;
                    StudentsGridView.DataBind();
                    return;
                }

                // Step 2: Build pivot columns for SQL
                StringBuilder pivotColumns = new StringBuilder();
                foreach (DataRow row in subjectList.Rows)
                {
                    string col = row["SubjectName"].ToString().Replace("]", "]]");
                    pivotColumns.Append($"[{col}],");
                }
                pivotColumns.Length--; // remove last comma

                // Step 3: Dynamic PIVOT query
                string query = $@"
        SELECT *
        FROM
        (
            SELECT 
              s.StudentID,
              sc.RollNo,
              s.StudentsName,
              translate(ers.TotalExamObtainedMark_ofStudent, N'0123456789', N'০১২৩৪৫৬৭৮৯') AS Total,
              ers.Student_Grade,
              translate(ers.Student_Point, N'0123456789', N'০১২৩৪৫৬৭৮৯') AS Student_Point,
              translate(ers.Position_InExam_Class, N'0123456789', N'০১২৩৪৫৬৭৮৯') AS Position_InExam_Class,
              translate(ers.Position_InExam_Subsection, N'0123456789', N'০১২৩৪৫৬৭৮৯') AS Position_InExam_Subsection,
              translate(ers.Average, N'0123456789', N'০১২৩৪৫৬৭৮৯') AS Average,
              sub.SubjectName,
              translate(subR.ObtainedMark_ofSubject, N'0123456789', N'০১২৩৪৫৬৭৮৯') AS ObtainedMark_ofSubject
            FROM Exam_Result_of_Student ers
            INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
            INNER JOIN Student s ON sc.StudentID = s.StudentID
            INNER JOIN Exam_Result_of_Subject subR ON ers.StudentResultID = subR.StudentResultID
            INNER JOIN Subject sub ON subR.SubjectID = sub.SubjectID
            WHERE ers.ExamID = @ExamID
              AND sc.ClassID = @ClassID
              AND sc.SectionID LIKE @SectionID
              AND sc.ShiftID LIKE @ShiftID
              AND sc.SubjectGroupID LIKE @GroupID
        ) AS SourceTable
        PIVOT
        (
            MAX(ObtainedMark_ofSubject)
            FOR SubjectName IN ({pivotColumns})
        ) AS PivotTable
        ORDER BY RollNo";


                // Step 4: Execute query
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ExamID", examID);
                cmd.Parameters.AddWithValue("@ClassID", classID);
                cmd.Parameters.AddWithValue("@SectionID", SectionDropDownList.SelectedValue);
                cmd.Parameters.AddWithValue("@ShiftID", ShiftDropDownList.SelectedValue);
                cmd.Parameters.AddWithValue("@GroupID", GroupDropDownList.SelectedValue);
                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                // Step 5: Build GridView columns in desired order
                StudentsGridView.Columns.Clear();

                // ID
                BoundField bfID = new BoundField { DataField = "StudentID", HeaderText = "আইডি" };
                StudentsGridView.Columns.Add(bfID);

                // Roll No
                BoundField bfRoll = new BoundField { DataField = "RollNo", HeaderText = "রোল" };
                StudentsGridView.Columns.Add(bfRoll);

                // Student Name
                BoundField bfName = new BoundField { DataField = "StudentsName", HeaderText = "নাম" };
                StudentsGridView.Columns.Add(bfName);

                // Dynamic subject columns
                foreach (DataRow row in subjectList.Rows)
                {
                    string subject = row["SubjectName"].ToString();
                    BoundField bfSub = new BoundField { DataField = subject, HeaderText = subject };
                    StudentsGridView.Columns.Add(bfSub);
                }

                // Total
                BoundField bfTotal = new BoundField { DataField = "Total", HeaderText = "মোট" };
                StudentsGridView.Columns.Add(bfTotal);

                // AVR
                BoundField bfAVG = new BoundField { DataField = "Average", HeaderText = "গড়" };
                StudentsGridView.Columns.Add(bfAVG);

                // Grade
                BoundField bfGrade = new BoundField { DataField = "Student_Grade", HeaderText = "গ্রেড" };
                StudentsGridView.Columns.Add(bfGrade);

                // Point
                BoundField bfPoint = new BoundField { DataField = "Student_Point", HeaderText = "পয়েন্ট" };
                StudentsGridView.Columns.Add(bfPoint);

                // Class Position
                BoundField bfClassPos = new BoundField { DataField = "Position_InExam_Class", HeaderText = "ক্লাশ মেধা" };
                StudentsGridView.Columns.Add(bfClassPos);

                // Section Position
                BoundField bfSectionPos = new BoundField { DataField = "Position_InExam_Subsection", HeaderText = "শাখা মেধা" };
                StudentsGridView.Columns.Add(bfSectionPos);

                // Step 6: Bind data
                StudentsGridView.DataSource = dt;
                StudentsGridView.DataBind();
            }
        }



        private DataTable GetSubjectList(SqlConnection con, string examID, int classID)
        {
            string subQuery = @"
    SELECT DISTINCT s.SubjectID, s.SubjectName, ISNULL(s.SN, 999) AS SN
    FROM Exam_Result_of_Subject ers
    INNER JOIN Subject s ON ers.SubjectID = s.SubjectID
    INNER JOIN Exam_Result_of_Student ersMain ON ers.StudentResultID = ersMain.StudentResultID
    INNER JOIN StudentsClass sc ON ersMain.StudentClassID = sc.StudentClassID
    WHERE ersMain.ExamID = @ExamID
      AND sc.ClassID = @ClassID
    ORDER BY SN, s.SubjectID";

            SqlCommand subCmd = new SqlCommand(subQuery, con);
            subCmd.Parameters.AddWithValue("@ExamID", examID);
            subCmd.Parameters.AddWithValue("@ClassID", classID);

            SqlDataAdapter da = new SqlDataAdapter(subCmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            return dt;
        }

        private string BuildPivotColumns(DataTable subjects)
        {
            StringBuilder sb = new StringBuilder();

            foreach (DataRow row in subjects.Rows)
            {
                string subID = row["SubjectID"].ToString();
                string subName = row["SubjectName"].ToString().Replace(" ", "_");

                sb.AppendLine($"MAX(CASE WHEN sub.SubjectID = {subID} THEN subR.ObtainedMark_ofSubject END) AS Mark_{subName},");
                sb.AppendLine($"MAX(CASE WHEN sub.SubjectID = {subID} THEN subR.PassStatus_InSubject END) AS Pass_{subName},");
            }

            if (sb.Length > 0)
                sb.Length -= 3;

            return sb.ToString();
        }

        private void AddDynamicColumns(DataTable subjects)
        {
            // পুরোনো Dynamic Columns মুছে ফেলা
            for (int i = StudentsGridView.Columns.Count - 1; i >= 7; i--)
            {
                StudentsGridView.Columns.RemoveAt(i);
            }

            foreach (DataRow row in subjects.Rows)
            {
                string subName = row["SubjectName"].ToString();

                // Marks Column
                BoundField markCol = new BoundField();
                markCol.HeaderText = subName + " (Marks)";
                markCol.DataField = "Mark_" + subName.Replace(" ", "_");
                StudentsGridView.Columns.Add(markCol);

                // Pass Column
                BoundField passCol = new BoundField();
                passCol.HeaderText = subName + " (Pass)";
                passCol.DataField = "Pass_" + subName.Replace(" ", "_");
                StudentsGridView.Columns.Add(passCol);
            }
        }


        protected void ExamDropDownList_DataBound(object sender, EventArgs e)
        {
            ExamDropDownList.Items.Insert(0, new ListItem("[ পরীক্ষার নাম নির্বাচন করুন ]", "0"));
        }



        protected void StudentsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //Class
                var classIndex = 9;


                if (e.Row.Cells[classIndex].Text == "১")
                {
                    e.Row.Cells[classIndex].CssClass = "First";
                    e.Row.Cells[classIndex].Text = " প্রথম";
                }

                else if (e.Row.Cells[classIndex].Text == "২")
                {
                    e.Row.Cells[classIndex].CssClass = "Second";
                    e.Row.Cells[classIndex].Text = " দ্বিতীয়";
                }

                else if (e.Row.Cells[classIndex].Text == "৩")
                {
                    e.Row.Cells[classIndex].CssClass = "Third";
                    e.Row.Cells[classIndex].Text = " তৃতীয়";

                }
                else
                {
                    e.Row.Cells[classIndex].Text += "";
                }

                //Section
                var sectionIndex = 10;
                if (e.Row.Cells[sectionIndex].Text == "১")
                {
                    e.Row.Cells[sectionIndex].CssClass = "First";
                    e.Row.Cells[sectionIndex].Text = " প্রথম";
                }

                else if (e.Row.Cells[sectionIndex].Text == "২")
                {
                    e.Row.Cells[sectionIndex].CssClass = "Second";
                    e.Row.Cells[sectionIndex].Text = " দ্বিতীয়";

                }

                else if (e.Row.Cells[sectionIndex].Text == "৩")
                {
                    e.Row.Cells[sectionIndex].CssClass = "Third";
                    e.Row.Cells[sectionIndex].Text = " তৃতীয়";

                }
                else
                {
                    e.Row.Cells[sectionIndex].Text += "";
                }


                if (StudentsGridView.DataKeys[e.Row.DataItemIndex]["PassStatus_InSubject"].ToString() == "F")
                {
                    e.Row.CssClass = "RowColor";
                }
            }

            if (StudentsGridView.Rows.Count > 0)
            {
                StudentsGridView.UseAccessibleHeader = true;
                StudentsGridView.HeaderRow.TableSection = TableRowSection.TableHeader;
            }
            // Print Header Fix
            if (StudentsGridView.Rows.Count > 0)
            {
                StudentsGridView.UseAccessibleHeader = true;
                StudentsGridView.HeaderRow.TableSection = TableRowSection.TableHeader;
            }

            // Optional: Row styling
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var sectionIndex = 10;
                if (e.Row.Cells[sectionIndex].Text == "১ম") e.Row.BackColor = System.Drawing.Color.Green;
            }

        }
        protected void StudentsGridView_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                // force header to render inside <thead>
                StudentsGridView.UseAccessibleHeader = true;
                e.Row.TableSection = TableRowSection.TableHeader;
            }
        }

        protected void ExportWordButton_Click(object sender, EventArgs e)
        {
            Export_ClassLabel.Text = CGSSLabel.Text;
            Response.Clear();
            Response.ContentEncoding = Encoding.Unicode;
            Response.BinaryWrite(Encoding.Unicode.GetPreamble());

            Response.AddHeader("content-disposition", "attachment;filename=ExmamPositionBangla.doc");
            Response.Charset = "";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.ContentType = "application/doc";
            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            // Read Style file (css) here and add to response 
            FileInfo fi = new FileInfo(Server.MapPath("~/Exam/CSS/ExamPosition.css"));
            StringBuilder sb = new StringBuilder();
            StreamReader sr = fi.OpenText();

            while (sr.Peek() >= 0)
            {
                sb.Append(sr.ReadLine());
            }

            sr.Close();
            StudentsGridView.Columns[0].Visible = false;
            ExportPanel.RenderControl(htmlWrite);
            Response.Write("<html><head><style type='text/css'>" + sb.ToString() + "</style></head><body>" + stringWrite.ToString() + "</body></html>");

            Response.Write(stringWrite.ToString());
            Response.End();



        }
        public override void VerifyRenderingInServerForm(Control control)
        {
            /* Confirms that an HtmlForm control is rendered for the specified ASP.NET
               server control at run time. */
        }
    }
}
