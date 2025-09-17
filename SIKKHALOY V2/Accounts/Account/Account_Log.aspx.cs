using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Accounts.Account
{
    public partial class Account_Log1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                FromDateTextBox.Text = DateTime.Today.ToString("d MMMM yyyy");
                ToDateTextBox.Text = DateTime.Today.ToString("d MMMM yyyy");
            }
        }


        protected void In_Export_Button_Click(object sender, EventArgs e)
        {
            HideDate.Visible = false;
            Def_Hide.Visible = false;
            DisDateLabel.Text = GetDateHF.Value;

            if (Session["School_Name"] != null)
            Insti_NameLabel.Text = Session["School_Name"].ToString();

            Response.Clear();
            Response.ContentEncoding = Encoding.Unicode;
            Response.BinaryWrite(Encoding.Unicode.GetPreamble());

            Response.AddHeader("content-disposition", "attachment;filename=Accounts_Log.doc");
            Response.Charset = "";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.ContentType = "application/doc";
           
            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            // Read Style file (css) here and add to response 
            FileInfo fi = new FileInfo(Server.MapPath("~/Accounts/Account/CSS/Accounts_Log.css"));
            StringBuilder sb = new StringBuilder();
            StreamReader sr = fi.OpenText();

            while (sr.Peek() >= 0)
            {
                sb.Append(sr.ReadLine());
            }
            sr.Close();

            Summary.RenderControl(htmlWrite);
            Cash_In.RenderControl(htmlWrite);

            Response.Write("<html><head><style type='text/css'>" + sb.ToString() + "</style></head><body>" + stringWrite.ToString() + "</body></html>");
            Response.Write(stringWrite.ToString());
            Response.End();
        }   
        protected void Out_Export_Button_Click(object sender, EventArgs e)
        {
            Out_Export_Button.Visible = false;
            HideDate.Visible = false;
            Def_Hide.Visible = false;
            DisDateLabel.Text = GetDateHF.Value;

            if (Session["School_Name"] != null)
            Insti_NameLabel.Text = Session["School_Name"].ToString();

            Response.Clear();
            Response.ContentEncoding = Encoding.Unicode;
            Response.BinaryWrite(Encoding.Unicode.GetPreamble());

            Response.AddHeader("content-disposition", "attachment;filename=Accounts_Log.doc");
            Response.Charset = "";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.ContentType = "application/doc";
            
            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            // Read Style file (css) here and add to response 
            FileInfo fi = new FileInfo(Server.MapPath("~/Accounts/Account/CSS/Accounts_Log.css"));
            StringBuilder sb = new StringBuilder();
            StreamReader sr = fi.OpenText();

            while (sr.Peek() >= 0)
            {
                sb.Append(sr.ReadLine());
            }
            sr.Close();

            Summary.RenderControl(htmlWrite);
            Cash_Out.RenderControl(htmlWrite);

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