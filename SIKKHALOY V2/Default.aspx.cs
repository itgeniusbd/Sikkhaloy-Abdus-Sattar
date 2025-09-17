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
using System.Web.Security;
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
                SetSessionInfoAfterOnlinePayment(paymentInfo.opt_a);
                Session["OnlinePaymentInfo"] = JsonConvert.SerializeObject(paymentInfo);
                string paymentRecordId = paymentInfo.opt_b;
                SavePaymentInfoAfterSuccess(paymentRecordId);
                //string MRid = paymentInfo.opt_b;
                //string Sid = paymentInfo.opt_c;

                //Response.Redirect(string.Format("~/Accounts/Payment/Money_Receipt.aspx?mN_R={0}&s_icD={1}", MRid, Sid));

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
            Dictionary<string, string> dictionary = sessionInfo.Split(',').ToDictionary(item => item.Split('=')[0], item => item.Split('=')[1]);
            if (dictionary != null)
            {
                foreach (KeyValuePair<string, string> entry in dictionary)
                {
                    Session[entry.Key] = !string.IsNullOrEmpty(entry.Value) ? entry.Value : null;
                }
            }
        }

        private void SavePaymentInfoAfterSuccess(string paymentRecordID)
        {
            var Payment_DataSet = new OrdersTableAdapter();

            double TotalPaid = 0;
            int MoneyReceiptID = 0;
            int StudentClassID = Convert.ToInt32(Session["StudentClassID"].ToString());
            int StudentID = Convert.ToInt32(Session["StudentID"].ToString());

            int Crrent_EduYearID = Convert.ToInt32(Session["Edu_Year"].ToString());
            int SchoolID = Convert.ToInt32(Session["SchoolID"].ToString());
            int RegistrationID = GetAdminRegistrationId(SchoolID);  //Convert.ToInt32(Session["RegistrationID"].ToString());

            MoneyReceiptID = Convert.ToInt32(Payment_DataSet.Insert_MoneyReceipt(StudentID, RegistrationID, StudentClassID, Crrent_EduYearID, "Institution", DateTime.Now, SchoolID));

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
                            Payment_DataSet.Insert_Payment_Record(StudentID, RegistrationID, RoleID, PayOrderID, PaidAmount, PayFor, PaidDate, MoneyReceiptID, StudentClassID, P_Order_EduYearID, SchoolID, AccountID);
                            Payment_DataSet.Update_payOrder(PaidAmount, PayOrderID);
                            TotalPaid += PaidAmount;
                        }
                    }
                    conn.Close();

                }
            }

            Payment_DataSet.Update_MoneyReceipt(TotalPaid, MoneyReceiptID);

            string MRid = HttpUtility.UrlEncode(Encrypt(Convert.ToString(MoneyReceiptID)));
            string Sid = HttpUtility.UrlEncode(Encrypt(GetStudentId()));
            Response.Redirect(string.Format("~/Accounts/Payment/Money_Receipt.aspx?mN_R={0}&s_icD={1}", MRid, Sid));

        }

        private string GetStudentId()
        {
            string id = "";
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
    }
}