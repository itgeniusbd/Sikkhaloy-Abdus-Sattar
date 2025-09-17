using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ATTENDANCES
{
    public partial class Attendance_Records : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;

            if (!this.IsPostBack)
            {
                if (Session["SchoolID"] != null)
                {
                    FromDateTextBox.Text = DateTime.Now.ToString("d MMM yyyy");
                    ToDateTextBox.Text = DateTime.Now.ToString("d MMM yyyy");
                    AttendanceCountLabel.Text = " Total: " + GetTotalRows().ToString();
                }

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
            AttendanceCountLabel.Text = " Total: " + GetTotalRows().ToString();
        }

        //Class DDL
        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Group"] = "%";
            Session["Shift"] = "%";
            Session["Section"] = "%";

            GroupDropDownList.DataBind();
            ShiftDropDownList.DataBind();
            SectionDropDownList.DataBind();
            view();
        }
        //Group DDL
        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }
        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ ALL GROUP ]", "%"));
            if (IsPostBack)
                GroupDropDownList.Items.FindByValue(Session["Group"].ToString()).Selected = true;
        }
        //Section DDL
        protected void SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }
        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ ALL SECTION ]", "%"));
            if (IsPostBack)
                SectionDropDownList.Items.FindByValue(Session["Section"].ToString()).Selected = true;
        }

        //Shift DDL
        protected void ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }
        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            ShiftDropDownList.Items.Insert(0, new ListItem("[ ALL SHIFT ]", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }



        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            AttendanceCountLabel.Text = " Total: " + GetTotalRows().ToString();
            Summery_GridView.DataBind();
        }

        protected int GetTotalRows()
        {
            AttendanceSQL.FilterParameters["To"].DefaultValue = Convert.ToDateTime(ToDateTextBox.Text).AddDays(1).ToString();

            DataView dv = (DataView)AttendanceSQL.Select(DataSourceSelectArguments.Empty);
            if (dv != null)
                return dv.Count;
            else
                return 0;

        }

        protected void AttenDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            AttendanceCountLabel.Text = " Total: " + GetTotalRows().ToString();
        }

        protected void AttendanceGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (AttendanceGridView.Rows.Count > 0)
            {
                AttendanceGridView.UseAccessibleHeader = true;
                AttendanceGridView.HeaderRow.TableSection = TableRowSection.TableHeader;
            }
        }
    }
}