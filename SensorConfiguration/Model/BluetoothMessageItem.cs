using System;
using System.Collections.Generic;
using System.Text;

namespace SensorConfiguration.Models
{
    public class BluetoothMessageItem
    {
        public int ResultCode { get; set; }
        public string? Type { get; set; }
        public string? Sender { get; set; }
        public string? Message { get; set; }
    }
}
