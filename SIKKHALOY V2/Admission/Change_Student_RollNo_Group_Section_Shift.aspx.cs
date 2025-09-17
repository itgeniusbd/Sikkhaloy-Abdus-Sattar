using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Admission
{
    public partial class Change_Student_RollNo_Group_Section_Shift : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;

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
        protected void IDFindButton_Click(object sender, EventArgs e)
        {
            StudentInfoFormView.DataBind();

            GroupSQL.SelectParameters["ClassID"].DefaultValue = StudentInfoFormView.DataKey["ClassID"].ToString();
            ShiftSQL.SelectParameters["ClassID"].DefaultValue = StudentInfoFormView.DataKey["ClassID"].ToString();
            SectionSQL.SelectParameters["ClassID"].DefaultValue = StudentInfoFormView.DataKey["ClassID"].ToString();


            ShiftSQL.SelectParameters["SubjectGroupID"].DefaultValue = "%";
            SectionSQL.SelectParameters["SubjectGroupID"].DefaultValue = "%";

            GroupSQL.SelectParameters["SectionID"].DefaultValue = "%";
            ShiftSQL.SelectParameters["SectionID"].DefaultValue = "%";


            GroupSQL.SelectParameters["ShiftID"].DefaultValue = "%";
            SectionSQL.SelectParameters["ShiftID"].DefaultValue = "%";

            RollNumberTextBox.Text = StudentInfoFormView.DataKey["RollNo"].ToString();

            Session["Group"] = "%";
            Session["Shift"] = "%";
            Session["Section"] = "%";

            GroupDropDownList.DataBind();
            ShiftDropDownList.DataBind();
            SectionDropDownList.DataBind();

            if (ShiftDropDownList.Items.FindByValue(StudentInfoFormView.DataKey["ShiftID"].ToString()) != null)
            {
                ShiftDropDownList.SelectedValue = StudentInfoFormView.DataKey["ShiftID"].ToString();
            }

            if (SectionDropDownList.Items.FindByValue(StudentInfoFormView.DataKey["SectionID"].ToString()) != null)
            {
                SectionDropDownList.SelectedValue = StudentInfoFormView.DataKey["SectionID"].ToString();
            }

            if (GroupDropDownList.Items.FindByValue(StudentInfoFormView.DataKey["SubjectGroupID"].ToString()) != null)
            {
                GroupDropDownList.SelectedValue = StudentInfoFormView.DataKey["SubjectGroupID"].ToString();
            }

            view();
        }

        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ SELECT GROUP ]", "%"));
            if (IsPostBack)
                GroupDropDownList.Items.FindByValue(Session["Group"].ToString()).Selected = true;
        }

        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShiftSQL.SelectParameters["SubjectGroupID"].DefaultValue = GroupDropDownList.SelectedValue;
            SectionSQL.SelectParameters["SubjectGroupID"].DefaultValue = GroupDropDownList.SelectedValue;
            view();
        }

        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ SELECT SECTION ]", "%"));
            if (IsPostBack)
                SectionDropDownList.Items.FindByValue(Session["Section"].ToString()).Selected = true;
        }

        protected void SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            GroupSQL.SelectParameters["SectionID"].DefaultValue = SectionDropDownList.SelectedValue;
            ShiftSQL.SelectParameters["SectionID"].DefaultValue = SectionDropDownList.SelectedValue;
            view();
        }

        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            ShiftDropDownList.Items.Insert(0, new ListItem("[ SELECT SHIFT ]", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }

        protected void ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            GroupSQL.SelectParameters["ShiftID"].DefaultValue = ShiftDropDownList.SelectedValue;
            SectionSQL.SelectParameters["ShiftID"].DefaultValue = ShiftDropDownList.SelectedValue;
            view();
        }

        protected void UpdateButton_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            string quary = "";
            SqlCommand cmd = new SqlCommand();

            cmd.Parameters.AddWithValue("@RollNo", RollNumberTextBox.Text);
            cmd.Parameters.AddWithValue("@StudentClassID", StudentInfoFormView.DataKey["StudentClassID"].ToString());

            quary = "UPDATE StudentsClass SET ";

            if (GroupDropDownList.Visible)
            {
                quary += "SubjectGroupID = @SubjectGroupID ,";
                cmd.Parameters.AddWithValue("@SubjectGroupID", GroupDropDownList.SelectedValue);
            }

            if (SectionDropDownList.Visible)
            {
                quary += "SectionID = @SectionID ,";
                cmd.Parameters.AddWithValue("@SectionID", SectionDropDownList.SelectedValue);
            }
            if (ShiftDropDownList.Visible)
            {
                quary += "ShiftID = @ShiftID ,";
                cmd.Parameters.AddWithValue("@ShiftID", ShiftDropDownList.SelectedValue);
            }

            quary += " RollNo = @RollNo WHERE (StudentClassID = @StudentClassID)";

            cmd.Connection = con;
            cmd.CommandText = quary;

            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();

            StudentInfoFormView.DataBind();
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Record Updated Successfully')", true);
        }
    }
}