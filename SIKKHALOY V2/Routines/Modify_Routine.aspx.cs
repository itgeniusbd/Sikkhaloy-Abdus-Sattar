using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Routines
{
    public partial class Modify_Routine : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Section"] = SectionDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Group"] = GroupDropDownList.SelectedValue;

            if (!IsPostBack)
            {
                GroupDropDownList.Visible = false;
                SectionDropDownList.Visible = false;
                ShiftDropDownList.Visible = false;
            }
        }

        protected void view()
        {
            MassageLabel.Text = string.Empty;
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

        //Class DDL
        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Group"] = "0";
            Session["Shift"] = "0";
            Session["Section"] = "0";

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
            GroupDropDownList.Items.Insert(0, new ListItem("[ ALL GROUP ]", "0"));
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
            SectionDropDownList.Items.Insert(0, new ListItem("[ ALL SECTION ]", "0"));
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
            ShiftDropDownList.Items.Insert(0, new ListItem("[ ALL SHIFT ]", "0"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }

        //End DD

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            bool Is_Teacher_Not_assigned = true;

            foreach (GridViewRow row in DayGridView.Rows)
            {
                DataList Routine = (DataList)row.FindControl("SubteacherDataList");
                string Day = row.Cells[0].Text;

                foreach (DataListItem item in Routine.Items)
                {
                    string RoutineTimeID = Routine.DataKeys[item.ItemIndex].ToString();

                    DropDownList Teacher = (DropDownList)item.FindControl("TeacherDropDownList");
                    string TeacherID = Teacher.SelectedValue;

                    SqlCommand cmd = new SqlCommand("SELECT RoutineForClass.RoutineForClassID, RoutineTime.StartTime, RoutineTime.EndTime FROM RoutineForClass INNER JOIN RoutineTime ON RoutineForClass.RoutineTimeID = RoutineTime.RoutineTimeID WHERE (RoutineForClass.SchoolID = @SchoolID) AND (RoutineForClass.TeacherID = @TeacherID) AND (RoutineForClass.TeacherID <> 0) AND (RoutineForClass.Day = @Day) AND (RoutineForClass.RoutineInfoID <> @RoutineInfoID)  AND (((RoutineTime.StartTime <= @S_time) AND (RoutineTime.EndTime > @S_time)) or ((RoutineTime.StartTime < @E_time) AND (RoutineTime.EndTime >= @E_time)) or ((RoutineTime.StartTime >= @S_time) AND (RoutineTime.EndTime <= @E_time)))", con);
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                    cmd.Parameters.AddWithValue("@TeacherID", TeacherID);
                    cmd.Parameters.AddWithValue("@Day", Day);
                    cmd.Parameters.AddWithValue("@RoutineInfoID", SpecificationDropDownList.SelectedValue);

                    HiddenField S_time = (HiddenField)item.FindControl("S_time_HiddenField");
                    HiddenField E_time = (HiddenField)item.FindControl("E_time_HiddenField");

                    cmd.Parameters.AddWithValue("@S_time", S_time.Value);
                    cmd.Parameters.AddWithValue("@E_time", E_time.Value);

                    con.Open();
                    object CheckTeacher = cmd.ExecuteScalar();

                    if (CheckTeacher != null)
                    {
                        SqlCommand Periodcmd = new SqlCommand("Select RoutinePeriod from RoutineTime where SchoolID = @SchoolID AND RoutineTimeID = @RoutineTimeID", con);
                        Periodcmd.Parameters.AddWithValue("@RoutineTimeID", RoutineTimeID);
                        Periodcmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());

                        MassageLabel.Text = "On [" + Day + "] day in [" + Periodcmd.ExecuteScalar().ToString() + "] " + Teacher.SelectedItem.Text + " alrady assigned in another class";
                        Is_Teacher_Not_assigned = false;

                    }
                    con.Close();
                }
            }



            if (Is_Teacher_Not_assigned)
            {
                MassageLabel.Text = string.Empty;
                foreach (GridViewRow row in DayGridView.Rows)
                {
                    DataList Routine = (DataList)row.FindControl("SubteacherDataList");

                    ClassRoutineSQL.UpdateParameters["Day"].DefaultValue = row.Cells[0].Text;

                    foreach (DataListItem item in Routine.Items)
                    {
                        ClassRoutineSQL.UpdateParameters["RoutineTimeID"].DefaultValue = Routine.DataKeys[item.ItemIndex].ToString();

                        DropDownList Subject = (DropDownList)item.FindControl("SubjectDropDownList");
                        ClassRoutineSQL.UpdateParameters["SubjectID"].DefaultValue = Subject.SelectedValue;

                        DropDownList Teacher = (DropDownList)item.FindControl("TeacherDropDownList");
                        ClassRoutineSQL.UpdateParameters["TeacherID"].DefaultValue = Teacher.SelectedValue;

                        ClassRoutineSQL.Update();
                    }
                }


                //for empty the grid view

                ClassDropDownList.SelectedIndex = 0;

                Session["Group"] = "0";
                Session["Section"] = "0";
                Session["Shift"] = "0";

                GroupDropDownList.DataBind();
                SectionDropDownList.DataBind();
                ShiftDropDownList.DataBind();
                view();
                DayGridView.DataBind();

                MassageLabel.Text = "Routine has been updated successfully!";
            }
        }

        protected void SubteacherDataList_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            DropDownList Teacher = (DropDownList)e.Item.FindControl("TeacherDropDownList");
            HiddenField TeacherID_HiddenField = (HiddenField)e.Item.FindControl("TeacherID_HiddenField");

            Teacher.SelectedValue = TeacherID_HiddenField.Value;
        }

        protected void SpecificationDropDownList_DataBound(object sender, EventArgs e)
        {
            SpecificationDropDownList.Items.Insert(0, new ListItem("[ ROUTINE NAME ]", "0"));
        }

        protected void TeacherDropDownList_DataBound(object sender, EventArgs e)
        {
            var TeacherDropDownList = sender as DropDownList;
            TeacherDropDownList.Items.Insert(0, new ListItem("[ No Class ]", "0"));

            var TeacherID_HiddenField = ((Control)sender).Parent.FindControl("TeacherID_HiddenField") as HiddenField;

            if (TeacherDropDownList.Items.FindByValue(TeacherID_HiddenField.Value) != null)
                TeacherDropDownList.SelectedValue = TeacherID_HiddenField.Value;
        }
    }
}