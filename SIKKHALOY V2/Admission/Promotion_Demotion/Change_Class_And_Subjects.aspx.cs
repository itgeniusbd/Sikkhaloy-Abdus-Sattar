using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Admission.Promotion_Demotion
{
    public partial class Change_Class_And_Subjects : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;

            if (!IsPostBack)
            {
                if (string.IsNullOrEmpty(Request.QueryString["Student"]))
                {
                    Response.Redirect("List_Of_Students.aspx");
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
        protected void ClassDropDownList_DataBound(object sender, EventArgs e)
        {
            ClassDropDownList.Items.Insert(0, new ListItem("[ SELECT CLASS ]", "0"));
        }
        //Group DDL
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

        //Section DDL
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

        //Shift DDL
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

            // Check if student already exists in the target class before proceeding
            string studentId = Request.QueryString["Student"].ToString();
            string targetClassID = ClassDropDownList.SelectedValue;
            string schoolId = Session["SchoolID"].ToString();
            string eduYear = Session["Edu_Year"].ToString();

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            SqlCommand checkDuplicateCommand = new SqlCommand(
                "SELECT COUNT(*) FROM StudentsClass WHERE (StudentID = @StudentID) AND (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID)", con);
            checkDuplicateCommand.Parameters.AddWithValue("@SchoolID", schoolId);
            checkDuplicateCommand.Parameters.AddWithValue("@EducationYearID", eduYear);
            checkDuplicateCommand.Parameters.AddWithValue("@StudentID", studentId);
            checkDuplicateCommand.Parameters.AddWithValue("@ClassID", targetClassID);

            con.Open();
            int duplicateCount = (int)checkDuplicateCommand.ExecuteScalar();
            con.Close();

            if (duplicateCount > 0)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage",
                    "alert('⚠️ Cannot change class!\\n\\nThis student already exists in the target class.\\n\\nPlease select a different class.');", true);
                return;
            }

            SubmitButton.Enabled = false;
            bool IS_Optional = false;
            foreach (GridViewRow row in GroupGridView.Rows)
            {
                CheckBox chechBox = (CheckBox)row.FindControl("SubjectCheckBox");
                if (chechBox.Checked)
                {
                    IS_Optional = true;
                }
            }

            if (IS_Optional)
            {
                StudentClassSQL.Insert();
                SqlCommand cmd = new SqlCommand("Select IDENT_CURRENT('StudentsClass')", con);

                con.Open();
                Session["StudentClassID"] = cmd.ExecuteScalar().ToString();
                con.Close();

                foreach (GridViewRow row in GroupGridView.Rows)
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


                StudentClassSQL.Update();  //Class Changing status update
                UpdatePaymantSQL.Delete(); //Delete Unpaid Record
                UpdatePaymantSQL.Update(); //Update Payment

                SubmitButton.Enabled = true;

                Response.Redirect("Payments.aspx?Student=" + Request.QueryString["Student"].ToString() + "&Class=" + ClassDropDownList.SelectedValue + "&StudentClass=" + Session["StudentClassID"]);
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Select subject')", true);
            }
        }
    }
}