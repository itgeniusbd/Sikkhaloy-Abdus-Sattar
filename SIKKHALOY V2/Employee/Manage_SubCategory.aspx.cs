using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee
{
    public partial class Manage_SubCategory : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e) { }

        protected void AddButton_Click(object sender, EventArgs e)
        {
            string name = SubCategoryNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(name)) { ShowErr("সাব-ক্যাটাগরির নাম লিখুন।"); return; }

            string cs = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand("INSERT INTO Employee_SubCategory(SchoolID,SubCategoryName,EmployeeType) VALUES(@SchoolID,@Name,@Type)", con))
            {
                cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Type", EmpTypeDropDownList.SelectedValue);
                con.Open(); cmd.ExecuteNonQuery();
            }
            SubCategoryNameTextBox.Text = "";
            SubCategoryGridView.DataBind();
            ShowMsg("'" + name + "' সফলভাবে যোগ হয়েছে।");
        }

        protected void SubCategoryGridView_RowDeleting(object sender, GridViewDeleteEventArgs e) { }
        protected void SubCategoryGridView_RowEditing(object sender, GridViewEditEventArgs e)
        {
            SubCategoryGridView.EditIndex = e.NewEditIndex;
            SubCategoryGridView.DataBind();
        }
        protected void SubCategoryGridView_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            SubCategoryGridView.EditIndex = -1;
            SubCategoryGridView.DataBind();
        }
        protected void SubCategoryGridView_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            var row = SubCategoryGridView.Rows[e.RowIndex];
            string newName = ((TextBox)row.FindControl("EditNameTextBox")).Text.Trim();
            string newType = ((DropDownList)row.FindControl("EditTypeDropDownList")).SelectedValue;
            int id = (int)SubCategoryGridView.DataKeys[e.RowIndex].Value;

            string cs = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand("UPDATE Employee_SubCategory SET SubCategoryName=@Name, EmployeeType=@Type WHERE SubCategoryID=@ID AND SchoolID=@SchoolID", con))
            {
                cmd.Parameters.AddWithValue("@Name", newName);
                cmd.Parameters.AddWithValue("@Type", newType);
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                con.Open(); cmd.ExecuteNonQuery();
            }
            SubCategoryGridView.EditIndex = -1;
            SubCategoryGridView.DataBind();
            ShowMsg("আপডেট সফল হয়েছে।");
        }

        private void ShowMsg(string msg) { MsgLabel.Text = msg; MsgLabel.Visible = true; }
        private void ShowErr(string msg) { ErrLabel.Text = msg; ErrLabel.Visible = true; }
    }
}
