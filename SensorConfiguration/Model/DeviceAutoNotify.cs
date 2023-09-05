using CommunityToolkit.Mvvm.ComponentModel;
using SensorConfiguration.Attributes;
using static SensorConfiguration.Constant.Enums;

namespace SensorConfiguration.Models
{
    public class DeviceAutoNotify : ObservableObject
    {
        [PropertyConfig("Dimmer Level", PropertyType.WithUnit, 0, 0, "%")]
        public byte LoadLevel { set; get; }
        [PropertyConfig("Sensor Level", PropertyType.WithUnit, 0, 0, "Foot Candles")]
        public byte LightSensorLevel { set; get; }
    }
}
