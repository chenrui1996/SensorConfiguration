using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensorConfiguration.Helper.BLE.Messages
{
    public class SendMessage
    {
        public byte Sequence { set; get; }

        public byte MessageId { set; get; }

        public byte[]? Payload { set; get; }

        public byte[] GetBytes()
        {
            var re = new[] { Sequence, MessageId };
            if (Payload == null || !Payload.Any())
            {
                return re;
            }
            return re.Concat(Payload).ToArray();
        }
    }
}
