using Microsoft.Reporting.WebForms;
using System;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee
{
    public partial class Salary_PaidDue_Report : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            PaidDueReportViewer.LocalReport.Refresh();
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

            PaidDueODS.SelectParameters["RoleIDs"].DefaultValue = Roleids;

            ReportParameter[] Para = new ReportParameter[2];
            Para[0] = new ReportParameter("SchoolName", Session["School_Name"].ToString());
            Para[1] = new ReportParameter("Role", Role.TrimEnd(','));
            PaidDueReportViewer.LocalReport.SetParameters(Para);
        }

        protected void RoleListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Filter();
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            Filter();
        }
    }
}