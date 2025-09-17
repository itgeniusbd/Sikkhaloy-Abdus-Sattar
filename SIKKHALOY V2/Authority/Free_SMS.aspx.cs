using System;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority
{
    public partial class Free_SMS : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void UpdateButton_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in SchoolGridView.Rows)
            {
                TextBox Free_SMS_TextBox = (TextBox)row.FindControl("Free_SMS_TextBox");
                TextBox Discount_TextBox = (TextBox)row.FindControl("Discount_TextBox");
                TextBox Fixed_TextBox = (TextBox)row.FindControl("Fixed_TextBox");
                TextBox Per_Student_Rate = (TextBox)row.FindControl("Per_Student_TextBox");
                CheckBox Validation_CheckBox = (CheckBox)row.FindControl("Validation_CheckBox");
                CheckBox Payment_Active_CheckBox = (CheckBox)row.FindControl("Payment_Active_CheckBox");


                InstitutionSQL.UpdateParameters["SchoolID"].DefaultValue = SchoolGridView.DataKeys[row.RowIndex]["SchoolID"].ToString();
                InstitutionSQL.UpdateParameters["Free_SMS"].DefaultValue = Free_SMS_TextBox.Text == "" ? "0" : Free_SMS_TextBox.Text;
                InstitutionSQL.UpdateParameters["Per_Student_Rate"].DefaultValue = Per_Student_Rate.Text == "" ? "0" : Per_Student_Rate.Text;
                InstitutionSQL.UpdateParameters["Discount"].DefaultValue = Discount_TextBox.Text == "" ? "0" : Discount_TextBox.Text;
                InstitutionSQL.UpdateParameters["Fixed"].DefaultValue = Fixed_TextBox.Text == "" ? "0" : Fixed_TextBox.Text;
                InstitutionSQL.UpdateParameters["IS_ServiceChargeActive"].DefaultValue = Payment_Active_CheckBox.Checked.ToString();
                InstitutionSQL.UpdateParameters["Validation"].DefaultValue = Validation_CheckBox.Checked ? "Valid" : "Invalid";
                InstitutionSQL.Update();

                DeviceActiveInactiveSQL.UpdateParameters["SchoolID"].DefaultValue = SchoolGridView.DataKeys[row.RowIndex]["SchoolID"].ToString();
                DeviceActiveInactiveSQL.UpdateParameters["IsActive"].DefaultValue = Validation_CheckBox.Checked.ToString();
                DeviceActiveInactiveSQL.Update();
            }
        }
    }
}