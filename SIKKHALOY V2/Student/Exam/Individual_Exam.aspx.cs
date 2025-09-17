using Microsoft.Reporting.WebForms;
using System;

namespace EDUCATION.COM.Student.Exam
{
    public partial class Individual_Exam : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResultReportViewer.LocalReport.Refresh();
            this.ResultReportViewer.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(ResultReportViewer_SubreportProcessing);
        }
        void ResultReportViewer_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            e.DataSources.Add(new ReportDataSource("DataSet1", GradingSystemODS));
            e.DataSources.Add(new ReportDataSource("SchoolDS", SchoolInfoODS));

        }
    }
}