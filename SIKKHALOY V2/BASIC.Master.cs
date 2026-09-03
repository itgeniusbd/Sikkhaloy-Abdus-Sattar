using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM
{
    public partial class BASIC : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated || Session["SchoolID"] == null)
            {
                var myCookies = Request.Cookies.AllKeys;
                foreach (var cookie in myCookies)
                {
                    Response.Cookies[cookie].Expires = DateTime.Now;
                }

                Roles.DeleteCookie();
                Session.Clear();
                FormsAuthentication.SignOut();
                Response.Redirect("~/Default.aspx");
            }

            // ─── Invoice Expiry Block Check ───────────────────────────────────────────
            var currentUser = HttpContext.Current.User.Identity.Name;
            bool isAuthority = Roles.IsUserInRole(currentUser, "Authority") || Roles.IsUserInRole(currentUser, "Sub-Authority");

            if (!isAuthority && Session["SchoolID"] != null)
            {
                string currentPath = Request.AppRelativeCurrentExecutionFilePath.ToLower();
                bool isAllowedInvoicePage =
                    currentPath.Contains("due_invoice")
                    || currentPath.Contains("shurjopaycallback")
                    || currentPath.Contains("invoice_list")
                    || currentPath.Contains("paid_receipt");

                if (!isAllowedInvoicePage && IsInvoiceExpiredAndBlocked())
                {
                    Response.Redirect("~/Profile/Invoice/Due_Invoice.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }
            }
            // ─────────────────────────────────────────────────────────────────────────

            if (Page.IsPostBack) return;

            var user = HttpContext.Current.User.Identity.Name;
            if (Roles.IsUserInRole(user, "Admin") || Roles.IsUserInRole(user, "Authority") || Roles.IsUserInRole(user, "Sub-Authority"))
            {
                var dt = GetData("SELECT DISTINCT Category, LinkCategoryID, Ascending FROM Link_Category ORDER BY Ascending");
                CategoryTreeView(dt);
            }
            else
            {
                var dt = GetData("SELECT DISTINCT Link_Category.Category, Link_Category.LinkCategoryID, Link_Users.RegistrationID,Link_Category.Ascending FROM Link_Users INNER JOIN Link_Pages ON Link_Users.LinkID = Link_Pages.LinkID INNER JOIN Link_Category ON Link_Pages.LinkCategoryID = Link_Category.LinkCategoryID WHERE (Link_Users.RegistrationID = " + Session["RegistrationID"].ToString() + ") ORDER BY Link_Category.Ascending");
                CategoryTreeView(dt);
            }

            if (Session["Edu_Year"] == null) return;

            Session_DropDownList.SelectedValue = Session["Edu_Year"].ToString();
            _redIdHidden.Value = Session["RegistrationID"].ToString();
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Force check school name logo on every page load
            if (Session["SchoolID"] != null && LogoFormView.CurrentMode == FormViewMode.ReadOnly)
            {
                int schoolId = 0;
                if (!int.TryParse(Session["SchoolID"].ToString(), out schoolId) || schoolId <= 0)
                    return;
                bool hasSchoolNameLogo = CheckSchoolNameLogoExists(schoolId);
                
                var schoolNameLogoPanel = LogoFormView.FindControl("SchoolNameLogoPanel") as Panel;
                var traditionalHeaderPanel = LogoFormView.FindControl("TraditionalHeaderPanel") as Panel;
                
                if (schoolNameLogoPanel != null && traditionalHeaderPanel != null)
                {
                    if (hasSchoolNameLogo)
                    {
                        // Show school name logo panel, hide traditional header
                        schoolNameLogoPanel.CssClass = "school-name-logo-wrapper show-panel";
                        schoolNameLogoPanel.Style.Add("display", "block");
                        schoolNameLogoPanel.Style.Add("visibility", "visible");
                        
                        var schoolNameLogoImage = LogoFormView.FindControl("SchoolNameLogoImage") as System.Web.UI.WebControls.Image;
                        if (schoolNameLogoImage != null)
                        {
                            schoolNameLogoImage.ImageUrl = string.Format("/Handeler/SchoolNameLogo.ashx?SchoolID={0}&t={1}", schoolId, DateTime.Now.Ticks);
                        }
                        
                        traditionalHeaderPanel.CssClass = "hide-panel";
                        traditionalHeaderPanel.Style.Add("display", "none");
                        traditionalHeaderPanel.Style.Add("visibility", "hidden");
                    }
                    else
                    {
                        // Show traditional header, hide school name logo panel
                        schoolNameLogoPanel.CssClass = "school-name-logo-wrapper hide-panel";
                        schoolNameLogoPanel.Style.Add("display", "none");
                        schoolNameLogoPanel.Style.Add("visibility", "hidden");
                        
                        traditionalHeaderPanel.CssClass = "show-panel";
                        traditionalHeaderPanel.Style.Add("display", "block");
                        traditionalHeaderPanel.Style.Add("visibility", "visible");
                    }
                }
            }
        }

        protected void LogoFormView_ItemDataBound(object sender, EventArgs e)
        {
            if (LogoFormView.CurrentMode != FormViewMode.ReadOnly) return;
            
            if (Session["SchoolID"] == null) return;
            
            int schoolId = 0;
            if (!int.TryParse(Session["SchoolID"].ToString(), out schoolId) || schoolId <= 0)
                return;
            
            // Check if School Name Logo exists
            bool hasSchoolNameLogo = CheckSchoolNameLogoExists(schoolId);
            
            // DEBUG: Log to check what's happening
            System.Diagnostics.Debug.WriteLine($"=== LogoFormView_ItemDataBound ===");
            System.Diagnostics.Debug.WriteLine($"SchoolID: {schoolId}");
            System.Diagnostics.Debug.WriteLine($"HasSchoolNameLogo: {hasSchoolNameLogo}");
            
            // Get the panels
            var schoolNameLogoPanel = LogoFormView.FindControl("SchoolNameLogoPanel") as Panel;
            var traditionalHeaderPanel = LogoFormView.FindControl("TraditionalHeaderPanel") as Panel;
            
            System.Diagnostics.Debug.WriteLine($"SchoolNameLogoPanel found: {schoolNameLogoPanel != null}");
            System.Diagnostics.Debug.WriteLine($"TraditionalHeaderPanel found: {traditionalHeaderPanel != null}");
            
            // Add a CSS class to indicate which panel should be visible
            if (hasSchoolNameLogo)
            {
                System.Diagnostics.Debug.WriteLine("Setting SchoolNameLogo to SHOW");
                
                // Show school name logo panel, hide traditional header
                if (schoolNameLogoPanel != null)
                {
                    // Remove hide-panel and add show-panel
                    schoolNameLogoPanel.CssClass = "school-name-logo-wrapper show-panel";
                    schoolNameLogoPanel.Style.Add("display", "block");
                    schoolNameLogoPanel.Style.Add("visibility", "visible");
                    
                    // Set the image source
                    var schoolNameLogoImage = LogoFormView.FindControl("SchoolNameLogoImage") as System.Web.UI.WebControls.Image;
                    if (schoolNameLogoImage != null)
                    {
                        schoolNameLogoImage.ImageUrl = string.Format("/Handeler/SchoolNameLogo.ashx?SchoolID={0}&t={1}", schoolId, DateTime.Now.Ticks);
                        schoolNameLogoImage.Style.Add("max-height", "120px");
                        schoolNameLogoImage.Style.Add("max-width", "90%");
                        schoolNameLogoImage.Style.Add("height", "auto");
                        
                        System.Diagnostics.Debug.WriteLine($"Image URL set: {schoolNameLogoImage.ImageUrl}");
                    }
                }
                
                if (traditionalHeaderPanel != null)
                {
                    traditionalHeaderPanel.CssClass = "hide-panel";
                    traditionalHeaderPanel.Style.Add("display", "none");
                    traditionalHeaderPanel.Style.Add("visibility", "hidden");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Setting TraditionalHeader to SHOW");
                
                // Show traditional header, hide school name logo panel
                if (schoolNameLogoPanel != null)
                {
                    schoolNameLogoPanel.CssClass = "school-name-logo-wrapper hide-panel";
                    schoolNameLogoPanel.Style.Add("display", "none");
                    schoolNameLogoPanel.Style.Add("visibility", "hidden");
                }
                
                if (traditionalHeaderPanel != null)
                {
                    traditionalHeaderPanel.CssClass = "show-panel";
                    traditionalHeaderPanel.Style.Add("display", "block");
                    traditionalHeaderPanel.Style.Add("visibility", "visible");
                }
            }
            
            System.Diagnostics.Debug.WriteLine("=================================");
        }

        private bool CheckSchoolNameLogoExists(int schoolId)
        {
            try
            {
                var constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (var con = new SqlConnection(constr))
                {
                    con.Open();
                    
                    // First check if column exists
                    using (var checkCmd = new SqlCommand(
                        @"IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchoolInfo]') AND name = 'SchoolNameLogo')
                          SELECT 1 ELSE SELECT 0", con))
                    {
                        int columnExists = (int)checkCmd.ExecuteScalar();
                        
                        System.Diagnostics.Debug.WriteLine($"SchoolNameLogo column exists: {columnExists == 1}");
                        
                        if (columnExists == 0)
                        {
                            System.Diagnostics.Debug.WriteLine("Column does not exist, returning false");
                            return false;
                        }
                    }
                    
                    // If column exists, check if logo exists for this school
                    using (var cmd = new SqlCommand("SELECT SchoolNameLogo FROM SchoolInfo WHERE SchoolID = @SchoolID", con))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                        
                        var result = cmd.ExecuteScalar();
                        
                        System.Diagnostics.Debug.WriteLine($"Query result: {(result == null ? "NULL" : result == DBNull.Value ? "DBNull" : "Has Data")}");
                        
                        // Check if logo exists and is not null/empty
                        if (result != null && result != DBNull.Value)
                        {
                            byte[] logoData = result as byte[];
                            var hasData = logoData != null && logoData.Length > 0;
                            
                            System.Diagnostics.Debug.WriteLine($"Logo data length: {(logoData != null ? logoData.Length : 0)} bytes");
                            System.Diagnostics.Debug.WriteLine($"Returning: {hasData}");
                            
                            return hasData;
                        }
                        
                        System.Diagnostics.Debug.WriteLine("No logo data found, returning false");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error if needed
                System.Diagnostics.Debug.WriteLine("Error checking school name logo: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack trace: " + ex.StackTrace);
                return false;
            }
        }

        private void CategoryTreeView(DataTable dtParent)
        {
            foreach (DataRow row in dtParent.Rows)
            {
                var child = new TreeNode { Text = row["Category"].ToString(), Value = row["LinkCategoryID"].ToString() };

                child.CollapseAll();
                child.SelectAction = TreeNodeSelectAction.Expand;


                var user = HttpContext.Current.User.Identity.Name;
                if (Roles.IsUserInRole(user, "Admin") || Roles.IsUserInRole(user, "Authority") || Roles.IsUserInRole(user, "Sub-Authority"))
                {
                    LinkTreeView.Nodes.Add(child);
                    var dtChild = GetData("SELECT DISTINCT Link_SubCategory.* FROM Link_Pages INNER JOIN Link_SubCategory ON Link_Pages.SubCategoryID = Link_SubCategory.SubCategoryID WHERE (Link_SubCategory.LinkCategoryID =" + child.Value + ") ORDER BY Link_SubCategory.Ascending");
                    SubCategoryTreeView(dtChild, child, child.Value);

                    var dtdChild = GetData("SELECT * FROM Link_Pages WHERE (SubCategoryID IS NULL) AND (LinkCategoryID = " + child.Value + ") ORDER BY Ascending");
                    ClickLinkTreeView(dtdChild, child);
                }
                else
                {

                    LinkTreeView.Nodes.Add(child);
                    var dtChild = GetData("SELECT DISTINCT Link_SubCategory.* FROM Link_Users INNER JOIN Link_Pages ON Link_Users.LinkID = Link_Pages.LinkID INNER JOIN Link_SubCategory ON Link_Pages.SubCategoryID = Link_SubCategory.SubCategoryID WHERE (Link_Users.RegistrationID = " + Session["RegistrationID"].ToString() + ") AND (Link_SubCategory.LinkCategoryID = " + child.Value + ") ORDER BY Link_SubCategory.Ascending");
                    SubCategoryTreeView(dtChild, child, child.Value);

                    var dtdChild = GetData("SELECT DISTINCT Link_Pages.* FROM Link_Users INNER JOIN  Link_Pages ON Link_Users.LinkID = Link_Pages.LinkID WHERE  (Link_Users.RegistrationID = " + Session["RegistrationID"].ToString() + ") AND (Link_Pages.SubCategoryID IS NULL) AND (Link_Pages.LinkCategoryID = " + child.Value + ") ORDER BY Link_Pages.Ascending");
                    ClickLinkTreeView(dtdChild, child);
                }

            }
        }

        private void SubCategoryTreeView(DataTable dtParent, TreeNode treeNode, string LinkCategoryID)
        {
            foreach (DataRow row in dtParent.Rows)
            {
                var child = new TreeNode { Text = row["SubCategory"].ToString() };

                child.CollapseAll();
                child.SelectAction = TreeNodeSelectAction.Expand;

                var user = HttpContext.Current.User.Identity.Name;
                if (Roles.IsUserInRole(user, "Admin") || Roles.IsUserInRole(user, "Authority") || Roles.IsUserInRole(user, "Sub-Authority"))
                {
                    if (child.Text == "") continue;

                    treeNode.ChildNodes.Add(child);
                    var dtChild = GetData("SELECT DISTINCT * FROM Link_Pages WHERE (SubCategoryID =" + row["SubCategoryID"].ToString() + ") AND (LinkCategoryID =  " + LinkCategoryID + ") ORDER BY Ascending");
                    ClickLinkTreeView(dtChild, child);
                }
                else
                {
                    if (child.Text == "") continue;

                    treeNode.ChildNodes.Add(child);
                    var dtChild = GetData("SELECT DISTINCT Link_Pages.* FROM Link_Users INNER JOIN  Link_Pages ON Link_Users.LinkID = Link_Pages.LinkID WHERE  (Link_Users.RegistrationID = " + Session["RegistrationID"].ToString() + ") AND (Link_Pages.SubCategoryID =" + row["SubCategoryID"].ToString() + ") AND (Link_Pages.LinkCategoryID =  " + LinkCategoryID + ") ORDER BY Link_Pages.Ascending");
                    ClickLinkTreeView(dtChild, child);
                }
            }
        }

        private void ClickLinkTreeView(DataTable dtParent, TreeNode treeNode)
        {
            foreach (DataRow row in dtParent.Rows)
            {
                var child = new TreeNode { Text = row["PageTitle"].ToString(), NavigateUrl = row["PageURL"].ToString() };

                treeNode.ChildNodes.Add(child);
                var currentPage = "~" + Request.CurrentExecutionFilePath;
                if (currentPage != child.NavigateUrl) continue;

                child.Select();
                treeNode.Expand();

                if (treeNode.Parent != null)
                    treeNode.Parent.Expand();
            }
        }

        private bool IsInvoiceExpiredAndBlocked()
        {
            try
            {
                int schoolId = 0;
                if (!int.TryParse(Session["SchoolID"].ToString(), out schoolId) || schoolId <= 0)
                    return false;

                var connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

                // ── Step 1: Grace period column আছে কিনা এবং grace চলছে কিনা ──
                using (var con1 = new SqlConnection(connStr))
                {
                    con1.Open();
                    using (var colCmd = new SqlCommand(
                        @"SELECT CASE 
                            WHEN EXISTS (
                                SELECT * FROM sys.columns 
                                WHERE object_id = OBJECT_ID(N'dbo.SchoolInfo') 
                                AND name = 'AccessGraceUntil'
                            ) THEN 1 ELSE 0 END", con1))
                    {
                        int colExists = (int)colCmd.ExecuteScalar();
                        if (colExists == 1)
                        {
                            using (var graceCmd = new SqlCommand(
                                "SELECT AccessGraceUntil FROM SchoolInfo WHERE SchoolID = @SID", con1))
                            {
                                graceCmd.Parameters.AddWithValue("@SID", schoolId);
                                var graceResult = graceCmd.ExecuteScalar();
                                if (graceResult != null && graceResult != DBNull.Value)
                                {
                                    DateTime graceUntil = Convert.ToDateTime(graceResult);
                                    if (graceUntil.Date >= DateTime.Today)
                                        return false; // Grace period চলছে, block করো না
                                }
                            }
                        }
                    }
                }

                // ── Step 2: Unpaid + EndDate পার হয়েছে এমন invoice আছে কিনা ──
                using (var con2 = new SqlConnection(connStr))
                {
                    con2.Open();
                    using (var expCmd = new SqlCommand(
                        @"SELECT COUNT(*) FROM AAP_Invoice i
                          INNER JOIN AAP_Invoice_Category c ON i.InvoiceCategoryID = c.InvoiceCategoryID
                          WHERE i.SchoolID = @SID
                            AND i.IsPaid = 0
                            AND c.InvoiceCategory <> N'SMS'
                            AND i.IssuDate IS NOT NULL
                            AND CASE
                                  WHEN DAY(i.IssuDate) <= 15 THEN DATEFROMPARTS(YEAR(i.IssuDate), MONTH(i.IssuDate), 15)
                                  ELSE DATEADD(MONTH, 1, DATEFROMPARTS(YEAR(i.IssuDate), MONTH(i.IssuDate), 15))
                                END < CAST(GETDATE() AS DATE)", con2))
                    {
                        expCmd.Parameters.AddWithValue("@SID", schoolId);
                        int expiredCount = (int)expCmd.ExecuteScalar();
                        return expiredCount > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("IsInvoiceExpiredAndBlocked error: " + ex.Message);
                return false;
            }
        }

        private static DataTable GetData(string query)
        {
            var dt = new DataTable();
            var constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            using (var con = new SqlConnection(constr))
            {
                using (var cmd = new SqlCommand(query))
                {
                    using (var sda = new SqlDataAdapter())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = con;
                        sda.SelectCommand = cmd;
                        sda.Fill(dt);
                    }
                }
                return dt;
            }
        }

        protected void LoginStatus1_LoggingOut(object sender, LoginCancelEventArgs e)
        {
            var myCookies = Request.Cookies.AllKeys;
            foreach (var cookie in myCookies)
            {
                Response.Cookies[cookie].Expires = DateTime.Now;
            }

            Roles.DeleteCookie();
            Session.Clear();
            FormsAuthentication.SignOut();
            Session.Abandon();
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            TestimonialSQL.Insert();
            MessageTextBox.Text = "";
            MsgLabel.Text = "Thank you for share your experience";
        }
    }
}