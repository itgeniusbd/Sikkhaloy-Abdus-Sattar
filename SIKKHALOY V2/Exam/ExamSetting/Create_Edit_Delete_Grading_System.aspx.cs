using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.EXAM.ExamSetting
{
    public partial class Create_Edit_Delete_Grading_System : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                DataTable ChargeTeble = new DataTable();
                ChargeTeble.Columns.AddRange(new DataColumn[5] { new DataColumn("MaxPercentage"), new DataColumn("MinPercentage"), new DataColumn("Grades"), new DataColumn("Point"), new DataColumn("Comments") });
                ViewState["ChargeTeble"] = ChargeTeble;
            }
        }

        protected void BindGrid()
        {
            AddGrading_System_GridView.DataSource = ViewState["ChargeTeble"] as DataTable;
            AddGrading_System_GridView.DataBind();
        }

        protected void RowDelete(object sender, EventArgs e)
        {
            GridViewRow row = (sender as LinkButton).NamingContainer as GridViewRow;
            DataTable ChargeTeble = ViewState["ChargeTeble"] as DataTable;

            ChargeTeble.Rows.RemoveAt(row.RowIndex);
            ViewState["ChargeTeble"] = ChargeTeble;
            this.BindGrid();
        }

        bool TestRange(double numberToCheck, double bottom, double top)
        {
            return (numberToCheck >= bottom && numberToCheck <= top);
        }

        /*Add To Cart Button*/
        protected void Add_Grading_Button_Click(object sender, EventArgs e)
        {
            bool Check_Grade = true;
            bool Check_Point = true;
            bool Check_MaxMin = true;

            double Bottom;
            double Top;

            double Max = Convert.ToDouble(Max_Marks_TextBox.Text);
            double Min = Convert.ToDouble(Mini_Marks_TextBox.Text);

            if (Max > Min)
            {
                foreach (GridViewRow row in AddGrading_System_GridView.Rows)
                {
                    if (row.Cells[2].Text == Grade_TextBox.Text)
                    {
                        Check_Grade = false;
                    }

                    if (row.Cells[3].Text == Point_TextBox.Text)
                    {
                        Check_Point = false;
                    }

                    Top = Convert.ToDouble(row.Cells[0].Text);
                    Bottom = Convert.ToDouble(row.Cells[1].Text);


                    if (TestRange(Max, Bottom, Top) || TestRange(Min, Bottom, Top))
                    {
                        Check_MaxMin = false;
                    }
                }

                if (Check_Grade)
                {
                    if (Check_Point)
                    {
                        if (Check_MaxMin)
                        {
                            DataTable ChargeTeble = ViewState["ChargeTeble"] as DataTable;
                            ChargeTeble.Rows.Add(Max_Marks_TextBox.Text, Mini_Marks_TextBox.Text, Grade_TextBox.Text, Point_TextBox.Text, Comment_TextBox.Text.Trim());
                            ViewState["ChargeTeble"] = ChargeTeble;
                            this.BindGrid();
                        }
                        else
                        {
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Minimum And Maximum Range Invalid!!')", true);
                        }
                    }
                    else
                    {
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Point Already Exists!!')", true);
                    }
                }
                else
                {
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Grade Already Exists!!')", true);
                }
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Minimum mumber is invalid')", true);
            }
        }

        /*Save Button*/
        protected void Save_Grading_System_Button_Click(object sender, EventArgs e)
        {
            bool Check_Max = false;
            bool Check_Min = false;
            bool Check_F = false;

            foreach (GridViewRow row in AddGrading_System_GridView.Rows)
            {
                if (100 == Convert.ToDouble(row.Cells[0].Text))
                {
                    Check_Max = true;
                }

                if (0 == Convert.ToDouble(row.Cells[1].Text))
                {
                    Check_Min = true;
                }
                if ("F" == row.Cells[2].Text)
                {
                    Check_F = true;
                }

            }
            if (Check_Max)
            {
                if (Check_Min)
                {
                    if (Check_F)
                    {
                        AddNameSQL.Insert();
                        foreach (GridViewRow row in AddGrading_System_GridView.Rows)
                        {
                            Label CommentsLB = (Label)row.FindControl("CommentsLB");
                            Add_GradingSystemSQL.InsertParameters["MaxPercentage"].DefaultValue = row.Cells[0].Text;
                            Add_GradingSystemSQL.InsertParameters["MinPercentage"].DefaultValue = row.Cells[1].Text;
                            Add_GradingSystemSQL.InsertParameters["Grades"].DefaultValue = row.Cells[2].Text;
                            Add_GradingSystemSQL.InsertParameters["Point"].DefaultValue = row.Cells[3].Text;
                            Add_GradingSystemSQL.InsertParameters["Comments"].DefaultValue = CommentsLB.Text;
                            Add_GradingSystemSQL.Insert();
                        }

                        Response.Redirect(Request.Url.AbsoluteUri);
                    }
                    else
                    {
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Grading system must be contain (F) Grade')", true);
                    }
                }
                else
                {
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Grading system must be contain value (0)')", true);
                }
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Grading system must be contain value (100)')", true);
            }
        }

        //Delete grading
        protected void DeleteButton_Command(object sender, CommandEventArgs e)
        {
            DeleteSQL.DeleteParameters["GradeNameID"].DefaultValue = e.CommandName.ToString();
            DeleteSQL.Delete();

            Grading_Repeater.DataBind();
        }

        protected void UpdateLinkButton_Command(object sender, CommandEventArgs e)
        {
            var btn = (LinkButton)sender;
            var item = (RepeaterItem)btn.NamingContainer;
            var GradeName_TextBox = (TextBox)item.FindControl("GradeName_TextBox");

            DeleteSQL.UpdateParameters["GradeNameID"].DefaultValue = e.CommandName.ToString();
            DeleteSQL.UpdateParameters["GradeName"].DefaultValue = GradeName_TextBox.Text;
            DeleteSQL.Update();

            Grading_Repeater.DataBind();
        }
    }
}