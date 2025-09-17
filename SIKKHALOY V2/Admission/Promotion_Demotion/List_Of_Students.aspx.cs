using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Admission.Promotion_Demotion
{
    public partial class List_Of_Students : System.Web.UI.Page
    {

        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;
            

            Session["Re_Group"] = Re_GroupDropDownList.SelectedValue;
            Session["Re_Shift"] = Re_ShiftDropDownList.SelectedValue;
            Session["Re_Section"] = Re_SectionDropDownList.SelectedValue;
            if (!IsPostBack)
            {
                GroupDropDownList.Visible = false;
                SectionDropDownList.Visible = false;
                ShiftDropDownList.Visible = false;
                

                Re_GroupDropDownList.Visible = false;
                Re_SectionDropDownList.Visible = false;
                Re_ShiftDropDownList.Visible = false;
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
            StudentsGridView.DataSource = ShowStudentClassSQL;
            StudentsGridView.DataBind();
        }

        //---------------------------------------Class DDL-------------------------------------------
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

        //---------------------------------------Group DDL-------------------------------------------
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


        //---------------------------------------Section DDL-------------------------------------------
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

        //---------------------------------------Shift DDL-------------------------------------------
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


        protected void SessionYearDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            StudentsGridView.DataSource = ShowStudentClassSQL;
            StudentsGridView.DataBind();
        }

        //---------------New Part--------------------//
        protected void Re_view()
        {
            DataView GroupDV = new DataView();
            GroupDV = (DataView)Re_GroupSQL.Select(DataSourceSelectArguments.Empty);
            if (GroupDV.Count < 1)
            {
                Re_GroupDropDownList.Visible = false;
            }
            else
            {
                Re_GroupDropDownList.Visible = true;
            }

            DataView SectionDV = new DataView();
            SectionDV = (DataView)Re_SectionSQL.Select(DataSourceSelectArguments.Empty);
            if (SectionDV.Count < 1)
            {
                Re_SectionDropDownList.Visible = false;
            }
            else
            {
                Re_SectionDropDownList.Visible = true;
            }

            DataView ShiftDV = new DataView();
            ShiftDV = (DataView)Re_ShiftSQL.Select(DataSourceSelectArguments.Empty);
            if (ShiftDV.Count < 1)
            {
                Re_ShiftDropDownList.Visible = false;
            }
            else
            {
                Re_ShiftDropDownList.Visible = true;
            }
        }
       
        //---------------------------------------New Class DDL-------------------------------------------

        
        protected void Re_ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Re_Group"] = "%";
            Session["Re_Shift"] = "%";
            Session["Re_Section"] = "%";

            string value = Re_ClassDropDownList.SelectedValue;
            Session["NewClassID"] = value;

            Re_GroupDropDownList.DataBind();
            Re_ShiftDropDownList.DataBind();
            Re_SectionDropDownList.DataBind();
            ErrorLabel.Text = "";
            Re_view();
        }
        //---------------------------------------New Group DDL-------------------------------------------
        protected void NewGroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Re_view();
        }

        //--Group DDL--
        protected void Re_GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Re_view();
        }

        protected void Re_GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            Re_GroupDropDownList.Items.Insert(0, new ListItem("[ SELECT GROUP ]", "%"));
            if (IsPostBack)
                Re_GroupDropDownList.Items.FindByValue(Session["Re_Group"].ToString()).Selected = true;
        }

        //--Section DDL--
        protected void Re_SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Re_view();
        }

        protected void Re_SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            Re_SectionDropDownList.Items.Insert(0, new ListItem("[ SELECT SECTION ]", "%"));
            if (IsPostBack)
                Re_SectionDropDownList.Items.FindByValue(Session["Re_Section"].ToString()).Selected = true;
        }

        //--Shift DDL--
        protected void Re_ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Re_view();
        }

        protected void Re_ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            Re_ShiftDropDownList.Items.Insert(0, new ListItem("[ SELECT SHIFT ]", "%"));
            if (IsPostBack)
                Re_ShiftDropDownList.Items.FindByValue(Session["Re_Shift"].ToString()).Selected = true;
        }        
        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            
            if (IsValidate()!=true)
            {
                btnSubmit.Enabled = false;
                SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

                string StudentID = "";
                string StudentClassID = "";

                TextBox Merit_StatusTextBox = new TextBox();
                CheckBox SingleCheckBox = new CheckBox();
                CheckBox SubjectCheckBox = new CheckBox();
                RadioButtonList SubjectType = new RadioButtonList();

                foreach (GridViewRow Row in StudentsGridView.Rows)
                {
                    SingleCheckBox = Row.FindControl("SingleCheckBox") as CheckBox;

                    if (SingleCheckBox.Checked)
                    {
                        StudentID = StudentsGridView.DataKeys[Row.DataItemIndex]["StudentID"].ToString();
                        Session["studentId"] = StudentID;

                        //----------
                        string edu_year = Session["Edu_Year"].ToString();
                        string schoolId = Session["SchoolID"].ToString();

                        SqlCommand StudnetIDcommand = new SqlCommand("SELECT StudentClassID FROM StudentsClass WHERE (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (StudentID = @StudentID)", con);
                        StudnetIDcommand.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                        StudnetIDcommand.Parameters.AddWithValue("@EducationYearID", edu_year);
                        StudnetIDcommand.Parameters.AddWithValue("@StudentID", StudentID);
                        con.Open();
                        object StudentID_Check = StudnetIDcommand.ExecuteScalar();
                        con.Close();
                        Merit_StatusTextBox = Row.FindControl("Merit_StatusTextBox") as TextBox;
                        #region Class-group-section-shift dropdownlist

                        if (Re_GroupDropDownList.SelectedValue == "%")
                        {
                            StudentClassSQL.InsertParameters["SubjectGroupID"].DefaultValue = "0";
                        }
                        else
                        {
                            StudentClassSQL.InsertParameters["SubjectGroupID"].DefaultValue = Re_GroupDropDownList.SelectedValue;
                        }

                        if (Re_SectionDropDownList.SelectedValue == "%")
                        {
                            StudentClassSQL.InsertParameters["SectionID"].DefaultValue = "0";
                        }
                        else
                        {
                            StudentClassSQL.InsertParameters["SectionID"].DefaultValue = Re_SectionDropDownList.SelectedValue;
                        }

                        if (Re_ShiftDropDownList.SelectedValue == "%")
                        {
                            StudentClassSQL.InsertParameters["ShiftID"].DefaultValue = "0";
                        }
                        else
                        {
                            StudentClassSQL.InsertParameters["ShiftID"].DefaultValue = Re_ShiftDropDownList.SelectedValue;
                        }
                        #endregion

                        StudentClassSQL.InsertParameters["StudentID"].DefaultValue = StudentID;
                        StudentClassSQL.Insert();  //----StudentClassId Creation

                        //---- Get StudentClass Last Id

                        SqlCommand cmd1 = new SqlCommand("Select TOP 1 * from StudentsClass where StudentID='" + StudentID + "' and SchoolID='" + schoolId + "' order by StudentClassID desc ", con);
                        con.Open();
                        Session["StudentClassLastID"] = cmd1.ExecuteScalar().ToString();
                        string lastId = Session["StudentClassLastID"].ToString();
                        con.Close();

                        //-- Current StudentClassId

                        

                        SqlCommand cmd = new SqlCommand("Select IDENT_CURRENT('StudentsClass')", con);
                        con.Open();
                        Session["StudentClassID"] = cmd.ExecuteScalar().ToString();
                        
                        con.Close();
                        foreach (GridViewRow row in GroupGridView.Rows) //insert subject
                        {
                            SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                            SubjectType = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");

                            if (SubjectCheckBox.Checked)
                            {
                                StudentRecordSQL.InsertParameters["StudentID"].DefaultValue = StudentID;
                                StudentRecordSQL.InsertParameters["SubjectID"].DefaultValue = GroupGridView.DataKeys[row.DataItemIndex]["SubjectID"].ToString();
                                StudentRecordSQL.InsertParameters["StudentClassID"].DefaultValue = StudentClassID;
                                StudentRecordSQL.InsertParameters["SubjectType"].DefaultValue = SubjectType.SelectedValue;
                                StudentRecordSQL.Insert();
                            }
                        }
                        StudentClassSQL.UpdateParameters["StudentClassID"].DefaultValue = StudentsGridView.DataKeys[Row.DataItemIndex]["StudentClassID"].ToString();

                        StudentClassSQL.Update();  //Class Changing status update




                        if (KeepPayOrderCheckbox.Checked)
                        {
                            UpdateIncomePayorder.Update();
                        }
                        else
                        {
                            string value= StudentID_Check.ToString();
                            UpdatePaymantSQL.DeleteParameters["StudentClassID"].DefaultValue = value;
                            UpdatePaymantSQL.Delete(); //Delete Unpaid Record
                        }
                        UpdatePaymantSQL.Update(); //Update Payment                                                   
                    }                    
                }                
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alert", "alert('Class Changed Successfully!');" +
                 "window.location='List_Of_Students.aspx';", true);
                btnSubmit.Enabled = true;
            }            
        }
        //----- Validation Method
        private bool IsValidate()  
        {
            CheckBox SingleCheckBox = new CheckBox();
            ErrorLabel.Text = "";
            bool check=true;
            bool check1=false;
            foreach (GridViewRow Row in StudentsGridView.Rows)
            {
                SingleCheckBox = Row.FindControl("SingleCheckBox") as CheckBox;
                if (SingleCheckBox.Checked == false)
                {
                    check = false;
                }
                else { check1 = true; }
            }
            bool flag = false;
            if (ClassDropDownList.SelectedValue == Re_ClassDropDownList.SelectedValue)
            {
                ErrorLabel.Text = "You are selected same class.Please select different class !";
                flag = true;
            }
            else if(check1 == false)
            {
                ErrorLabel.Text = "Please check at least one student !";
                flag = true;
            }
            return flag;
        }
    }
}