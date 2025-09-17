using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Exam.ExamSetting
{
    public partial class Marks_Collect_Paper : System.Web.UI.Page
    {
        protected void view()//Function for view dropdownlist
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

        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)//Class DDL
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
            ClassDropDownList.Items.Insert(0, new ListItem("[ SELECT CLASS ]", "0"));
        }

        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
            Session["Subject"] = "0";
        }
        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ SELECT GROUP ]", "%"));
            if (IsPostBack)
                GroupDropDownList.Items.FindByValue(Session["Group"].ToString()).Selected = true;
        }

        protected void SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }
        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ All SECTION ]", "%"));
            if (IsPostBack)
                SectionDropDownList.Items.FindByValue(Session["Section"].ToString()).Selected = true;
        }

        protected void ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }
        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)  //...End DDl
        {
            ShiftDropDownList.Items.Insert(0, new ListItem("[ All SHIFT]", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }


        protected void SubjectDropDownList_DataBound(object sender, EventArgs e)
        {
            if (SubjectDropDownList.Items.FindByValue("0") == null)
                SubjectDropDownList.Items.Insert(0, new ListItem("[ SELECT SUBJECT ]", "0"));
            if (IsPostBack)
            {
                if (Session["Subject"] != null)
                    SubjectDropDownList.Items.FindByValue(Session["Subject"].ToString()).Selected = true;
            }
        }

        protected void SubjectDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int a = StudentsGridView.Columns.Count - 3;
            for (int i = 0; i < a; i++)
            {
                StudentsGridView.Columns.RemoveAt(3);
            }


            DataView SubExamDV = new DataView();
            SubExamDV = (DataView)SubExamSQL.Select(DataSourceSelectArguments.Empty);

            if (SubExamDV.Count > 0)
            {
                for (int i = 0; i < SubExamDV.Count; i++)
                {
                    BoundField Marks_BoundField = new BoundField();

                    string Sub_Ex_Name = SubExamDV[i]["SubExamName"].ToString();

                    if (!string.IsNullOrEmpty(Sub_Ex_Name))
                    {
                        Marks_BoundField.HeaderText = Sub_Ex_Name;
                    }
                    else
                    {
                        Marks_BoundField.HeaderText = "Marks";
                    }

                    StudentsGridView.Columns.Add(Marks_BoundField);
                }
            }

            BoundField Sign_BoundField = new BoundField();
            Sign_BoundField.HeaderText = "Student Signature";
            StudentsGridView.Columns.Add(Sign_BoundField);

            BoundField Code_BoundField = new BoundField();
            Code_BoundField.HeaderText = "Code";
            StudentsGridView.Columns.Add(Code_BoundField);
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