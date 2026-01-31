using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EDUCATION.COM.Student.OnlinePayment;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace EDUCATION.COM.Committee
{
    public partial class Donor_Dues : System.Web.UI.Page
    {
        private static readonly bool IsSandbox = false; // ? LIVE MODE - Production
        private string StoreId = "";
        private string SignatureKey = "";
        private string PaymentGatewayBase = "";
        private string ConfirmationBase = "http://localhost:3326";
        private string RequestUrl = "/jsonpost.php";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["RegistrationID"] == null)
            {
                Response.Redirect("~/Default.aspx");
                return;
            }

            // Handle postback from PayDonationButton
            string eventTarget = Request["__EVENTTARGET"];
            if (!string.IsNullOrEmpty(eventTarget) && eventTarget == "PayDonationButton")
            {
                PayDonationButton_Click(sender, e);
            }
        }

        protected void PayDonationButton_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("PayDonationButton_Click started");

                if (!IsOnlinePaymentApplicable())
                {
                    ShowAlert("Online Payment is not applicable for this institute.");
                    return;
                }

                string selectedIds = selectedDonationIds.Value;
                System.Diagnostics.Debug.WriteLine($"Selected IDs: {selectedIds}");
                
                if (string.IsNullOrEmpty(selectedIds))
                {
                    ShowAlert("Please select at least one donation to pay.");
                    return;
                }

                double totalPaid = 0;
                int committeeMemberId = GetCommitteeMemberId();
                System.Diagnostics.Debug.WriteLine($"CommitteeMemberId: {committeeMemberId}");
                
                var dateString = DateTime.Now.ToString("yyyyMMdd");
                long time = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                string paymentRecordId = "DON_" + dateString + "_" + time.ToString() + committeeMemberId;
                System.Diagnostics.Debug.WriteLine($"PaymentRecordId: {paymentRecordId}");
                
                int accountId = GetAccountId();
                System.Diagnostics.Debug.WriteLine($"AccountId: {accountId}");
                
                var donationList = new List<DonationRecordInfo>();

                var donationIds = selectedIds.Split(',').Select(id => int.Parse(id.Trim())).Distinct().ToArray();

                foreach (var donationId in donationIds)
                {
                    double dueAmount = GetDonationDueAmount(donationId);
                    System.Diagnostics.Debug.WriteLine($"Donation {donationId} - Due: {dueAmount}");

                    var donationInfo = new DonationRecordInfo
                    {
                        PaymentRecordID = paymentRecordId,
                        CommitteeMemberId = committeeMemberId,
                        CommitteeDonationId = donationId,
                        PaidAmount = dueAmount,
                        PaidDate = DateTime.Now,
                        AccountID = accountId
                    };

                    donationList.Add(donationInfo);
                    totalPaid += dueAmount;
                }

                System.Diagnostics.Debug.WriteLine($"Total Paid: {totalPaid}");

                if (totalPaid <= 0)
                {
                    ShowAlert("Invalid donation amount.");
                    return;
                }

                var donorInfo = GetDonorInformation();
                System.Diagnostics.Debug.WriteLine($"Donor Info - Name: {donorInfo["name"]}, Email: {donorInfo["email"]}");
                
                if (string.IsNullOrEmpty(donorInfo["email"]))
                {
                    ShowAlert("Email is required for Online Payment.");
                    return;
                }

                InsertOnlineTemporaryDonationRecords(donationList);
                System.Diagnostics.Debug.WriteLine("Temporary records inserted");
                
                SetAmarpayCredentials();
                System.Diagnostics.Debug.WriteLine("Amarpay credentials set");
                
                MakeOnlinePayment(paymentRecordId, totalPaid, donorInfo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in PayDonationButton_Click: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowAlert($"Error processing payment: {ex.Message}");
            }
        }

        private void ShowAlert(string message)
        {
            string safeMessage = message.Replace("'", "\\'").Replace("\n", "\\n");
            string script = $"<script type=\"text/javascript\">alert('{safeMessage}'); window.location.href=window.location.href;</script>";
            ClientScript.RegisterStartupScript(this.GetType(), "Alert_" + Guid.NewGuid().ToString(), script);
        }

        private int GetCommitteeMemberId()
        {
            int committeeMemberId = 0;
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = @"SELECT CM.CommitteeMemberId 
                               FROM CommitteeMember CM
                               INNER JOIN Registration R ON R.SchoolID = CM.SchoolID AND R.CommitteeMemberId = CM.CommitteeMemberId
                               WHERE R.RegistrationID = @RegistrationID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@RegistrationID", Session["RegistrationID"]);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        committeeMemberId = Convert.ToInt32(result);
                    }
                }
            }
            return committeeMemberId;
        }

        private double GetDonationDueAmount(int committeeDonationId)
        {
            double dueAmount = 0;
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "SELECT Due FROM CommitteeDonation WHERE CommitteeDonationId = @CommitteeDonationId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CommitteeDonationId", committeeDonationId);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        dueAmount = Convert.ToDouble(result);
                    }
                }
            }
            return dueAmount;
        }

        private bool IsOnlinePaymentApplicable()
        {
            bool isOnlinePaymentApplicable = false;
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT OnlinePaymentEnable FROM SchoolInfo WHERE SchoolID = @SchoolID";
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            int onlinePaymentEnable = Int32.Parse(sdr["OnlinePaymentEnable"].ToString());
                            isOnlinePaymentApplicable = onlinePaymentEnable == 1;
                        }
                    }
                    conn.Close();
                }
            }
            return isOnlinePaymentApplicable;
        }

        private Dictionary<string, string> GetDonorInformation()
        {
            var dic = new Dictionary<string, string>();
            int committeeMemberId = GetCommitteeMemberId();
            
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        cmd.CommandText = "SELECT MemberName, SmsNumber, Email FROM CommitteeMember WHERE CommitteeMemberId = @CommitteeMemberId";
                        cmd.Parameters.AddWithValue("@CommitteeMemberId", committeeMemberId);
                        cmd.Connection = conn;
                        conn.Open();

                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            if (sdr.Read())
                            {
                                dic["name"] = sdr["MemberName"] != DBNull.Value ? sdr["MemberName"].ToString() : "Donor";
                                dic["phone"] = sdr["SmsNumber"] != DBNull.Value ? sdr["SmsNumber"].ToString() : "01700000000";
                                string memberEmail = sdr["Email"] != DBNull.Value ? sdr["Email"].ToString() : "";
                                dic["email"] = !string.IsNullOrEmpty(memberEmail) ? memberEmail : GetInstituteEmailAddress();
                            }
                        }
                        conn.Close();
                    }
                    catch
                    {
                        dic["name"] = "Donor";
                        dic["phone"] = "01700000000";
                        dic["email"] = GetInstituteEmailAddress();
                        if (conn.State == ConnectionState.Open) conn.Close();
                    }
                }
            }
            
            return dic;
        }

        private string GetInstituteEmailAddress()
        {
            string email = "";
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT Email FROM SchoolInfo WHERE SchoolID = @SchoolID";
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            email = sdr["Email"].ToString();
                        }
                    }
                    conn.Close();
                }
            }
            return email;
        }

        private void SetAmarpayCredentials()
        {
            if (IsSandbox)
            {
                StoreId = "aamarpaytest";
                SignatureKey = "dbb74894e82415a2f7ff0ec3a97e4183";
                PaymentGatewayBase = "https://sandbox.aamarpay.com";
            }
            else
            {
                PaymentGatewayBase = "https://secure.aamarpay.com";
                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SELECT StoreId, SignatureKey FROM SchoolInfo WHERE SchoolID = @SchoolID";
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                        cmd.Connection = conn;
                        conn.Open();

                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                StoreId = sdr["StoreId"].ToString();
                                SignatureKey = sdr["SignatureKey"].ToString();
                            }
                        }
                        conn.Close();
                    }
                }
            }
        }

        private void InsertOnlineTemporaryDonationRecords(List<DonationRecordInfo> donationList)
        {
            using (SqlConnection oConnection = new SqlConnection())
            {
                oConnection.ConnectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                oConnection.Open();
                
                using (SqlCommand createCmd = oConnection.CreateCommand())
                {
                    createCmd.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Temp_Online_DonationPaymentRecord]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE [dbo].[Temp_Online_DonationPaymentRecord](
                                [PaymentRecordID] [nvarchar](100) NOT NULL,
                                [CommitteeMemberId] [int] NOT NULL,
                                [CommitteeDonationId] [int] NOT NULL,
                                [PaidAmount] [decimal](18, 2) NOT NULL,
                                [PaidDate] [datetime] NOT NULL,
                                [AccountID] [int] NOT NULL,
                                [InsertDate] [datetime] NOT NULL DEFAULT (GETDATE())
                            )
                        END";
                    createCmd.ExecuteNonQuery();
                }
                
                using (SqlTransaction oTransaction = oConnection.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand oCommand = oConnection.CreateCommand())
                        {
                            oCommand.Transaction = oTransaction;
                            oCommand.CommandType = CommandType.Text;
                            oCommand.CommandText = "INSERT INTO [Temp_Online_DonationPaymentRecord]" +
                                                " ([PaymentRecordID], [CommitteeMemberId], [CommitteeDonationId], [PaidAmount], [PaidDate], [AccountID])" +
                                                " VALUES (@PaymentRecordID, @CommitteeMemberId, @CommitteeDonationId, @PaidAmount, @PaidDate, @AccountID);";
                            oCommand.Parameters.Add(new SqlParameter("@PaymentRecordID", SqlDbType.NVarChar));
                            oCommand.Parameters.Add(new SqlParameter("@CommitteeMemberId", SqlDbType.Int));
                            oCommand.Parameters.Add(new SqlParameter("@CommitteeDonationId", SqlDbType.Int));
                            oCommand.Parameters.Add(new SqlParameter("@PaidAmount", SqlDbType.Decimal));
                            oCommand.Parameters.Add(new SqlParameter("@PaidDate", SqlDbType.DateTime));
                            oCommand.Parameters.Add(new SqlParameter("@AccountID", SqlDbType.Int));
                            
                            foreach (var record in donationList)
                            {
                                oCommand.Parameters[0].Value = record.PaymentRecordID;
                                oCommand.Parameters[1].Value = record.CommitteeMemberId;
                                oCommand.Parameters[2].Value = record.CommitteeDonationId;
                                oCommand.Parameters[3].Value = record.PaidAmount;
                                oCommand.Parameters[4].Value = record.PaidDate;
                                oCommand.Parameters[5].Value = record.AccountID;
                                
                                int rowsAffected = oCommand.ExecuteNonQuery();
                                if (rowsAffected != 1)
                                {
                                    throw new InvalidProgramException("Failed to insert temporary donation record");
                                }
                            }
                            
                            oTransaction.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        oTransaction.Rollback();
                        throw new Exception("Failed to save temporary payment records: " + ex.Message, ex);
                    }
                }
                
                oConnection.Close();
            }
        }

        public void MakeOnlinePayment(string paymentRecordId, double totalPaid, Dictionary<string, string> donorInfo)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("MakeOnlinePayment started");
                
                string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority;
                var sessionInfo = GetSessionInfo();
                var encodedSessionInfo = HttpUtility.UrlEncode(sessionInfo);
                var encodedPaymentRecordId = HttpUtility.UrlEncode(paymentRecordId);

                System.Diagnostics.Debug.WriteLine($"Base URL: {baseUrl}");
                System.Diagnostics.Debug.WriteLine($"Session Info: {sessionInfo}");

                var request = new PaymentRequest
                {
                    store_id = StoreId,
                    signature_key = SignatureKey,
                    tran_id = RandomString(10),
                    amount = totalPaid,
                    currency = "BDT",
                    desc = "Donation Payment",
                    cus_name = donorInfo["name"],
                    cus_email = donorInfo["email"],
                    cus_phone = donorInfo["phone"],
                    type = "json",
                    success_url = baseUrl + "/Default.aspx?opt_a=" + encodedSessionInfo + "&opt_b=" + encodedPaymentRecordId,
                    fail_url = baseUrl + "/Committee/Donor_Dues.aspx?status=failed",
                    cancel_url = baseUrl + "/Committee/Donor_Dues.aspx?status=cancelled",
                    opt_a = sessionInfo,
                    opt_b = paymentRecordId
                };

                System.Diagnostics.Debug.WriteLine($"Payment Gateway URL: {PaymentGatewayBase}{RequestUrl}");
                System.Diagnostics.Debug.WriteLine($"Transaction ID: {request.tran_id}");
                System.Diagnostics.Debug.WriteLine($"Amount: {request.amount}");

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

                string requestUrl = PaymentGatewayBase + RequestUrl;
                WebRequest wRequest = WebRequest.Create(requestUrl);
                wRequest.Method = "POST";
                wRequest.ContentType = "application/json";

                string responseFromServer = "";
                string jsonRequest = JsonConvert.SerializeObject(request);
                System.Diagnostics.Debug.WriteLine($"Request JSON: {jsonRequest}");
                
                using (var streamWriter = new StreamWriter(wRequest.GetRequestStream()))
                {
                    streamWriter.Write(jsonRequest);
                }

                var httpResponse = (WebResponse)wRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    responseFromServer = streamReader.ReadToEnd();
                }

                System.Diagnostics.Debug.WriteLine($"Response from server: {responseFromServer}");

                var result = JsonConvert.DeserializeObject<ResponseInfo>(responseFromServer);
                if (result == null || string.IsNullOrEmpty(result.payment_url))
                {
                    string[] subStrings = responseFromServer.Split(':');
                    string errorMsg = subStrings.Length > 1 ? subStrings[1].Replace('"', ' ').Replace('}', ' ').Trim() : "Unknown error";
                    System.Diagnostics.Debug.WriteLine($"Payment URL is null. Error: {errorMsg}");
                    ShowAlert($"Payment Error: {errorMsg}");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Redirecting to: {result.payment_url}");
                Response.Redirect(result.payment_url, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (WebException webEx)
            {
                System.Diagnostics.Debug.WriteLine($"WebException in MakeOnlinePayment: {webEx.Message}");
                if (webEx.Response != null)
                {
                    using (var errorResponse = (HttpWebResponse)webEx.Response)
                    using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                    {
                        string errorText = reader.ReadToEnd();
                        System.Diagnostics.Debug.WriteLine($"Error Response: {errorText}");
                        ShowAlert($"Payment Gateway Error: {errorText}");
                    }
                }
                else
                {
                    ShowAlert($"Network Error: {webEx.Message}");
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in MakeOnlinePayment: {e.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {e.StackTrace}");
                ShowAlert($"Exception Occurred: {e.Message}");
            }
        }

        private static Random random = new Random();
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string GetSessionInfo()
        {
            string schoolID = Session["SchoolID"] != null ? Session["SchoolID"].ToString() : "";
            string schoolName = Session["School_Name"] != null ? Session["School_Name"].ToString() : "";
            string registrationID = Session["RegistrationID"] != null ? Session["RegistrationID"].ToString() : "";
            string committeeMemberId = GetCommitteeMemberId().ToString();
            string eduYear = Session["Edu_Year"] != null ? Session["Edu_Year"].ToString() : "";

            var dictionary = new Dictionary<string, string>
            {
                {"SchoolID", schoolID},
                {"SchoolName", schoolName},
                {"RegistrationID", registrationID},
                {"CommitteeMemberId", committeeMemberId},
                {"Edu_Year", eduYear},
                {"Category", "Donor"}
            };

            var items = from kvp in dictionary
                        select kvp.Key + "=" + kvp.Value;
            return "{" + string.Join(",", items) + "}";
        }

        private int GetAccountId()
        {
            string schoolID = Session["SchoolID"] != null ? Session["SchoolID"].ToString() : "";
            int accountId = 0;
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT AccountID FROM Account WHERE SchoolID = @SchoolID AND AccountName = 'Online Payment'";
                    cmd.Parameters.AddWithValue("@SchoolID", schoolID);
                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            accountId = Int32.Parse(sdr["AccountID"].ToString());
                        }
                    }
                    conn.Close();
                }
            }
            return accountId;
        }
    }

    class DonationRecordInfo
    {
        public string PaymentRecordID { get; set; }
        public int CommitteeMemberId { get; set; }
        public int CommitteeDonationId { get; set; }
        public double PaidAmount { get; set; }
        public DateTime PaidDate { get; set; }
        public int AccountID { get; set; }
    }

    class ResponseInfo
    {
        public string result { get; }
        public string payment_url { get; set; }
    }
}
