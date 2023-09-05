using SensorConfiguration.Attributes;
using static SensorConfiguration.Constant.Enums;

namespace SensorConfiguration.Models
{
    public class ContinuousDimmingConfiguration
    {
        [PropertyConfig("Basic", "EnableStatus", PropertyType.Enum, typeof(EnableStatus), false)]
        public byte EnableStatus { get; set; }
        [PropertyConfig("Day", "Occupied Target", PropertyType.WithUnit, 0, 250, "Foot Candles")]
        public byte DayOccupiedTarget { get; set; }
        [PropertyConfig("Day", "Occupied Time Delay", PropertyType.WithUnit, 0, 30, "Minutes")]
        public byte DayOccupiedTimeDelay { get; set; }
        [PropertyConfig("Day", "Unoccupied Target", PropertyType.WithUnit, 0, 250, "Foot Candles")]
        public byte DayUnoccupiedTarget { get; set; }
        [PropertyConfig("Day", "Unoccupied Fixed Level", PropertyType.WithUnit, 0, 100, "Volts")]
        public byte DayUnoccupiedFixedLevel { get; set; }
        [PropertyConfig("Day", "Unoccupied Time Delay", PropertyType.WithUnit, 0, 66, "Minutes")]
        public byte DayUnoccupiedTimeDelay { get; set; }
        [PropertyConfig("Night", "Occupied Target", PropertyType.WithUnit, 0, 250, "Foot Candles")]
        public byte NightOccupiedTarget { get; set; }
        [PropertyConfig("Night", "Occupied Time Delay", PropertyType.WithUnit, 0, 30, "Minutes")]
        public byte NightOccupiedTimeDelay { get; set; }
        [PropertyConfig("Night", "Unoccupied Target", PropertyType.WithUnit, 0, 250, "Foot Candles")]
        public byte NightUnoccupiedTarget { get; set; }
        [PropertyConfig("Night", "Unoccupied Fixed Level", PropertyType.WithUnit, 0, 100, "Volts")]
        public byte NightUnoccupiedFixedLevel { get; set; }
        [PropertyConfig("Night", "Unoccupied Time Delay", PropertyType.WithUnit, 0, 66, "Minutes")]
        public byte NightUnoccupiedTimeDelay { get; set; }
    }

    public static class ContinuousDimmingConfigurationExtension
    {
        public static byte[] ToByteArr(this ContinuousDimmingConfiguration continuousDimmingConfiguration)
        {
            return new byte[]
            {
                continuousDimmingConfiguration.EnableStatus,
                continuousDimmingConfiguration.DayOccupiedTarget,
                continuousDimmingConfiguration.DayOccupiedTimeDelay,
                continuousDimmingConfiguration.DayUnoccupiedTarget,
                continuousDimmingConfiguration.DayUnoccupiedFixedLevel,
                continuousDimmingConfiguration.DayUnoccupiedTimeDelay,
                continuousDimmingConfiguration.NightOccupiedTarget,
                continuousDimmingConfiguration.NightOccupiedTimeDelay,
                continuousDimmingConfiguration.NightUnoccupiedTarget,
                continuousDimmingConfiguration.NightUnoccupiedFixedLevel,
                continuousDimmingConfiguration.NightUnoccupiedTimeDelay
            };

        }
    }
}
