using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.Services;
using System.Web.UI;
using Education;

namespace EDUCATION.COM.ACCOUNTS.Payment
{
    public partial class Payment_Collection_By_Date : Page
    {
        protected void Page_Load(object sender, EventArgs e) { }

        private static string ConnStr => ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
        private static string SchoolID => HttpContext.Current.Session["SchoolID"]?.ToString();
        private static string EduYear => HttpContext.Current.Session["Edu_Year"]?.ToString();
        private static string RegistrationID => HttpContext.Current.Session["RegistrationID"]?.ToString();

        // ?? Student Data ??????????????????????????????????????????????????????
        [WebMethod(EnableSession = true)]
        public static object GetStudentData(string studentID)
        {
            const string sql = @"SELECT Student.StudentID, StudentsClass.StudentClassID, StudentsClass.ClassID,
                Student.StudentImageID, Student.ID, Student.StudentsName, Student.SMSPhoneNo,
                CreateClass.Class, CreateSection.Section, CreateSubjectGroup.SubjectGroup,
                CreateShift.Shift, StudentsClass.RollNo, Student.FathersName,
                Education_Year.EducationYearID, Education_Year.EducationYear, Student.Status
                FROM StudentsClass
                INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID
                INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID
                LEFT JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID
                LEFT JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID
                LEFT JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID
                LEFT JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
                WHERE Student.ID = @ID AND StudentsClass.SchoolID = @SchoolID AND StudentsClass.Class_Status IS NULL";

            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@ID", studentID);
                cmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    if (!dr.Read()) return null;
                    return new
                    {
                        StudentID = dr["StudentID"], StudentClassID = dr["StudentClassID"],
                        ClassID = dr["ClassID"], StudentImageID = dr["StudentImageID"],
                        ID = dr["ID"], StudentsName = dr["StudentsName"],
                        SMSPhoneNo = dr["SMSPhoneNo"], Class = dr["Class"],
                        Section = dr["Section"] == DBNull.Value ? "" : dr["Section"],
                        Shift = dr["Shift"] == DBNull.Value ? "" : dr["Shift"],
                        RollNo = dr["RollNo"], FathersName = dr["FathersName"],
                        EducationYearID = dr["EducationYearID"], EducationYear = dr["EducationYear"],
                        Status = dr["Status"]
                    };
                }
            }
        }

        // ?? Due Data ??????????????????????????????????????????????????????????
        [WebMethod(EnableSession = true)]
        public static object GetDues(string studentID)
        {
            // LateFee শুধু তখনই যোগ হবে যখন EndDate < GETDATE() (সময় পার হয়েছে)
            const string sql = @"
                SELECT
                    po.PayOrderID, po.StudentID, po.EducationYearID, po.StudentClassID, po.ClassID,
                    cc.Class, ey.EducationYear, ir.Role,
                    po.PayFor, po.EndDate, po.StartDate,
                    po.Amount, po.Discount,
                    CASE WHEN po.EndDate < GETDATE() THEN ISNULL(po.LateFee, 0) ELSE 0 END AS LateFee,
                    po.LateFee_Discount, po.PaidAmount, po.RoleID,
                    (ISNULL(po.Amount,0)
                     + CASE WHEN po.EndDate < GETDATE() THEN ISNULL(po.LateFee,0) ELSE 0 END
                     - ISNULL(po.Discount,0)
                     - ISNULL(po.PaidAmount,0)
                     - ISNULL(po.LateFee_Discount,0)) AS Due
                FROM Income_PayOrder po
                INNER JOIN Student st ON po.StudentID = st.StudentID AND st.ID = @ID AND st.SchoolID = @SchoolID
                INNER JOIN Income_Roles ir ON po.RoleID = ir.RoleID
                INNER JOIN Education_Year ey ON po.EducationYearID = ey.EducationYearID
                INNER JOIN CreateClass cc ON po.ClassID = cc.ClassID
                WHERE po.SchoolID = @SchoolID AND po.Status = 'Due'
                ORDER BY po.EndDate";

            var currentDues = new List<object>();
            var otherDues   = new List<object>();

            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@ID", studentID);
                cmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                cmd.Parameters.AddWithValue("@EduYear", EduYear);
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var row = new
                        {
                            PayOrderID      = Convert.ToInt32(dr["PayOrderID"]),
                            StudentID       = Convert.ToInt32(dr["StudentID"]),
                            EducationYearID = Convert.ToInt32(dr["EducationYearID"]),
                            StudentClassID  = Convert.ToInt32(dr["StudentClassID"]),
                            Class           = dr["Class"].ToString(),
                            EducationYear   = dr["EducationYear"].ToString(),
                            Role            = dr["Role"].ToString(),
                            PayFor          = dr["PayFor"].ToString(),
                            EndDate         = dr["EndDate"],
                            StartDate       = dr["StartDate"],
                            Amount          = Convert.ToDouble(dr["Amount"]),
                            Discount        = dr["Discount"] == DBNull.Value ? 0 : Convert.ToDouble(dr["Discount"]),
                            LateFee         = dr["LateFee"] == DBNull.Value ? 0 : Convert.ToDouble(dr["LateFee"]),
                            LateFeeDiscount = dr["LateFee_Discount"] == DBNull.Value ? 0 : Convert.ToDouble(dr["LateFee_Discount"]),
                            PaidAmount      = dr["PaidAmount"] == DBNull.Value ? 0 : Convert.ToDouble(dr["PaidAmount"]),
                            RoleID          = Convert.ToInt32(dr["RoleID"]),
                            Due             = Convert.ToDouble(dr["Due"])
                        };
                        if (Convert.ToInt32(dr["EducationYearID"]) == Convert.ToInt32(EduYear))
                            currentDues.Add(row);
                        else
                            otherDues.Add(row);
                    }
                }
            }
            return new { CurrentDues = currentDues, OtherDues = otherDues };
        }

        // ?? Recent Payments ???????????????????????????????????????????????????
        [WebMethod(EnableSession = true)]
        public static List<object> GetRecentPayments(string studentID)
        {
            const string sql = @"SELECT TOP 10 mr.MoneyReceipt_SN, mr.TotalAmount,
                FORMAT(mr.PaidDate, 'dd MMM yyyy (hh:mm tt)') AS PaidDate,
                mr.MoneyReceiptID
                FROM Income_MoneyReceipt mr
                INNER JOIN Student st ON mr.StudentID = st.StudentID AND st.ID = @ID AND st.SchoolID = @SchoolID
                WHERE mr.EducationYearID = @EduYear AND mr.SchoolID = @SchoolID
                ORDER BY mr.PaidDate DESC";
            var list = new List<object>();
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@EduYear", EduYear);
                cmd.Parameters.AddWithValue("@ID", studentID);
                cmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                con.Open();
                using (var dr = cmd.ExecuteReader())
                    while (dr.Read())
                        list.Add(new { MoneyReceiptID = dr["MoneyReceiptID"], MoneyReceipt_SN = dr["MoneyReceipt_SN"], TotalAmount = dr["TotalAmount"], PaidDate = dr["PaidDate"] });
            }
            return list;
        }

        // ?? All Paid Records ??????????????????????????????????????????????????
        [WebMethod(EnableSession = true)]
        public static List<object> GetAllPaidRecords(string studentID)
        {
            const string sql = @"SELECT TOP 50 mr.MoneyReceipt_SN, mr.PrintedReceiptNo,
                mr.TotalAmount,
                FORMAT(mr.PaidDate, 'dd MMM yyyy (hh:mm tt)') AS PaidDate,
                mr.MoneyReceiptID,
                ad.FirstName+' '+ad.LastName AS ReceivedBy
                FROM Income_MoneyReceipt mr
                INNER JOIN Student st ON mr.StudentID = st.StudentID AND st.ID = @ID AND st.SchoolID = @SchoolID
                INNER JOIN Admin ad ON mr.RegistrationID = ad.RegistrationID
                WHERE mr.EducationYearID = @EduYear AND mr.SchoolID = @SchoolID
                ORDER BY mr.PaidDate DESC";

            var list = new List<object>();
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@EduYear", EduYear);
                cmd.Parameters.AddWithValue("@ID", studentID);
                cmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                con.Open();
                using (var dr = cmd.ExecuteReader())
                    while (dr.Read())
                        list.Add(new { MoneyReceiptID = dr["MoneyReceiptID"], MoneyReceipt_SN = dr["MoneyReceipt_SN"], PrintedReceiptNo = dr["PrintedReceiptNo"], TotalAmount = dr["TotalAmount"], PaidDate = dr["PaidDate"], ReceivedBy = dr["ReceivedBy"] });
            }
            return list;
        }

        // ── Previous Year Paid Records ────────────────────────────────────────
        [WebMethod(EnableSession = true)]
        public static List<object> GetPreviousYearPaidRecords(string studentID)
        {
            const string sql = @"SELECT mr.MoneyReceipt_SN,
                mr.TotalAmount,
                FORMAT(mr.PaidDate, 'dd MMM yyyy (hh:mm tt)') AS PaidDate,
                mr.MoneyReceiptID,
                ey.EducationYear
                FROM Income_MoneyReceipt mr
                INNER JOIN Student st ON mr.StudentID = st.StudentID AND st.ID = @ID AND st.SchoolID = @SchoolID
                INNER JOIN Education_Year ey ON mr.EducationYearID = ey.EducationYearID
                WHERE mr.SchoolID = @SchoolID AND mr.EducationYearID <> @EduYear
                  AND mr.EducationYearID = (
                    SELECT MAX(EducationYearID) FROM Income_MoneyReceipt
                    WHERE StudentID = st.StudentID AND SchoolID = @SchoolID AND EducationYearID <> @EduYear
                )
                ORDER BY mr.PaidDate DESC";

            var list = new List<object>();
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@ID", studentID);
                cmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                cmd.Parameters.AddWithValue("@EduYear", EduYear);
                con.Open();
                using (var dr = cmd.ExecuteReader())
                    while (dr.Read())
                        list.Add(new
                        {
                            MoneyReceiptID = dr["MoneyReceiptID"],
                            MoneyReceipt_SN = dr["MoneyReceipt_SN"],
                            TotalAmount = dr["TotalAmount"],
                            PaidDate = dr["PaidDate"],
                            EducationYear = dr["EducationYear"]
                        });
            }
            return list;
        }

        // ?? Receipt Detail ????????????????????????????????????????????????????
        [WebMethod(EnableSession = true)]
        public static List<object> GetReceiptDetail(int moneyReceiptID)
        {
            const string sql = @"SELECT Income_PaymentRecord.PaidAmount,
                Income_PaymentRecord.PayFor+' ('+Education_Year.EducationYear+')' AS PayFor,
                Income_Roles.Role
                FROM Income_PaymentRecord
                INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID=Income_Roles.RoleID
                INNER JOIN Income_MoneyReceipt ON Income_PaymentRecord.MoneyReceiptID=Income_MoneyReceipt.MoneyReceiptID
                INNER JOIN Education_Year ON Income_PaymentRecord.EducationYearID=Education_Year.EducationYearID
                WHERE Income_PaymentRecord.SchoolID=@SchoolID AND Income_PaymentRecord.MoneyReceiptID=@MID";

            var list = new List<object>();
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                cmd.Parameters.AddWithValue("@MID", moneyReceiptID);
                con.Open();
                using (var dr = cmd.ExecuteReader())
                    while (dr.Read())
                        list.Add(new { PaidAmount = Convert.ToDouble(dr["PaidAmount"]), PayFor = dr["PayFor"], Role = dr["Role"] });
            }
            return list;
        }

        // ?? Accounts ??????????????????????????????????????????????????????????
        [WebMethod(EnableSession = true)]
        public static List<object> GetAccounts()
        {
            var list = new List<object>();
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand("SELECT AccountID, AccountName, ISNULL(Default_Status, 0) AS IsDefault FROM Account WHERE SchoolID=@SchoolID ORDER BY ISNULL(Default_Status,0) DESC", con))
            {
                cmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                con.Open();
                using (var dr = cmd.ExecuteReader())
                    while (dr.Read())
                        list.Add(new { AccountID = dr["AccountID"], AccountName = dr["AccountName"], IsDefault = Convert.ToBoolean(dr["IsDefault"]) });
            }
            return list;
        }

        // ?? Roles
        [WebMethod(EnableSession = true)]
        public static List<object> GetRoles()
        {
            var list = new List<object>();
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand("SELECT RoleID, Role FROM Income_Roles WHERE SchoolID=@SchoolID", con))
            {
                cmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                con.Open();
                using (var dr = cmd.ExecuteReader())
                    while (dr.Read())
                        list.Add(new { RoleID = dr["RoleID"], Role = dr["Role"] });
            }
            return list;
        }

        // ?? SMS Setting ???????????????????????????????????????????????????????
        [WebMethod(EnableSession = true)]
        public static int GetSMSSetting()
        {
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand("SELECT TOP 1 PAY_Buttton_SMS_Enable_Disable FROM Account WHERE SchoolID=@SchoolID", con))
            {
                cmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                con.Open();
                var val = cmd.ExecuteScalar();
                return val != null && val != DBNull.Value ? Convert.ToInt32(val) : 0;
            }
        }

        [WebMethod(EnableSession = true)]
        public static void SaveSMSSetting(int value)
        {
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand("UPDATE Account SET PAY_Buttton_SMS_Enable_Disable=@V WHERE SchoolID=@SchoolID", con))
            {
                cmd.Parameters.AddWithValue("@V", value);
                cmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                con.Open(); cmd.ExecuteNonQuery();
            }
        }

        // ?? Concession Permission ?????????????????????????????????????????????
        [WebMethod(EnableSession = true)]
        public static bool GetConcessionPermission()
        {
            var user = HttpContext.Current.User.Identity.Name;
            if (Roles.IsUserInRole(user, "Admin")) return true;
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand("SELECT * FROM Link_Users WHERE SchoolID=@SchoolID AND RegistrationID=@RegID AND LinkID=3074", con))
            {
                cmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                cmd.Parameters.AddWithValue("@RegID", RegistrationID);
                con.Open();
                using (var dr = cmd.ExecuteReader()) return dr.HasRows;
            }
        }

        // ?? Current Due Banner ????????????????????????????????????????????????
        [WebMethod(EnableSession = true)]
        public static double GetCurrentDue(string studentID)
        {
            const string sql = @"SELECT ISNULL(SUM(
                ISNULL(po.Amount,0)+ISNULL(po.LateFee,0)-ISNULL(po.Discount,0)-ISNULL(po.PaidAmount,0)-ISNULL(po.LateFee_Discount,0)
                ),0) AS Due
                FROM Income_PayOrder po
                INNER JOIN Student st ON po.StudentID = st.StudentID AND st.ID = @ID AND st.SchoolID = @SchoolID
                WHERE po.SchoolID = @SchoolID AND po.Status = 'Due' AND po.EndDate <= GETDATE()";
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@ID", studentID);
                cmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                con.Open();
                var val = cmd.ExecuteScalar();
                return val != null && val != DBNull.Value ? Convert.ToDouble(val) : 0;
            }
        }

        // ?? Encrypt Receipt ID ????????????????????????????????????????????????
        [WebMethod(EnableSession = true)]
        public static object EncryptReceiptID(int moneyReceiptID, string studentID)
        {
            return new
            {
                MRid = HttpUtility.UrlEncode(Encrypt(moneyReceiptID.ToString())),
                Sid  = HttpUtility.UrlEncode(Encrypt(studentID))
            };
        }

        // ?? Process Payment (with custom Paid Date) ???????????????????????????
        [WebMethod(EnableSession = true)]
        public static object ProcessPayment(int studentDbID, int studentClassID, int educationYearID,
            string smsPhoneNo, string studentID, string studentName,
            int accountID, bool smsActive, string paidDate, List<PayItem> items)
        {
            if (items == null || items.Count == 0)
                return new { Success = false, Message = "No items selected." };

            // Parse the paid date from the date picker (format: yyyy-MM-dd)
            DateTime paidDateTime;
            if (!DateTime.TryParse(paidDate, out paidDateTime))
                return new { Success = false, Message = "Invalid Paid Date format." };

            // Keep time as current time but use the selected date
            paidDateTime = paidDateTime.Date.Add(DateTime.Now.TimeOfDay);

            int schoolID       = Convert.ToInt32(SchoolID);
            int registrationID = Convert.ToInt32(HttpContext.Current.Session["RegistrationID"]);

            try
            {
                using (var con = new SqlConnection(ConnStr))
                {
                    con.Open();

                    // Validate: no item exceeds due
                    foreach (var item in items)
                    {
                        double due = GetDueByPayOrderID(con, item.PayOrderID);
                        if (item.PaidAmount > due)
                            return new { Success = false, Message = "Paid amount exceeds due for PayOrder " + item.PayOrderID };
                    }

                    // Insert Money Receipt via Stored Procedure
                    int moneyReceiptID = 0;
                    using (var cmd = new SqlCommand("dbo.MoneyReceipt", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@StudentID", studentDbID);
                        cmd.Parameters.AddWithValue("@RegistrationID", registrationID);
                        cmd.Parameters.AddWithValue("@StudentClassID", studentClassID);
                        cmd.Parameters.AddWithValue("@EducationYearID", educationYearID);
                        cmd.Parameters.AddWithValue("@PaymentBy", "Institution");
                        cmd.Parameters.AddWithValue("@PaidDate", paidDateTime);  // ? custom paid date
                        cmd.Parameters.AddWithValue("@SchoolID", schoolID);
                        var result = cmd.ExecuteScalar();
                        moneyReceiptID = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                    }

                    if (moneyReceiptID <= 0)
                        return new { Success = false, Message = "Money Receipt ???? ??????" };

                    double totalPaid = 0;
                    string message = "";
                    string sessionInfo = "";

                    foreach (var item in items)
                    {
                        double due = GetDueByPayOrderID(con, item.PayOrderID);
                        if (item.PaidAmount > due) continue;

                        int roleID = 0, payOrderEduYearID = educationYearID, scid = studentClassID;
                        string payFor = "", roleName = "";
                        using (var cmd = new SqlCommand("SELECT po.RoleID, po.PayFor, po.EducationYearID, po.StudentClassID, ir.Role FROM Income_PayOrder po INNER JOIN Income_Roles ir ON po.RoleID = ir.RoleID WHERE po.PayOrderID=@P", con))
                        {
                            cmd.Parameters.AddWithValue("@P", item.PayOrderID);
                            using (var dr = cmd.ExecuteReader())
                                if (dr.Read()) { roleID = Convert.ToInt32(dr["RoleID"]); payFor = dr["PayFor"].ToString(); payOrderEduYearID = Convert.ToInt32(dr["EducationYearID"]); scid = Convert.ToInt32(dr["StudentClassID"]); roleName = dr["Role"].ToString(); }
                        }

                        using (var cmd = new SqlCommand(@"INSERT INTO Income_PaymentRecord(StudentID,RegistrationID,RoleID,PayOrderID,PaidAmount,PayFor,PaidDate,MoneyReceiptID,StudentClassID,EducationYearID,SchoolID,AccountID)
                        VALUES(@SID,@RID,@RoleID,@PID,@PA,@PF,@Date,@MID,@SCID,@EID,@SchID,@AccID)", con))
                        {
                            cmd.Parameters.AddWithValue("@SID", studentDbID);
                            cmd.Parameters.AddWithValue("@RID", registrationID);
                            cmd.Parameters.AddWithValue("@RoleID", roleID);
                            cmd.Parameters.AddWithValue("@PID", item.PayOrderID);
                            cmd.Parameters.AddWithValue("@PA", item.PaidAmount);
                            cmd.Parameters.AddWithValue("@PF", payFor);
                            cmd.Parameters.AddWithValue("@Date", paidDateTime);  // ? custom paid date
                            cmd.Parameters.AddWithValue("@MID", moneyReceiptID);
                            cmd.Parameters.AddWithValue("@SCID", scid);
                            cmd.Parameters.AddWithValue("@EID", payOrderEduYearID);
                            cmd.Parameters.AddWithValue("@SchID", schoolID);
                            cmd.Parameters.AddWithValue("@AccID", accountID);
                            cmd.ExecuteNonQuery();
                        }

                        // Update PayOrder PaidAmount and Is_LateFeeAdded
                        // Is_LateFeeAdded=1 ensures the computed Status column uses LateFee in calculation
                        using (var cmd = new SqlCommand(@"UPDATE Income_PayOrder
                            SET PaidAmount = PaidAmount + @PA,
                                LastPaidDate = @Date,
                                NumberOfPayment = NumberOfPayment + 1,
                                Is_LateFeeAdded = CASE 
                                    WHEN EndDate < GETDATE() AND ISNULL(LateFee,0) > 0 THEN 1 
                                    ELSE Is_LateFeeAdded 
                                END
                            WHERE PayOrderID = @P", con))
                        {
                            cmd.Parameters.AddWithValue("@PA", item.PaidAmount);
                            cmd.Parameters.AddWithValue("@Date", paidDateTime);
                            cmd.Parameters.AddWithValue("@P", item.PayOrderID);
                            cmd.ExecuteNonQuery();
                        }

                        totalPaid += item.PaidAmount;
                        message += $", {roleName}-{payFor}";
                        sessionInfo = payOrderEduYearID.ToString();
                    }

                    using (var cmd = new SqlCommand("UPDATE Income_MoneyReceipt SET TotalAmount=@T WHERE MoneyReceiptID=@MID", con))
                    {
                        cmd.Parameters.AddWithValue("@T", totalPaid);
                        cmd.Parameters.AddWithValue("@MID", moneyReceiptID);
                        cmd.ExecuteNonQuery();
                    }

                    if (totalPaid == 0) return new { Success = false, Message = "No payment processed." };

                    string receiptSN = "";
                    using (var cmd = new SqlCommand("SELECT MoneyReceipt_SN FROM Income_MoneyReceipt WHERE MoneyReceiptID=@MID", con))
                    {
                        cmd.Parameters.AddWithValue("@MID", moneyReceiptID);
                        receiptSN = cmd.ExecuteScalar()?.ToString();
                    }
                    if (string.IsNullOrEmpty(receiptSN))
                        receiptSN = moneyReceiptID.ToString();

                    // Get education year name for {Session} placeholder
                    string sessionName = "";
                    if (!string.IsNullOrEmpty(sessionInfo))
                    {
                        using (var cmd = new SqlCommand("SELECT EducationYear FROM Education_Year WHERE EducationYearID=@EID", con))
                        {
                            cmd.Parameters.AddWithValue("@EID", sessionInfo);
                            sessionName = cmd.ExecuteScalar()?.ToString() ?? "";
                        }
                    }

                    if (smsActive) TrySendSMS(smsPhoneNo, studentID, studentName, totalPaid, receiptSN, message, schoolID, studentDbID, sessionName);

                    return new
                    {
                        Success = true,
                        MRid = HttpUtility.UrlEncode(Encrypt(moneyReceiptID.ToString())),
                        Sid  = HttpUtility.UrlEncode(Encrypt(studentID))
                    };
                }
            }
            catch (Exception ex)
            {
                return new { Success = false, Message = "Exception: " + ex.Message + (ex.InnerException != null ? " | Inner: " + ex.InnerException.Message : "") };
            }
        }

        private static double GetDueByPayOrderID(SqlConnection con, int payOrderID)
        {
            // LateFee শুধু EndDate পার হলে Due তে যোগ হবে
            const string sql = @"SELECT ISNULL(Amount,0)
                + CASE WHEN EndDate < GETDATE() THEN ISNULL(LateFee,0) ELSE 0 END
                - ISNULL(Discount,0) - ISNULL(LateFee_Discount,0) - ISNULL(PaidAmount,0) AS Due
                FROM Income_PayOrder WHERE PayOrderID=@P";
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@P", payOrderID);
                var val = cmd.ExecuteScalar();
                return val != null && val != DBNull.Value ? Convert.ToDouble(val) : 0;
            }
        }

        // ?? Update Concession / LateFee ???????????????????????????????????????
        [WebMethod(EnableSession = true)]
        public static object UpdateConcession(List<ConcessionItem> items)
        {
            if (items == null || items.Count == 0) return new { Success = false, Message = "No items." };
            using (var con = new SqlConnection(ConnStr))
            {
                con.Open();
                foreach (var item in items)
                {
                    using (var cmd = new SqlCommand("UPDATE Income_PayOrder SET Discount=@D WHERE PayOrderID=@P", con))
                    {
                        cmd.Parameters.AddWithValue("@D", item.Discount);
                        cmd.Parameters.AddWithValue("@P", item.PayOrderID);
                        cmd.ExecuteNonQuery();
                    }
                    if (item.LateFee != item.PrevLateFee)
                    {
                        using (var cmd = new SqlCommand("UPDATE Income_PayOrder SET LateFee=@L WHERE PayOrderID=@P", con))
                        {
                            cmd.Parameters.AddWithValue("@L", item.LateFee);
                            cmd.Parameters.AddWithValue("@P", item.PayOrderID);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            return new { Success = true };
        }

        // ?? Add More Payment ??????????????????????????????????????????????????
        [WebMethod(EnableSession = true)]
        public static object AddMorePayment(int studentDbID, int studentClassID, int classID,
            int educationYearID, int roleID, string payFor, double amount, double discount)
        {
            int registrationID = Convert.ToInt32(HttpContext.Current.Session["RegistrationID"]);
            const string sql = @"INSERT INTO Income_PayOrder(SchoolID,RegistrationID,StudentID,ClassID,StudentClassID,Amount,Discount,LateFee,RoleID,PayFor,StartDate,EndDate,CreatedDate,EducationYearID)
                VALUES(@SchID,@RID,@SID,@CID,@SCID,@Amt,@Dis,0,@RoleID,@PF,GETDATE(),GETDATE(),GETDATE(),@EID)";
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@SchID", SchoolID);
                cmd.Parameters.AddWithValue("@RID", registrationID);
                cmd.Parameters.AddWithValue("@SID", studentDbID);
                cmd.Parameters.AddWithValue("@CID", classID);
                cmd.Parameters.AddWithValue("@SCID", studentClassID);
                cmd.Parameters.AddWithValue("@Amt", amount);
                cmd.Parameters.AddWithValue("@Dis", discount);
                cmd.Parameters.AddWithValue("@RoleID", roleID);
                cmd.Parameters.AddWithValue("@PF", payFor);
                cmd.Parameters.AddWithValue("@EID", educationYearID);
                con.Open(); cmd.ExecuteNonQuery();
            }
            return new { Success = true };
        }

        // ?? SMS ???????????????????????????????????????????????????????????????
        private static void TrySendSMS(string phoneNo, string studentID, string studentName,
            double totalAmount, string receiptNo, string details, int schoolID, int studentDbID, string sessionName = "")
        {
            try
            {
                var sms = new SMS_Class(schoolID.ToString());
                string template = GetSMSTemplate(schoolID);
                decimal currentDue = GetCurrentDueDecimal(studentID, schoolID);

                string msg = !string.IsNullOrEmpty(template)
                    ? template
                        .Replace("{StudentName}", studentName).Replace("{ID}", studentID)
                        .Replace("{Amount}", totalAmount.ToString("0.00")).Replace("{ReceiptNo}", receiptNo)
                        .Replace("{CurrentDue}", currentDue.ToString("0.00"))
                        .Replace("{PaymentDetails}", details.TrimStart(',', ' '))
                        .Replace("{Session}", sessionName)
                        .Replace("{SchoolName}", HttpContext.Current.Session["School_Name"]?.ToString())
                    : $"অভিনন্দন! {studentName} (ID:{studentID}). আপনি: {totalAmount} টাকা পরিশোধ করেছেন. রিসিট নম্বর: {receiptNo}, ধন্যবাদ, {HttpContext.Current.Session["School_Name"]}";

                int totalSMS = sms.SMS_Conut(msg);
                if (sms.SMSBalance >= totalSMS && sms.SMS_GetBalance() >= totalSMS)
                {
                    var valid = sms.SMS_Validation(phoneNo, msg);
                    if (valid.Validation)
                    {
                        Guid smsSendId = sms.SMS_Send(phoneNo, msg, "Payment Collection");
                        if (smsSendId != Guid.Empty)
                        {
                            InsertSmsOtherInfo(smsSendId, schoolID, studentDbID);
                        }
                    }
                }
            }
            catch { }
        }

        private static void InsertSmsOtherInfo(Guid smsSendId, int schoolID, int studentDbID)
        {
            try
            {
                int eduYearID = Convert.ToInt32(HttpContext.Current.Session["Edu_Year"]);
                using (var con = new SqlConnection(ConnStr))
                using (var cmd = new SqlCommand("INSERT INTO SMS_OtherInfo(SMS_Send_ID, SchoolID, StudentID, EducationYearID) VALUES (@SMS_Send_ID, @SchoolID, @StudentID, @EducationYearID)", con))
                {
                    cmd.Parameters.AddWithValue("@SMS_Send_ID", smsSendId);
                    cmd.Parameters.AddWithValue("@SchoolID", schoolID);
                    cmd.Parameters.AddWithValue("@StudentID", studentDbID);
                    cmd.Parameters.AddWithValue("@EducationYearID", eduYearID);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }
        }

        private static string GetSMSTemplate(int schoolID)
        {
            try
            {
                using (var con = new SqlConnection(ConnStr))
                using (var cmd = new SqlCommand(@"SELECT TOP 1 MessageTemplate FROM SMS_Template
                    WHERE SchoolID=@SchoolID AND TemplateType='Payment' AND IsActive=1 ORDER BY CreatedDate DESC", con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", schoolID);
                    con.Open();
                    var r = cmd.ExecuteScalar();
                    return r?.ToString() ?? "";
                }
            }
            catch { return ""; }
        }

        private static decimal GetCurrentDueDecimal(string studentID, int schoolID)
        {
            try
            {
                using (var con = new SqlConnection(ConnStr))
                using (var cmd = new SqlCommand(@"SELECT ISNULL(SUM(CASE WHEN Income_PayOrder.EndDate<GETDATE()-1
                    THEN ISNULL(Income_PayOrder.Amount,0)+ISNULL(Income_PayOrder.LateFee,0)-ISNULL(Income_PayOrder.Discount,0)-ISNULL(Income_PayOrder.PaidAmount,0)-ISNULL(Income_PayOrder.LateFee_Discount,0)
                    ELSE ISNULL(Income_PayOrder.Amount,0)-ISNULL(Income_PayOrder.Discount,0)-ISNULL(Income_PayOrder.PaidAmount,0) END),0)
                    FROM Income_PayOrder INNER JOIN Student ON Income_PayOrder.StudentID=Student.StudentID
                    WHERE Income_PayOrder.Status='Due' AND Student.ID=@ID AND Income_PayOrder.SchoolID=@SchID", con))
                {
                    cmd.Parameters.AddWithValue("@ID", studentID);
                    cmd.Parameters.AddWithValue("@SchID", schoolID);
                    con.Open();
                    var r = cmd.ExecuteScalar();
                    return r != null && r != DBNull.Value ? Convert.ToDecimal(r) : 0;
                }
            }
            catch { return 0; }
        }

        // ?? Encrypt ???????????????????????????????????????????????????????????
        private static string Encrypt(string clearText)
        {
            const string key = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (var aes = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(key, new byte[] { 0x49,0x76,0x61,0x6e,0x20,0x4d,0x65,0x64,0x76,0x65,0x64,0x65,0x76 });
                aes.Key = pdb.GetBytes(32); aes.IV = pdb.GetBytes(16);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    { cs.Write(clearBytes, 0, clearBytes.Length); cs.Close(); }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        // ?? DTOs ??????????????????????????????????????????????????????????????
        public class PayItem
        {
            public int PayOrderID { get; set; }
            public double PaidAmount { get; set; }
            public bool IsOtherSession { get; set; }
        }

        public class ConcessionItem
        {
            public int PayOrderID { get; set; }
            public double Discount { get; set; }
            public double LateFee { get; set; }
            public double PrevLateFee { get; set; }
        }
    }
}
