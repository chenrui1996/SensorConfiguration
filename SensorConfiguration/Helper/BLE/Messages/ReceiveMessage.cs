using SensorConfiguration.Models;
using SensorConfiguration.SharedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SensorConfiguration.Constant.Enums;

namespace SensorConfiguration.Helper.BLE.Messages
{
    public class ReceiveMessage
    {
        private byte Sequence { set; get; }

        private byte MessageId { set; get; }

        private byte[]? Payload { set; get; }

        public byte[] Bytes { get; }

        private byte messageType { set; get; }

        public byte GetMessageType()
        {
            return messageType;
        }

        private object? result { set; get; }

        public object? GetResult()
        {
            return result;
        }

        public ReceiveMessage(byte[] bytes, bool specialFlag = false)
        {
            Bytes = bytes;
            Sequence = bytes[0];
            MessageId = bytes[1];
            messageType = MessageId;
            if (Bytes.Length > 2)
            {
                Payload = bytes.Skip(2).ToArray();
                ConvertToModel(specialFlag);
            }
        }

        private void ConvertToModel(bool specialFlag = false)
        {
            if (specialFlag)
            {
                if (messageType == 0x16)
                {
                    ToConfigurationInfo(specialFlag);
                    return;
                }
                if (messageType == 0x17)
                {
                    return;
                }
            }
            switch ((MessageType)messageType)
            {
                case MessageType.ConfigurationInfo:
                    ToConfigurationInfo();
                    break;
                case MessageType.DeviceName:
                    ToDevicename();
                    break;
                case MessageType.DeviceParameters:
                    ToDeviceParameters();
                    break;
                case MessageType.LoadLevel:
                    ToLoadLevel();
                    break;
                case MessageType.LightSensorLevel:
                    ToLightSensorLevel();
                    break;
                case MessageType.AuthenticationGranted:
                    ToAuthenticationGranted();
                    break;
                case MessageType.ContinuousDimmingConfiguration:
                    ToContinuousDimmingConfiguration();
                    break;
                default:
                    break;
            }
        }

        private void ToConfigurationInfo(bool specialFlag = false)
        {
            if (Payload == null || Payload.Length < 9)
            {
                return;
            }
            var configurationInfo = new ConfigurationInfo()
            {
                HighMode = Payload[0],
                LowMode = Payload[1],
                TimeDelay = Payload[2],
                CutOff = Payload[3],
                Sensitivity = Payload[4],
                HoldOff = Payload[5],
                RampUp = Payload[6],
                FadeDown = Payload[7],
                Photocell = Payload[8]
            };
            if (specialFlag)
            {
                if (Payload.Length < 12)
                {
                    return;
                }
                configurationInfo.FadeTime = Payload[9];
                configurationInfo.FadeRate = Payload[10];
                configurationInfo.HoldTime = Payload[11];
            }
            result = configurationInfo;
        }

        private void ToDevicename()
        {
            if (Payload == null || Payload.Length < 16)
            {
                return;
            }
            result = Encoding.ASCII.GetString(Payload).Trim((char)0x20, (char)0x00);
        }

        private void ToDeviceParameters()
        {
            if (Payload == null || Payload.Length < 9)
            {
                return;
            }
            var deviceParameters = new DeviceParameters()
            {
                DeviceType = DeviceParameters.GetDeviceType(Payload[0]),
                DeviceClass = Payload[1],
                Year = Payload[2],
                Week = Payload[3],
                HWVersion = Payload[4],
                FWVersion = string.Format("{0}.{1}", Payload[5], Payload[6]),
                ZXCalibrationValueForOpen = BitConverter.ToUInt16(new byte[] { Payload[7], Payload[8] }, 0) ,
                ZXCalibrationValueForClose = BitConverter.ToUInt16(new byte[] { Payload[9], Payload[10] }, 0),
                RelayCycleCounter = BitConverter.ToUInt16(new byte[] { Payload[11], Payload[12] }, 0),
                BootCodeVersion = string.Format("{0}.{1}", Payload[13], Payload[14])
            };
            result = deviceParameters;
        }

        private void ToLoadLevel()
        {
            if (Payload == null || Payload.Length < 1)
            {
                return;
            }
            result = Payload[0];
        }

        private void ToLightSensorLevel()
        {
            if (Payload == null || Payload.Length < 2)
            {
                return;
            }
            result = BitConverter.ToUInt16(new byte[] { Payload[0], Payload[1] }, 0);

        }

        private void ToAuthenticationGranted()
        {
            if (Payload == null || Payload.Length < 1)
            {
                return;
            }
            result = Payload[0] == 1;
        }

        private void ToContinuousDimmingConfiguration()
        {
            if (Payload == null || Payload.Length < 11)
            {
                return;
            }
            result = new ContinuousDimmingConfiguration()
            {
                EnableStatus = Payload[0],
                DayOccupiedTarget = Payload[1],
                DayOccupiedTimeDelay = Payload[2],
                DayUnoccupiedTarget = Payload[3],
                DayUnoccupiedFixedLevel = Payload[4],
                DayUnoccupiedTimeDelay = Payload[5],
                NightOccupiedTarget = Payload[6],
                NightOccupiedTimeDelay = Payload[7],
                NightUnoccupiedTarget = Payload[8],
                NightUnoccupiedFixedLevel = Payload[9],
                NightUnoccupiedTimeDelay = Payload[10]
            };
        }
    }
}
