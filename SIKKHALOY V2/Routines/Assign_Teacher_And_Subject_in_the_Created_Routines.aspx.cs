using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ROUTINES
{
    public partial class Assign_Teacher_And_Subject_in_the_Created_Routines__ : System.Web.UI.Page
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

        //End DD

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            bool Check_Routine_bool = true;

            SqlCommand Check_cmd = new SqlCommand("SELECT RoutineForClassID FROM RoutineForClass WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (RoutineInfoID = @RoutineInfoID) AND (SubjectGroupID Like @SubjectGroupID) AND (ShiftID Like @ShiftID) AND (SectionID Like @SectionID)", con);
            Check_cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            Check_cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"].ToString());
            Check_cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
            Check_cmd.Parameters.AddWithValue("@RoutineInfoID", SpecificationDropDownList.SelectedValue);
            Check_cmd.Parameters.AddWithValue("@SubjectGroupID", GroupDropDownList.SelectedValue);
            Check_cmd.Parameters.AddWithValue("@ShiftID", ShiftDropDownList.SelectedValue);
            Check_cmd.Parameters.AddWithValue("@SectionID", SectionDropDownList.SelectedValue);

            con.Open();
            object Check_Routine_obj = Check_cmd.ExecuteScalar();
            con.Close();

            if (Check_Routine_obj != null)
            {
                Check_Routine_bool = false;
            }


            if (Check_Routine_bool)
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

                        SqlCommand cmd = new SqlCommand("SELECT RoutineForClass.RoutineForClassID, RoutineTime.StartTime, RoutineTime.EndTime FROM RoutineForClass INNER JOIN RoutineTime ON RoutineForClass.RoutineTimeID = RoutineTime.RoutineTimeID WHERE (RoutineForClass.SchoolID = @SchoolID) AND (RoutineForClass.TeacherID = @TeacherID) AND (RoutineForClass.TeacherID <> 0) AND (RoutineForClass.Day = @Day)  AND (((RoutineTime.StartTime <= @S_time) AND (RoutineTime.EndTime > @S_time)) or ((RoutineTime.StartTime < @E_time) AND (RoutineTime.EndTime >= @E_time)) or ((RoutineTime.StartTime >= @S_time) AND (RoutineTime.EndTime <= @E_time)))", con);
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                        cmd.Parameters.AddWithValue("@TeacherID", TeacherID);
                        cmd.Parameters.AddWithValue("@Day", Day);

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

                        ClassRoutineSQL.InsertParameters["Day"].DefaultValue = row.Cells[0].Text;

                        foreach (DataListItem item in Routine.Items)
                        {
                            ClassRoutineSQL.InsertParameters["RoutineTimeID"].DefaultValue = Routine.DataKeys[item.ItemIndex].ToString();

                            DropDownList Subject = (DropDownList)item.FindControl("SubjectDropDownList");
                            ClassRoutineSQL.InsertParameters["SubjectID"].DefaultValue = Subject.SelectedValue;

                            DropDownList Teacher = (DropDownList)item.FindControl("TeacherDropDownList");
                            ClassRoutineSQL.InsertParameters["TeacherID"].DefaultValue = Teacher.SelectedValue;

                            if (SectionDropDownList.SelectedValue == "%")
                            {
                                ClassRoutineSQL.InsertParameters["SectionID"].DefaultValue = "0";
                            }
                            else
                            {
                                ClassRoutineSQL.InsertParameters["SectionID"].DefaultValue = SectionDropDownList.SelectedValue;
                            }


                            if (GroupDropDownList.SelectedValue == "%")
                            {
                                ClassRoutineSQL.InsertParameters["SubjectGroupID"].DefaultValue = "0";
                            }
                            else
                            {
                                ClassRoutineSQL.InsertParameters["SubjectGroupID"].DefaultValue = GroupDropDownList.SelectedValue;
                            }

                            if (ShiftDropDownList.SelectedValue == "%")
                            {
                                ClassRoutineSQL.InsertParameters["ShiftID"].DefaultValue = "0";
                            }
                            else
                            {
                                ClassRoutineSQL.InsertParameters["ShiftID"].DefaultValue = ShiftDropDownList.SelectedValue;
                            }

                            ClassRoutineSQL.Insert();
                        }
                    }

                    DayGridView.DataBind();
                    RoutineGridView.DataBind();

                    MassageLabel.Text = "Routine has been successfully assigned !";
                    MassageLabel.ForeColor = Color.Green;
                }
            }
            else
            {
                MassageLabel.Text = "Routine already generated!";
                MassageLabel.ForeColor = Color.Red;
            }
        }

        protected void TeacherDropDownList_DataBound(object sender, EventArgs e)
        {
            var TeacherDropDownList = sender as DropDownList;
            TeacherDropDownList.Items.Insert(0, new ListItem("[ Select Teacher ]", "0"));
        }

        protected void DeleteRoutine_Button_Click(object sender, EventArgs e)
        {
            RoutineSQL.Delete();
            DayGridView.DataBind();
            RoutineGridView.DataBind();
        }
    }
}