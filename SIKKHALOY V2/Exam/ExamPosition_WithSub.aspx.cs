using System;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Exam
{
    public partial class ExamPosition_WithSub : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;

            try
            {
                if (!IsPostBack)
                {
                    GroupDropDownList.Visible = false;
                    SectionDropDownList.Visible = false;
                    ShiftDropDownList.Visible = false;
                }
            }

            catch { }
        }
        protected void view()
        {
            DataView GroupDV = new DataView();
            GroupDV = (DataView)GroupSQL.Select(DataSourceSelectArguments.Empty);
            if (GroupDV.Count < 1)
            {
                GroupDropDownList.Visible = false;
            }
            else
            {
                GroupDropDownList.Visible = true;
            }

            DataView SectionDV = new DataView();
            SectionDV = (DataView)SectionSQL.Select(DataSourceSelectArguments.Empty);
            if (SectionDV.Count < 1)
            {
                SectionDropDownList.Visible = false;
            }
            else
            {
                SectionDropDownList.Visible = true;
            }

            DataView ShiftDV = new DataView();
            ShiftDV = (DataView)ShiftSQL.Select(DataSourceSelectArguments.Empty);
            if (ShiftDV.Count < 1)
            {
                ShiftDropDownList.Visible = false;
            }
            else
            {
                ShiftDropDownList.Visible = true;
            }

            if (ExamDropDownList.SelectedIndex > 0)
            {
                string name = "Tabulation Sheet Of " + ExamDropDownList.SelectedItem.Text;

                name += " For Class: " + ClassDropDownList.SelectedItem.Text;

                if (SectionDropDownList.SelectedIndex != 0)
                {
                    name += ", Section: " + SectionDropDownList.SelectedItem.Text;
                }
                if (GroupDropDownList.SelectedIndex != 0)
                {
                    name += ", Group: " + GroupDropDownList.SelectedItem.Text;
                }
                if (ShiftDropDownList.SelectedIndex != 0)
                {
                    name += ", Shift: " + ShiftDropDownList.SelectedItem.Text;
                }
                CGSSLabel.Text = name;
            }
        }
        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Group"] = "%";
            Session["Shift"] = "%";
            Session["Section"] = "%";

            GroupDropDownList.DataBind();
            ShiftDropDownList.DataBind();
            SectionDropDownList.DataBind();
            view();
        }

        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ ALL GROUP ]", "%"));
            if (IsPostBack)
                GroupDropDownList.Items.FindByValue(Session["Group"].ToString()).Selected = true;
        }

        protected void SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ ALL SECTION ]", "%"));
            if (IsPostBack)
                SectionDropDownList.Items.FindByValue(Session["Section"].ToString()).Selected = true;
        }

        protected void ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            ShiftDropDownList.Items.Insert(0, new ListItem("[ ALL SHIFT ]", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }
        //End DDL
        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }
        protected void ExamDropDownList_DataBound(object sender, EventArgs e)
        {
            ExamDropDownList.Items.Insert(0, new ListItem("[ SELECT EXAM ]", "0"));
        }

        protected void StudentsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //Class
                if (e.Row.Cells[6].Text == "1")
                {
                    e.Row.Cells[6].CssClass = "First";

                    e.Row.Cells[6].Text += " st";
                }

                else if (e.Row.Cells[6].Text == "2")
                {
                    e.Row.Cells[6].CssClass = "Second";
                    e.Row.Cells[6].Text += " nd";
                }

                else if (e.Row.Cells[6].Text == "3")
                {
                    e.Row.Cells[6].CssClass = "Third";
                    e.Row.Cells[6].Text += " rd";
                }
                else
                {
                    e.Row.Cells[6].Text += " th";
                }

                //Section
                if (e.Row.Cells[7].Text == "1")
                {
                    e.Row.Cells[7].CssClass = "First";
                    e.Row.Cells[7].Text += " st";
                }

                else if (e.Row.Cells[7].Text == "2")
                {
                    e.Row.Cells[7].CssClass = "Second";
                    e.Row.Cells[7].Text += " nd";

                }

                else if (e.Row.Cells[7].Text == "3")
                {
                    e.Row.Cells[7].CssClass = "Third";
                    e.Row.Cells[7].Text += " rd";
                }
                else
                {
                    e.Row.Cells[7].Text += " th";
                }


                if (StudentsGridView.DataKeys[e.Row.DataItemIndex]["PassStatus_InSubject"].ToString() == "F")
                {
                    e.Row.CssClass = "RowColor";
                }
            }
        }

        //Export To Word
        protected void ExportWordButton_Click(object sender, EventArgs e)
        {
            Export_ClassLabel.Text = CGSSLabel.Text;
            Response.Clear();
            Response.ContentEncoding = Encoding.Unicode;
            Response.BinaryWrite(Encoding.Unicode.GetPreamble());

            Response.AddHeader("content-disposition", "attachment;filename=Tabulation_Sheet.doc");
            Response.Charset = "";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.ContentType = "application/doc";
            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            // Read Style file (css) here and add to response 
            FileInfo fi = new FileInfo(Server.MapPath("~/Exam/CSS/ExamPosition.css"));
            StringBuilder sb = new StringBuilder();
            StreamReader sr = fi.OpenText();

            while (sr.Peek() >= 0)
            {
                sb.Append(sr.ReadLine());
            }

            sr.Close();
            //StudentsGridView.Columns[0].Visible = false;
            ExportPanel.RenderControl(htmlWrite);
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