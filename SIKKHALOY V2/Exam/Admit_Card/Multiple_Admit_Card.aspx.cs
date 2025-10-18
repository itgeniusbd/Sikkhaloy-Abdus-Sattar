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

            // Load saved colors from session on every page load (not just initial load)
            LoadSavedColors();
        }

        private void LoadSavedColors()
        {
            // Create JavaScript variables for saved colors
            string bgColor = GetSavedBackgroundColor();
            string fontColor = GetSavedFontColor();
            
            // Set default colors if no saved colors exist
            if (string.IsNullOrEmpty(bgColor)) bgColor = "#0075d2";
            if (string.IsNullOrEmpty(fontColor)) fontColor = "#ffffff";
            
            string userKey = Session["SchoolID"]?.ToString() + "_" + Session["UserID"]?.ToString();
            
            // Register client script to apply saved colors
            string script = $@"
                window.savedBgColor = '{bgColor}';
                window.savedFontColor = '{fontColor}';
                window.userColorKey = '{userKey}';
                
                function loadColorsFromStorage() {{
                    try {{
                        var storedBgColor = localStorage.getItem('admitCard_bgColor_' + window.userColorKey);
                        var storedFontColor = localStorage.getItem('admitCard_fontColor_' + window.userColorKey);
                        
                        if (storedBgColor) {{
                            window.savedBgColor = storedBgColor;
                        }}
                        if (storedFontColor) {{
                            window.savedFontColor = storedFontColor;
                        }}
                    }} catch(e) {{
                        console.log('LocalStorage not available');
                    }}
                }}
                
                function applySavedColors() {{
                    loadColorsFromStorage();
                    
                    if (window.savedBgColor) {{
                        $('.getColor').val(window.savedBgColor);
                        $('.color-output').css('background', window.savedBgColor);
                        $('.idcardborder').css('border-color', window.savedBgColor);
                        $('.headcolor').css('background', window.savedBgColor);
                    }}
                    
                    if (window.savedFontColor) {{
                        $('.getfontColor').val(window.savedFontColor);
                        $('.color-output').css('color', window.savedFontColor);
                    }}
                }}
                
                $(document).ready(function() {{
                    setTimeout(function() {{
                        applySavedColors();
                    }}, 200);
                }});
                
                // Also apply colors after any postback
                if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {{
                    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function() {{
                        setTimeout(function() {{
                            applySavedColors();
                        }}, 100);
                    }});
                }}
            ";
            
            ScriptManager.RegisterStartupScript(this, GetType(), "LoadSavedColors", script, true);
        }

        private string GetSavedBackgroundColor()
        {
            string userKey = Session["SchoolID"]?.ToString() + "_" + Session["UserID"]?.ToString();
            return Session["AdmitCard_BgColor_" + userKey]?.ToString() ?? "";
        }

        private string GetSavedFontColor()
        {
            string userKey = Session["SchoolID"]?.ToString() + "_" + Session["UserID"]?.ToString();
            return Session["AdmitCard_FontColor_" + userKey]?.ToString() ?? "";
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

        // Save Background Color to Session
        [WebMethod]
        public static void SaveBackgroundColor(string color)
        {
            string userKey = HttpContext.Current.Session["SchoolID"]?.ToString() + "_" + HttpContext.Current.Session["UserID"]?.ToString();
            HttpContext.Current.Session["AdmitCard_BgColor_" + userKey] = color;
        }

        // Save Font Color to Session
        [WebMethod]
        public static void SaveFontColor(string color)
        {
            string userKey = HttpContext.Current.Session["SchoolID"]?.ToString() + "_" + HttpContext.Current.Session["UserID"]?.ToString();
            HttpContext.Current.Session["AdmitCard_FontColor_" + userKey] = color;
        }

        // Reset Colors to Default
        [WebMethod]
        public static void ResetColors()
        {
            string userKey = HttpContext.Current.Session["SchoolID"]?.ToString() + "_" + HttpContext.Current.Session["UserID"]?.ToString();
            HttpContext.Current.Session["AdmitCard_BgColor_" + userKey] = null;
            HttpContext.Current.Session["AdmitCard_FontColor_" + userKey] = null;
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