using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Profile.Invoice
{
    public partial class ShurjoPayCallback : System.Web.UI.Page
    {
        // Application-level lock: একই spOrderId-এর জন্য দুটো concurrent request একসাথে process হবে না
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _orderLocks
            = new ConcurrentDictionary<string, SemaphoreSlim>();
        public string HeaderCssClass { get; private set; } = "pending";
        public string HeaderIcon     { get; private set; } = "fa-clock";
        public string HeaderTitle    { get; private set; } = "পেমেন্ট যাচাই করা হচ্ছে...";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // ShurjoPay callback URL-এ order_id = ShurjoPay-generated ID (SIK...) আসে
                // এটাই /api/verification-এ পাঠাতে হবে
                string spOrderId  = Request.QueryString["order_id"];

                // mer_order_id = আমাদের নিজস্ব merchant order_id (SMSR_... বা ITG_...)
                // session fallback থেকে নেওয়া হয়
                string merOrderId = Session["PendingSMSRecharge_MerOrderID"] != null
                    ? Session["PendingSMSRecharge_MerOrderID"].ToString()
                    : null;

                // Verify সবসময় ShurjoPay-এর নিজস্ব order_id (SIK...) দিয়ে করতে হবে
                if (string.IsNullOrEmpty(spOrderId))
                {
                    SetHeader("failed", "fa-times-circle", "পেমেন্ট ব্যর্থ");
                    SetLabel("lblMessage", "alert alert-danger d-block text-center",
                             "<i class='fa fa-exclamation-circle'></i> অর্ডার আইডি পাওয়া যায়নি।");
                    return;
                }

                VerifyAndUpdate(spOrderId, merOrderId);
            }
        }

        private void VerifyAndUpdate(string spOrderId, string merOrderId)
        {
            // একই spOrderId-এর জন্য concurrent requests block করি
            SemaphoreSlim orderLock = _orderLocks.GetOrAdd(spOrderId, _ => new SemaphoreSlim(1, 1));
            bool lockAcquired = orderLock.Wait(TimeSpan.FromSeconds(30));
            if (!lockAcquired)
            {
                SetHeader("failed", "fa-times-circle", "সংযোগ ব্যর্থ");
                SetLabel("lblMessage", "alert alert-warning d-block text-center",
                    "<i class='fa fa-exclamation-circle'></i> অনুরোধ প্রক্রিয়াকরণে বিলম্ব হচ্ছে। পুনরায় চেষ্টা করুন।");
                return;
            }
            try
            {
                var service = new ShurjoPayService();
                // Verify সবসময় ShurjoPay-এর নিজস্ব ID (SIK...) দিয়ে করতে হবে
                ShurjoPayVerifyResponse verify = service.VerifyPayment(spOrderId);

                if (verify == null)
                {
                    SetHeader("failed", "fa-times-circle", "পেমেন্ট ব্যর্থ");
                    SetLabel("lblMessage", "alert alert-danger d-block",
                             "<i class='fa fa-exclamation-circle'></i> পেমেন্ট যাচাই করা সম্ভব হয়নি।"
                             + "<br/>SP_OrderID: " + spOrderId
                             + "<br/>RAW Response: <pre style='text-align:left;font-size:11px;word-break:break-all'>" 
                             + HttpUtility.HtmlEncode(service.LastRawVerifyResponse ?? "null") + "</pre>");
                    return;
                }

                // DB-তে save করার জন্য আমাদের নিজস্ব merchant order_id ব্যবহার করি
                // verify.order_id = আমাদের merchant order_id যেটা CreateOrder-এ পাঠিয়েছিলাম
                string ourOrderId = verify.order_id
                    ?? merOrderId
                    ?? spOrderId;

                // Session cleanup
                Session.Remove("PendingSMSRecharge_MerOrderID");

                SetLabel("lblOrderId", "", ourOrderId);
                SetLabel("lblAmount",  "", (verify.recv_amt ?? verify.amount ?? "0") + " ৳");
                SetLabel("lblMethod",  "", verify.method ?? "-");
                SetLabel("lblTrxId",   "", verify.bank_trx_id ?? "-");
                SetLabel("lblDate",    "", verify.recv_dt ?? DateTime.Now.ToString("dd MMM yyyy"));
                SetLabel("lblStatus",  "", verify.transaction_status ?? verify.bank_status ?? "-");

                bool isPaid = IsSuccessStatus(verify.sp_code, verify.bank_status, verify.transaction_status);

                if (isPaid)
                {
                    // SchoolID: value1 থেকে অথবা Session থেকে
                    int schoolId = 0;
                    if (!string.IsNullOrEmpty(verify.value1))
                        int.TryParse(verify.value1, out schoolId);
                    if (schoolId <= 0 && Session["PendingSMSRecharge_SchoolID"] != null)
                        int.TryParse(Session["PendingSMSRecharge_SchoolID"].ToString(), out schoolId);

                    // invoiceNote: value2 থেকে অথবা Session থেকে reconstruct
                    string invoiceNote = verify.value2 ?? "";
                    if (string.IsNullOrEmpty(invoiceNote) && Session["PendingSMSRecharge_SMSQty"] != null)
                    {
                        string sqty = Session["PendingSMSRecharge_SMSQty"].ToString();
                        string srid = Session["PendingSMSRecharge_RegistrationID"] != null
                                    ? Session["PendingSMSRecharge_RegistrationID"].ToString() : "0";
                        invoiceNote = "SMS_RECHARGE|" + sqty + "|" + srid;
                    }

                    bool isSmsRecharge = invoiceNote.StartsWith("SMS_RECHARGE|")
                                     || Session["PendingSMSRecharge_SMSQty"] != null
                                     || ourOrderId.StartsWith("SMSR_")
                                     || spOrderId.StartsWith("SMSR_")
                                     || (!string.IsNullOrEmpty(merOrderId) && merOrderId.StartsWith("SMSR_"));

                    if (isSmsRecharge)
                    {
                        bool saved = SaveSMSRechargeAfterPayment(spOrderId, ourOrderId, verify, schoolId, invoiceNote);
                        if (saved)
                        {
                            SetHeader("success", "fa-check-circle", "পেমেন্ট সফল!");
                            // Message is set inside SaveSMSRechargeAfterPayment with charge breakdown
                        }
                        else
                        {
                            SetHeader("failed", "fa-times-circle", "সংরক্ষণ ব্যর্থ");
                        }
                    }
                    else
                    {
                        bool saved = MarkInvoiceAsPaid(spOrderId, ourOrderId, verify, schoolId);
                        if (saved)
                        {
                            SetHeader("success", "fa-check-circle", "পেমেন্ট সফল!");
                            // Message is set inside MarkInvoiceAsPaid with charge breakdown
                        }
                        else
                        {
                            SetHeader("failed", "fa-times-circle", "সংরক্ষণ ব্যর্থ");
                        }
                    }
                }
                else
                {
                    SetHeader("failed", "fa-times-circle", "পেমেন্ট ব্যর্থ");
                    SetLabel("lblMessage", "alert alert-danger d-block text-center",
                             "<i class='fa fa-exclamation-circle'></i> পেমেন্ট সফল হয়নি। স্ট্যাটাস: "
                             + (verify.bank_status ?? verify.transaction_status ?? "Failed")
                             + " | sp_code: " + (verify.sp_code ?? "null"));
                }
            }
            catch (Exception ex)
            {
                SetHeader("failed", "fa-times-circle", "পেমেন্ট ব্যর্থ");
                SetLabel("lblMessage", "alert alert-danger d-block text-center",
                         "<i class='fa fa-exclamation-circle'></i> ত্রুটি: " + ex.Message);
            }
            finally
            {
                orderLock.Release();
                // কিছুক্ষণ পর lock entry cleanup করি
                if (orderLock.CurrentCount == 1)
                    _orderLocks.TryRemove(spOrderId, out _);
            }
        }

        private bool IsSuccessStatus(string spCode, string bankStatus, string transStatus)
        {
            if (!string.IsNullOrEmpty(spCode) && (spCode == "1000" || spCode == "200"))
                return true;
            if (!string.IsNullOrEmpty(bankStatus) &&
                (bankStatus.Equals("Success", StringComparison.OrdinalIgnoreCase) ||
                 bankStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase)))
                return true;
            if (!string.IsNullOrEmpty(transStatus) &&
                (transStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase) ||
                 transStatus.Equals("Success", StringComparison.OrdinalIgnoreCase) ||
                 transStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase)))
                return true;
            return false;
        }

        private bool SaveSMSRechargeAfterPayment(string spOrderId, string orderId, ShurjoPayVerifyResponse verify, int schoolId, string invoiceNote)
        {
            // invoiceNote = "SMS_RECHARGE|smsQty|registrationID|schoolID"
            int smsQty         = 0;
            int registrationID = 0;

            if (invoiceNote.StartsWith("SMS_RECHARGE|"))
            {
                // Format: SMS_RECHARGE|smsQty|registrationID|schoolID
                // Split gives: [0]=SMS_RECHARGE [1]=smsQty [2]=registrationID [3]=schoolID
                var parts = invoiceNote.Split('|');
                if (parts.Length >= 2) int.TryParse(parts[1], out smsQty);
                if (parts.Length >= 3) int.TryParse(parts[2], out registrationID);
                if (parts.Length >= 4 && schoolId <= 0) int.TryParse(parts[3], out schoolId);
            }

            // Session fallback
            if (smsQty <= 0 && Session["PendingSMSRecharge_SMSQty"] != null)
                int.TryParse(Session["PendingSMSRecharge_SMSQty"].ToString(), out smsQty);
            if (registrationID <= 0 && Session["PendingSMSRecharge_RegistrationID"] != null)
                int.TryParse(Session["PendingSMSRecharge_RegistrationID"].ToString(), out registrationID);
            if (schoolId <= 0 && Session["PendingSMSRecharge_SchoolID"] != null)
                int.TryParse(Session["PendingSMSRecharge_SchoolID"].ToString(), out schoolId);

            // Session clear
            Session.Remove("PendingSMSRecharge_SMSQty");
            Session.Remove("PendingSMSRecharge_RegistrationID");
            Session.Remove("PendingSMSRecharge_SchoolID");
            Session.Remove("PendingSMSRecharge_Amount");

            // Debug info — screen-এ দেখাও
            string debugInfo = "schoolId=" + schoolId + " | smsQty=" + smsQty
                             + " | regID=" + registrationID
                             + " | orderId=" + orderId
                             + " | value1=" + (verify.value1 ?? "null")
                             + " | value2=" + (verify.value2 ?? "null")
                             + " | sp_code=" + (verify.sp_code ?? "null")
                             + " | bank_status=" + (verify.bank_status ?? "null")
                             + " | recv_amt=" + (verify.recv_amt ?? "null");

            if (smsQty <= 0 || schoolId <= 0)
            {
                SetLabel("lblMessage", "alert alert-warning d-block",
                    "⚠️ SMS Qty বা SchoolID পাওয়া যায়নি। Debug: " + debugInfo);
                return false;
            }

            double  totalAmount   = smsQty * 0.36;
            decimal invoiceDueAmt = (decimal)totalAmount;

            // Amounts from ShurjoPay verify response
            decimal customerPaidAmt = 0m;
            decimal recvAmtSms      = 0m;
            decimal spAmtSms        = 0m;

            decimal.TryParse(verify.payable_amount ?? "", out customerPaidAmt);
            decimal.TryParse(verify.recv_amt       ?? "", out recvAmtSms);
            decimal.TryParse(verify.amount         ?? "", out spAmtSms);

            if (invoiceDueAmt <= 0m)
                invoiceDueAmt = recvAmtSms > 0m ? recvAmtSms : spAmtSms;
            if (customerPaidAmt <= 0m)
                customerPaidAmt = spAmtSms > 0m ? spAmtSms : invoiceDueAmt;

            decimal paidAmt = recvAmtSms > 0m ? recvAmtSms : invoiceDueAmt;

            // Gateway charge calculation (same dual-mode logic)
            decimal gatewayCharge = 0m;
            if (customerPaidAmt > invoiceDueAmt + 0.01m)
            {
                gatewayCharge = customerPaidAmt - invoiceDueAmt;
            }
            else if (recvAmtSms > 0m && spAmtSms > recvAmtSms + 0.01m)
            {
                gatewayCharge   = spAmtSms - recvAmtSms;
                customerPaidAmt = spAmtSms;
            }
            if (gatewayCharge < 0m) gatewayCharge = 0m;

            // ShurjoPay verify API-এ payable_amount সবসময় original amount return করে।
            // তাই 2.038% হারে manually gateway charge calculate করা হচ্ছে।
            bool isSimulated = false;
            if (gatewayCharge == 0m && invoiceDueAmt > 0m)
            {
                try
                {
                    HttpContext ctx = HttpContext.Current;
                    if (ctx != null && ctx.Request.IsLocal)
                    {
                        // Localhost: simulated charge
                        gatewayCharge   = Math.Round(invoiceDueAmt * 0.02038m, 2);
                        customerPaidAmt = invoiceDueAmt + gatewayCharge;
                        isSimulated     = true;
                    }
                    else
                    {
                        // Live: actual 2.038% ShurjoPay charge
                        gatewayCharge   = Math.Round(invoiceDueAmt * 0.02038m, 2);
                        customerPaidAmt = invoiceDueAmt + gatewayCharge;
                    }
                }
                catch { }
            }

            string  method      = verify.method ?? "ShurjoPay";
            string  trxId       = verify.bank_trx_id ?? orderId;
            string  paymentByName = GetUserDisplayName(registrationID, schoolId);

            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    try
                    {
                        // ─── PRIMARY Duplicate check ───────────────────────────────────────────
                        // SP_OrderID (ShurjoPay unique ID) দিয়ে SMS_Balance update হয়েছে কিনা
                        // check করি — এটাই সবচেয়ে নির্ভরযোগ্য guard
                        // SP_OrderID column exist করে কিনা আগে check করি
                        bool spOrderIdColumnExists = false;
                        using (SqlCommand colChk = new SqlCommand(
                            "SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.SMS_Recharge_Record') AND name='SP_OrderID'", conn, tran))
                        {
                            spOrderIdColumnExists = (int)colChk.ExecuteScalar() > 0;
                        }

                        if (spOrderIdColumnExists)
                        {
                            // SP_OrderID দিয়ে ইতিমধ্যে balance দেওয়া হয়েছে কিনা — UPDLOCK দিয়ে lock নাও
                            using (SqlCommand chkSms = new SqlCommand(
                                "SELECT COUNT(*) FROM SMS_Recharge_Record WITH (UPDLOCK,HOLDLOCK) WHERE SP_OrderID=@OID", conn, tran))
                            {
                                chkSms.Parameters.AddWithValue("@OID", spOrderId);
                                if ((int)chkSms.ExecuteScalar() > 0)
                                {
                                    tran.Rollback();
                                    SetHeader("success", "fa-check-circle", "পেমেন্ট সফল!");
                                    SetLabel("lblMessage", "alert alert-info d-block text-center",
                                        "<i class='fa fa-check-circle'></i> এই পেমেন্ট আগেই সংরক্ষিত হয়েছে।");
                                    return true;
                                }
                            }
                        }

                        // ─── SECONDARY Duplicate check ─────────────────────────────────────────
                        // AAP_Invoice_OnlinePayment table-এও check করি (UPDLOCK দিয়ে)
                        using (SqlCommand chk = new SqlCommand(
                            "SELECT COUNT(*) FROM AAP_Invoice_OnlinePayment WITH (UPDLOCK,HOLDLOCK) WHERE SP_OrderID=@OID", conn, tran))
                        {
                            chk.Parameters.AddWithValue("@OID", spOrderId);
                            if ((int)chk.ExecuteScalar() > 0)
                            {
                                tran.Rollback();
                                SetHeader("success", "fa-check-circle", "পেমেন্ট সফল!");
                                SetLabel("lblMessage", "alert alert-info d-block text-center",
                                    "<i class='fa fa-check-circle'></i> এই পেমেন্ট আগেই সংরক্ষিত হয়েছে।");
                                return true;
                            }
                        }

                        // 1. SMS_Recharge_Record — SP_OrderID সহ insert (Total_Price computed column, তাই বাদ)
                        string insertSql = spOrderIdColumnExists
                            ? "INSERT INTO SMS_Recharge_Record(SchoolID,RechargeSMS,PerSMS_Price,Date,Is_Paid,RegistrationID,SP_OrderID)"
                              + " VALUES(@SchoolID,@RechargeSMS,@PerSMS_Price,GETDATE(),1,@RegistrationID,@OID)"
                            : "INSERT INTO SMS_Recharge_Record(SchoolID,RechargeSMS,PerSMS_Price,Date,Is_Paid,RegistrationID)"
                              + " VALUES(@SchoolID,@RechargeSMS,@PerSMS_Price,GETDATE(),1,@RegistrationID)";
                        using (SqlCommand cmd = new SqlCommand(insertSql, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@SchoolID",       schoolId);
                            cmd.Parameters.AddWithValue("@RechargeSMS",    smsQty);
                            cmd.Parameters.AddWithValue("@PerSMS_Price",   0.36);
                            cmd.Parameters.AddWithValue("@RegistrationID", registrationID > 0 ? (object)registrationID : DBNull.Value);
                            if (spOrderIdColumnExists)
                                cmd.Parameters.AddWithValue("@OID",        spOrderId);
                            cmd.ExecuteNonQuery();
                        }

                        // NOTE: SMS_Recharge_Record table-এ Tr_SMS_Recharge_InsertUpdate trigger আছে
                        // যা INSERT হলে স্বয়ংক্রিয়ভাবে SMS.SMS_Balance update করে।
                        // তাই এখানে আলাদাভাবে balance update করার দরকার নেই।

                        // 3. Invoice category
                        int invoiceCategoryID = 0;
                        using (SqlCommand catCmd = new SqlCommand(
                            "SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory=N'SMS'", conn, tran))
                        {
                            object catResult = catCmd.ExecuteScalar();
                            if (catResult != null) invoiceCategoryID = Convert.ToInt32(catResult);
                        }

                        int newInvoiceID = 0;
                        if (invoiceCategoryID > 0)
                        {
                            DateTime issueDate = DateTime.Now;
                            string   invoiceFor = "SMS Recharge: " + smsQty + " SMS @ 0.36 (" + issueDate.ToString("d MMM yyyy") + ")";
                            string   monthName  = issueDate.ToString("MMM yyyy");

                            // IsPaid is a computed column — insert without it, then UPDATE afterwards
                            using (SqlCommand invCmd = new SqlCommand(
                                "INSERT INTO AAP_Invoice(RegistrationID,InvoiceCategoryID,SchoolID,IssuDate,EndDate,"
                                + "Invoice_For,TotalAmount,MonthName,Invoice_SN,Unit,UnitPrice,PaidAmount)"
                                + " VALUES(@RegistrationID,@InvoiceCategoryID,@SchoolID,@IssuDate,@IssuDate,"
                                + "@Invoice_For,@TotalAmount,@MonthName,dbo.Invoice_SerialNumber(@SchoolID),@Unit,@UnitPrice,@TotalAmount);"
                                + " SELECT SCOPE_IDENTITY();", conn, tran))
                            {
                                invCmd.Parameters.AddWithValue("@RegistrationID",    registrationID > 0 ? (object)registrationID : DBNull.Value);
                                invCmd.Parameters.AddWithValue("@InvoiceCategoryID", invoiceCategoryID);
                                invCmd.Parameters.AddWithValue("@SchoolID",          schoolId);
                                invCmd.Parameters.AddWithValue("@IssuDate",          issueDate);
                                invCmd.Parameters.AddWithValue("@Invoice_For",       invoiceFor);
                                invCmd.Parameters.AddWithValue("@TotalAmount",       totalAmount);
                                invCmd.Parameters.AddWithValue("@MonthName",         monthName);
                                invCmd.Parameters.AddWithValue("@Unit",              smsQty);
                                invCmd.Parameters.AddWithValue("@UnitPrice",         0.36);
                                object idResult = invCmd.ExecuteScalar();
                                if (idResult != null) newInvoiceID = Convert.ToInt32(idResult);
                            }

                            // UPDATE NumberOfPayment and LastPaidDate (these are regular columns)
                            if (newInvoiceID > 0)
                            {
                                using (SqlCommand updInv = new SqlCommand(
                                    "UPDATE AAP_Invoice SET NumberOfPayment=1, LastPaidDate=GETDATE() WHERE InvoiceID=@InvoiceID", conn, tran))
                                {
                                    updInv.Parameters.AddWithValue("@InvoiceID", newInvoiceID);
                                    updInv.ExecuteNonQuery();
                                }
                            }
                        }

                        // 4. AAP_Invoice_Receipt
                        int receiptID = 0;
                        using (SqlCommand rcpCmd = new SqlCommand(
                            "INSERT INTO AAP_Invoice_Receipt(SchoolID,RegistrationID,InvoiceReceipt_SN,TotalAmount,PaidDate,PaymentBy,Collected_By,Payment_Method,PaidByUser)"
                            + " VALUES(@SchoolID,@RegistrationID,dbo.F_InvoiceReceipt_SN(),@Amt,GETDATE(),@PayBy,@ColBy,@Method,@PaidByUser);"
                            + " SELECT SCOPE_IDENTITY();", conn, tran))
                        {
                            rcpCmd.Parameters.AddWithValue("@SchoolID",       schoolId);
                            rcpCmd.Parameters.AddWithValue("@RegistrationID", registrationID > 0 ? (object)registrationID : DBNull.Value);
                            rcpCmd.Parameters.AddWithValue("@Amt",            paidAmt);
                            rcpCmd.Parameters.AddWithValue("@PayBy",          paymentByName);
                            rcpCmd.Parameters.AddWithValue("@ColBy",          "Sikkhaloy.com (By ShurjoPay)");
                            rcpCmd.Parameters.AddWithValue("@Method",         method);
                            rcpCmd.Parameters.AddWithValue("@PaidByUser",     paymentByName);
                            object ridResult = rcpCmd.ExecuteScalar();
                            if (ridResult != null) receiptID = Convert.ToInt32(ridResult);
                        }

                        // 5. AAP_Invoice_Payment_Record
                        if (newInvoiceID > 0 && receiptID > 0)
                        {
                            using (SqlCommand prCmd = new SqlCommand(
                                "INSERT INTO AAP_Invoice_Payment_Record(InvoiceID,InvoiceReceiptID,RegistrationID,SchoolID,Amount,PaidDate)"
                                + " VALUES(@InvoiceID,@ReceiptID,@RegistrationID,@SchoolID,@Amount,GETDATE())", conn, tran))
                            {
                                prCmd.Parameters.AddWithValue("@InvoiceID",      newInvoiceID);
                                prCmd.Parameters.AddWithValue("@ReceiptID",      receiptID);
                                prCmd.Parameters.AddWithValue("@RegistrationID", registrationID > 0 ? (object)registrationID : DBNull.Value);
                                prCmd.Parameters.AddWithValue("@SchoolID",       schoolId);
                                prCmd.Parameters.AddWithValue("@Amount",         paidAmt);
                                prCmd.ExecuteNonQuery();
                            }
                        }

                        // 6. Online payment log — ReceiptID + gateway charge info
                        string smsChargeInfo = string.Format(
                            "ReceiptID:{0} | Invoice: {1:F2} | GatewayCharge: {2:F2} | CustomerPaid: {3:F2} | SMS: {4}",
                            receiptID, invoiceDueAmt, gatewayCharge, customerPaidAmt, smsQty);

                        using (SqlCommand logCmd = new SqlCommand(
                            "INSERT INTO AAP_Invoice_OnlinePayment"
                            + "(SchoolID,SP_OrderID,SP_TrxID,SP_Method,Amount,SP_Code,SP_Message,PaymentDate,CreatedDate)"
                            + " VALUES(@SchoolID,@OID,@TrxID,@Method,@Amt,@Code,@Msg,@PDate,GETDATE())", conn, tran))
                        {
                            logCmd.Parameters.AddWithValue("@SchoolID", schoolId);
                            logCmd.Parameters.AddWithValue("@OID",      spOrderId);
                            logCmd.Parameters.AddWithValue("@TrxID",    trxId);
                            logCmd.Parameters.AddWithValue("@Method",   method);
                            logCmd.Parameters.AddWithValue("@Amt",      customerPaidAmt > 0m ? customerPaidAmt : paidAmt);
                            logCmd.Parameters.AddWithValue("@Code",     verify.sp_code ?? "");
                            logCmd.Parameters.AddWithValue("@Msg",      smsChargeInfo.Length > 500 ? smsChargeInfo.Substring(0, 500) : smsChargeInfo);
                            logCmd.Parameters.AddWithValue("@PDate",    DateTime.Now);
                            logCmd.ExecuteNonQuery();
                        }

                        
                        // Grace Period auto-cancel: payment success holei AccessGraceUntil = NULL
                        using (var graceCmd = new SqlCommand(
                            "UPDATE SchoolInfo SET AccessGraceUntil = NULL WHERE SchoolID = @SID AND AccessGraceUntil IS NOT NULL", conn, tran))
                        {
                            graceCmd.Parameters.AddWithValue("@SID", schoolId);
                            graceCmd.ExecuteNonQuery();
                        }

                        tran.Commit();

                        // Success message with gateway charge breakdown
                        string smsSuccessMsg = string.Format(
                            "<i class='fa fa-check-circle'></i> SMS রিচার্জ সফল! <b>{0} SMS</b> যোগ হয়েছে।", smsQty);
                        if (gatewayCharge > 0m)
                        {
                            if (isSimulated)
                                smsSuccessMsg += string.Format(
                                    "<br/><small class='text-info'>🧪 [Sandbox] বিলের পরিমাণ: <b>{0:F2} ৳</b> | গেটওয়ে চার্জ (2.038%): <b>{1:F2} ৳</b> | মোট পরিশোধিত: <b>{2:F2} ৳</b></small>",
                                    invoiceDueAmt, gatewayCharge, customerPaidAmt);
                            else
                                smsSuccessMsg += string.Format(
                                    "<br/><small>বিলের পরিমাণ: <b>{0:F2} ৳</b> | গেটওয়ে চার্জ (2.038%): <b>{1:F2} ৳</b> | মোট পরিশোধিত: <b>{2:F2} ৳</b></small>",
                                    invoiceDueAmt, gatewayCharge, customerPaidAmt);
                        }
                        SetLabel("lblMessage", "alert alert-success d-block text-center", smsSuccessMsg);
                        return true;
                    }
                    catch (Exception exInner)
                    {
                        tran.Rollback();
                        SetLabel("lblMessage", "alert alert-danger d-block",
                            "❌ DB Save Error: " + exInner.Message
                            + "<br/>Debug: " + debugInfo);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                SetLabel("lblMessage", "alert alert-danger d-block",
                    "❌ Connection Error: " + ex.Message
                    + "<br/>Debug: " + debugInfo);
                return false;
            }
        }

        private string GetUserDisplayName(int registrationID, int schoolId)
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    // 1. RegistrationID দিয়ে Admin table থেকে নাম নাও
                    if (registrationID > 0)
                    {
                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT TOP 1 FirstName + ISNULL(' ' + NULLIF(LastName,''), '') FROM Admin " +
                            "WHERE RegistrationID=@RID", conn))
                        {
                            cmd.Parameters.AddWithValue("@RID", registrationID);
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                string name = result.ToString().Trim();
                                if (!string.IsNullOrEmpty(name)) return name;
                            }
                        }
                        // 2. Registration table থেকে UserName নাও
                        using (SqlCommand cmd2 = new SqlCommand(
                            "SELECT TOP 1 UserName FROM Registration WHERE RegistrationID=@RID", conn))
                        {
                            cmd2.Parameters.AddWithValue("@RID", registrationID);
                            object result2 = cmd2.ExecuteScalar();
                            if (result2 != null && result2 != DBNull.Value)
                            {
                                string uname = result2.ToString().Trim();
                                if (!string.IsNullOrEmpty(uname)) return uname;
                            }
                        }
                    }
                    // 3. SchoolID দিয়ে SchoolName return করি fallback হিসেবে
                    if (schoolId > 0)
                    {
                        using (SqlCommand cmd3 = new SqlCommand(
                            "SELECT TOP 1 SchoolName FROM SchoolInfo WHERE SchoolID=@SID", conn))
                        {
                            cmd3.Parameters.AddWithValue("@SID", schoolId);
                            object result3 = cmd3.ExecuteScalar();
                            if (result3 != null && result3 != DBNull.Value)
                            {
                                string sname = result3.ToString().Trim();
                                if (!string.IsNullOrEmpty(sname)) return sname;
                            }
                        }
                    }
                }
            }
            catch { }
            return "ShurjoPay Online";
        }

        private bool MarkInvoiceAsPaid(string spOrderId, string ourOrderId, ShurjoPayVerifyResponse verify, int schoolId)
        {
            try
            {
                // Invoice due amount (value3 থেকে) — আমরা ShurjoPay-এ পাঠিয়েছিলাম
                decimal invoiceDueAmount = 0m;
                if (!string.IsNullOrEmpty(verify.value3))
                    decimal.TryParse(verify.value3, out invoiceDueAmount);

                // Amounts from ShurjoPay verify response
                decimal customerPaidAmount = 0m;
                decimal recvAmt            = 0m;
                decimal spAmount           = 0m;

                decimal.TryParse(verify.payable_amount ?? "", out customerPaidAmount);
                decimal.TryParse(verify.recv_amt       ?? "", out recvAmt);
                decimal.TryParse(verify.amount         ?? "", out spAmount);

                // Invoice fallback
                if (invoiceDueAmount <= 0m)
                    invoiceDueAmount = recvAmt > 0m ? recvAmt : spAmount;

                // Customer paid: payable_amount > amount means customer-bearing charge
                // Otherwise use spAmount (what customer paid = invoice amount)
                if (customerPaidAmount <= 0m)
                    customerPaidAmount = spAmount > 0m ? spAmount : invoiceDueAmount;

                // Gateway charge:
                // 1) Customer-bearing: payable_amount - invoiceDue
                // 2) Merchant-bearing fallback: spAmount - recv_amt (ShurjoPay deducted from merchant)
                decimal gatewayCharge = 0m;
                if (customerPaidAmount > invoiceDueAmount + 0.01m)
                {
                    // customer paid more → customer-bearing
                    gatewayCharge = customerPaidAmount - invoiceDueAmount;
                }
                else if (recvAmt > 0m && spAmount > recvAmt + 0.01m)
                {
                    // merchant received less → merchant-bearing (show as info)
                    gatewayCharge     = spAmount - recvAmt;
                    customerPaidAmount = spAmount; // customer paid spAmount
                }
                if (gatewayCharge < 0m) gatewayCharge = 0m;

                // ShurjoPay verify API-এ payable_amount সবসময় original amount return করে।
                        // তাই 2.038% হারে manually gateway charge calculate করা হচ্ছে।
                        bool isSimulated = false;
                        if (gatewayCharge == 0m && invoiceDueAmount > 0m)
                        {
                            try
                            {
                                HttpContext ctx = HttpContext.Current;
                                if (ctx != null && ctx.Request.IsLocal)
                                {
                                    // Localhost: simulated charge
                                    gatewayCharge     = Math.Round(invoiceDueAmount * 0.02038m, 2);
                                    customerPaidAmount = invoiceDueAmount + gatewayCharge;
                                    isSimulated       = true;
                                }
                                else
                                {
                                    // Live: actual 2.038% ShurjoPay charge
                                    gatewayCharge     = Math.Round(invoiceDueAmount * 0.02038m, 2);
                                    customerPaidAmount = invoiceDueAmount + gatewayCharge;
                                }
                            }
                            catch { }
                        }

                string  method  = verify.method ?? "ShurjoPay";
                string  trxId   = verify.bank_trx_id ?? ourOrderId;

                // RegistrationID: school-এর primary admin থেকে নেওয়া হবে
                int registrationID = GetSchoolRegistrationID(schoolId);
                string paymentByName = GetUserDisplayName(registrationID, schoolId);

                string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    try
                    {
                        // Duplicate check — spOrderId দিয়ে
                        using (SqlCommand chk = new SqlCommand(
                            "SELECT COUNT(*) FROM AAP_Invoice_OnlinePayment WITH (UPDLOCK,HOLDLOCK) WHERE SP_OrderID=@OID", conn, tran))
                        {
                            chk.Parameters.AddWithValue("@OID", spOrderId);
                            if ((int)chk.ExecuteScalar() > 0)
                            {
                                tran.Rollback();
                                SetLabel("lblMessage", "alert alert-info d-block text-center",
                                    "<i class='fa fa-check-circle'></i> এই পেমেন্ট আগেই সংরক্ষিত হয়েছে।");
                                return true;
                            }
                        }

                        // 1. AAP_Invoice_Receipt insert (receipt amount = invoice due, not customer paid)
                        int receiptID = 0;
                        using (SqlCommand rcpCmd = new SqlCommand(
                            "INSERT INTO AAP_Invoice_Receipt(SchoolID,RegistrationID,InvoiceReceipt_SN,TotalAmount,PaidDate,PaymentBy,Collected_By,Payment_Method,PaidByUser)"
                            + " VALUES(@SchoolID,@RegistrationID,dbo.F_InvoiceReceipt_SN(),@Amt,GETDATE(),@PayBy,@ColBy,@Method,@PaidByUser);"
                            + " SELECT SCOPE_IDENTITY();", conn, tran))
                        {
                            rcpCmd.Parameters.AddWithValue("@SchoolID",       schoolId);
                            rcpCmd.Parameters.AddWithValue("@RegistrationID", registrationID > 0 ? (object)registrationID : DBNull.Value);
                            rcpCmd.Parameters.AddWithValue("@Amt",            invoiceDueAmount);
                            rcpCmd.Parameters.AddWithValue("@PayBy",          paymentByName);
                            rcpCmd.Parameters.AddWithValue("@ColBy", "Sikkhaloy.com (By ShurjoPay)");
                            rcpCmd.Parameters.AddWithValue("@Method",         method);
                            rcpCmd.Parameters.AddWithValue("@PaidByUser",     paymentByName);
                            object ridResult = rcpCmd.ExecuteScalar();
                            if (ridResult != null) receiptID = Convert.ToInt32(ridResult);
                        }

                        // 2. Unpaid invoices গুলো update করি এবং প্রতিটির জন্য Payment_Record insert করি
                        // NOTE: IsPaid computed column — PaidAmount আপডেট করলে স্বয়ংক্রিয়ভাবে IsPaid=1 হয়
                        if (schoolId > 0)
                        {
                            // আগে unpaid invoice IDs সংগ্রহ করি
                            var invoiceIds = new System.Collections.Generic.List<int>();
                            var invoiceAmounts = new System.Collections.Generic.Dictionary<int, decimal>();
                            using (SqlCommand selCmd = new SqlCommand(
                                @"SELECT InvoiceID, TotalAmount - ISNULL(Discount,0) - ISNULL(PaidAmount,0) AS DueAmt
                                  FROM AAP_Invoice
                                  WHERE SchoolID=@SID AND ISNULL(PaidAmount,0) < TotalAmount - ISNULL(Discount,0)", conn, tran))
                            {
                                selCmd.Parameters.AddWithValue("@SID", schoolId);
                                using (SqlDataReader rdr = selCmd.ExecuteReader())
                                {
                                    while (rdr.Read())
                                    {
                                        int iid = Convert.ToInt32(rdr[0]);
                                        decimal due = Convert.ToDecimal(rdr[1]);
                                        invoiceIds.Add(iid);
                                        invoiceAmounts[iid] = due;
                                    }
                                }
                            }

                            foreach (int invoiceId in invoiceIds)
                            {
                                // Update PaidAmount
                                using (SqlCommand updCmd = new SqlCommand(
                                    @"UPDATE AAP_Invoice
                                      SET PaidAmount = TotalAmount - ISNULL(Discount,0),
                                          NumberOfPayment = ISNULL(NumberOfPayment,0) + 1,
                                          LastPaidDate = GETDATE()
                                      WHERE InvoiceID=@IID", conn, tran))
                                {
                                    updCmd.Parameters.AddWithValue("@IID", invoiceId);
                                    updCmd.ExecuteNonQuery();
                                }

                                // Insert Payment_Record
                                if (receiptID > 0)
                                {
                                    using (SqlCommand prCmd = new SqlCommand(
                                        "INSERT INTO AAP_Invoice_Payment_Record(InvoiceID,InvoiceReceiptID,RegistrationID,SchoolID,Amount,PaidDate)"
                                        + " VALUES(@InvoiceID,@ReceiptID,@RegistrationID,@SchoolID,@Amount,GETDATE())", conn, tran))
                                    {
                                        prCmd.Parameters.AddWithValue("@InvoiceID",      invoiceId);
                                        prCmd.Parameters.AddWithValue("@ReceiptID",      receiptID);
                                        prCmd.Parameters.AddWithValue("@RegistrationID", registrationID > 0 ? (object)registrationID : DBNull.Value);
                                        prCmd.Parameters.AddWithValue("@SchoolID",       schoolId);
                                        prCmd.Parameters.AddWithValue("@Amount",         invoiceAmounts[invoiceId]);
                                        prCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }

                        // 3. Online payment log — SP_Message-এ gateway charge + ReceiptID store
                        string chargeInfo = string.Format(
                            "ReceiptID:{0} | Invoice: {1:F2} | GatewayCharge: {2:F2} | CustomerPaid: {3:F2}",
                            receiptID, invoiceDueAmount, gatewayCharge, customerPaidAmount);

                        // AAP_Invoice_OnlinePayment table exist check
                        bool logTableExists = false;
                        using (SqlCommand chkTbl = new SqlCommand(
                            "SELECT COUNT(*) FROM sys.tables WHERE name='AAP_Invoice_OnlinePayment'", conn, tran))
                        {
                            logTableExists = (int)chkTbl.ExecuteScalar() > 0;
                        }

                        if (logTableExists)
                        {
                            using (SqlCommand logCmd = new SqlCommand(
                                "INSERT INTO AAP_Invoice_OnlinePayment"
                                + "(SchoolID,SP_OrderID,SP_TrxID,SP_Method,Amount,SP_Code,SP_Message,PaymentDate,CreatedDate)"
                                + " VALUES(@SchoolID,@OID,@TrxID,@Method,@Amt,@Code,@Msg,@PDate,GETDATE())", conn, tran))
                            {
                                logCmd.Parameters.AddWithValue("@SchoolID", schoolId);
                                logCmd.Parameters.AddWithValue("@OID",      spOrderId);
                                logCmd.Parameters.AddWithValue("@TrxID",    trxId);
                                logCmd.Parameters.AddWithValue("@Method",   method);
                                logCmd.Parameters.AddWithValue("@Amt",      customerPaidAmount > 0m ? customerPaidAmount : invoiceDueAmount);
                                logCmd.Parameters.AddWithValue("@Code",     verify.sp_code ?? "");
                                logCmd.Parameters.AddWithValue("@Msg",      chargeInfo.Length > 500 ? chargeInfo.Substring(0, 500) : chargeInfo);
                                logCmd.Parameters.AddWithValue("@PDate",    DateTime.Now);
                                logCmd.ExecuteNonQuery();
                            }
                        }

                        tran.Commit();

                        // Success message-এ charge + log status দেখানো
                        string successMsg = "<i class='fa fa-check-circle'></i> পেমেন্ট সফলভাবে সম্পন্ন হয়েছে! ধন্যবাদ।";
                        if (gatewayCharge > 0m)
                        {
                            if (isSimulated)
                            {
                                successMsg += string.Format(
                                    "<br/><small class='text-info'>🧪 [Sandbox] বিলের পরিমাণ: <b>{0:F2} ৳</b> | গেটওয়ে চার্জ (2.038%): <b>{1:F2} ৳</b> | মোট পরিশোধিত: <b>{2:F2} ৳</b></small>",
                                    invoiceDueAmount, gatewayCharge, customerPaidAmount);
                            }
                            else
                            {
                                successMsg += string.Format(
                                    "<br/><small>বিলের পরিমাণ: <b>{0:F2} ৳</b> | গেটওয়ে চার্জ (2.038%): <b>{1:F2} ৳</b> | মোট পরিশোধিত: <b>{2:F2} ৳</b></small>",
                                    invoiceDueAmount, gatewayCharge, customerPaidAmount);
                            }
                        }
                        if (!logTableExists)
                        {
                            successMsg += "<br/><small class='text-warning'>⚠️ AAP_Invoice_OnlinePayment table পাওয়া যায়নি — log সংরক্ষিত হয়নি।</small>";
                        }
                        SetLabel("lblMessage", "alert alert-success d-block text-center", successMsg);

                        return true;
                    }
                    catch (Exception exInner)
                    {
                        tran.Rollback();
                        SetLabel("lblMessage", "alert alert-danger d-block text-center",
                            "<i class='fa fa-exclamation-circle'></i> সংরক্ষণ ব্যর্থ: " + exInner.Message);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("MarkInvoiceAsPaid error: " + ex.Message);
                SetLabel("lblMessage", "alert alert-danger d-block text-center",
                    "<i class='fa fa-exclamation-circle'></i> সংযোগ ব্যর্থ: " + ex.Message);
                return false;
            }
        }
        private int GetSchoolRegistrationID(int schoolId)
        {
            if (schoolId <= 0) return 0;
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    // 1. Registration table থেকে school-এর primary user নাও
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT TOP 1 RegistrationID FROM Registration WHERE SchoolID=@SID ORDER BY RegistrationID", conn))
                    {
                        cmd.Parameters.AddWithValue("@SID", schoolId);
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                            return Convert.ToInt32(result);
                    }
                    // 2. Admin table থেকে যেকোনো admin
                    using (SqlCommand cmd2 = new SqlCommand(
                        "SELECT TOP 1 RegistrationID FROM Admin WHERE SchoolID=@SID ORDER BY RegistrationID", conn))
                    {
                        cmd2.Parameters.AddWithValue("@SID", schoolId);
                        object result2 = cmd2.ExecuteScalar();
                        if (result2 != null && result2 != DBNull.Value)
                            return Convert.ToInt32(result2);
                    }
                }
            }
            catch { }
            return 0;
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────
        private void SetHeader(string css, string icon, string title)
        {
            HeaderCssClass = css;
            HeaderIcon     = icon;
            HeaderTitle    = title;
        }

        private void SetLabel(string id, string cssClass, string text)
        {
            Label lbl = FindControlRecursive(this, id) as Label;
            if (lbl == null) return;
            if (!string.IsNullOrEmpty(cssClass)) lbl.CssClass = cssClass;
            lbl.Text = text;
        }

        private static Control FindControlRecursive(Control root, string id)
        {
            if (root.ID == id) return root;
            foreach (Control child in root.Controls)
            {
                Control found = FindControlRecursive(child, id);
                if (found != null) return found;
            }
            return null;
        }
    }
}
