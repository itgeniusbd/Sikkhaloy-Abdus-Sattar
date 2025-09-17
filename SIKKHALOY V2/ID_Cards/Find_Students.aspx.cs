using System;
using System.Data;
using System.Web.UI;

namespace EDUCATION.COM.ID_CARDS
{
    public partial class Find_Students : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                DataView dv = (DataView)AllStudentSQL.Select(DataSourceSelectArguments.Empty);
                StudentCountLabel.Text = "Total: " + dv.Count.ToString() + " Student(s)";
            }
        }

        protected void FindButton_Click(object sender, EventArgs e)
        {
            StudentGridView.DataBind(); // Refresh the GridView with current filter
            DataView dv = (DataView)AllStudentSQL.Select(DataSourceSelectArguments.Empty);
            StudentCountLabel.Text = "Total: " + dv.Count.ToString() + " Student(s)";
        }

        protected void ClearButton_Click(object sender, EventArgs e)
        {
            // Clear the search textbox
            SearchTextBox.Text = "";
            
            // Reset the dropdown to show all students
            OldNewDropDownList.SelectedValue = "%";
            
            // Refresh the GridView
            StudentGridView.DataBind();
            
            // Update the count
            DataView dv = (DataView)AllStudentSQL.Select(DataSourceSelectArguments.Empty);
            StudentCountLabel.Text = "Total: " + dv.Count.ToString() + " Student(s)";
        }
    }
}