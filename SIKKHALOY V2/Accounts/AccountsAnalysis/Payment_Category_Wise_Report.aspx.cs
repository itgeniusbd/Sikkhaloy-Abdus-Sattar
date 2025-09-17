using Microsoft.Reporting.WebForms;
using System;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Accounts.AccountsAnalysis
{
    public partial class Payment_Category_Wise_Report : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                DateTime date = DateTime.Now;
                var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);

                FormDateTextBox.Text = firstDayOfMonth.ToString("d MMM yyyy");
                ToDateTextBox.Text = DateTime.Now.ToString("d MMM yyyy");
            }

            Daily_Income_ReportViewer.LocalReport.Refresh();
            Monthly_Income_ReportViewer.LocalReport.Refresh();

            Daily_Expense_ReportViewer.LocalReport.Refresh();
            Monthly_Expense_ReportViewer.LocalReport.Refresh();

            Student_Icome_ReportViewer.LocalReport.Refresh();
        }

        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ ALL SECTION ]", "%"));
        }

        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Filter();
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            Filter();
        }

        protected void DateButton_Click(object sender, EventArgs e)
        {
            string from = "";
            if (FormDateTextBox.Text != "")
            {
                from = FormDateTextBox.Text + " To ";
            }

            ReportParameter[] Para = new ReportParameter[3];
            Para[0] = new ReportParameter("SchoolName", Session["School_Name"].ToString());
            Para[1] = new ReportParameter("FromDate", from);
            Para[2] = new ReportParameter("ToDate", ToDateTextBox.Text);

            Daily_Expense_ReportViewer.LocalReport.SetParameters(Para);
            Monthly_Expense_ReportViewer.LocalReport.SetParameters(Para);

            Daily_Income_ReportViewer.LocalReport.SetParameters(Para);
            Monthly_Income_ReportViewer.LocalReport.SetParameters(Para);
        }

        public void Filter()
        {
            string Roleids = "";
            string Role = "";
            foreach (ListItem item in RoleListBox.Items)
            {
                if (item.Selected)
                {
                    Roleids += item.Value + ",";
                    Role += item.Text + ",";
                }
            }

            Student_Inc_ODS.SelectParameters["RoleIDs"].DefaultValue = Roleids;

            ReportParameter[] Para = new ReportParameter[4];
            Para[0] = new ReportParameter("Class", ClassDropDownList.SelectedItem.Text);
            Para[1] = new ReportParameter("Section", SectionDropDownList.SelectedItem.Text);
            Para[2] = new ReportParameter("SchoolName", Session["School_Name"].ToString());
            Para[3] = new ReportParameter("Role", Role.TrimEnd(','));
            Student_Icome_ReportViewer.LocalReport.SetParameters(Para);
        }
    }
}