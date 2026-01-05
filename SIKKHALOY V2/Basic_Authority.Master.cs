using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace EDUCATION.COM
{
    public partial class Basic_Authority : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated || Session["RegistrationID"] == null)
            {
                string[] myCookies = Request.Cookies.AllKeys;
                foreach (string cookie in myCookies)
                {
                    Response.Cookies[cookie].Expires = DateTime.Now;
                }

                Roles.DeleteCookie();
                Session.Clear();
                FormsAuthentication.SignOut();
                Response.Redirect("~/Default.aspx");
            }

            // Update session activity timestamp
            UpdateSessionActivity();

            if (!this.IsPostBack)
            {
                if (Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "Authority"))
                {
                    DataTable dt = this.GetData("SELECT DISTINCT Category, LinkCategoryID, Ascending FROM Authority_Link_Category ORDER BY Ascending");
                    CategoryTreeView(dt, null);
                }
                else
                {
                    DataTable dt = this.GetData("SELECT DISTINCT Authority_Link_Category.Category, Authority_Link_Category.LinkCategoryID, Authority_Link_Users.RegistrationID,Authority_Link_Category.Ascending FROM Authority_Link_Users INNER JOIN Authority_Link_Pages ON Authority_Link_Users.LinkID = Authority_Link_Pages.LinkID INNER JOIN Authority_Link_Category ON Authority_Link_Pages.LinkCategoryID = Authority_Link_Category.LinkCategoryID WHERE (Authority_Link_Users.RegistrationID = " + Session["RegistrationID"].ToString() + ") ORDER BY Authority_Link_Category.Ascending");
                    CategoryTreeView(dt, null);
                }
            }
        }

        private void UpdateSessionActivity()
        {
            try
            {
                string sessionKey = Session["SessionKey"] as string;
                
                if (!string.IsNullOrEmpty(sessionKey))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                    
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        string query = @"
                            UPDATE User_Active_Sessions 
                            SET LastActivity = GETDATE() 
                            WHERE SessionKey = @SessionKey";
                        
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@SessionKey", sessionKey);
                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log but don't throw - activity update is not critical
                System.Diagnostics.Debug.WriteLine("UpdateSessionActivity Error: " + ex.Message);
            }
        }

        protected void LoginStatus1_LoggingOut(object sender, LoginCancelEventArgs e)
        {
            // Remove session tracking before logout
            try
            {
                string sessionKey = Session["SessionKey"] as string;
                
                if (!string.IsNullOrEmpty(sessionKey))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                    
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        string query = "DELETE FROM User_Active_Sessions WHERE SessionKey = @SessionKey";
                        
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@SessionKey", sessionKey);
                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch { }

            string[] myCookies = Request.Cookies.AllKeys;
            foreach (string cookie in myCookies)
            {
                Response.Cookies[cookie].Expires = DateTime.Now;
            }
            Roles.DeleteCookie();
            Session.Clear();
            FormsAuthentication.SignOut();
            Session.Abandon();
        }

        //Page Link
        private void CategoryTreeView(DataTable dtParent, TreeNode treeNode)
        {
            foreach (DataRow row in dtParent.Rows)
            {
                TreeNode child = new TreeNode();

                child.Text = row["Category"].ToString();
                child.Value = row["LinkCategoryID"].ToString();
                child.CollapseAll();
                child.SelectAction = TreeNodeSelectAction.Expand;


                if (Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "Authority"))
                {
                    LinkTreeView.Nodes.Add(child);
                    DataTable dtChild = GetData("SELECT DISTINCT Authority_Link_SubCategory.* FROM Authority_Link_Pages INNER JOIN Authority_Link_SubCategory ON Authority_Link_Pages.SubCategoryID = Authority_Link_SubCategory.SubCategoryID WHERE (Authority_Link_SubCategory.LinkCategoryID =" + child.Value + ") ORDER BY Authority_Link_SubCategory.Ascending");
                    SubCategoryTreeView(dtChild, child, child.Value);

                    DataTable dtttChild = GetData("SELECT * FROM Authority_Link_Pages WHERE (SubCategoryID IS NULL) AND (LinkCategoryID = " + child.Value + ") ORDER BY Ascending");
                    ClickLinkTreeView(dtttChild, child);
                }
                else
                {

                    LinkTreeView.Nodes.Add(child);
                    DataTable dtChild = GetData("SELECT DISTINCT Authority_Link_SubCategory.* FROM Authority_Link_Users INNER JOIN Authority_Link_Pages ON Authority_Link_Users.LinkID = Authority_Link_Pages.LinkID INNER JOIN Authority_Link_SubCategory ON Authority_Link_Pages.SubCategoryID = Authority_Link_SubCategory.SubCategoryID WHERE (Authority_Link_Users.RegistrationID = " + Session["RegistrationID"].ToString() + ") AND (Authority_Link_SubCategory.LinkCategoryID = " + child.Value + ") ORDER BY Authority_Link_SubCategory.Ascending");
                    SubCategoryTreeView(dtChild, child, child.Value);

                    DataTable dtttChild = GetData("SELECT DISTINCT Authority_Link_Pages.* FROM Authority_Link_Users INNER JOIN Authority_Link_Pages ON Authority_Link_Users.LinkID = Authority_Link_Pages.LinkID WHERE (Authority_Link_Users.RegistrationID = " + Session["RegistrationID"].ToString() + ") AND (Authority_Link_Pages.SubCategoryID IS NULL) AND (Authority_Link_Pages.LinkCategoryID = " + child.Value + ") ORDER BY Authority_Link_Pages.Ascending");
                    ClickLinkTreeView(dtttChild, child);
                }

            }
        }
        private void SubCategoryTreeView(DataTable dtParent, TreeNode treeNode, string LinkCategoryID)
        {
            foreach (DataRow row in dtParent.Rows)
            {
                TreeNode child = new TreeNode();

                child.Text = row["SubCategory"].ToString();
                child.CollapseAll();
                child.SelectAction = TreeNodeSelectAction.Expand;

                if (Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "Authority"))
                {
                    if (child.Text != "")
                    {
                        treeNode.ChildNodes.Add(child);
                        DataTable dtChild = GetData("SELECT DISTINCT * FROM Authority_Link_Pages WHERE (SubCategoryID =" + row["SubCategoryID"].ToString() + ") AND (LinkCategoryID =  " + LinkCategoryID + ") ORDER BY Ascending");
                        ClickLinkTreeView(dtChild, child);
                    }
                }
                else
                {
                    if (child.Text != "")
                    {
                        treeNode.ChildNodes.Add(child);
                        DataTable dtChild = GetData("SELECT DISTINCT Authority_Link_Pages.* FROM Authority_Link_Users INNER JOIN Authority_Link_Pages ON Authority_Link_Users.LinkID = Authority_Link_Pages.LinkID WHERE (Authority_Link_Users.RegistrationID = " + Session["RegistrationID"].ToString() + ") AND (Authority_Link_Pages.SubCategoryID =" + row["SubCategoryID"].ToString() + ") AND (Authority_Link_Pages.LinkCategoryID =  " + LinkCategoryID + ") ORDER BY Authority_Link_Pages.Ascending");
                        ClickLinkTreeView(dtChild, child);
                    }
                }
            }
        }

        private void ClickLinkTreeView(DataTable dtParent, TreeNode treeNode)
        {
            foreach (DataRow row in dtParent.Rows)
            {
                TreeNode child = new TreeNode();

                child.Text = row["PageTitle"].ToString();
                child.NavigateUrl = row["PageURL"].ToString();
                treeNode.ChildNodes.Add(child);
                string CurrentPage = "~" + Request.CurrentExecutionFilePath;
                if (CurrentPage == child.NavigateUrl)
                {
                    child.Select();
                    treeNode.Expand();

                    if (treeNode.Parent != null)
                        treeNode.Parent.Expand();
                }
            }
        }
        private DataTable GetData(string query)
        {
            DataTable dt = new DataTable();
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter())
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
    }
}