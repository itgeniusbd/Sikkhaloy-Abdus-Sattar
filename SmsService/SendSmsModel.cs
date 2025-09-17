using System;

namespace SmsService
{
    public class SendSmsModel
    {
        public Guid Guid { get; }
        public string Number { get; set; }
        public string Text { get; set; }

        public SendSmsModel()
        {
            Guid = Guid.NewGuid();
        }
    }
}
