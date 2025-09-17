using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee
{
    public partial class Employee_List : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                DataView dv = (DataView)EmployeeSQL.Select(DataSourceSelectArguments.Empty);
                CountLabel.Text = "Total: " + dv.Count.ToString() + " Employee(s)";
            }
        }


        protected void EditLinkButton_Command(object sender, CommandEventArgs e)
        {
            if (e.CommandArgument.ToString() == "Teacher")
            {
                Response.Redirect("Edit_Employee/Employee.aspx?Emp=" + e.CommandName.ToString());
            }
            else
            {
                Response.Redirect("Edit_Employee/Staff.aspx?Emp=" + e.CommandName.ToString());
            }
        }

        protected void FindButton_Click(object sender, EventArgs e)
        {
            DataView dv = (DataView)EmployeeSQL.Select(DataSourceSelectArguments.Empty);
            CountLabel.Text = "Total: " + dv.Count.ToString() + " Employee(s)";
        }

        protected void EmpTypeRadioButtonList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataView dv = (DataView)EmployeeSQL.Select(DataSourceSelectArguments.Empty);
            CountLabel.Text = "Total: " + dv.Count.ToString() + " Employee(s)";
        }
        protected void UploadButton_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            bool Up = false;

            foreach (GridViewRow rows in EmployeeGridView.Rows)
            {
                FileUpload ImgFileUpload = (FileUpload)rows.FindControl("ImgFileUpload");
                TextBox Emp_ID_TextBox = (TextBox)rows.FindControl("Emp_ID_TextBox");
                TextBox EmployeeTypeTextBox = (TextBox)rows.FindControl("EmployeeTypeTextBox");
                TextBox SalaryTextBox = (TextBox)rows.FindControl("SalaryTextBox");
                TextBox AccNoTextBox = (TextBox)rows.FindControl("AccNoTextBox");


                //Update Acc No
                if (AccNoTextBox.Text != "")
                {
                    Bank_AccNoUpdateSQL.UpdateParameters["Bank_AccNo"].DefaultValue = AccNoTextBox.Text;
                    Bank_AccNoUpdateSQL.UpdateParameters["EmployeeID"].DefaultValue = EmployeeGridView.DataKeys[rows.DataItemIndex]["EmployeeID"].ToString();
                    Bank_AccNoUpdateSQL.Update();
                }
                //Update Salary
                if (SalaryTextBox.Text != "")
                {
                    SalaryUpdateSQL.UpdateParameters["Salary"].DefaultValue = SalaryTextBox.Text;
                    SalaryUpdateSQL.UpdateParameters["EmployeeID"].DefaultValue = EmployeeGridView.DataKeys[rows.DataItemIndex]["EmployeeID"].ToString();
                    SalaryUpdateSQL.Update();
                }

                //Update EmployeeType
                if (EmployeeTypeTextBox.Text != "")
                {
                    EmployeeSQL.InsertParameters["EmployeeType"].DefaultValue = EmployeeTypeTextBox.Text;
                    EmployeeSQL.InsertParameters["EmployeeID"].DefaultValue = EmployeeGridView.DataKeys[rows.DataItemIndex]["EmployeeID"].ToString();
                    EmployeeSQL.Insert();
                }


                //Update Employee ID
                if (Emp_ID_TextBox.Text != "")
                {
                    EmployeeSQL.UpdateParameters["ID"].DefaultValue = Emp_ID_TextBox.Text;
                    EmployeeSQL.UpdateParameters["EmployeeID"].DefaultValue = EmployeeGridView.DataKeys[rows.DataItemIndex]["EmployeeID"].ToString();
                    EmployeeSQL.Update();

                    Device_DataUpdateSQL.Insert();
                    Up = true;
                }

                // Resize Image
                if (ImgFileUpload.PostedFile != null && ImgFileUpload.PostedFile.FileName != "")
                {
                    string strExtension = Path.GetExtension(ImgFileUpload.FileName);
                    if ((strExtension.ToUpper() == ".JPG") | (strExtension.ToUpper() == ".GIF") | (strExtension.ToUpper() == ".PNG"))
                    {
                        // Resize Image Before Uploading to DataBase
                        System.Drawing.Image imageToBeResized = System.Drawing.Image.FromStream(ImgFileUpload.PostedFile.InputStream);
                        int imageHeight = imageToBeResized.Height;
                        int imageWidth = imageToBeResized.Width;

                        int maxHeight = 330;
                        int maxWidth = 300;

                        imageHeight = (imageHeight * maxWidth) / imageWidth;
                        imageWidth = maxWidth;

                        if (imageHeight > maxHeight)
                        {
                            imageWidth = (imageWidth * maxHeight) / imageHeight;
                            imageHeight = maxHeight;
                        }

                        Bitmap bitmap = new Bitmap(imageToBeResized, imageWidth, imageHeight);
                        MemoryStream stream = new MemoryStream();
                        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                        stream.Position = 0;
                        byte[] image = new byte[stream.Length + 1];
                        stream.Read(image, 0, image.Length);


                        //Teacher Create SQL Command
                        SqlCommand cmd = new SqlCommand();
                        cmd.CommandText = "UPDATE Teacher SET Image = @Image WHERE (EmployeeID = @EmployeeID) AND (SchoolID = @SchoolID)";
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                        cmd.Parameters.AddWithValue("@EmployeeID", EmployeeGridView.DataKeys[rows.DataItemIndex]["EmployeeID"].ToString());
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = con;

                        //Staff Create SQL Command
                        SqlCommand Staff_cmd = new SqlCommand();
                        Staff_cmd.CommandText = "UPDATE Staff_Info SET Image = @Image2 WHERE (EmployeeID = @EmployeeID) AND (SchoolID = @SchoolID)";
                        Staff_cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                        Staff_cmd.Parameters.AddWithValue("@EmployeeID", EmployeeGridView.DataKeys[rows.DataItemIndex]["EmployeeID"].ToString());
                        Staff_cmd.CommandType = CommandType.Text;
                        Staff_cmd.Connection = con;


                        SqlParameter TImage = new SqlParameter("@Image", SqlDbType.Image, image.Length);
                        TImage.Value = image;
                        cmd.Parameters.Add(TImage);

                        SqlParameter SImage = new SqlParameter("@Image2", SqlDbType.Image, image.Length);
                        SImage.Value = image;
                        Staff_cmd.Parameters.Add(SImage);

                        con.Open();
                        cmd.ExecuteNonQuery();
                        Staff_cmd.ExecuteNonQuery();
                        con.Close();
                        Up = true;
                    }
                }
            }

            if (Up)
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Update Successfully!!')", true);
        }
    }
}