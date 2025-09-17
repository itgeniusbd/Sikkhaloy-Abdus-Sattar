using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ROUTINES
{
    public partial class Create_Routines_for_Classes : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                DataTable ChargeTeble = new DataTable();
                ChargeTeble.Columns.AddRange(new DataColumn[5] { new DataColumn("RoutinePeriod"), new DataColumn("StartTime"), new DataColumn("EndTime"), new DataColumn("Duration"), new DataColumn("OffTime", typeof(bool)) });
                ViewState["ChargeTeble"] = ChargeTeble;
            }
        }

        /*Add To Cart Button*/
        protected void BindGrid()
        {
            RoutineGridView.DataSource = ViewState["ChargeTeble"] as DataTable;
            RoutineGridView.DataBind();

        }

        protected void RowDelete(object sender, EventArgs e)
        {
            GridViewRow row = (sender as LinkButton).NamingContainer as GridViewRow;
            DataTable ChargeTeble = ViewState["ChargeTeble"] as DataTable;

            ChargeTeble.Rows.RemoveAt(row.RowIndex);
            ViewState["ChargeTeble"] = ChargeTeble;
            this.BindGrid();
        }

        protected void AddToCartButton_Click(object sender, EventArgs e)
        {
            DateTime start = DateTime.Parse(StartTimeTextBox.Text);
            DateTime end = DateTime.Parse(EndTimeTextBox.Text);
            TimeSpan span = end.Subtract(start);

            if (start < end)
            {
                DataTable ChargeTeble = ViewState["ChargeTeble"] as DataTable;

                bool ifExsists = true;
                foreach (DataRow dr in ChargeTeble.Rows)
                {
                    DateTime startTime = DateTime.Parse(dr["StartTime"].ToString());
                    DateTime endTime = DateTime.Parse(dr["EndTime"].ToString());

                    if (start < endTime && startTime < end)
                    {
                        ifExsists = false;
                    }
                    else
                    {
                        ifExsists = true;
                    }
                }

                if (ifExsists)
                {
                    ChargeTeble.Rows.Add(RoutinePeriodTextBox.Text, StartTimeTextBox.Text, EndTimeTextBox.Text, span.TotalMinutes + " min", OffTime_CheckBox.Checked);
                    ViewState["ChargeTeble"] = ChargeTeble;
                    this.BindGrid();

                    RoutinePeriodTextBox.Text = "";
                    StartTimeTextBox.Text = "";
                    EndTimeTextBox.Text = "";
                    OffTime_CheckBox.Checked = false;
                }
                else
                {
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('This time already exists')", true);
                }
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Invalid date')", true);
            }

        }

        //Add Routine Name
        protected void RoutineNameButton_Click(object sender, EventArgs e)
        {
            RoutineInfoSQL.Insert();
            Response.Redirect(Request.Url.AbsoluteUri);
        }

        protected void CompleteButton_Click(object sender, EventArgs e)
        {
            bool IS_Checked = false;
            foreach (ListItem item in WeekNameCheckBoxList.Items)
            {
                if (item.Selected)
                {
                    IS_Checked = true;
                }
            }


            if (IS_Checked)
            {
                foreach (ListItem item in WeekNameCheckBoxList.Items)
                {
                    if (item.Selected)
                    {
                        RoutineDaySQL.InsertParameters["Day"].DefaultValue = item.Value;
                        RoutineDaySQL.Insert();
                    }
                }

                foreach (GridViewRow row in RoutineGridView.Rows)
                {
                    var OffTime_CheckBox = row.FindControl("Show_OffTime_CheckBox") as CheckBox;

                    RoutineTimeSQL.InsertParameters["RoutinePeriod"].DefaultValue = row.Cells[0].Text;
                    RoutineTimeSQL.InsertParameters["StartTime"].DefaultValue = row.Cells[1].Text;
                    RoutineTimeSQL.InsertParameters["EndTime"].DefaultValue = row.Cells[2].Text;
                    RoutineTimeSQL.InsertParameters["Duration"].DefaultValue = row.Cells[3].Text;
                    RoutineTimeSQL.InsertParameters["Is_OffTime"].DefaultValue = OffTime_CheckBox.Checked.ToString();
                    RoutineTimeSQL.Insert();
                }

                Response.Redirect("Assign_Teacher_And_Subject_in_the_Created_Routines.aspx");
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Select day for class Routine.')", true);
            }
        }

    }
}