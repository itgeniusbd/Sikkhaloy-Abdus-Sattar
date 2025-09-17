using Education;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee.Payment
{
    public partial class Salary_Payment : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (Page.IsPostBack) return;

            if (Session["SchoolID"] != null)
                SelectedAccount();           


        }

        protected void PayButton_Click(object sender, EventArgs e)
        {
            
            


            var con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            var accountCmd = new SqlCommand("SELECT AccountBalance FROM [Account] WHERE SchoolID = @SchoolID AND AccountID = @AccountID", con);
            accountCmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            accountCmd.Parameters.AddWithValue("@AccountID", AccountDropDownList.SelectedValue);

            con.Open();
            var accountBalance = Convert.ToDouble(accountCmd.ExecuteScalar());
            con.Close();

            var grandTotal = Convert.ToDouble(GrandTotalHF.Value);

            CheckBox smscheckBox = (CheckBox)this.FindControl("SMSCheckBox");

            if (accountBalance >= grandTotal)
            {
                foreach (GridViewRow row in EmployeeGridView.Rows)
                {
                    var dueCheckBox = (CheckBox)row.FindControl("AddCheckBox");
                   //var smscheckBox = FindControl("SMSCheckBox") as CheckBox;// (CheckBox)row.FindControl("SMSCheckBox");
                    

                    var dueAmountTextBox = (TextBox)row.FindControl("PayTextBox");

                    var dueCmd = new SqlCommand("SELECT Due FROM Employee_Payorder WHERE (SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (Employee_PayorderID = @Employee_PayorderID)", con);
                    dueCmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                    dueCmd.Parameters.AddWithValue("@EmployeeID", EmployeeGridView.DataKeys[row.RowIndex]?["EmployeeID"].ToString());
                    dueCmd.Parameters.AddWithValue("@Employee_PayorderID", EmployeeGridView.DataKeys[row.RowIndex]?["Employee_PayorderID"].ToString());

                    con.Open();
                    var dueByPayOrder = Convert.ToDouble(dueCmd.ExecuteScalar());
                    con.Close();

                    if (EmployeeGridView.DataKeys[row.RowIndex]?["PaidStatus"].ToString() == "Due")
                    {
                        if (dueCheckBox.Checked && double.TryParse(dueAmountTextBox.Text.Trim(), out var paidAmount))
                        {

                            string teacherid = EmployeeGridView.DataKeys[row.RowIndex]["EmployeeID"].ToString();
                            var paid = paidAmount.ToString();
                            if (paidAmount <= dueByPayOrder)
                            {
                                var paidFor = "Salary Paid To: " + EmployeeGridView.DataKeys[row.RowIndex]["Name"] + ". Pay For: " + MonthNameDropDownList.SelectedItem.Text + ". Paid Amount: " + paid + " Tk.";

                                Employee_Payorder_RecordsSQL.InsertParameters["Employee_PayorderID"].DefaultValue = EmployeeGridView.DataKeys[row.RowIndex]["Employee_PayorderID"].ToString();
                                Employee_Payorder_RecordsSQL.InsertParameters["EmployeeID"].DefaultValue = EmployeeGridView.DataKeys[row.RowIndex]["EmployeeID"].ToString();
                                Employee_Payorder_RecordsSQL.InsertParameters["Amount"].DefaultValue = paid;
                                Employee_Payorder_RecordsSQL.InsertParameters["Paid_For"].DefaultValue = paidFor;
                                Employee_Payorder_RecordsSQL.Insert();

                                EmplyoeePayOrderSQL.UpdateParameters["PaidAmount"].DefaultValue = paid;
                                EmplyoeePayOrderSQL.UpdateParameters["Employee_PayorderID"].DefaultValue = EmployeeGridView.DataKeys[row.RowIndex]["Employee_PayorderID"].ToString();
                                EmplyoeePayOrderSQL.Update();


                                // SMS Send

                                if (SMSRadioButtonList.SelectedValue== "Yes")
                                {
                                    ErrorLabel.Text = "";
                                    var msg = "Congrats! ";
                                    var isSentSMS = false;

                                    var phoneNo = EmployeeGridView.DataKeys[row.RowIndex]["Phone"].ToString();
                                    var employeeId = EmployeeGridView.DataKeys[row.RowIndex]["ID"].ToString();
                                    var employeePaid = paidAmount;
                                    var employeeName = EmployeeGridView.DataKeys[row.RowIndex]["Name"].ToString();
                                    string monthname = MonthNameDropDownList.SelectedItem.Text;
                                    msg += $" {employeeName} (ID: {employeeId}). You've Received: {paid} Tk. from {monthname} Salary";
                                    msg += ". Regards, " + Session["School_Name"];
                                    var sms = new SMS_Class(Session["SchoolID"].ToString());
                                    var smsBalance = sms.SMSBalance;
                                    var totalSMS = sms.SMS_Conut(msg);
                                    if (smsBalance >= totalSMS)
                                    {
                                        if (sms.SMS_GetBalance() >= totalSMS)
                                        {
                                            var isValid = sms.SMS_Validation(phoneNo, msg);

                                            if (isValid.Validation)
                                            {
                                                var smsSendId = sms.SMS_Send(phoneNo, msg, "Employee Salary Payment");
                                                if (smsSendId != Guid.Empty)
                                                {
                                                    SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = smsSendId.ToString();
                                                    SMS_OtherInfoSQL.InsertParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
                                                    SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();
                                                    SMS_OtherInfoSQL.InsertParameters["StudentID"].DefaultValue = "";
                                                    SMS_OtherInfoSQL.InsertParameters["TeacherID"].DefaultValue = EmployeeGridView.DataKeys[row.RowIndex]["EmployeeID"].ToString();
                                                    SMS_OtherInfoSQL.Insert();

                                                }
                                                isSentSMS = true;
                                            }
                                            else
                                            {
                                                ErrorLabel.Text = isValid.Message;
                                            }
                                        }
                                        else
                                        {
                                            ErrorLabel.Text = "SMS Service Updating. Try again later or contact to authority";
                                        }
                                    }
                                    else
                                    {
                                        ErrorLabel.Text = "You don't have sufficient SMS balance, Your Current Balance is " + smsBalance;
                                    }
                                }
                                //------End SMS---------
                                
                                dueCheckBox.Checked = false;
                            }
                        }
                    }
                }

                AccountDropDownList.DataBind();
                AccerrorLabel.Text = "";
            }
            else
            {
                AccerrorLabel.Text = "You don't have enough balance";
            }
        }

        protected void SelectedAccount()
        {
            var con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            var accountCmd = new SqlCommand("Select AccountID from Account where SchoolID = @SchoolID AND AccountBalance <> 0 AND Default_Status = 'True'", con);
            accountCmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());

            con.Open();
            var accountId = accountCmd.ExecuteScalar();
            con.Close();

            if (accountId != null)
                AccountDropDownList.SelectedValue = accountId.ToString();
        }

        protected void EmployeeGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            if (EmployeeGridView.DataKeys[e.Row.RowIndex]?["PaidStatus"].ToString() == "Paid")
            {
                e.Row.CssClass = "row-muted";
            }
        }

        //get data
        [WebMethod]
        public static string GetPaidRecords(int employeeId, int employeePayOrderId)
        {
            var list = new List<EmployeePayOrderRecordModel>();

            var cs = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString();
            using (var con = new SqlConnection(cs))
            {
                var cmd = new SqlCommand("SELECT Employee_Payorder_Records.Employee_Payorder_RecordID, Employee_Payorder_Records.Amount, Employee_Payorder_Records.Paid_date, Account.AccountName FROM Employee_Payorder_Records LEFT OUTER JOIN Account ON Employee_Payorder_Records.AccountID = Account.AccountID WHERE(Employee_Payorder_Records.SchoolID = @SchoolID) AND(Employee_Payorder_Records.EmployeeID = @EmployeeID) AND(Employee_Payorder_Records.Employee_PayorderID = @Employee_PayorderID)", con);
                cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                cmd.Parameters.AddWithValue("@Employee_PayorderID", employeePayOrderId);

                con.Open();
                var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    var record = new EmployeePayOrderRecordModel
                    {
                        EmployeePayOrderRecordId = Convert.ToInt32(rdr["Employee_Payorder_RecordID"]),
                        Amount = Convert.ToDecimal(rdr["Amount"]),
                        PaidDate = Convert.ToDateTime(rdr["Paid_date"]).ToString("dd MMM yyyy"),
                        AccountName = rdr["AccountName"].ToString()
                    };
                    list.Add(record);
                }
            }

            var js = new JavaScriptSerializer();
            return js.Serialize(list);
        }

        //post delete
        [WebMethod]
        public static bool PaidRecordDelete(int id)
        {
            int result;
            var cs = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString();
            using (var con = new SqlConnection(cs))
            {
                var cmd = new SqlCommand("DELETE FROM Employee_Payorder_Records WHERE (Employee_Payorder_RecordID = @Employee_Payorder_RecordID)", con);
                cmd.Parameters.AddWithValue("@Employee_Payorder_RecordID", id);

                con.Open();
                result = cmd.ExecuteNonQuery();
                con.Close();
            }

            return result > 0;
        }

        public class EmployeePayOrderRecordModel
        {
            public int EmployeePayOrderRecordId { get; set; }
            public decimal Amount { get; set; }
            public string PaidDate { get; set; }
            public string AccountName { get; set; }
        }
    }
}