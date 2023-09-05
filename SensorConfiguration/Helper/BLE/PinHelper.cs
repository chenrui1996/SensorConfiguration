using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SensorConfiguration.Helper.BLE
{
    public class PinHelper
    {
        private PinHelper()
        {

        }

        public static PinHelper GetPinHelper()
        {
            return new PinHelper();
        }

        public static string GetPin(string macAddress)
        {
            return GetPinHelper().GetPinPass(macAddress);
        }

        public static byte[] GetBytePin(string macAddress)
        {
            return GetPinHelper().GetPinPassByte(macAddress);
        }

        public byte[] GetPinPassByte(string macAddress)
        {
            return Encoding.UTF8.GetBytes(GetPinPass(macAddress));
        }

        public string GetPinPass(string macAddress)
        {
            var macByte = ParseMacAddress(macAddress);
            var crc = GetCRC32Str(macByte);
            return crc.ToString().Substring(0, 6);
        }

        protected ulong[] Crc32Table;
        //生成CRC32码表
        private void GetCRC32Table()
        {
            ulong Crc;
            Crc32Table = new ulong[256];
            int i, j;
            for (i = 0; i < 256; i++)
            {
                Crc = (ulong)i;
                for (j = 8; j > 0; j--)
                {
                    if ((Crc & 1) == 1)
                        Crc = Crc >> 1 ^ 0xEDB88320;
                    else
                        Crc >>= 1;
                }
                Crc32Table[i] = Crc;
            }
        }

        //获取字符串的CRC32校验值
        public ulong GetCRC32Str(string sInputString)
        {
            //生成码表
            GetCRC32Table();
            byte[] buffer = Encoding.ASCII.GetBytes(sInputString);
            ulong value = 0xffffffff;
            int len = buffer.Length;
            for (int i = 0; i < len; i++)
            {
                value = value >> 8 ^ Crc32Table[value & 0xFF ^ buffer[i]];
            }
            return value ^ 0xffffffff;
        }

        public ulong GetCRC32Str(byte[] buffer)
        {
            //生成码表
            GetCRC32Table();
            ulong value = 0xffffffff;
            int len = buffer.Length;
            for (int i = 0; i < len; i++)
            {
                value = value >> 8 ^ Crc32Table[value & 0xFF ^ buffer[i]];
            }
            return value ^ 0xffffffff;
        }

        public byte[] ParseMacAddress(string macAddress)
        {
            // Remove colon separators and convert to byte array
            macAddress = macAddress.Replace(":", string.Empty);
            byte[] macBytes = new byte[macAddress.Length / 2];
            for (int i = 0; i < macBytes.Length; i++)
            {
                macBytes[i] = Convert.ToByte(macAddress.Substring(i * 2, 2), 16);
            }
            return macBytes;
        }
    }
}
