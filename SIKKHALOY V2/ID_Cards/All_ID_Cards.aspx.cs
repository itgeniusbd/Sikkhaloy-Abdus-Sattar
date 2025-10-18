using System;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ID_CARDS
{
    public partial class All_ID_Cards : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;

            if (!IsPostBack)
            {
                GroupDropDownList.Visible = false;
                SectionDropDownList.Visible = false;
                ShiftDropDownList.Visible = false;
            }

            // Load saved colors from session on every page load
            LoadSavedColors();
        }

        private void LoadSavedColors()
        {
            // Create JavaScript variables for saved colors (using same ID Card color system)
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
                        var storedBgColor = localStorage.getItem('idCard_bgColor_' + window.userColorKey);
                        var storedFontColor = localStorage.getItem('idCard_fontColor_' + window.userColorKey);
                        
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
                        $('#wrapper .grid_Header').css('background-color', window.savedBgColor);
                        $('#wrapper .iCard-title').css('background-color', window.savedBgColor);
                        $('#wrapper .c-address').css('background-color', window.savedBgColor);
                        $('#wrapper > div').css('border-color', window.savedBgColor);
                    }}
                    
                    if (window.savedFontColor) {{
                        $('.getfontColor').val(window.savedFontColor);
                        $('#wrapper .grid_Header').css('color', window.savedFontColor);
                        $('#wrapper .iCard-title').css('color', window.savedFontColor);
                        $('#wrapper .c-address').css('color', window.savedFontColor);
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
            return Session["IDCard_BgColor_" + userKey]?.ToString() ?? "";
        }

        private string GetSavedFontColor()
        {
            string userKey = Session["SchoolID"]?.ToString() + "_" + Session["UserID"]?.ToString();
            return Session["IDCard_FontColor_" + userKey]?.ToString() ?? "";
        }

        // Save Background Color to Session (using same ID Card system)
        [WebMethod]
        public static void SaveBackgroundColor(string color)
        {
            string userKey = HttpContext.Current.Session["SchoolID"]?.ToString() + "_" + HttpContext.Current.Session["UserID"]?.ToString();
            HttpContext.Current.Session["IDCard_BgColor_" + userKey] = color;
        }

        // Save Font Color to Session (using same ID Card system)
        [WebMethod]
        public static void SaveFontColor(string color)
        {
            string userKey = HttpContext.Current.Session["SchoolID"]?.ToString() + "_" + HttpContext.Current.Session["UserID"]?.ToString();
            HttpContext.Current.Session["IDCard_FontColor_" + userKey] = color;
        }

        // Reset Colors to Default (using same ID Card system)
        [WebMethod]
        public static void ResetColors()
        {
            string userKey = HttpContext.Current.Session["SchoolID"]?.ToString() + "_" + HttpContext.Current.Session["UserID"]?.ToString();
            HttpContext.Current.Session["IDCard_BgColor_" + userKey] = null;
            HttpContext.Current.Session["IDCard_FontColor_" + userKey] = null;
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

            IDCardRepeater.DataSource = ICardInfoSQL;
            IDCardRepeater.DataBind();
            Find_ID_TextBox.Text = "";

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

        protected void FindButton_Click(object sender, EventArgs e)
        {
            ClassDropDownList.SelectedIndex = 0;
            GroupDropDownList.Visible = false;
            SectionDropDownList.Visible = false;
            ShiftDropDownList.Visible = false;

            IDCardRepeater.DataSource = IDsSQL;
            IDCardRepeater.DataBind();
        }
    }
}