using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Admission
{
    public partial class Print_TC : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (string.IsNullOrEmpty(Request.QueryString["Student"]) || string.IsNullOrEmpty(Request.QueryString["S_Class"]))
                    Response.Redirect("Reject_Student_from_school.aspx");
            }
        }

        protected void Export_Button_Click(object sender, EventArgs e)
        {
            Response.Clear();
            Response.ContentEncoding = Encoding.Unicode;
            Response.BinaryWrite(Encoding.Unicode.GetPreamble());

            Response.AddHeader("content-disposition", "attachment;filename=TC.doc");
            Response.Charset = "";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.ContentType = "application/doc";

            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            // Read Style file (css) here and add to response 
            FileInfo fi = new FileInfo(Server.MapPath("~/Admission/CSS/print-tc.css"));
            StringBuilder sb = new StringBuilder();
            StreamReader sr = fi.OpenText();

            while (sr.Peek() >= 0)
            {
                sb.Append(sr.ReadLine());
            }
            sr.Close();

            Panel Data_Panel = (Panel)TCFormView.FindControl("Data_Panel");

            Data_Panel.RenderControl(htmlWrite);


            Response.Write("<html><head><style type='text/css'>" + sb.ToString() + "</style></head><body>" + stringWrite.ToString() + "</body></html>");
            Response.Write(stringWrite.ToString());
            Response.End();
        }
        public override void VerifyRenderingInServerForm(Control control)
        {
            /* Confirms that an HtmlForm control is rendered for the specified ASP.NET
               server control at run time. */
        }
    }
}