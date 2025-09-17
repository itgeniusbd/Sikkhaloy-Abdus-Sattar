using System;
using System.Collections.Generic;

namespace SmsService
{
    public class SmsServiceBuilder
    {
        private readonly ISmsProvider _provider;
        private readonly ISmsProvider _providerMultiple;
        public string Error { get; private set; }
        //public int StatusCode { get; private set; }
        public bool IsSuccess { get; private set; }

        public SmsServiceBuilder(ProviderEnum provider, ProviderEnum providerMultiple)
        {

            if (provider == ProviderEnum.BanglaPhone)
            {
                _provider = new SmsProviderBanglaPhone();
            }
            else if (provider == ProviderEnum.GreenWeb)
            {
                _provider = new SmsProviderGreenWeb();
            }


            if (providerMultiple == ProviderEnum.BanglaPhone)
            {
                _providerMultiple = new SmsProviderBanglaPhone();
            }
            else if (providerMultiple == ProviderEnum.GreenWeb)
            {
                _providerMultiple = new SmsProviderGreenWeb();
            }
        }
        public int SmsBalance()
        {
            try
            {
                this.IsSuccess = true;
                return _provider.GetSmsBalance();
            }
            catch (Exception e)
            {
                this.IsSuccess = false;
                this.Error = e.Message;
            }
            return 0;

        }

        public string SendSms(string massage, string number)
        {
            try
            {
                this.IsSuccess = true;
                return _provider.SendSms(massage, number);
            }
            catch (Exception e)
            {
                this.Error = e.Message;
                this.IsSuccess = false;
            }

            return string.Empty;
        }

        public void SendSmsMultiple(List<SendSmsModel> smsList)
        {
            try
            {
                this.IsSuccess = true;
                _provider.SendSmsMultiple(smsList);
            }
            catch (Exception e)
            {
                this.Error = e.Message;
                this.IsSuccess = false;
            }
        }
    }
}