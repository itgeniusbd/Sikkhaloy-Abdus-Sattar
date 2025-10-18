using System;
using System.Configuration;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Exam.Admit_Card
{
    public partial class Old_AdmitCard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Load saved colors from session on every page load
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

        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ ALL SECTION ]", "%"));
        }
    }
}