using SmsService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmsSenderApp
{
    public class SMS_Class
    {
        private SmsServiceBuilder SmsService { get; }

        public int SMSBalance { get; }


        public SMS_Class()
        {
            var smsProvider = GlobalClass.Instance.Setting.SmsProvider;
            var smsProviderMultiple = GlobalClass.Instance.Setting.SmsProviderMultiple;

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

                var smsRecords = new List<SMS_Send_Record>(){
                    new SMS_Send_Record {
                    SMS_Send_ID = smsSendId,
                    PhoneNumber = number,
                    TextSMS = text,
                    TextCount = text.Length,
                    SMSCount = SmsValidator.TotalSmsCount(text),
                    PurposeOfSMS = smsPurpose,
                    SMS_Response = "Sent",
                    Status = responseMessage,
                    }
                };

                Task.Run(() => GlobalClass.Instance.SMS_Send_RecordAddAsync(smsRecords));

                return smsSendId;
            }
            else
            {
                return Guid.Empty;
            }
        }

        public Get_Validation SmsSendMultiple(List<SendSmsModel> smsList, string smsPurpose = "Purpose Not Define")
        {
            //---------For SMS Send off
            SmsService.SendSmsMultiple(smsList);

            if (!SmsService.IsSuccess)
            {
                return new Get_Validation
                {
                    Validation = false,
                    Message = SmsService.Error
                };
            }
            //-----------------------------

            var smsRecords = smsList.Select(s => new SMS_Send_Record
            {
                SMS_Send_ID = s.Guid,
                PhoneNumber = s.Number,
                TextSMS = s.Text,
                TextCount = s.Text.Length,
                SMSCount = SmsValidator.TotalSmsCount(s.Text),
                PurposeOfSMS = smsPurpose,
                SMS_Response = "Sent",
                Status = "Multiple SMS Send",
            }).ToList();

            Task.Run(() => GlobalClass.Instance.SMS_Send_RecordAddAsync(smsRecords));

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