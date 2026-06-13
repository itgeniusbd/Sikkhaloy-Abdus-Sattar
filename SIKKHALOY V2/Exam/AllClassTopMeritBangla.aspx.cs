using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Exam
{
    public partial class AllClassTopMeritBangla : Page
    {
        private const string ViewModeClass = "Class";
        private const string ViewModeSection = "Section";

        private sealed class MeritStudent
        {
            public string MeritText { get; set; }
            public string PositionCss { get; set; }
            public string StudentID { get; set; }
            public string RollNo { get; set; }
            public string StudentsName { get; set; }
            public string FathersName { get; set; }
            public string TotalMark { get; set; }
            public string Average { get; set; }
            public string Grade { get; set; }
            public string Point { get; set; }
        }

        private sealed class MeritGroup
        {
            public string GroupTitle { get; set; }
            public List<MeritStudent> Students { get; set; }
        }

        private string MeritViewMode
        {
            get { return ViewState["MeritViewMode"] as string ?? ViewModeClass; }
            set { ViewState["MeritViewMode"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            LoadSchoolHeader();

            if (!IsPostBack)
            {
                MeritViewRadioButtonList.SelectedValue = ViewModeClass;

                if (Session["Edu_Year"] != null)
                {
                    string eduYear = Session["Edu_Year"].ToString();
                    ListItem yearItem = EduYearDropDownList.Items.FindByValue(eduYear);
                    if (yearItem != null)
                    {
                        EduYearDropDownList.ClearSelection();
                        yearItem.Selected = true;
                    }
                }

                LoadTopMerit();
            }
        }

        protected void EduYearDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ExamDropDownList.DataBind();
            LoadTopMerit();
        }

        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTopMerit();
        }

        protected void MeritViewRadioButtonList_SelectedIndexChanged(object sender, EventArgs e)
        {
            MeritViewMode = MeritViewRadioButtonList.SelectedValue;
            LoadTopMerit();
        }

        protected void ExamDropDownList_DataBound(object sender, EventArgs e)
        {
            if (ExamDropDownList.Items.FindByValue("0") == null)
                ExamDropDownList.Items.Insert(0, new ListItem("[ পরীক্ষা নির্বাচন করুন ]", "0"));
        }

        protected void ClassMeritRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            MeritGroup group = e.Item.DataItem as MeritGroup;
            Repeater studentRepeater = e.Item.FindControl("StudentMeritRepeater") as Repeater;
            if (group == null || studentRepeater == null)
                return;

            studentRepeater.DataSource = group.Students;
            studentRepeater.DataBind();
        }

        private void LoadSchoolHeader()
        {
            if (Session["School_Name"] != null)
                SchoolNameLabel.Text = Session["School_Name"].ToString();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT SchoolName, Address FROM SchoolInfo WHERE SchoolID = @SchoolID", con))
            {
                cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return;

                    if (string.IsNullOrWhiteSpace(SchoolNameLabel.Text))
                        SchoolNameLabel.Text = reader["SchoolName"].ToString();

                    SchoolAddressLabel.Text = reader["Address"] == DBNull.Value ? string.Empty : reader["Address"].ToString();
                }
            }
        }

        private void LoadTopMerit()
        {
            ResultsPanel.Visible = false;
            EmptyLabel.Visible = true;
            ExamSessionLabel.Text = string.Empty;
            MeritListLabel.Text = string.Empty;
            ClassMeritRepeater.DataSource = null;
            ClassMeritRepeater.DataBind();

            if (EduYearDropDownList.SelectedIndex < 0 || ExamDropDownList.SelectedIndex <= 0)
                return;

            MeritViewMode = MeritViewRadioButtonList.SelectedValue;

            string examName = ExamDropDownList.SelectedItem.Text;
            string eduYearName = EduYearDropDownList.SelectedItem.Text;
            string modeText = MeritViewMode == ViewModeSection
                ? "শাখা/গ্রুপ ওয়াইজ"
                : "ক্লাশ ওয়াইজ";

            ExamSessionLabel.Text = examName + " - " + eduYearName + " শিক্ষাবর্ষ";
            MeritListLabel.Text = "সকল শ্রেণির " + modeText + " সেরাদের মেধা তালিকা";

            List<MeritGroup> groups = MeritViewMode == ViewModeSection
                ? GetTopMeritBySection()
                : GetTopMeritByClass();

            if (groups.Count == 0)
                return;

            ClassMeritRepeater.DataSource = groups;
            ClassMeritRepeater.DataBind();
            ResultsPanel.Visible = true;
            EmptyLabel.Visible = false;
        }

        private List<MeritGroup> GetTopMeritByClass()
        {
            const string sql = @"
SELECT
    cc.Class,
    cc.ClassID,
    ISNULL(cc.SN, 999) AS ClassSN,
    s.ID,
    sc.RollNo,
    s.StudentsName,
    s.FathersName,
    translate(ers.TotalExamObtainedMark_ofStudent, N'0123456789', N'০১২৩৪৫৬৭৮৯') AS TotalMark,
    translate(ers.Average, N'0123456789', N'০১২৩৪৫৬৭৮৯') AS Average,
    ers.Student_Grade AS Grade,
    translate(ers.Student_Point, N'0123456789', N'০১২৩৪৫৬৭৮৯') AS Point,
    ers.Position_InExam_Class AS MeritPosition
FROM Exam_Result_of_Student ers
INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
INNER JOIN Student s ON sc.StudentID = s.StudentID
INNER JOIN CreateClass cc ON sc.ClassID = cc.ClassID
WHERE ers.SchoolID = @SchoolID
  AND ers.EducationYearID = @EducationYearID
  AND ers.ExamID = @ExamID
  AND ISNUMERIC(ers.Position_InExam_Class) = 1
  AND CAST(ers.Position_InExam_Class AS INT) BETWEEN 1 AND 3
ORDER BY ClassSN, cc.ClassID, CAST(ers.Position_InExam_Class AS INT)";

            DataTable dt = ExecuteMeritQuery(sql);

            return dt.AsEnumerable()
                .GroupBy(r => new
                {
                    GroupTitle = r["Class"].ToString(),
                    ClassSN = Convert.ToInt32(r["ClassSN"]),
                    ClassID = Convert.ToInt32(r["ClassID"])
                })
                .OrderBy(g => g.Key.ClassSN)
                .ThenBy(g => g.Key.ClassID)
                .Select(g => new MeritGroup
                {
                    GroupTitle = g.Key.GroupTitle,
                    Students = g.Select(CreateMeritStudent).ToList()
                })
                .ToList();
        }

        private List<MeritGroup> GetTopMeritBySection()
        {
            const string sql = @"
SELECT
    cc.Class,
    cc.ClassID,
    ISNULL(cc.SN, 999) AS ClassSN,
    ISNULL(cs.Section, N'') AS SectionName,
    ISNULL(csg.SubjectGroup, N'') AS GroupName,
    s.ID,
    sc.RollNo,
    s.StudentsName,
    s.FathersName,
    translate(ers.TotalExamObtainedMark_ofStudent, N'0123456789', N'০১২৩৪৫৬৭৮৯') AS TotalMark,
    translate(ers.Average, N'0123456789', N'০১২৩৪৫৬৭৮৯') AS Average,
    ers.Student_Grade AS Grade,
    translate(ers.Student_Point, N'0123456789', N'০১২৩৪৫৬৭৮৯') AS Point,
    ers.Position_InExam_Subsection AS MeritPosition
FROM Exam_Result_of_Student ers
INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
INNER JOIN Student s ON sc.StudentID = s.StudentID
INNER JOIN CreateClass cc ON sc.ClassID = cc.ClassID
LEFT JOIN CreateSection cs ON sc.SectionID = cs.SectionID
LEFT JOIN CreateSubjectGroup csg ON sc.SubjectGroupID = csg.SubjectGroupID
WHERE ers.SchoolID = @SchoolID
  AND ers.EducationYearID = @EducationYearID
  AND ers.ExamID = @ExamID
  AND ISNUMERIC(ers.Position_InExam_Subsection) = 1
  AND CAST(ers.Position_InExam_Subsection AS INT) BETWEEN 1 AND 3
ORDER BY ClassSN, cc.ClassID, SectionName, GroupName, CAST(ers.Position_InExam_Subsection AS INT)";

            DataTable dt = ExecuteMeritQuery(sql);

            return dt.AsEnumerable()
                .GroupBy(r => new
                {
                    GroupTitle = BuildSectionGroupTitle(
                        r["Class"].ToString(),
                        r["SectionName"].ToString(),
                        r["GroupName"].ToString()),
                    ClassSN = Convert.ToInt32(r["ClassSN"]),
                    ClassID = Convert.ToInt32(r["ClassID"]),
                    SectionName = r["SectionName"].ToString(),
                    GroupName = r["GroupName"].ToString()
                })
                .OrderBy(g => g.Key.ClassSN)
                .ThenBy(g => g.Key.ClassID)
                .ThenBy(g => g.Key.SectionName)
                .ThenBy(g => g.Key.GroupName)
                .Select(g => new MeritGroup
                {
                    GroupTitle = g.Key.GroupTitle,
                    Students = g.Select(CreateMeritStudent).ToList()
                })
                .ToList();
        }

        private DataTable ExecuteMeritQuery(string sql)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                cmd.Parameters.AddWithValue("@EducationYearID", EduYearDropDownList.SelectedValue);
                cmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                new SqlDataAdapter(cmd).Fill(dt);
            }

            return dt;
        }

        private static string BuildSectionGroupTitle(string className, string sectionName, string groupName)
        {
            string title = className;

            if (!string.IsNullOrWhiteSpace(sectionName))
                title += " , শাখা: " + sectionName;

            if (!string.IsNullOrWhiteSpace(groupName))
                title += " , গ্রুপ: " + groupName;

            return title;
        }

        private static MeritStudent CreateMeritStudent(DataRow row)
        {
            int position = Convert.ToInt32(row["MeritPosition"]);
            string meritText;
            string positionCss;

            switch (position)
            {
                case 1:
                    meritText = "১ম";
                    positionCss = "pos-first";
                    break;
                case 2:
                    meritText = "২য়";
                    positionCss = "pos-second";
                    break;
                case 3:
                    meritText = "৩য়";
                    positionCss = "pos-third";
                    break;
                default:
                    meritText = ToBanglaNumber(position.ToString());
                    positionCss = string.Empty;
                    break;
            }

            return new MeritStudent
            {
                MeritText = meritText,
                PositionCss = positionCss,
                StudentID = row["ID"].ToString(),
                RollNo = row["RollNo"].ToString(),
                StudentsName = row["StudentsName"].ToString(),
                FathersName = row["FathersName"] == DBNull.Value ? string.Empty : row["FathersName"].ToString(),
                TotalMark = row["TotalMark"].ToString(),
                Average = row["Average"].ToString(),
                Grade = row["Grade"].ToString(),
                Point = row["Point"].ToString()
            };
        }

        private static string ToBanglaNumber(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return value
                .Replace("0", "০").Replace("1", "১").Replace("2", "২").Replace("3", "৩")
                .Replace("4", "৪").Replace("5", "৫").Replace("6", "৬").Replace("7", "৭")
                .Replace("8", "৮").Replace("9", "৯");
        }
    }
}
