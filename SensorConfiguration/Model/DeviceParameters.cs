using SensorConfiguration.Attributes;
using static SensorConfiguration.Constant.Enums;

namespace SensorConfiguration.Models
{
    public class DeviceParameters
    {
        [PropertyConfig("Device Type", PropertyType.WithUnit, 0, 0, "")]
        public string DeviceType { get; set; }
        [PropertyConfig("Device Class", PropertyType.WithUnit, 0, 0, "")]
        public byte DeviceClass { get; set; }
        [PropertyConfig("Year", PropertyType.WithUnit, 0, 0, "")]
        public byte Year { get; set; }
        [PropertyConfig("Week", PropertyType.WithUnit, 0, 0, "")]
        public byte Week { get; set; }
        [PropertyConfig("Hardware Version", PropertyType.WithUnit, 0, 0, "")]
        public byte HWVersion { get; set; }
        [PropertyConfig("Firmware Version", PropertyType.WithUnit, 0, 0, "")]
        public string FWVersion { get; set; }
        [PropertyConfig("Open Calibration", PropertyType.WithUnit, 0, 0, "")]
        public ushort ZXCalibrationValueForOpen { get; set; }
        [PropertyConfig("Close Calibration", PropertyType.WithUnit, 0, 0, "")]
        public ushort ZXCalibrationValueForClose { get; set; }
        [PropertyConfig("Relay CycleCounter", PropertyType.WithUnit, 0, 0, "")]
        public ushort RelayCycleCounter { get; set; }
        [PropertyConfig("Boot Code Version", PropertyType.WithUnit, 0, 0, "")]
        public string? BootCodeVersion { get; set; }


        public static string GetDeviceType(byte deviceType)
        {
            switch (deviceType)
            {
                case 0xA0: return "FSP201";
                case 0xA1: return "FSP211";
                case 0xA2: return "FSP301";
                case 0xA3: return "FSP311";
                case 0xA4: return "FSP301B";
                case 0xA5: return "FSP311B";
                case 0xA6: return "FSP321B";
                case 0xA7: return "FDP301SR";
                case 0xA8: return "FDP301";
                case 0xA9: return "FSP321";
                case 0xAA: return "FSP221";
            }
            return "";
        }
    }
}
