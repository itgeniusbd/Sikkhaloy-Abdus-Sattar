using SmsService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace Attendance_API
{
    public class SMS_Class
    {
        private SmsServiceBuilder SmsService { get; }

        public int SMSBalance { get; }

        private readonly SqlConnection _con =
            new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

        public SMS_Class(string schoolId)
        {
            var getSmsBalanceCmd = new SqlCommand("SELECT SMS_Balance FROM SMS WHERE (SchoolID = @SchoolID)", _con);
            getSmsBalanceCmd.Parameters.AddWithValue("@SchoolID", schoolId);

            var smsProviderCmd = new SqlCommand("SELECT TOP(1) SmsProvider FROM  SikkhaloySetting", _con);
            var smsProviderMultipleCmd =
                new SqlCommand("SELECT TOP(1) SmsProviderMultiple FROM  SikkhaloySetting", _con);


            _con.Open();
            this.SMSBalance = Convert.ToInt32(getSmsBalanceCmd.ExecuteScalar());
            var smsProvider = smsProviderCmd.ExecuteScalar().ToString();
            var smsProviderMultiple = smsProviderMultipleCmd.ExecuteScalar().ToString();
            _con.Close();

            var isProviderExist = Enum.TryParse<ProviderEnum>(smsProvider, out var provider);
            var isProviderMultipleExist = Enum.TryParse<ProviderEnum>(smsProviderMultiple, out var providerMultiple);

            if (!isProviderExist) provider = ProviderEnum.BanglaPhone;
            if (!isProviderMultipleExist) provider = ProviderEnum.GreenWeb;

            SmsService = new SmsServiceBuilder(provider, providerMultiple);
        }

        public Get_Validation SMS_Validation(string number, string text)
        {
            var isValid = true;
            var validationMessage = "";

            if (!SmsValidator.IsValidNumber(number))
            {
                isValid = false;
                validationMessage += "Invalid Mobile Number";
            }


            //if (!Enumerable.Range(1, 450).Contains(Text.Length))
            //{
            //    IsValid = false;
            //    Validation_Message += "SMS Text Length Out Of Range. Maximum Size Is (450 character) ";
            //}

            if (isValid)
            {
                validationMessage = "Valid";
            }

            var v = new Get_Validation
            {
                Validation = isValid,
                Message = validationMessage
            };

            return v;
        }

        public Guid SMS_Send(string number, string text, string smsPurpose)
        {
            var smsSendId = Guid.NewGuid();

            var responseMessage = SmsService.SendSms(text, number);
            var isError = !SmsService.IsSuccess;

            if (!isError)
            {
                var sendRecordCmd = new SqlCommand(
                    "INSERT INTO [SMS_Send_Record] ([SMS_Send_ID], [PhoneNumber], [TextSMS], [TextCount], [SMSCount], [PurposeOfSMS], [Status], [Date], [SMS_Response]) VALUES (@SMS_Send_ID, @PhoneNumber, @TextSMS, @TextCount, @SMSCount, @PurposeOfSMS, @Status, Getdate(), @SMS_Response)",
                    _con);
                sendRecordCmd.Parameters.AddWithValue("@SMS_Send_ID", smsSendId);
                sendRecordCmd.Parameters.AddWithValue("@PhoneNumber", number);
                sendRecordCmd.Parameters.AddWithValue("@TextSMS", text);
                sendRecordCmd.Parameters.AddWithValue("@TextCount", text.Length);
                sendRecordCmd.Parameters.AddWithValue("@SMSCount", SmsValidator.TotalSmsCount(text));
                sendRecordCmd.Parameters.AddWithValue("@PurposeOfSMS", smsPurpose);
                sendRecordCmd.Parameters.AddWithValue("@Status", "Sent");
                sendRecordCmd.Parameters.AddWithValue("@SMS_Response", responseMessage);

                _con.Open();
                sendRecordCmd.ExecuteNonQuery();
                _con.Close();

                return smsSendId;
            }
            else
            {
                return Guid.Empty;
            }
        }

        public Get_Validation SmsSendMultiple(List<SendSmsModel> smsList, string smsPurpose = "Purpose Not Define")
        {
            //SmsService.SendSmsMultiple(smsList);

            //if (!SmsService.IsSuccess)
            //{
            //    return new Get_Validation
            //    {
            //        Validation = false,
            //        Message = SmsService.Error
            //    };
            //}

            foreach (var smsModel in smsList)
            {


                var sendRecordCmd = new SqlCommand(
                    "INSERT INTO [SMS_Send_Record] ([SMS_Send_ID], [PhoneNumber], [TextSMS], [TextCount], [SMSCount], [PurposeOfSMS], [Status], [Date], [SMS_Response]) VALUES (@SMS_Send_ID, @PhoneNumber, @TextSMS, @TextCount, @SMSCount, @PurposeOfSMS, @Status, Getdate(), @SMS_Response)",
                    _con);
                sendRecordCmd.Parameters.AddWithValue("@SMS_Send_ID", smsModel.Guid);
                sendRecordCmd.Parameters.AddWithValue("@PhoneNumber", smsModel.Number);
                sendRecordCmd.Parameters.AddWithValue("@TextSMS", smsModel.Text);
                sendRecordCmd.Parameters.AddWithValue("@TextCount", smsModel.Text.Length);
                sendRecordCmd.Parameters.AddWithValue("@SMSCount", SmsValidator.TotalSmsCount(smsModel.Text));
                sendRecordCmd.Parameters.AddWithValue("@PurposeOfSMS", smsPurpose);
                sendRecordCmd.Parameters.AddWithValue("@Status", "Sent");
                sendRecordCmd.Parameters.AddWithValue("@SMS_Response", "Multiple SMS Send");

                _con.Open();
                sendRecordCmd.ExecuteNonQuery();
                _con.Close();
            }

            return new Get_Validation
            {
                Validation = true,
                Message = "Success"
            };
        }

        public int SMS_Conut(string text)
        {
            return SmsValidator.TotalSmsCount(text);
        }

        public int SMS_GetBalance()
        {
            return SmsService.SmsBalance();
        }
    }

    public class Get_Validation
    {
        public bool Validation { get; set; }
        public string Message { get; set; }
    }
}