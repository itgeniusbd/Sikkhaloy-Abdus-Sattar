using Education.Handeler;
using EDUCATION.COM.PaymentDataSetTableAdapters;
using EDUCATION.COM.Student.OnlinePayment;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Student
{
    public partial class Accounts : System.Web.UI.Page
    {
        private static readonly bool IsSandbox = false;
        private string StoreId = "";
        private string SignatureKey = "";
        private string PaymentGatewayBase = "";
        private string RequestURL = "";
        private string ConfirmationBase = "http://localhost:3326";
        private string RequestUrl = "/jsonpost.php";

        protected void Page_Load(object sender, EventArgs e)
        {
            //PaymentFactory<PaymentResponse> paymentFactory = new PaymentFactory<PaymentResponse>();
            //var paymentInfo = paymentFactory.GetPaymentInfoFromQueryString(Request);
        }
        protected void MreceiptLinkButton_Command(object sender, CommandEventArgs e)
        {
            AllPayRecordSQL.SelectParameters["MoneyReceiptID"].DefaultValue = e.CommandArgument.ToString();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Pop", "openModal();", true);
        }

        /* 
        protected void PayButton_Click(object sender, EventArgs e)
        {
            //MakeOnlinePayment("", "");
            // return;

            var Payment_DataSet = new OrdersTableAdapter();

            double TotalPaid = 0;
            int MoneyReceiptID = 0;
            int StudentClassID = Convert.ToInt32(Session["StudentClassID"].ToString());
            int StudentID = Convert.ToInt32(Session["StudentID"].ToString());

            int Crrent_EduYearID = Convert.ToInt32(Session["Edu_Year"].ToString());
            int SchoolID = Convert.ToInt32(Session["SchoolID"].ToString());
            int RegistrationID = GetAdminRegistrationId(SchoolID);  //Convert.ToInt32(Session["RegistrationID"].ToString());

            MoneyReceiptID = Convert.ToInt32(Payment_DataSet.Insert_MoneyReceipt(StudentID, RegistrationID, StudentClassID, Crrent_EduYearID, "Institution", DateTime.Now, SchoolID));

            //Current Session GV
            foreach (GridViewRow row in DueGridView.Rows)
            {
                CheckBox DueCheckBox = (CheckBox)row.FindControl("DueCheckBox");
                int PayOrderID = Convert.ToInt32(DueGridView.DataKeys[row.RowIndex]["PayOrderID"]);
                int RoleID = Convert.ToInt32(DueGridView.DataKeys[row.RowIndex]["RoleID"]);
                int P_Order_EduYearID = Convert.ToInt32(DueGridView.DataKeys[row.RowIndex]["EducationYearID"]);

                double PaidAmount = Convert.ToDouble(Payment_DataSet.DueByPayOrderID(PayOrderID));

                if (DueCheckBox.Checked)
                {
                    //Payment_DataSet.Insert_Payment_Record(StudentID, RegistrationID, RoleID, PayOrderID, PaidAmount, DueGridView.DataKeys[row.RowIndex]["PayFor"].ToString(), DateTime.Now, MoneyReceiptID, StudentClassID, P_Order_EduYearID, SchoolID, Convert.ToInt32(AccountDropDownList.SelectedValue));
                    Payment_DataSet.Insert_Payment_Record(StudentID, RegistrationID, RoleID, PayOrderID, PaidAmount, DueGridView.DataKeys[row.RowIndex]["PayFor"].ToString(), DateTime.Now, MoneyReceiptID, StudentClassID, P_Order_EduYearID, SchoolID, 2);
                    Payment_DataSet.Update_payOrder(PaidAmount, PayOrderID);

                    TotalPaid += PaidAmount;
                    DueCheckBox.Checked = false;
                }
            }

            Payment_DataSet.Update_MoneyReceipt(TotalPaid, MoneyReceiptID);


            string MRid = HttpUtility.UrlEncode(Encrypt(Convert.ToString(MoneyReceiptID)));
            string Sid = HttpUtility.UrlEncode(Encrypt(GetStudentId()));
            //Response.Redirect(string.Format("Money_Receipt.aspx?mN_R={0}&s_icD={1}", MRid, Sid));

            //Response.Redirect(string.Format("Money_Receipt.aspx?mN_R={0}&s_icD={1}", MRid, Sid));
            MakeOnlinePayment(MRid, Sid);
        }

*/
        protected void PayButton_Click(object sender, EventArgs e)
        {
            if (!IsOnlinePaymentApplicable())
            {
                string script = "<script type=\"text/javascript\">alert('Online Payment is not applicable for this institute.');</script>";
                ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script);
                return; //Online payment is not applicable
            }

            double totalPaid = 0;
            int StudentID = Convert.ToInt32(Session["StudentID"].ToString());
            var dateString = DateTime.Now.ToString("yyyyMMdd");
            long time = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            string paymentRecordId = dateString + "_" + time.ToString() + StudentID;
            int accountId = GetAccountId();
            var recordList = new List<RecordInfo>();
            var Payment_DataSet = new OrdersTableAdapter();

            //Current Session GV
            foreach (GridViewRow row in DueGridView.Rows)
            {
                CheckBox DueCheckBox = (CheckBox)row.FindControl("DueCheckBox");
                int PayOrderID = Convert.ToInt32(DueGridView.DataKeys[row.RowIndex]["PayOrderID"]);
                int RoleID = Convert.ToInt32(DueGridView.DataKeys[row.RowIndex]["RoleID"]);
                int P_Order_EduYearID = Convert.ToInt32(DueGridView.DataKeys[row.RowIndex]["EducationYearID"]);

                double PaidAmount = Convert.ToDouble(Payment_DataSet.DueByPayOrderID(PayOrderID));

                if (DueCheckBox.Checked)
                {
                    var recordInfo = new RecordInfo
                    {
                        PaymentRecordID = paymentRecordId,
                        StudentID = StudentID,
                        RoleID = RoleID,
                        PayOrderID = PayOrderID,
                        PayOrderEduYearID = P_Order_EduYearID,
                        PaidAmount = PaidAmount,
                        PayFor = DueGridView.DataKeys[row.RowIndex]["PayFor"].ToString(),
                        PaidDate = DateTime.Now,
                        AccountID = accountId
                    };

                    recordList.Add(recordInfo);

                    totalPaid += PaidAmount;
                    DueCheckBox.Checked = false;
                }
            }

            var studentInfo = GetStudentInformation();
            if (string.IsNullOrEmpty(studentInfo["email"]))
            {
                string script = "<script type=\"text/javascript\">alert('Email is required for Online Payment.');</script>";
                ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script);
                return; //Email is mandatory for online payment
            }

            InsertOnlineTeamporaryRecords(recordList);
            SetAmarpayCredentials();
            MakeOnlinePayment(paymentRecordId, totalPaid, studentInfo);
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
                    cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
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

        private Dictionary<string,string> GetStudentInformation()
        {
            var studentinfo = GetStudentInfo();
            string email = studentinfo["email"];
            if (string.IsNullOrEmpty(email))
            {
                email = GetInstituteEmailAddress();
                studentinfo["email"] = email;
            }

            return studentinfo;
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
                        cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
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

        private void InsertOnlineTeamporaryRecords(List<RecordInfo> recordList)
        {
            using (SqlConnection oConnection = new SqlConnection())
            {
                oConnection.ConnectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                oConnection.Open();
                using (SqlTransaction oTransaction = oConnection.BeginTransaction())
                {
                    using (SqlCommand oCommand = oConnection.CreateCommand())
                    {
                        oCommand.Transaction = oTransaction;
                        oCommand.CommandType = CommandType.Text;
                        oCommand.CommandText = "INSERT INTO [Temp_Online_PaymentRecord]" +
                                            " ([PaymentRecordID], [StudentID], [RoleID], [PayOrderID], [PayOrderEduYearID], [PaidAmount], [PayFor], [PaidDate], [AccountID])" +
                                            " VALUES (@PaymentRecordID, @StudentID, @RoleID, @PayOrderID, @PayOrderEduYearID, @PaidAmount, @PayFor, @PaidDate, @AccountID);";
                        oCommand.Parameters.Add(new SqlParameter("@PaymentRecordID", SqlDbType.NVarChar));
                        oCommand.Parameters.Add(new SqlParameter("@StudentID", SqlDbType.Int));
                        oCommand.Parameters.Add(new SqlParameter("@RoleID", SqlDbType.Int));
                        oCommand.Parameters.Add(new SqlParameter("@PayOrderID", SqlDbType.Int));
                        oCommand.Parameters.Add(new SqlParameter("@PayOrderEduYearID", SqlDbType.Int));
                        oCommand.Parameters.Add(new SqlParameter("@PaidAmount", SqlDbType.Decimal));
                        oCommand.Parameters.Add(new SqlParameter("@PayFor", SqlDbType.NVarChar));
                        oCommand.Parameters.Add(new SqlParameter("@PaidDate", SqlDbType.DateTime));
                        oCommand.Parameters.Add(new SqlParameter("@AccountID", SqlDbType.Int));
                        try
                        {
                            foreach (var record in recordList)
                            {
                                oCommand.Parameters[0].Value = record.PaymentRecordID;
                                oCommand.Parameters[1].Value = record.StudentID;
                                oCommand.Parameters[2].Value = record.RoleID;
                                oCommand.Parameters[3].Value = record.PayOrderID;
                                oCommand.Parameters[4].Value = record.PayOrderEduYearID;
                                oCommand.Parameters[5].Value = record.PaidAmount;
                                oCommand.Parameters[6].Value = record.PayFor;
                                oCommand.Parameters[7].Value = record.PaidDate;
                                oCommand.Parameters[8].Value = record.AccountID;
                                if (oCommand.ExecuteNonQuery() != 1)
                                {
                                    //'handled as needed, 
                                    //' but this snippet will throw an exception to force a rollback
                                    throw new InvalidProgramException();
                                }
                            }
                            oTransaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            oTransaction.Rollback();
                            throw;
                        }
                    }
                }
            }
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
        public void MakeOnlinePayment(string paymentRecordId, double totalPaid, Dictionary<string, string> dicStudentInfo)
        {
            string messgae = "";
            try
            {
                //Dictionary<string, string> parameters = new Dictionary<string, string>();

                var request = new PaymentRequest
                {
                    store_id = StoreId, //"aamarpaytest";
                    signature_key = SignatureKey,// "dbb74894e82415a2f7ff0ec3a97e4183";
                    tran_id = RandomString(10),
                    amount = totalPaid,
                    currency = "BDT",
                    desc = "Pay Fee",
                    cus_name = dicStudentInfo["name"],
                    cus_email = dicStudentInfo["email"],
                    cus_phone = dicStudentInfo["phone"],
                    type = "json",
                    //request.success_url = confirmationBase + "/Student/OnlinePayment/Success.aspx";
                    success_url = "https://sikkhaloy.com/Default.aspx",
                    fail_url = ConfirmationBase + "/Student/OnlinePayment/Failed.aspx",
                    cancel_url = ConfirmationBase + "/Student/OnlinePayment/Cancelled.aspx",
                    opt_a = GetSessionInfo(),
                    opt_b = paymentRecordId
                };

                //request.opt_b = moneyReceipt;
                //request.opt_c = sid;

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

                string requestUrl = PaymentGatewayBase + RequestUrl; // "/jsonpost.php";
                WebRequest wRequest = WebRequest.Create(requestUrl);
                wRequest.Method = "POST";
                wRequest.ContentType = "application/json";
                
                //wRequest.ContentType = "application/x-www-form-urlencoded";
                //byte[] bArray = Encoding.UTF8.GetBytes(postData);

                //wRequest.ContentLength = bArray.Length;
                //Stream webData = wRequest.GetRequestStream();
                //webData.Write(bArray, 0, bArray.Length);
                //webData.Close();

                //WebResponse webResponse = wRequest.GetResponse();
                //webData = webResponse.GetResponseStream();
                //StreamReader reader = new StreamReader(webData);
                //string responseFromServer = reader.ReadToEnd();
                //reader.Close();
                //webData.Close();
                //webResponse.Close();

                string responseFromServer = "";
                using (var streamWriter = new StreamWriter(wRequest.GetRequestStream()))
                {
                    string json = JsonConvert.SerializeObject(request);
                    streamWriter.Write(json);
                }

                var httpResponse = (WebResponse)wRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    responseFromServer = streamReader.ReadToEnd();
                }


                ////\/ paynow.php ? track = AAM1581391191203665

                //var trans = responseFromServer.Remove(0, 2).Split('"')[0];
                //string url = PaymentGatewayBase + trans;
                //Response.Redirect(url);

                var result = JsonConvert.DeserializeObject<ResponseInfo>(responseFromServer);
                if(result.payment_url == null){
                    string[] subStrings = responseFromServer.Split(':');
                    string errorMsg = subStrings[1].Replace('"', ' ').Replace('}', ' ').Trim();
                    string script = "<script type=\"text/javascript\">alert('" + errorMsg + "');</script>";
                    ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script);
                    return; //Email is mandatory for online payment
                }

                Response.Redirect(result.payment_url);
            }
            catch (Exception e)
            {
                string script = "<script type=\"text/javascript\">alert('Exception Occured.');</script>";
                ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script);
            }

            //return "";
        }

        private static Random random = new Random();
        public static string RandomString(int length)
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
            string educationYearID = Session["Edu_Year"] != null ? Session["Edu_Year"].ToString() : "";
            string studentID = Session["StudentID"] != null ? Session["StudentID"].ToString() : "";
            string classID = Session["ClassID"] != null ? Session["ClassID"].ToString() : "";
            string studentClassID = Session["StudentClassID"] != null ? Session["StudentClassID"].ToString() : "";
            string teacherID = Session["TeacherID"] != null ? Session["TeacherID"].ToString() : "";

            var dictionary = new Dictionary<string, string>
            {
                {"SchoolID", schoolID},
                {"SchoolName", schoolName},
                {"RegistrationID", registrationID},
                {"Edu_Year", educationYearID},
                {"StudentID", studentID},
                {"ClassID", classID},
                {"StudentClassID", studentClassID},
                {"TeacherID", teacherID},
            };

            var items = from kvp in dictionary
                        select kvp.Key + "=" + kvp.Value;
            return "{" + string.Join(",", items) + "}";

        }
        private string GetAdminRegistrationId(int schoolId)
        {
            string registrationId = "";

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
                            registrationId = sdr["RegistrationID"].ToString();
                        }
                    }
                    conn.Close();

                }
            }
            return registrationId;
        }
        private Dictionary<string,string> GetStudentInfo()
        {
            var dic = new Dictionary<string,string>(); 
            string studentID = Session["StudentID"] != null ? Session["StudentID"].ToString() : "";
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT StudentsName,StudentEmailAddress,SMSPhoneNo FROM Student WHERE StudentID = @StudentID";
                    cmd.Parameters.AddWithValue("@StudentID", studentID);
                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            dic["name"] = sdr["StudentsName"].ToString();
                            dic["email"] = sdr["StudentEmailAddress"].ToString();
                            dic["phone"] = sdr["SMSPhoneNo"].ToString();
                        }
                    }
                    conn.Close();

                }
            }

            return dic;
        }
        private string GetInstituteEmailAddress()
        {
            string schoolID = Session["SchoolID"] != null ? Session["SchoolID"].ToString() : "";
            string email = "";
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT Email FROM SchoolInfo WHERE SchoolID = @SchoolID";
                    cmd.Parameters.AddWithValue("@SchoolID", schoolID);
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

    class RecordInfo
    {
        public string PaymentRecordID { get; set; }
        public int StudentID { get; set; }
        public int RoleID { get; set; }
        public int PayOrderID { get; set; }
        public int PayOrderEduYearID { get; set; }
        public double PaidAmount { get; set; }
        public string PayFor { get; set; }
        public DateTime PaidDate { get; set; }
        public int AccountID { get; set; }

    }

    class ResponseInfo
    {
        public string result { get;}   
        public string payment_url { get; set;}
    }
}