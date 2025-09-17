using Microsoft.Reporting.WebForms;
using System;

namespace EDUCATION.COM.Student.Exam
{
    public partial class Cumulative_Exam : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void Cum_ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cu_ResultReportViewer.LocalReport.Refresh();
            this.Cu_ResultReportViewer.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(Cum_ResultReportViewer_SubreportProcessing);
        }
        void Cum_ResultReportViewer_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            e.DataSources.Add(new ReportDataSource("DataSet1", Cu_GradingSystemODS));
        }

    }
}