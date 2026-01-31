using EDUCATION.COM.Handeler;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Committee
{
    public partial class DonationAdd : Page
    {
        readonly SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
      
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Session["SchoolID"] != null)
            {
                SqlCommand AccountCmd = new SqlCommand("Select AccountID from Account where SchoolID = @SchoolID AND Default_Status = 'True'", con);
                AccountCmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());

                con.Open();
                object AccountID = AccountCmd.ExecuteScalar();
                con.Close();

                if (AccountID != null)
                    AccountDropDownList.SelectedValue = AccountID.ToString();
            }
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            if(HiddenCommitteeMemberId.Value != null)
            {
                AddDonationSQL.Insert();

                double.TryParse(PaidAmountTextBox.Text, out var paidAmount);
                

                if (paidAmount > 0) {
                    ReceiptSQL.Insert();

                    PaymentRecordSQL.InsertParameters["CommitteeDonationId"].DefaultValue = ViewState["CommitteeDonationId"].ToString();
                    PaymentRecordSQL.InsertParameters["CommitteeMoneyReceiptId"].DefaultValue = ViewState["CommitteeMoneyReceiptId"].ToString();
                    PaymentRecordSQL.Insert();

                    //if paid amount return to receipt
                    Response.Redirect($"./DonationReceipt.aspx?id={ViewState["CommitteeMoneyReceiptId"]}");
                }

                //if not paid return to donations list
                Response.Redirect("Donations.aspx");
            }
        }

        protected void AddDonationSQL_Inserted(object sender, SqlDataSourceStatusEventArgs e)
        {
            ViewState["CommitteeDonationId"] = e.Command.Parameters["@CommitteeDonationId"].Value;
        }

        protected void ReceiptSQL_Inserted(object sender, SqlDataSourceStatusEventArgs e)
        {
            ViewState["CommitteeMoneyReceiptId"] = e.Command.Parameters["@CommitteeMoneyReceiptId"].Value;
        }



        [WebMethod]
        public static string DonerAddApi(DonerAddModel model)
        {
            var committeeMemberModel = new CommitteeMemberViewModel();
            
            // Trim phone number
            string phone = model.SmsNumber?.Trim();
            
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    // Check for duplicate phone number
                    cmd.CommandText = @"SELECT COUNT(*) FROM CommitteeMember 
                                      WHERE SchoolID = @SchoolID 
                                      AND LTRIM(RTRIM(SmsNumber)) = @Phone";
                    cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                    cmd.Parameters.AddWithValue("@Phone", phone);
                    cmd.Connection = con;
                    
                    con.Open();
                    int count = (int)cmd.ExecuteScalar();
                    con.Close();
                    
                    if (count > 0)
                    {
                        return "{\"error\": \"এই মোবাইল নাম্বার দিয়ে ইতিমধ্যে একজন ডোনার/মেম্বার যুক্ত আছে।\"}";
                    }
                    
                    // Insert new member
                    cmd.Parameters.Clear();
                    cmd.CommandText = @"INSERT INTO CommitteeMember(CommitteeMemberTypeId, RegistrationID, SchoolID, MemberName, SmsNumber, Address) VALUES (@CommitteeMemberTypeId, @RegistrationID, @SchoolID, @MemberName, @SmsNumber, @Address); SELECT SCOPE_IDENTITY()";
                    cmd.Parameters.AddWithValue("@CommitteeMemberTypeId", model.CommitteeMemberTypeId);
                    cmd.Parameters.AddWithValue("@RegistrationID", HttpContext.Current.Session["RegistrationID"].ToString());
                    cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                    cmd.Parameters.AddWithValue("@MemberName", model.MemberName);
                    cmd.Parameters.AddWithValue("@SmsNumber", phone);
                    cmd.Parameters.AddWithValue("@Address", model.Address);

                    con.Open();
                    var committeeMemberIdObj = cmd.ExecuteScalar();
                    con.Close();

                    cmd.CommandText = @"SELECT CommitteeMemberId, MemberName, SmsNumber FROM CommitteeMember WHERE(SchoolID = @SchoolID) AND (CommitteeMemberId = @CommitteeMemberId)";

                    cmd.Parameters.Clear();

                    cmd.Parameters.AddWithValue("@CommitteeMemberId", committeeMemberIdObj.ToString());
                    cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                    cmd.Connection = con;

                    con.Open();
                    var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        committeeMemberModel = new CommitteeMemberViewModel
                        {
                            CommitteeMemberId = dr["CommitteeMemberId"].ToString(),
                            MemberName = dr["MemberName"].ToString(),
                            SmsNumber = dr["SmsNumber"].ToString(),
                        };
                    }

                    con.Close();

                    var json = new JavaScriptSerializer().Serialize(committeeMemberModel);
                    return json;
                }
            }
        }

        public class DonerAddModel
        {
            public int CommitteeMemberTypeId { get; set; }
            public int RegistrationID { get; set; }
            public int SchoolID { get; set; }
            public string MemberName { get; set; }
            public string SmsNumber { get; set; }
            public string Address { get; set; }

        }
    }
}