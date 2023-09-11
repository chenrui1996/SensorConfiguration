using Plugin.BLE.Abstractions.Contracts;
using SensorConfiguration.Helper.BLE.Messages;
using SensorConfiguration.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SensorConfiguration.SharedData
{
    public class DeviceInfo
    {
        public static ConcurrentDictionary<Guid, IDevice> DeviceDic = new ConcurrentDictionary<Guid, IDevice>();

        public static ConcurrentDictionary<Guid, string> DevicePasswordDic = new ConcurrentDictionary<Guid, string>();

        public static ConcurrentDictionary<Guid, DeviceParameters> DeviceParameterDic = new ConcurrentDictionary<Guid, DeviceParameters>();
        
        public static ConcurrentDictionary<Guid, DeviceAutoNotify> DeviceAutoNotifyDic = new ConcurrentDictionary<Guid, DeviceAutoNotify>();

        private static object lockObj = new object();

        public static bool IsFDPDevice(Guid deviceId)
        {
            lock (lockObj)
            {
                if (!DeviceParameterDic.ContainsKey(deviceId))
                {
                    return false;
                }
                var deviceParameter = DeviceParameterDic[deviceId];
                if (string.IsNullOrWhiteSpace(deviceParameter.DeviceType))
                {
                    return false;
                }
                return deviceParameter.DeviceType.Contains("FDP");
            }
        }
    }
}
