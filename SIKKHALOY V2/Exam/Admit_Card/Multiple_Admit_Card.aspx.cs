using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Exam.Admit_Card
{
    public partial class Multiple_Admit_Card : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {
            string Filter = "";

            if (Paid_DropDownList.SelectedValue == "Paid")
            {
                con.Open();
                SqlCommand SubjectCommand = new SqlCommand("SELECT StudentID FROM Income_PayOrder WHERE (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EndDate < GETDATE()) GROUP BY StudentID HAVING (SUM(Receivable_Amount) = 0)", con);

                SubjectCommand.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                SubjectCommand.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);


                SqlDataReader SubjectDR;
                SubjectDR = SubjectCommand.ExecuteReader();

                while (SubjectDR.Read())
                {
                    Filter += SubjectDR["StudentID"].ToString() + ",";
                }
                con.Close();


                if (Filter != "")
                {
                    ICardInfoSQL.FilterExpression = "StudentID in (" + Filter + ")";
                }
            }

            if (Paid_DropDownList.SelectedValue == "Due")
            {
                con.Open();
                SqlCommand SubjectCommand = new SqlCommand("SELECT StudentID FROM Income_PayOrder WHERE (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EndDate < GETDATE()) GROUP BY StudentID HAVING (SUM(Receivable_Amount) <> 0)", con);

                SubjectCommand.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                SubjectCommand.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);


                SqlDataReader SubjectDR;
                SubjectDR = SubjectCommand.ExecuteReader();

                while (SubjectDR.Read())
                {
                    Filter += SubjectDR["StudentID"].ToString() + ",";
                }

                con.Close();


                if (Filter != "")
                {
                    ICardInfoSQL.FilterExpression = "StudentID in (" + Filter + ")";
                }
            }

            IDCardDL.DataBind();
        }
        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ ALL GROUP ]", "%"));
        }
        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ ALL SECTION ]", "%"));
        }
        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ExamDropDownList.SelectedIndex != 0)
            {
                IDCardDL.Visible = true;
            }
            else
            {
                IDCardDL.Visible = false;
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Select Exam')", true);
            }

            IDCardDL.DataSource = ICardInfoSQL;
            IDCardDL.DataBind();
            Find_ID_TextBox.Text = "";
            TotalCardLabel.Text = "Total Admit Card: " + IDCardDL.Items.Count.ToString();

        }
        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ExamDropDownList.SelectedIndex != 0)
            {
                IDCardDL.Visible = true;
                IDCardDL.DataBind();

            }
            else
            {
                IDCardDL.Visible = false;
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Select Exam')", true);
            }
        }
        protected void FindButton_Click(object sender, EventArgs e)
        {
            ClassDropDownList.SelectedIndex = 0;
            SectionDropDownList.Visible = false;

            IDCardDL.DataSource = IDsSQL;
            IDCardDL.DataBind();
            TotalCardLabel.Text = "Total Admit Card: " + IDCardDL.Items.Count.ToString();
        }

        //Principal Sign
        [WebMethod]
        public static void Principal_Sign(string Image)
        {
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("UPDATE SchoolInfo SET Principal_Sign = CAST(N'' AS xml).value('xs:base64Binary(sql:variable(\"@Image\"))', 'varbinary(max)') Where SchoolID = @SchoolID"))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                    cmd.Parameters.AddWithValue("@Image", Image);
                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }

        //Teacher Sign
        [WebMethod]
        public static void Teacher_Sign(string Image)
        {
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("UPDATE SchoolInfo SET Teacher_Sign = CAST(N'' AS xml).value('xs:base64Binary(sql:variable(\"@Image\"))', 'varbinary(max)') Where SchoolID = @SchoolID"))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                    cmd.Parameters.AddWithValue("@Image", Image);
                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }
    }
}