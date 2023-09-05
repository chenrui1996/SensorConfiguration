using System;
using System.Collections.Generic;
using System.Text;

namespace SensorConfiguration.Constant
{
    public class Common
    {
        public ushort ByteArrToInt(byte[] param)
        {
            return BitConverter.ToUInt16(param, 0);
        }

        public static readonly string DefultPassword = "FSP123";
    }
}
