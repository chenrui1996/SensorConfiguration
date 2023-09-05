using System;
using System.ComponentModel;

namespace SensorConfiguration.Constant
{
    public class EnumKeyValue
    {
        public string? EnumKey { set; get; }
        public int EnumValue { set; get; }

    }

    public class Enums
    {
        public enum MessageType
        {
            ConfigurationInfo = 0x01,
            DeviceName = 0x02,
            DeviceParameters = 0x03,
            LoadLevel = 0x08,
            LightSensorLevel = 0x09,
            AuthenticationGranted = 0x0b,
            ContinuousDimmingConfiguration = 0x18
        }

        public enum PropertyType
        {
            None = 0,
            WithUnit = 1,
            Enum = 2,
            StrEnum = 3
        }

        public enum Sensitivity
        {
            OffFix = 0,
            Low = 1,
            Medium = 2,
            High = 3,
            OnFix = 255
        }

        public enum EnableStatus
        {
            Disable = 0,
            Enable = 1
        }

        public enum FadeTime
        {
            [Description("Extended")]
            Extended = 0,

            [Description("0.7 Seconds")]
            Value0_7 = 1,

            [Description("1.0 Seconds")]
            Value1_0 = 2,

            [Description("1.4 Seconds")]
            Value1_4 = 3,

            [Description("2.0 Seconds")]
            Value2_0 = 4,

            [Description("2.8 Seconds")]
            Value2_8 = 5,

            [Description("4.0 Seconds")]
            Value4_0 = 6,

            [Description("5.7 Seconds")]
            Value5_7 = 7,

            [Description("8.0 Seconds")]
            Value8_0 = 8,

            [Description("11.3 Seconds")]
            Value11_3 = 9,

            [Description("16.0 Seconds")]
            Value16_0 = 10,

            [Description("32.0 Seconds")]
            Value32_0 = 12,

            [Description("45.3 Seconds")]
            Value45_3 = 13,

            [Description("64 Seconds")]
            Value64 = 14,

            [Description("90.5 Seconds")]
            Value90_5 = 15,
        }

        public enum FadeRate
        {
            [Description("358 Steps/sec")]
            _1 = 1,

            [Description("253 Steps/sec")]
            _2 = 2,

            [Description("179 Steps/sec")]
            _3 = 3,

            [Description("127 Steps/sec")]
            _4 = 4,

            [Description("89.4 Steps/sec")]
            _5 = 5,

            [Description("63.3 Steps/sec")]
            _6 = 6,

            [Description("44.7 Steps/sec")]
            _7 = 7,

            [Description("31.6 Steps/sec")]
            _8 = 8,

            [Description("22.4 Steps/sec")]
            _9 = 9,

            [Description("15.8 Steps/sec")]
            _10 = 10,

            [Description("11.2 Steps/sec")]
            _11 = 11,

            [Description("7.9 Steps/sec")]
            _12 = 12,

            [Description("5.6 Steps/sec")]
            _13 = 13,

            [Description("4.0 Steps/sec")]
            _14 = 14,

            [Description("2.8 Steps/sec")]
            _15 = 15
        }


        public static string GetEnumStringValue(Type type, int value)
        {
            try
            {
                var memberInfo = type.GetMember(Enum.ToObject(type, value).ToString() ?? "");
                var descriptionAttribute = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (descriptionAttribute.Length > 0)
                {
                    return ((DescriptionAttribute)descriptionAttribute[0]).Description;
                }
                return value.ToString();
            }
            catch
            {
                return value.ToString();
            }
            
        }
    }
}
