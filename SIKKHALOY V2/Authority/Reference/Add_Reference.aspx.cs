using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority.Reference
{
    public partial class Add_Reference : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            ReferenceSQl.Insert();
        }

        protected void PaidButton_Click(object sender, EventArgs e)
        {
            Reference_PaymentRecordSQL.Insert();
        }
    }
}