using Microsoft.Reporting.WebForms;
using System;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Exam.Result
{
    public partial class Analytical_Smart_Result : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.UnsuccessfulRV.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(ResultReportViewer_SubreportProcessing);
            this.Pass_FailRV.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(ResultReportViewer_SubreportProcessing);
            this.Individual_SubjectRV.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(ResultReportViewer_SubreportProcessing);
            this.Examinee_Vs_GradeRV.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(ResultReportViewer_SubreportProcessing);
        }


        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ReportParameter[] Para = new ReportParameter[2];
            Para[0] = new ReportParameter("Class", ClassDropDownList.SelectedItem.Text);
            Para[1] = new ReportParameter("Exam", ExamDropDownList.SelectedItem.Text);

            Examinee_Vs_GradeRV.LocalReport.SetParameters(Para);
            Individual_SubjectRV.LocalReport.SetParameters(Para);
            UnsuccessfulRV.LocalReport.SetParameters(Para);
            Pass_FailRV.LocalReport.SetParameters(Para);

            Examinee_Vs_GradeRV.LocalReport.Refresh();
            Individual_SubjectRV.LocalReport.Refresh();
            UnsuccessfulRV.LocalReport.Refresh();
            Pass_FailRV.LocalReport.Refresh();
        }

        void ResultReportViewer_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            e.DataSources.Add(new ReportDataSource("SchoolDS", SchoolInfoODS));
        }

        protected void ExamDropDownList_DataBound(object sender, EventArgs e)
        {
            ExamDropDownList.Items.Insert(0, new ListItem("[ SELECT ]", "0"));
        }
    }
}