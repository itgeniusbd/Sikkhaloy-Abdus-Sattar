using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Admission.Re_Admission
{
    public partial class Assign_Class_And_Subjects : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {

            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;

            if (!IsPostBack)
            {
                string StudentID = Request.QueryString["Student"];

                if (string.IsNullOrEmpty(StudentID))
                    Response.Redirect("Students.aspx");

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
                GRequiredFieldValidator.Visible = false;
            }
            else
            {
                GroupDropDownList.Visible = true;
                GRequiredFieldValidator.Visible = true;
            }

            DataView SectionDV = new DataView();
            SectionDV = (DataView)SectionSQL.Select(DataSourceSelectArguments.Empty);
            if (SectionDV.Count < 1)
            {
                SectionDropDownList.Visible = false;
                SeRequiredFieldValidator.Visible = false;
            }
            else
            {
                SectionDropDownList.Visible = true;
                SeRequiredFieldValidator.Visible = true;
            }

            DataView ShiftDV = new DataView();
            ShiftDV = (DataView)ShiftSQL.Select(DataSourceSelectArguments.Empty);
            if (ShiftDV.Count < 1)
            {
                ShiftDropDownList.Visible = false;
                ShRequiredFieldValidator.Visible = false;
            }
            else
            {
                ShiftDropDownList.Visible = true;
                ShRequiredFieldValidator.Visible = true;
            }
        }
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

        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
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
            SectionDropDownList.Items.Insert(0, new ListItem("[ SELECT SECTION ]", "%"));
            if (IsPostBack)
                SectionDropDownList.Items.FindByValue(Session["Section"].ToString()).Selected = true;
        }

        protected void ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            ShiftDropDownList.Items.Insert(0, new ListItem("[ SELECT SHIFT ]", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }


        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            SqlCommand StudentClass_cmd = new SqlCommand("SELECT * FROM  StudentsClass WHERE (StudentID = @StudentID) AND (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)", con);
            StudentClass_cmd.Parameters.AddWithValue("@StudentID", Request.QueryString["Student"]);
            StudentClass_cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            StudentClass_cmd.Parameters.AddWithValue("@EducationYearID", SessionYearDropDownList.SelectedValue);

            con.Open();
            object StudentClass_Obj = StudentClass_cmd.ExecuteScalar();
            con.Close();

            if (StudentClass_Obj == null)
            {
                #region Class-group-section-shift dropdownlist

                if (GroupDropDownList.SelectedValue == "%")
                {
                    Session["GroupID"] = "0";
                }
                else
                {
                    Session["GroupID"] = GroupDropDownList.SelectedValue;
                }

                if (SectionDropDownList.SelectedValue == "%")
                {
                    Session["SectionID"] = "0";
                }
                else
                {
                    Session["SectionID"] = SectionDropDownList.SelectedValue;
                }

                if (ShiftDropDownList.SelectedValue == "%")
                {
                    Session["ShiftID"] = "0";
                }
                else
                {
                    Session["ShiftID"] = ShiftDropDownList.SelectedValue;
                }
                #endregion

                StudentClassSQL.Insert();
                SqlCommand cmd = new SqlCommand("Select IDENT_CURRENT('StudentsClass')", con);

                con.Open();
                Session["StudentClassID"] = cmd.ExecuteScalar().ToString();
                con.Close();


                foreach (GridViewRow row in GroupGridView.Rows)//insert Compulsory subject
                {
                    CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                    RadioButtonList SubjectType = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");

                    if (SubjectCheckBox.Checked)
                    {
                        StudentRecordSQL.InsertParameters["SubjectID"].DefaultValue = GroupGridView.DataKeys[row.DataItemIndex]["SubjectID"].ToString();
                        StudentRecordSQL.InsertParameters["SubjectType"].DefaultValue = SubjectType.SelectedValue;
                        StudentRecordSQL.Insert();
                    }
                }

                StudentClassSQL.Update();
                Response.Redirect("New_Payorder.aspx?Year=" + SessionYearDropDownList.SelectedValue + "&Student=" + Request.QueryString["Student"].ToString() + "&Class=" + ClassDropDownList.SelectedValue + "&StudentClass=" + Session["StudentClassID"]);

            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('This Student has been already admitted in this session')", true);
            }
        }

        protected void FinishButton_Click(object sender, EventArgs e)
        {
            SqlCommand StudentClass_cmd = new SqlCommand("SELECT * FROM  StudentsClass WHERE (StudentID = @StudentID) AND (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)", con);
            StudentClass_cmd.Parameters.AddWithValue("@StudentID", Request.QueryString["Student"]);
            StudentClass_cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            StudentClass_cmd.Parameters.AddWithValue("@EducationYearID", SessionYearDropDownList.SelectedValue);

            con.Open();
            object StudentClass_Obj = StudentClass_cmd.ExecuteScalar();
            con.Close();

            if (StudentClass_Obj == null)
            {
                #region Class-group-section-shift dropdownlist

                if (GroupDropDownList.SelectedValue == "%")
                {
                    Session["GroupID"] = "0";
                }
                else
                {
                    Session["GroupID"] = GroupDropDownList.SelectedValue;
                }

                if (SectionDropDownList.SelectedValue == "%")
                {
                    Session["SectionID"] = "0";
                }
                else
                {
                    Session["SectionID"] = SectionDropDownList.SelectedValue;
                }

                if (ShiftDropDownList.SelectedValue == "%")
                {
                    Session["ShiftID"] = "0";
                }
                else
                {
                    Session["ShiftID"] = ShiftDropDownList.SelectedValue;
                }
                #endregion

                StudentClassSQL.Insert();
                SqlCommand cmd = new SqlCommand("Select IDENT_CURRENT('StudentsClass')", con);

                con.Open();
                Session["StudentClassID"] = cmd.ExecuteScalar().ToString();
                con.Close();


                foreach (GridViewRow row in GroupGridView.Rows)//insert Compulsory subject
                {
                    CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                    RadioButtonList SubjectType = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");

                    if (SubjectCheckBox.Checked)
                    {
                        StudentRecordSQL.InsertParameters["SubjectID"].DefaultValue = GroupGridView.DataKeys[row.DataItemIndex]["SubjectID"].ToString();
                        StudentRecordSQL.InsertParameters["SubjectType"].DefaultValue = SubjectType.SelectedValue;
                        StudentRecordSQL.Insert();
                    }
                }

                StudentClassSQL.Update();
                Response.Redirect("Students.aspx");
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('This Student has been already admitted in this session')", true);
            }
        }
    }
}