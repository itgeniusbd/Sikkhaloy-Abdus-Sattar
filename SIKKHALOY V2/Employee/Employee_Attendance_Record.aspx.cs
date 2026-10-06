using System;
using System.Linq;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee
{
    public partial class Employee_Attendance_Record : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var scheduleChanged = ApplyScheduleFilter();

            if (!this.IsPostBack)
            {
                if (Session["SchoolID"] != null)
                {
                    FromDateTextBox.Text = DateTime.Now.ToString("d MMM yyyy");
                    ToDateTextBox.Text = DateTime.Now.ToString("d MMM yyyy");
                }
                else
                {
                    FormsAuthentication.SignOut();
                    Response.Redirect("~/Login.aspx");
                }
            }
            else if (scheduleChanged)
            {
                // The schedule parameter is set here in Page_Load, before the grids hook up to their
                // data sources, so they would not re-query by themselves (they kept showing the old
                // rows from ViewState). Re-query them explicitly whenever the ticked schedules change.
                RebindReports();
            }
        }

        private void RebindReports()
        {
            EmployeeAttRecordGridView.PageIndex = 0;
            EmployeeAttRecordGridView.DataBind();
            Atten_Status_DataList.DataBind();
            Employee_Att_Summary_GridView.DataBind();
        }

        /// <summary>
        /// Ticked schedules as ",3,5," (only validated integers). Empty = all schedules
        /// (nothing ticked or every schedule ticked), same as the old [ ALL SCHEDULE ].
        /// </summary>
        private string SelectedScheduleIds()
        {
            var ids = ScheduleCheckBoxList.Items.Cast<ListItem>()
                .Where(i => i.Selected)
                .Select(i => { int n; return int.TryParse(i.Value, out n) ? n : 0; })
                .Where(n => n > 0)
                .Distinct()
                .ToList();

            if (ids.Count == 0 || ids.Count >= ScheduleCheckBoxList.Items.Count)
                return "";

            return "," + string.Join(",", ids) + ",";
        }

        /// <summary>Sets the ScheduleIDs parameter on every report query; true when the selection changed since the last request.</summary>
        private bool ApplyScheduleFilter()
        {
            var csv = SelectedScheduleIds();
            foreach (var ds in new[] { EmployeeAttendanceSQL, Att_Status_SQL, Employee_Att_Summary_SQL })
            {
                var p = ds.SelectParameters["ScheduleIDs"];
                if (p != null)
                    p.DefaultValue = csv;
            }

            var changed = !string.Equals(ViewState["ScheduleIDs"] as string ?? "", csv, StringComparison.Ordinal);
            ViewState["ScheduleIDs"] = csv;
            return changed;
        }

        protected void EmployeeDropDownList_DataBound(object sender, EventArgs e)
        {
            EmployeeDropDownList.Items.Insert(0, new ListItem("[ All Employee ]", "0"));
        }

        protected void EmployeeAttendanceSQL_Selected(object sender, SqlDataSourceStatusEventArgs e)
        {
            RecordLabel.Text = "Total: " + e.AffectedRows.ToString() + " Record";
        }

        protected void EmployeeAttRecordGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (EmployeeAttRecordGridView.Rows.Count > 0)
            {
                EmployeeAttRecordGridView.UseAccessibleHeader = true;
                EmployeeAttRecordGridView.HeaderRow.TableSection = TableRowSection.TableHeader;
            }
        }

        protected void Employee_Att_Summary_GridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (Employee_Att_Summary_GridView.Rows.Count > 0)
            {
                Employee_Att_Summary_GridView.UseAccessibleHeader = true;
                Employee_Att_Summary_GridView.HeaderRow.TableSection = TableRowSection.TableHeader;
            }
        }
    }
}