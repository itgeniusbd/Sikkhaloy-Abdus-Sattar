using EDUCATION.COM.Student.OnlinePayment;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;

namespace EDUCATION.COM.Student.OnlinePayment
{
    public partial class Payment_Success : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string paymentResponse = Session["OnlinePaymentInfo"].ToString();
            Session.Remove("OnlinePaymentInfo");
            var paymentInfo = JsonConvert.DeserializeObject<PaymentResponse>(paymentResponse);

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



            //PaymentFactory<PaymentResponse> paymentFactory = new PaymentFactory<PaymentResponse>();
            //var paymentInfo = paymentFactory.GetPaymentInfoFromQueryString(Request);

        }
    }
}