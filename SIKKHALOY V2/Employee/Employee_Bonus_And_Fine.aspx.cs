using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee
{
    public partial class Employee_Bonus_And_Fine : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void AddFineButton_Click(object sender, EventArgs e)
        {
            FineSQL.Insert();
            FineGridView.DataBind();
            FineNameTextBox.Text = "";
        }

        protected void BonusButton_Click(object sender, EventArgs e)
        {
            BonusSQL.Insert();
            BonusGridView.DataBind();
            BonusTextBox.Text = "";
        }
    }
}