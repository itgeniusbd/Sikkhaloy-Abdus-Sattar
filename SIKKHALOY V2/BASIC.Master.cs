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