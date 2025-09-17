using System.Collections.Generic;

namespace SmsService
{
    public interface ISmsProvider
    {
        int GetSmsBalance();
        string SendSms(string massage, string number);
        void SendSmsMultiple(IEnumerable<SendSmsModel> smsList);
    }
}