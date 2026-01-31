using EDUCATION.COM.ACCOUNTS.Payment;
using EDUCATION.COM.Committee;
using EDUCATION.COM.PaymentDataSetTableAdapters;
using EDUCATION.COM.Student;
using EDUCATION.COM.Student.OnlinePayment;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.UI.WebControls;

namespace EDUCATION.COM
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            PaymentFactory<PaymentResponse> paymentFactory = new PaymentFactory<PaymentResponse>();
            var paymentInfo = paymentFactory.GetPaymentInfoFromQueryString(Request);

            if (!string.IsNullOrEmpty(paymentInfo.opt_a))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Processing payment...");

                    SetSessionInfoAfterOnlinePayment(paymentInfo.opt_a);

                    Session["OnlinePaymentInfo"] = JsonConvert.SerializeObject(paymentInfo);

                    string paymentRecordId = paymentInfo.opt_b;
                    System.Diagnostics.Debug.WriteLine("Payment Record ID: " + paymentRecordId);

                    SavePaymentInfoAfterSuccess(paymentRecordId);

                    System.Diagnostics.Debug.WriteLine("Payment processing completed successfully!");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR in Page_Load:");
                    System.Diagnostics.Debug.WriteLine("Message: " + ex.Message);
                    System.Diagnostics.Debug.WriteLine("Stack Trace: " + ex.StackTrace);

                    string errorDisplay = "<div style='position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(255,0,0,0.9);color:white;padding:20px;z-index:9999;overflow:auto;'>";
                    errorDisplay += "<h2>Payment Processing Error</h2>";
                    errorDisplay += "<p><strong>Error:</strong> " + ex.Message + "</p>";
                    errorDisplay += "<pre>" + ex.StackTrace + "</pre>";
                    errorDisplay += "</div>";
                    Response.Write(errorDisplay);
                }
            }
        }

        protected void SendButton_Click(object sender, EventArgs e)
        {
            if (NameTextBox.Text == "" || MobileTextBox.Text == "") return;

            ContactSQL.Insert();

            NameTextBox.Text = "";
            EmailTextBox.Text = "";
            MobileTextBox.Text = "";
            SubjectTextBox.Text = "";
            MessageTextBox.Text = "";
            MsgLabel.Text = "Thank you for your query, we will respond as soon as possible";
        }

        //Change Session
        [WebMethod]
        public static void Session_Change(string id)
        {
            var con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            try
            {
                var cmd = new SqlCommand("UPDATE Education_Year_User SET EducationYearID = Education_Year.EducationYearID FROM Education_Year_User INNER JOIN Education_Year ON Education_Year_User.SchoolID = Education_Year.SchoolID WHERE (Education_Year_User.SchoolID = @SchoolID) AND (Education_Year_User.RegistrationID = @RegistrationID) AND (Education_Year.EducationYearID = @EducationYearID)", con);
                cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                cmd.Parameters.AddWithValue("@RegistrationID", HttpContext.Current.Session["RegistrationID"].ToString());
                cmd.Parameters.AddWithValue("@EducationYearID", id);

                con.Open();
                cmd.ExecuteNonQuery();
            }
            finally
            {
                con.Close();
                HttpContext.Current.Session["Edu_Year"] = id;
            }
        }


        //Change Session Student
        [WebMethod]
        public static void Student_Session_Change(string id)
        {
            var con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            var cmd = new SqlCommand("UPDATE Education_Year_User SET EducationYearID = @EducationYearID WHERE (SchoolID = @SchoolID) AND (RegistrationID = @RegistrationID)", con);

            cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
            cmd.Parameters.AddWithValue("@RegistrationID", HttpContext.Current.Session["RegistrationID"].ToString());
            cmd.Parameters.AddWithValue("@EducationYearID", id);

            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();

            var cmd2 = new SqlCommand("SELECT StudentsClass.StudentClassID,StudentsClass.ClassID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (StudentsClass.EducationYearID = @EducationYearID) AND (Student.StudentRegistrationID = @StudentRegistrationID)", con);
            cmd2.Parameters.AddWithValue("@EducationYearID", id);
            cmd2.Parameters.AddWithValue("@StudentRegistrationID", HttpContext.Current.Session["RegistrationID"].ToString());

            con.Open();
            var dr = cmd2.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    HttpContext.Current.Session["ClassID"] = dr["ClassID"].ToString();
                    HttpContext.Current.Session["StudentClassID"] = dr["StudentClassID"].ToString();
                }
            }

            con.Close();

            HttpContext.Current.Session["Edu_Year"] = id;
        }

        //google reCaptcha
        protected static string ReCaptchaKey = "6LdXPY0UAAAAAHg7W3SLGr_MQt7wRvV0HLZ8JTBi";
        protected static string ReCaptchaSecret = "6LdXPY0UAAAAALlL5doPnCfNOmEoeSvGum5CdVTq";

        [WebMethod]
        public static string VerifyCaptcha(string response)
        {
            var url = "https://www.google.com/recaptcha/api/siteverify?secret=" + ReCaptchaSecret + "&response=" + response;
            return (new WebClient()).DownloadString(url);
        }
        private void SetSessionInfoAfterOnlinePayment(string sessionInfo)
        {
            //sessionInfo = "{SchoolID=1012,SchoolName=SIKKHALOY,RegistrationID=13548,EducationYearID=2464,StudentID=41148,ClassID=130,StudentClassID=189569,TeacherID=null}";
            sessionInfo = sessionInfo.TrimStart('{').TrimEnd('}');
            
            // Split by comma and create dictionary
            var items = sessionInfo.Split(',');
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            
            foreach (var item in items)
            {
                var parts = item.Split('=');
                if (parts.Length == 2)
                {
                    dictionary[parts[0].Trim()] = parts[1].Trim();
                }
            }
            
            if (dictionary != null)
            {
                foreach (KeyValuePair<string, string> entry in dictionary)
                {
                    // Set session only if value is not null or "null" string
                    if (!string.IsNullOrEmpty(entry.Value) && entry.Value.ToLower() != "null")
                    {
                        Session[entry.Key] = entry.Value;
                    }
                }
                
                // Special handling for Donor payment
                if (dictionary.ContainsKey("CommitteeMemberId") && !string.IsNullOrEmpty(dictionary["CommitteeMemberId"]))
                {
                    Session["CommitteeMemberId"] = dictionary["CommitteeMemberId"];
                }
                
                // Special handling for Category
                if (dictionary.ContainsKey("Category") && dictionary["Category"] == "Donor")
                {
                    Session["Category"] = "Donor";
                }
            }
        }

        private void SavePaymentInfoAfterSuccess(string paymentRecordID)
        {
            // Validate that required session variables are set
            if (Session["SchoolID"] == null || Session["Edu_Year"] == null)
            {
                Response.Redirect("~/Default.aspx");
                return;
            }

            int SchoolID = Convert.ToInt32(Session["SchoolID"].ToString());
            int Crrent_EduYearID = Convert.ToInt32(Session["Edu_Year"].ToString());
            
            // Check if this is a donor payment (starts with "DON_")
            if (paymentRecordID.StartsWith("DON_"))
            {
                ProcessDonorPayment(paymentRecordID, SchoolID, Crrent_EduYearID);
                return;
            }
            
            // Otherwise, process as student payment
            var Payment_DataSet = new OrdersTableAdapter();
            double TotalPaid = 0;
            int MoneyReceiptID = 0;
            int RegistrationID = GetAdminRegistrationId(SchoolID);

            // Check if this is a student payment
            bool isStudentPayment = Session["StudentClassID"] != null && Session["StudentID"] != null;
            
            int StudentClassID = 0;
            int StudentID = 0;

            if (isStudentPayment)
            {
                StudentClassID = Convert.ToInt32(Session["StudentClassID"].ToString());
                StudentID = Convert.ToInt32(Session["StudentID"].ToString());
                
                MoneyReceiptID = Convert.ToInt32(Payment_DataSet.Insert_MoneyReceipt(StudentID, RegistrationID, StudentClassID, Crrent_EduYearID, "Institution", DateTime.Now, SchoolID));
            }

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT PaymentRecordID, StudentID, RoleID, PayOrderID, PayOrderEduYearID, PaidAmount, PayFor, PaidDate, AccountID" + " " +
                                      "FROM Temp_Online_PaymentRecord WHERE PaymentRecordID = @PaymentRecordID";
                    cmd.Parameters.AddWithValue("@PaymentRecordID", paymentRecordID);
                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            int PayOrderID = Int32.Parse(sdr["PayOrderID"].ToString());
                            int RoleID = Int32.Parse(sdr["RoleID"].ToString());
                            int P_Order_EduYearID = Int32.Parse(sdr["PayOrderEduYearID"].ToString());
                            double PaidAmount = double.Parse(sdr["PaidAmount"].ToString());
                            string PayFor = sdr["PayFor"].ToString();
                            var PaidDate = DateTime.Parse(sdr["PaidDate"].ToString());
                            int AccountID = Int32.Parse(sdr["AccountID"].ToString());
                            
                            if (isStudentPayment)
                            {
                                Payment_DataSet.Insert_Payment_Record(StudentID, RegistrationID, RoleID, PayOrderID, PaidAmount, PayFor, PaidDate, MoneyReceiptID, StudentClassID, P_Order_EduYearID, SchoolID, AccountID);
                                Payment_DataSet.Update_payOrder(PaidAmount, PayOrderID);
                            }
                            
                            TotalPaid += PaidAmount;
                        }
                    }
                    conn.Close();
                }
            }

            if (isStudentPayment)
            {
                Payment_DataSet.Update_MoneyReceipt(TotalPaid, MoneyReceiptID);

                string studentIdStr = GetStudentId();
                if (!string.IsNullOrEmpty(studentIdStr))
                {
                    string MRid = HttpUtility.UrlEncode(Encrypt(Convert.ToString(MoneyReceiptID)));
                    string Sid = HttpUtility.UrlEncode(Encrypt(studentIdStr));
                    Response.Redirect(string.Format("~/Accounts/Payment/Money_Receipt.aspx?mN_R={0}&s_icD={1}", MRid, Sid));
                }
                else
                {
                    Response.Redirect("~/Default.aspx");
                }
            }
            else
            {
                Response.Redirect("~/Default.aspx");
            }
        }

        private void ProcessDonorPayment(string paymentRecordID, int schoolID, int educationYearID)
        {
            try
            {
                double totalPaid = 0;
                int committeeMoneyReceiptId = 0;
                int committeeMemberId = 0;
                int registrationID = 0;
                int accountId = 0;

                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                    conn.Open();

                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // ✅ Get donation records from temporary table - NOW WITH DELETE OUTPUT to prevent duplicates
                            using (SqlCommand cmd = new SqlCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.Connection = conn;
                                
                                // ✅ Check if table exists, if not create it
                                cmd.CommandText = @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Temp_Online_DonationPaymentRecord')
                                           BEGIN
                                               CREATE TABLE Temp_Online_DonationPaymentRecord (
                                                   PaymentRecordID NVARCHAR(100),
                                                   CommitteeMemberId INT,
                                                   CommitteeDonationId INT,
                                                   PaidAmount DECIMAL(18,2),
                                                   PaidDate DATETIME,
                                                   AccountID INT
                                               )
                                           END";
                                cmd.ExecuteNonQuery();
                                
                                // ✅ Delete and get records in single atomic operation
                                cmd.CommandText = @"DELETE FROM Temp_Online_DonationPaymentRecord
                                           OUTPUT DELETED.PaymentRecordID, DELETED.CommitteeMemberId, DELETED.CommitteeDonationId, DELETED.PaidAmount, DELETED.PaidDate, DELETED.AccountID 
                                           WHERE PaymentRecordID = @PaymentRecordID";
                                

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@PaymentRecordID", paymentRecordID);

                                var donationRecords = new List<DonationTempRecord>();

                                using (SqlDataReader sdr = cmd.ExecuteReader())
                                {
                                    while (sdr.Read())
                                    {
                                        var record = new DonationTempRecord
                                        {
                                            CommitteeMemberId = Convert.ToInt32(sdr["CommitteeMemberId"]),
                                            CommitteeDonationId = Convert.ToInt32(sdr["CommitteeDonationId"]),
                                            PaidAmount = Convert.ToDouble(sdr["PaidAmount"]),
                                            PaidDate = Convert.ToDateTime(sdr["PaidDate"]),
                                            AccountID = Convert.ToInt32(sdr["AccountID"])
                                        };

                                        donationRecords.Add(record);

                                        if (committeeMemberId == 0)
                                        {
                                            committeeMemberId = record.CommitteeMemberId;
                                            accountId = record.AccountID;
                                        }
                                    }
                                }

                                if (donationRecords.Count == 0)
                                {
                                    // Log error - no records found
                                    string script = "<script type='text/javascript'>alert('No payment records found. PaymentRecordID: " + paymentRecordID + "');</script>";
                                    ClientScript.RegisterStartupScript(this.GetType(), "NoRecords", script);
                                    Response.Redirect("~/Committee/Donor_Dues.aspx?error=no_records", false);
                                    return;
                                }

                                // ✅ Get admin registration ID
                                registrationID = GetAdminRegistrationId(schoolID);

                                // ✅ Insert money receipt
                                using (SqlCommand receiptCmd = new SqlCommand())
                                {
                                    receiptCmd.Transaction = transaction;
                                    receiptCmd.CommandText = @"INSERT INTO CommitteeMoneyReceipt 
                                                      (RegistrationId, SchoolId, CommitteeMemberId, EducationYearId, AccountId, CommitteeMoneyReceiptSn, PaidDate, TotalAmount) 
                                                      VALUES (@RegistrationID, @SchoolID, @CommitteeMemberId, @EducationYearId, @AccountId, 
                                                              dbo.F_CommitteeMoneyReceiptSn(@SchoolID), @PaidDate, 0);
                                                      SELECT SCOPE_IDENTITY();";


                                    receiptCmd.Parameters.AddWithValue("@RegistrationID", registrationID);
                                    receiptCmd.Parameters.AddWithValue("@SchoolID", schoolID);
                                    receiptCmd.Parameters.AddWithValue("@CommitteeMemberId", committeeMemberId);
                                    receiptCmd.Parameters.AddWithValue("@EducationYearId", educationYearID);
                                    receiptCmd.Parameters.AddWithValue("@AccountId", accountId);
                                    receiptCmd.Parameters.AddWithValue("@PaidDate", DateTime.Now);
                                    receiptCmd.Connection = conn;

                                    committeeMoneyReceiptId = Convert.ToInt32(receiptCmd.ExecuteScalar());
                                }

                                // ✅ Process UNIQUE donations ONLY to prevent double payments
                                var uniqueDonations = donationRecords
                                    .GroupBy(d => d.CommitteeDonationId)
                                    .Select(g => g.First())
                                    .ToList();

                                // ✅ Insert payment records and update donations
                                foreach (var record in uniqueDonations)
                                {
                                    // ✅ Insert payment record
                                    using (SqlCommand paymentCmd = new SqlCommand())
                                    {
                                        paymentCmd.Transaction = transaction;
                                        paymentCmd.CommandText = @"INSERT INTO CommitteePaymentRecord 
                                                          (SchoolId, RegistrationId, CommitteeDonationId, CommitteeMoneyReceiptId, PaidAmount) 
                                                          VALUES (@SchoolID, @RegistrationID, @CommitteeDonationId, @CommitteeMoneyReceiptId, @PaidAmount)";

                                        paymentCmd.Parameters.AddWithValue("@SchoolID", schoolID);
                                        paymentCmd.Parameters.AddWithValue("@RegistrationID", registrationID);
                                        paymentCmd.Parameters.AddWithValue("@CommitteeDonationId", record.CommitteeDonationId);
                                        paymentCmd.Parameters.AddWithValue("@CommitteeMoneyReceiptId", committeeMoneyReceiptId);
                                        paymentCmd.Parameters.AddWithValue("@PaidAmount", record.PaidAmount);
                                        paymentCmd.Connection = conn;

                                        paymentCmd.ExecuteNonQuery();
                                    }

                                    // ✅ SELF-HEALING UPDATE: Recalculate total paid from Payment Records
                                    using (SqlCommand updateDonationCmd = new SqlCommand())
                                    {
                                        updateDonationCmd.Transaction = transaction;
                                        updateDonationCmd.CommandText = @"
                                            DECLARE @TotalPaid DECIMAL(18,2);
                                            
                                            SELECT @TotalPaid = ISNULL(SUM(PaidAmount), 0) 
                                            FROM CommitteePaymentRecord 
                                            WHERE CommitteeDonationId = @CommitteeDonationId AND SchoolID = @SchoolID;

                                            UPDATE CommitteeDonation
                                            SET PaidAmount = @TotalPaid
                                            WHERE CommitteeDonationId = @CommitteeDonationId AND SchoolID = @SchoolID";
                                        
                                        updateDonationCmd.Parameters.AddWithValue("@CommitteeDonationId", record.CommitteeDonationId);
                                        updateDonationCmd.Parameters.AddWithValue("@SchoolID", schoolID);
                                        updateDonationCmd.Connection = conn;

                                        updateDonationCmd.ExecuteNonQuery();
                                    }

                                    totalPaid += record.PaidAmount;
                                }

                                // ✅ Update total amount in money receipt
                                using (SqlCommand updateCmd = new SqlCommand())
                                {
                                    updateCmd.Transaction = transaction;
                                    updateCmd.CommandText = "UPDATE CommitteeMoneyReceipt SET TotalAmount = @TotalAmount WHERE CommitteeMoneyReceiptId = @CommitteeMoneyReceiptId";
                                    updateCmd.Parameters.AddWithValue("@TotalAmount", totalPaid);
                                    updateCmd.Parameters.AddWithValue("@CommitteeMoneyReceiptId", committeeMoneyReceiptId);
                                    updateCmd.Connection = conn;

                                    updateCmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                    
                    conn.Close();
                }

                // ✅ Success - Redirect to donation receipt
                // Use Response.Redirect(url, false) to prevent ThreadAbortException
                string successUrl = "~/Committee/DonationThankYou.aspx";
                Response.Redirect(successUrl, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                // Log detailed error
                string errorScript = "<script type='text/javascript'>alert('Payment processing error: " + ex.Message.Replace("'", "\\'") + "');</script>";
                ClientScript.RegisterStartupScript(this.GetType(), "PaymentError", errorScript);
                
                // Redirect to donor dashboard with error message
                Response.Redirect("~/Committee/Donor_Dues.aspx?error=payment_failed&msg=" + HttpUtility.UrlEncode(ex.Message));
            }
        }

        private string GetStudentId()
        {
            string id = "";
            
            // Check if session variables exist
            if (Session["StudentID"] == null || Session["RegistrationID"] == null)
            {
                return id;
            }
            
            string StudentID = Session["StudentID"].ToString();
            string RegistrationID = Session["RegistrationID"].ToString();

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT ID FROM Student WHERE StudentID = @StudentID AND StudentRegistrationID = @RegistrationID";
                    cmd.Parameters.AddWithValue("@StudentID", StudentID);
                    cmd.Parameters.AddWithValue("@RegistrationID", RegistrationID);

                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            id = sdr["ID"].ToString();
                        }
                    }
                    conn.Close();

                }
            }
            return id;
        }

        private string Encrypt(string clearText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        private int GetAdminRegistrationId(int schoolId)
        {
            int registrationId = 0;

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT RegistrationID FROM Registration WHERE SchoolID = @SchoolID AND Category = 'admin'";
                    cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            registrationId = Int32.Parse(sdr["RegistrationID"].ToString());
                        }
                    }
                    conn.Close();

                }
            }
            return registrationId;
        }
    }
    
    // Helper class for donation temp records
    internal class DonationTempRecord
    {
        public int CommitteeMemberId { get; set; }
        public int CommitteeDonationId { get; set; }
        public double PaidAmount { get; set; }
        public DateTime PaidDate { get; set; }
        public int AccountID { get; set; }
    }
}