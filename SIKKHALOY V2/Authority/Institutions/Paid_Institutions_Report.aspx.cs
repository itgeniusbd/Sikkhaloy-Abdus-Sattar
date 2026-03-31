using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority.Institutions
{
    public partial class Paid_Institutions_Report : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Default: current month (1st to today)
                DateTime today = DateTime.Today;
                DateTime firstOfMonth = new DateTime(today.Year, today.Month, 1);

                FromDateTextBox.Text = firstOfMonth.ToString("dd MMM yyyy");
                ToDateTextBox.Text   = today.ToString("dd MMM yyyy");

                LoadReport(firstOfMonth, today);
            }
        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";

            DateTime fromDate, toDate;

            if (!DateTime.TryParse(FromDateTextBox.Text.Trim(), out fromDate))
            {
                ErrorLabel.Text = "From Date is invalid. Please select a valid date.";
                SummaryPanel.Visible = false;
                return;
            }
            if (!DateTime.TryParse(ToDateTextBox.Text.Trim(), out toDate))
            {
                ErrorLabel.Text = "To Date is invalid. Please select a valid date.";
                SummaryPanel.Visible = false;
                return;
            }
            if (fromDate > toDate)
            {
                ErrorLabel.Text = "From Date cannot be greater than To Date.";
                SummaryPanel.Visible = false;
                return;
            }

            LoadReport(fromDate, toDate);
        }

        private void LoadReport(DateTime fromDate, DateTime toDate)
        {
            string categoryID = CategoryDropDownList.SelectedValue;
            int    schoolID   = 0;
            int.TryParse(InstitutionDropDownList.SelectedValue, out schoolID);

            PrintPeriodLabel.Text = fromDate.ToString("dd MMM yyyy") + " — " + toDate.ToString("dd MMM yyyy");

            // Must set Visible = true BEFORE accessing controls inside the Panel
            SummaryPanel.Visible = true;

            // Set parameters on all SqlDataSources
            SetParams(SummarySQL,          fromDate, toDate, categoryID, schoolID);
            SetParams(InstitutionSQL_Data,  fromDate, toDate, categoryID, schoolID);
            SetParams(MonthSQL_Data,        fromDate, toDate, categoryID, schoolID);

            // Due institutions — find controls inside Panel
            var dueSQL  = (SqlDataSource)SummaryPanel.FindControl("DueInstitutionSQL");
            var dueGrid = (GridView)SummaryPanel.FindControl("DueInstitutionGridView");
            if (dueSQL != null)
            {
                dueSQL.SelectParameters["FromDate"].DefaultValue          = fromDate.ToString("yyyy-MM-dd");
                dueSQL.SelectParameters["ToDate"].DefaultValue            = toDate.ToString("yyyy-MM-dd");
                dueSQL.SelectParameters["InvoiceCategoryID"].DefaultValue = categoryID;
                dueSQL.SelectParameters["SchoolID"].DefaultValue          = schoolID.ToString();
            }

            SummaryRepeater.DataBind();
            InstitutionGridView.DataBind();
            if (dueGrid != null) dueGrid.DataBind();
            MonthGridView.DataBind();
        }

        private void SetParams(SqlDataSource ds, DateTime from, DateTime to, string category, int schoolID)
        {
            ds.SelectParameters["FromDate"].DefaultValue          = from.ToString("yyyy-MM-dd");
            ds.SelectParameters["ToDate"].DefaultValue            = to.ToString("yyyy-MM-dd");
            ds.SelectParameters["InvoiceCategoryID"].DefaultValue = category;
            ds.SelectParameters["SchoolID"].DefaultValue          = schoolID.ToString();
        }

        protected void GridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                try
                {
                    decimal due = Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "StillDue"));
                    if (due > 0)
                        e.Row.CssClass = "due-row";
                    else
                        e.Row.CssClass = "paid-row";
                }
                catch { }
            }
        }
    }
}
