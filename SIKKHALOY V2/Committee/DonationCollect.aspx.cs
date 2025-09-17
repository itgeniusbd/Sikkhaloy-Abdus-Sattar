using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Committee
{
    public partial class DonationCollect : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Session["SchoolID"] != null)
            {
                var AccountCmd = new SqlCommand("Select AccountID from Account where SchoolID = @SchoolID AND Default_Status = 'True'", con);
                AccountCmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());

                con.Open();
                object AccountID = AccountCmd.ExecuteScalar();
                con.Close();

                if (AccountID != null)
                    AccountDropDownList.SelectedValue = AccountID.ToString();
            }
        }

        protected void FindButton_Click(object sender, EventArgs e)
        {
            MemberInfoFormView.DataBind();
            PaidRecordGridView.DataBind();
        }

        protected void ReceiptSQL_Inserted(object sender, SqlDataSourceStatusEventArgs e)
        {
            ViewState["CommitteeMoneyReceiptId"] = e.Command.Parameters["@CommitteeMoneyReceiptId"].Value;
        }

        protected void CollectButton_Click(object sender, EventArgs e)
        {
            if (HiddenCommitteeMemberId.Value != null)
            {
                //check paid amount
                var isPaymentValid = true;
                foreach (GridViewRow row in DonationGridView.Rows)
                {
                    var DueCheckBox = (CheckBox)row.FindControl("DueCheckBox");
                    var PaidAmountTextBox = (TextBox)row.FindControl("PaidAmountTextBox");
                    var CommitteeDonationId = Convert.ToInt32(DonationGridView.DataKeys[row.RowIndex]["CommitteeDonationId"]);

                    var dueCmd = new SqlCommand("SELECT Due FROM CommitteeDonation WHERE (SchoolID = @SchoolID) AND (CommitteeDonationId = @CommitteeDonationId)", con);
                    dueCmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                    dueCmd.Parameters.AddWithValue("@CommitteeDonationId", CommitteeDonationId);

                    con.Open();
                    var dueAmount = (double)dueCmd.ExecuteScalar();
                    con.Close();

                    if (DueCheckBox.Checked && double.TryParse(PaidAmountTextBox.Text.Trim(), out double PaidAmount))
                    {
                        if (PaidAmount > dueAmount)
                        {
                            isPaymentValid = false;
                        }
                    }
                }


                //insert data
                if (isPaymentValid)
                {
                    ReceiptSQL.Insert();

                    foreach (GridViewRow row in DonationGridView.Rows)
                    {
                        var DueCheckBox = (CheckBox)row.FindControl("DueCheckBox");
                        var PaidAmountTextBox = (TextBox)row.FindControl("PaidAmountTextBox");
                        var CommitteeDonationId = DonationGridView.DataKeys[row.RowIndex]["CommitteeDonationId"];

                        if (DueCheckBox.Checked && double.TryParse(PaidAmountTextBox.Text.Trim(), out double PaidAmount))
                        {
                            PaymentRecordSQL.InsertParameters["CommitteeDonationId"].DefaultValue = CommitteeDonationId.ToString();
                            PaymentRecordSQL.InsertParameters["CommitteeMoneyReceiptId"].DefaultValue = ViewState["CommitteeMoneyReceiptId"].ToString();
                            PaymentRecordSQL.InsertParameters["PaidAmount"].DefaultValue = PaidAmountTextBox.Text;
                            PaymentRecordSQL.Insert();
                        }
                    }

                    //if paid amount return to receipt
                    Response.Redirect($"./DonationReceipt.aspx?id={ViewState["CommitteeMoneyReceiptId"]}");
                }
            }
        }
    }
}