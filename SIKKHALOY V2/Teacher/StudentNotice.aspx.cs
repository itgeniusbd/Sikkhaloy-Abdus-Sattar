using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Teacher
{
    public partial class StudentNotice : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void NoticeButton_Click(object sender, EventArgs e)
        {
            if (NoticeTitleTextBox.Text != "")
            {
                //cehck
                var isSelected = false;
                foreach (ListItem item in ClassCheckBoxList.Items)
                {
                    if (item.Selected)
                    {
                        isSelected = true;
                    }
                }

                if (isSelected)
                {

                    //string myFilePath = @"C:\MyFile.txt";
                    //string ext = Path.GetExtension(myFilePath);


                    if (FileUploadPDF.HasFile) //If the used Uploaded a file  
                    {
                        string FileName = FileUploadPDF.FileName; //Name of the file is stored in local Variable  
                        string ext1 = System.IO.Path.GetExtension(this.FileUploadPDF.PostedFile.FileName);
                        string filetype = GetFileTypeByFileExtension(ext1);

                        string noticePath = Server.MapPath("~/AllNotice/");
                        if (!Directory.Exists(noticePath))
                        {
                            Directory.CreateDirectory(noticePath);
                        }

                        int filesize = FileUploadPDF.PostedFile.ContentLength / 1024;
                        if (filesize <= 500)
                        {
                            FileUploadPDF.PostedFile.SaveAs(Server.MapPath("~/AllNotice/") + FileName); //File is saved in the Physical folder
                            StudentNoticeSQL.InsertParameters["Notice_file"].DefaultValue = "~/AllNotice/" + FileName;

                            StudentNoticeSQL.Insert();

                            foreach (ListItem item in ClassCheckBoxList.Items)
                            {
                                if (item.Selected)
                                {
                                    StudentNoticeClassSQL.InsertParameters["StudentNoticeId"].DefaultValue = ViewState["StudentNoticeId"].ToString();
                                    StudentNoticeClassSQL.InsertParameters["ClassId"].DefaultValue = item.Value;
                                    StudentNoticeClassSQL.Insert();
                                }
                            }
                            Response.Write("<script>alert('Notice Submitted Successfully');</script>");
                        }
                        else
                        {
                            Response.Write("<script>alert('File size Below 500 kb ');</script>");
                        }
                    }
                    else
                    {
                        StudentNoticeSQL.Insert();

                        foreach (ListItem item in ClassCheckBoxList.Items)
                        {
                            if (item.Selected)
                            {
                                StudentNoticeClassSQL.InsertParameters["StudentNoticeId"].DefaultValue = ViewState["StudentNoticeId"].ToString();
                                StudentNoticeClassSQL.InsertParameters["ClassId"].DefaultValue = item.Value;
                                StudentNoticeClassSQL.Insert();
                            }
                        }

                        Response.Write("<script>alert('Notice Submitted Successfully');</script>");
                    }

                }
                else
                {
                    Response.Write("<script>alert('Please Select Class!');</script>");
                }
            }
            else
            {
                Response.Write("<script>alert('Notice Title is Required!');</script>");
            }
        }
        protected void DownloadFile(object sender, EventArgs e)
        {
            string filePath = (sender as LinkButton).CommandArgument;

            string path = Server.MapPath(filePath);

            if (File.Exists(path))
            {
                Response.ContentType = ContentType;
                Response.AppendHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(filePath));
                Response.WriteFile(filePath);
                Response.End();
            }
        }
        private string GetFileTypeByFileExtension(string fileExtension)
        {
            switch (fileExtension.ToLower()) //Checking the file Extension and Showing the Hard Coded Values on the Basis of Extension Type  
            {
                case ".pdf":
                    return "pdf File";
                case ".doc":
                case ".docx":
                    return "Microsoft Word Document";
                case ".xls":
                case ".xlsx":
                    return "Microsoft Excel Document";
                case ".txt":
                    return "Text File";
                case ".png":
                case ".jpg":
                    return "Windows Image file";
                default:
                    return "Unknown file type";
            }
        }

        protected void StudentNoticeSQL_Inserted(object sender, SqlDataSourceStatusEventArgs e)
        {
            ViewState["StudentNoticeId"] = e.Command.Parameters["@StudentNoticeId"].Value;
        }
        protected void DeleteButton_Click(object sender, EventArgs e)
        {
            CheckBox SingleCheckBox = new CheckBox();
            SqlTransaction transaction = null;

            if (IsValidate() != true)
            {
                foreach (GridViewRow Row in NoticeGridView.Rows)
                {
                    SingleCheckBox = Row.FindControl("SingleCheckBox") as CheckBox;

                    if (SingleCheckBox.Checked)
                    {
                        string noticeId = NoticeGridView.DataKeys[Row.DataItemIndex]["StudentNoticeId"].ToString();

                        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString()))
                        {
                            con.Open();
                            SqlCommand cmd = new SqlCommand();
                            cmd.Connection = con;
                            SqlTransaction transection;

                            transection = con.BeginTransaction();
                            cmd.Transaction = transection;

                            try
                            {
                                cmd.CommandText = "DELETE  FROM  StudentNoticeClass WHERE (StudentNoticeId ='" + noticeId + "')";
                                cmd.ExecuteNonQuery();
                                cmd.CommandText = "DELETE  FROM  StudentNotice WHERE (StudentNoticeId ='" + noticeId + "')";
                                cmd.ExecuteNonQuery();
                                transection.Commit();

                            }
                            catch (Exception ex)
                            {
                                transection.Rollback();
                            }
                            finally
                            {
                                con.Close();
                            }

                        }
                    }
                }
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alert", "alert(' Notice Deleted Successfully!');" +
            "window.location='StudentNotice.aspx';", true);
                NoticeGridView.DataBind();

            }
        }
        private bool IsValidate()
        {
            CheckBox SingleCheckBox = new CheckBox();
            bool check = true;
            bool check1 = false;
            foreach (GridViewRow Row in NoticeGridView.Rows)
            {
                SingleCheckBox = Row.FindControl("SingleCheckBox") as CheckBox;
                if (SingleCheckBox.Checked == false)
                {
                    check = false;
                }
                else { check1 = true; }
            }
            bool flag = false;

            if (check1 == false)
            {
                Response.Write("<script>alert('Please check at least one');</script>");
                flag = true;
            }
            return flag;
        }
    }
}