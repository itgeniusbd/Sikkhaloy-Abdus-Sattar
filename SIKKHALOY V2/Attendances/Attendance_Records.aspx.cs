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
            if (!this.IsPostBack)
            {
                if (Session["SchoolID"] != null)
                {
                    FromDateTextBox.Text = DateTime.Now.ToString("d MMM yyyy");
                    ToDateTextBox.Text = DateTime.Now.ToString("d MMM yyyy");
                }

                Session["Group"] = "%";
                Session["Shift"] = "%";
                Session["Section"] = "%";
                Session["Schedule"] = "0";

                GroupDropDownList.Visible = false;
                SectionDropDownList.Visible = false;
                ShiftDropDownList.Visible = false;

                EnsureWildcardFilters();
                BindAttendanceData();
            }
            else
            {
                Session["Group"] = GroupDropDownList.SelectedValue;
                Session["Shift"] = ShiftDropDownList.SelectedValue;
                Session["Section"] = SectionDropDownList.SelectedValue;
                Session["Schedule"] = ScheduleDropDownList.SelectedValue ?? "0";
            }
        }

        private void EnsureWildcardFilters()
        {
            if (ClassDropDownList.SelectedValue != "0")
                return;

            if (GroupDropDownList.Items.Count == 0)
                GroupDropDownList.Items.Add(new ListItem("[ ALL GROUP ]", "%"));
            if (SectionDropDownList.Items.Count == 0)
                SectionDropDownList.Items.Add(new ListItem("[ ALL SECTION ]", "%"));
            if (ShiftDropDownList.Items.Count == 0)
                ShiftDropDownList.Items.Add(new ListItem("[ ALL SHIFT ]", "%"));

            GroupDropDownList.SelectedValue = "%";
            SectionDropDownList.SelectedValue = "%";
            ShiftDropDownList.SelectedValue = "%";
        }

        private void BindAttendanceData()
        {
            EnsureWildcardFilters();
            AttendanceGridView.DataSourceID = "AttendanceSQL";
            AttendanceGridView.DataBind();
            AttendanceCountLabel.Text = " Total: " + GetTotalRows().ToString();
            Summery_GridView.DataBind();
        }

        protected void view()
        {
            try
            {
                DataView GroupDV = (DataView)GroupSQL.Select(DataSourceSelectArguments.Empty);
                GroupDropDownList.Visible = GroupDV != null && GroupDV.Count > 0;
            }
            catch { GroupDropDownList.Visible = false; }

            try
            {
                DataView SectionDV = (DataView)SectionSQL.Select(DataSourceSelectArguments.Empty);
                SectionDropDownList.Visible = SectionDV != null && SectionDV.Count > 0;
            }
            catch { SectionDropDownList.Visible = false; }

            try
            {
                DataView ShiftDV = (DataView)ShiftSQL.Select(DataSourceSelectArguments.Empty);
                ShiftDropDownList.Visible = ShiftDV != null && ShiftDV.Count > 0;
            }
            catch { ShiftDropDownList.Visible = false; }

            // GridView reconnect করো যাতে data load হয়
            BindAttendanceData();
        }

        //Class DDL
        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Group"] = "%";
            Session["Shift"] = "%";
            Session["Section"] = "%";
            Session["Schedule"] = "0";

            GroupDropDownList.DataBind();
            ShiftDropDownList.DataBind();
            SectionDropDownList.DataBind();

            if (ClassDropDownList.SelectedValue == "0")
                EnsureWildcardFilters();

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

        //Schedule DDL
        protected void ScheduleDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Schedule"] = ScheduleDropDownList.SelectedValue;
            view();
        }



        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            BindAttendanceData();
        }

        protected int GetTotalRows()
        {
            try
            {
                DataView dv = AttendanceSQL.Select(DataSourceSelectArguments.Empty) as DataView;
                return dv != null ? dv.Count : 0;
            }
            catch
            {
                return 0;
            }
        }

        protected void AttenDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindAttendanceData();
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