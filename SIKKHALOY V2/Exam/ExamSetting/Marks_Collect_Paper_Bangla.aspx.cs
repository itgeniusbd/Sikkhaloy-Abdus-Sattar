using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Exam.ExamSetting
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        private const int FixedColumnCount = 4;

        protected bool HasMarksDistribution { get; private set; }

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
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;
            Session["Subject"] = SubjectDropDownList.SelectedValue;

            if (!IsPostBack)
            {
                GroupDropDownList.Visible = false;
                SectionDropDownList.Visible = false;
                ShiftDropDownList.Visible = false;
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            bool filtersSelected = SubjectDropDownList.SelectedIndex > 0
                && ExamDropDownList.SelectedIndex > 0
                && ClassDropDownList.SelectedIndex > 0;

            HasMarksDistribution = filtersSelected && HasMarksDistributionForSelection();

            NoMarksDistributionPanel.Visible = filtersSelected && !HasMarksDistribution;
            GridPanel.Visible = HasMarksDistribution;

            if (NoMarksDistributionPanel.Visible)
            {
                NoMarksDistributionLiteral.Text = string.Format(
                    "<strong>মার্ক ড্রিটিভিউশন পাওয়া যায়নি।</strong>" +
                    "<b>{0}</b> পরীক্ষা, শ্রেণি <b>{1}</b>, বিষয় <b>{2}</b>-এর জন্য পূর্ণ নম্বর সেট করা হয়নি। " +
                    "প্রিন্ট করার আগে <a href=\"Marks_Distribution.aspx\">পরীক্ষা সেটিং &rarr; মার্ক ড্রিটিভিউশন</a> " +
                    "থেকে একই পরীক্ষা ও শ্রেণি নির্বাচন করে বিষয়ের পূর্ণ নম্বর দিয়ে Submit করুন।",
                    ExamDropDownList.SelectedItem.Text,
                    ClassDropDownList.SelectedItem.Text,
                    SubjectDropDownList.SelectedItem.Text);
            }

            if (HasMarksDistribution)
            {
                RebuildMarkColumns();
            }
        }

        private bool HasMarksDistributionForSelection()
        {
            DataView marksDv = (DataView)SubExamSQL.Select(DataSourceSelectArguments.Empty);
            return marksDv != null && marksDv.Count > 0;
        }

        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Group"] = "%";
            Session["Shift"] = "%";
            Session["Section"] = "%";
            Session["Subject"] = "0";

            GroupDropDownList.Visible = false;
            SectionDropDownList.Visible = false;
            ShiftDropDownList.Visible = false;
        }

        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Group"] = "%";
            Session["Shift"] = "%";
            Session["Section"] = "%";
            Session["Subject"] = "0";

            GroupDropDownList.DataBind();
            ShiftDropDownList.DataBind();
            SectionDropDownList.DataBind();
            StudentsGridView.DataBind();
            view();
        }

        protected void ClassDropDownList_DataBound(object sender, EventArgs e)
        {
            ClassDropDownList.Items.Insert(0, new ListItem("[ শ্রেণি নির্বাচন করুন ]", "0"));
        }

        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
            Session["Subject"] = "0";
        }

        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ গ্রুপ নির্বাচন করুন ]", "%"));
            if (IsPostBack)
                GroupDropDownList.Items.FindByValue(Session["Group"].ToString()).Selected = true;
        }

        protected void SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ সকল শাখা ]", "%"));
            if (IsPostBack)
                SectionDropDownList.Items.FindByValue(Session["Section"].ToString()).Selected = true;
        }

        protected void ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            ShiftDropDownList.Items.Insert(0, new ListItem("[ সকল শিফট ]", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }

        protected void SubjectDropDownList_DataBound(object sender, EventArgs e)
        {
            if (SubjectDropDownList.Items.FindByValue("0") == null)
                SubjectDropDownList.Items.Insert(0, new ListItem("[ বিষয় নির্বাচন করুন ]", "0"));
            if (IsPostBack)
            {
                if (Session["Subject"] != null)
                    SubjectDropDownList.Items.FindByValue(Session["Subject"].ToString()).Selected = true;
            }
        }

        protected void SubjectDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            RebuildMarkColumns();
        }

        private void RebuildMarkColumns()
        {
            while (StudentsGridView.Columns.Count > FixedColumnCount)
            {
                StudentsGridView.Columns.RemoveAt(FixedColumnCount);
            }

            DataView SubExamDV = new DataView();
            SubExamDV = (DataView)SubExamSQL.Select(DataSourceSelectArguments.Empty);

            if (SubExamDV.Count > 0)
            {
                for (int i = 0; i < SubExamDV.Count; i++)
                {
                    BoundField Marks_BoundField = new BoundField();

                    string Sub_Ex_Name = SubExamDV[i]["SubExamName"] == DBNull.Value
                        ? string.Empty
                        : SubExamDV[i]["SubExamName"].ToString().Trim();

                    if (!string.IsNullOrEmpty(Sub_Ex_Name))
                    {
                        Marks_BoundField.HeaderText = "নম্বর (" + Sub_Ex_Name + ")";
                    }
                    else
                    {
                        Marks_BoundField.HeaderText = "নম্বর";
                    }

                    StudentsGridView.Columns.Add(Marks_BoundField);
                }
            }
            else
            {
                BoundField Marks_BoundField = new BoundField();
                Marks_BoundField.HeaderText = "নম্বর";
                StudentsGridView.Columns.Add(Marks_BoundField);
            }

            BoundField Sign_BoundField = new BoundField();
            Sign_BoundField.HeaderText = "শিক্ষার্থীর স্বাক্ষর";
            StudentsGridView.Columns.Add(Sign_BoundField);
        }

        protected void StudentsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (StudentsGridView.Rows.Count > 0)
            {
                StudentsGridView.UseAccessibleHeader = true;
                StudentsGridView.HeaderRow.TableSection = TableRowSection.TableHeader;
            }
        }
    }
}
