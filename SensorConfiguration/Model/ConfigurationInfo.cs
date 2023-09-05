using SensorConfiguration.Attributes;
using static SensorConfiguration.Constant.Enums;

namespace SensorConfiguration.Models
{
    public class ConfigurationInfo
    {
        [PropertyConfig("Basic", "High Mode", PropertyType.WithUnit, 0, 100, "Volts", "%")]
        public byte HighMode { get; set; }
        [PropertyConfig("Basic", "Low Mode", PropertyType.WithUnit, 0, 90, "Volts", "%")]
        public byte LowMode { get; set; }
        [PropertyConfig("Basic", "Time Delay", PropertyType.WithUnit, 0, 30, "Minutes")]
        public byte TimeDelay { get; set; }
        [PropertyConfig("Basic", "Cut Off", PropertyType.WithUnit, 0, 59, "Minutes")]
        public byte CutOff { get; set; }
        [PropertyConfig("Basic", "Sensitivity", PropertyType.Enum, typeof(Sensitivity))]
        public byte Sensitivity { get; set; }
        [PropertyConfig("Advanced", "Hold Off", PropertyType.WithUnit, 0, 250, "Foot Candles")]
        public byte HoldOff { get; set; }
        [PropertyConfig("Advanced", "Ramp Up", PropertyType.WithUnit, 0, 60, "Seconds")]
        public byte RampUp { get; set; }
        [PropertyConfig("Advanced", "Fade Down", PropertyType.WithUnit, 0, 60, "Seconds")]
        public byte FadeDown { get; set; }
        [PropertyConfig("Advanced", "Photocell", PropertyType.WithUnit, 0, 250, "Foot Candles")]
        public byte Photocell { get; set; }
        [PropertyConfig("DALI Specific", "Fade Time", PropertyType.StrEnum, typeof(FadeTime))]
        public byte FadeTime { get; set; }
        [PropertyConfig("DALI Specific", "Fade Rate", PropertyType.StrEnum, typeof(FadeRate))]
        public byte FadeRate { get; set; }
        [PropertyConfig("DALI Specific", "Hold Time", PropertyType.WithUnit, 0, 255, "Seconds")]
        public byte HoldTime { get; set; }
    }

    public static class ConfigurationInfoExtension
    {
        public static byte[] ToByteArr(this ConfigurationInfo configurationInfo, bool isFDPDevice = false)
        {
            if (isFDPDevice)
            {
                return new byte[]
                {
                    configurationInfo.HighMode,
                    configurationInfo.LowMode,
                    configurationInfo.TimeDelay,
                    configurationInfo.CutOff,
                    configurationInfo.Sensitivity,
                    configurationInfo.HoldOff,
                    configurationInfo.RampUp,
                    configurationInfo.FadeDown,
                    configurationInfo.Photocell,
                    configurationInfo.FadeTime,
                    configurationInfo.FadeRate,
                    configurationInfo.HoldTime
                };
            }
            return new byte[]
            {
                configurationInfo.HighMode,
                configurationInfo.LowMode,
                configurationInfo.TimeDelay,
                configurationInfo.CutOff,
                configurationInfo.Sensitivity,
                configurationInfo.HoldOff,
                configurationInfo.RampUp,
                configurationInfo.FadeDown,
                configurationInfo.Photocell,
            };
            
            
        }
    }
}
