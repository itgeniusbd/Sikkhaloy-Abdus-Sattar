using System;
using System.Text.RegularExpressions;

namespace SmsService
{
    public static class SmsValidator
    {
        private const string Gsm7BitChars =
            "@£$¥èéùìòÇ\\nØø\\rÅåΔ_ΦΓΛΩΠΨΣΘΞÆæßÉ !\\\"#¤%&'()*+,-./0123456789:;<=>?¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ§¿abcdefghijklmnopqrstuvwxyzäöñüà";

        private const string Gsm7BitExChar = "\\^{}\\\\\\[~\\]|€";
        private static readonly Regex Gsm7BitRegExp = new Regex(@"^[" + Gsm7BitChars + "]*$");
        private static readonly Regex Gsm7BitExRegExp = new Regex(@"^[" + Gsm7BitChars + Gsm7BitExChar + "]*$");
        private static readonly Regex Gsm7BitExOnlyRegExp = new Regex(@"^[\\" + Gsm7BitExChar + "]*$");

        public static int TotalSmsCount(string massage)
        {
            var sms = massage.Replace(Environment.NewLine, " ").Trim();

            var results = 0;

            for (var ctr = 0; ctr <= sms.Length - 1; ctr++)
            {
                if (Gsm7BitExOnlyRegExp.IsMatch(sms[ctr].ToString()))
                {
                    results++;
                }
            }

            double smsCount;

            double smsLength;

            if (Gsm7BitRegExp.IsMatch(sms))
            {
                smsLength = sms.Length + results;
                if (smsLength > 160)
                {
                    smsCount = Math.Ceiling(smsLength / 153);
                }
                else
                {
                    smsCount = 1;
                }
            }
            else if (Gsm7BitExRegExp.IsMatch(sms))
            {
                smsLength = sms.Length + results;
                if (smsLength > 160)
                {
                    smsCount = Math.Ceiling(smsLength / 153);
                }
                else
                {
                    smsCount = 1;
                }
            }
            else
            {
                smsLength = sms.Length;
                if (smsLength > 70)
                {
                    smsCount = Math.Ceiling(smsLength / 67);
                }
                else
                {
                    smsCount = 1;
                }

            }

            return (int)smsCount;
        }

        public static int MassageLength(string massage)
        {
            var sms = massage.Replace(Environment.NewLine, " ").Trim();

            var results = 0;

            for (var ctr = 0; ctr <= sms.Length - 1; ctr++)
            {
                if (Gsm7BitExOnlyRegExp.IsMatch(sms[ctr].ToString()))
                {
                    results++;
                }
            }

            if (Gsm7BitRegExp.IsMatch(sms) || Gsm7BitExRegExp.IsMatch(sms))
            {
                return sms.Length + results;
            }
            else
            {
                return sms.Length;
            }
        }

        public static bool IsValidNumber(string number)
        {
            return Regex.IsMatch(number, @"^(88)?((011)|(015)|(016)|(017)|(018)|(019)|(013)|(014))\d{8,8}$");
        }
    }
}
