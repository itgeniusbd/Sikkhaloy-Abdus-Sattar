using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Committee
{
    public partial class MemberAdd : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {


        }

        protected void AddMemberButton_Click(object sender, EventArgs e)
        {
            MemberSQL.Insert();
            MemberGridView.DataBind();
        }
        
    }
}
