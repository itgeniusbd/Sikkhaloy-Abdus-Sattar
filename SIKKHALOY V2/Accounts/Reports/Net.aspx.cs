using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Accounts.Reports
{
    public partial class Net : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                DateTime date = DateTime.Now;
                var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);

                if (string.IsNullOrEmpty(Request.QueryString["Category"]) && string.IsNullOrEmpty(Request.QueryString["All"]))
                {
                    From_Date_TextBox.Text = DateTime.Now.ToString("d MMM yyyy");
                    To_Date_TextBox.Text = DateTime.Now.ToString("d MMM yyyy");
                }
                if (!string.IsNullOrEmpty(Request.QueryString["f"]))
                {
                    From_Date_TextBox.Text = Request.QueryString["f"];
                    To_Date_TextBox.Text = Request.QueryString["t"];
                }
            }
        }

   
        private void GridView_Header_Printer(GridView gridView)
        {
            if (gridView.Rows.Count > 0)
            {
                gridView.UseAccessibleHeader = true;
                gridView.HeaderRow.TableSection = TableRowSection.TableHeader;
            }
        }

        protected void IncomeCategoryGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GridView_Header_Printer(IncomeCategoryGridView);
            }
        }

        protected void Ex_CategoryGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GridView_Header_Printer(Ex_CategoryGridView);
            }
        }

        protected void ClassGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GridView_Header_Printer(ClassGridView);
            }
        }

        protected void DetailsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GridView DetailsGridView = (GridView)e.Row.NamingContainer;
                GridView_Header_Printer(DetailsGridView);
            }
        }
    }
}