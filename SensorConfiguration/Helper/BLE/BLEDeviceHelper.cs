using HandyControl.Tools.Converter;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using SensorConfiguration.Helper.BLE.Messages;
using SensorConfiguration.Models;
using SensorConfiguration.Services;
using SensorConfiguration.SharedData;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SensorConfiguration.Constant.Enums;

namespace SensorConfiguration.Helper.BLE
{
    public class BLEDeviceHelper
    {
        private object _lockObj = new object();

        private byte Sequence { set; get; }

        private IDevice Device { set; get; }

        public IDevice GetDevice() { return Device; }   

        private byte GetSequence()
        {
            lock (_lockObj)
            {
                if (Sequence + 1 > 255)
                {
                    return 0;
                }
                return Sequence++;
            }
        }

        public event EventHandler<ReceiveMessage>? ValueUpdated;

        private static ConcurrentDictionary<Guid, BLEDeviceHelper> BLEDeviceHelperDic = new ConcurrentDictionary<Guid, BLEDeviceHelper>();

        private ObservableCollection<BluetoothMessageItem> bluetoothLog;

        private BLEDeviceHelper(IDevice device)
        {
            bluetoothLog = BluetoothLog.BluetoothMessages;
            Sequence = 1;
            Device = device;
        }

        public static BLEDeviceHelper GetBLEDeviceHelper(IDevice device)
        {
            if (device.State != DeviceState.Connected)
            {
                return null;
            }
            if (!BLEDeviceHelperDic.ContainsKey(device.Id))
            {
                var bleDeviceHelper = new BLEDeviceHelper(device);
                var re = bleDeviceHelper.EnableNotification();
                if (!re)
                {
                    return null;
                }
                BLEDeviceHelperDic.TryAdd(device.Id, bleDeviceHelper);
            }
            return BLEDeviceHelperDic[device.Id];
        }

        public static void RemoveBLEDeviceHelper(Guid deviceId)
        {
            if (BLEDeviceHelperDic.ContainsKey(deviceId))
            {
                BLEDeviceHelperDic.TryRemove(deviceId, out _);
            }
        }

        private async Task<ICharacteristic> GetWriteCharacteristic()
        {
            var services = await Device.GetServicesAsync();
            if (!services.Any())
            {
                return null;
            }
            var sendService = services.FirstOrDefault(r => r.Id.ToString().StartsWith("0000fff0"));
            if (sendService == null)
            {
                return null;
            }
            var characteristics = await sendService.GetCharacteristicsAsync();
            if (!characteristics.Any())
            {
                return null;
            }
            return characteristics.FirstOrDefault(r => r.Id.ToString().StartsWith("0000fff2"));
        }

        private async Task<ICharacteristic> GetReadCharacteristic()
        {
            var services = await Device.GetServicesAsync();
            if (!services.Any())
            {
                return null;
            }
            var sendService = services.FirstOrDefault(r => r.Id.ToString().StartsWith("0000fff0"));
            if (sendService == null)
            {
                return null;
            }
            var characteristics = await sendService.GetCharacteristicsAsync();
            if (!characteristics.Any())
            {
                return null;
            }
            return characteristics.FirstOrDefault(r => r.Id.ToString().StartsWith("0000fff1"));
        }

        /// <summary>
        /// 启用通知
        /// </summary>
        private bool EnableNotification()
        {
            var re = Task.Run( async () =>
            {
                try
                {
                    var characteristic = await GetReadCharacteristic();
                    if (characteristic == null)
                    {
                        return false;
                    }
                    var re = await characteristic.EnableNotification();
                    if (!re)
                    {
                        return false;
                    }
                    characteristic.ValueUpdated += Characteristic_ValueUpdated;
                    return true;
                }
                catch
                {
                    return false;
                }
               
            });
            return re.Result;
        }

        /// <summary>
        /// 获取16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private string GetHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", " ");
        }

        /// <summary>
        /// 异步写入
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        private async Task<bool> WriteAsync(byte messageId, byte[]? payload = null)
        {
            try
            {
                var characteristic = await GetWriteCharacteristic();
                if (characteristic == null)
                {
                    return false;
                }
                characteristic.WriteType = CharacteristicWriteType.Default;
                var message = new SendMessage
                {
                    Sequence = GetSequence(),
                    MessageId = messageId,
                    Payload = payload
                };
                var bytes = message.GetBytes();
                var re = await characteristic.WriteAsync(bytes);
                bluetoothLog?.Add(new BluetoothMessageItem
                {
                    ResultCode = re,
                    Sender = Device.Name,
                    Message = GetHexString(bytes),
                    Type = "Send"
                });
                App.InfoLog.Info(string.Format("Type:[{0}]|Sender:[{1}]|Message:[{2}]", "Send", Device.Name, GetHexString(bytes)));
                return re == 0;
            }
            catch (Exception e)
            {
                App.ErrorLog.Info(e.Message);
                return false;
            }
            
        }

        /// <summary>
        /// 获取设备信息
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RequestInformation()
        {
            if (DeviceInfo.IsFDPDevice(Device.Id))
            {
                return await WriteAsync(0x00, new byte[] { 0x16 });
            }
            return await WriteAsync(0x00, new byte[] { 0x01 });
        }

        /// <summary>
        /// 写入设备信息
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RequestInformation(ConfigurationInfo configurationInfo)
        {
            if (DeviceInfo.IsFDPDevice(Device.Id))
            {
                return await WriteAsync(0x16, configurationInfo.ToByteArr(true));
            }
            return await WriteAsync(0x01, configurationInfo.ToByteArr());
        }

        /// <summary>
        /// 开启自动通知
        /// </summary>
        /// <returns></returns>
        public async Task<bool> GetDeviceName()
        {
            return await WriteAsync(0x00, new byte[] { 0x02 });
        }

        /// <summary>
        /// 开启自动通知
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SetDeviceName(string deviceName)
        {
            return await WriteAsync(0x02, 
                Encoding.ASCII.GetBytes(deviceName.Trim((char)0x20, (char)0x00).PadRight(16, (char)0x20)));
        }

        /// <summary>
        /// 获取设备参数
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DeviceParameters()
        {
            return await WriteAsync(0x00, new byte[] { 0x03 });
        }

        /// <summary>
        /// 设置DimmerLevel
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SetDimmerLevel(byte dimmerLevel)
        {
            return await WriteAsync(0x08, new byte[] { dimmerLevel });
        }

        /// <summary>
        /// 设置DimmerLevel
        /// </summary>
        /// <returns></returns>
        public async Task<bool> LoadFlash(byte flashTime)
        {
            return await WriteAsync(0x0D, new byte[] { flashTime });
        }

        /// <summary>
        /// 设置密码
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<bool> AuthenticationSetPassword(string password)
        {
            return await WriteAsync(0x0C,
                Encoding.ASCII.GetBytes(password.PadRight(16, '0')));
        }

        public CancellationTokenSource? CancellationTokenSource { set; get; }

        /// <summary>
        /// 权限认证
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<bool> AuthenticationRequest(string password)
        {
            return await WriteAsync(0x0A,
                Encoding.ASCII.GetBytes(password.PadRight(16, '0')));
        }

        /// <summary>
        /// 开启自动通知
        /// </summary>
        /// <returns></returns>
        public async Task<bool> EnableAutoNotification()
        {
            return await WriteAsync(0x0e, new byte[] { 0x03 });
        }

        /// <summary>
        /// 关闭自动通知
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DisableAutoNotification()
        {
            return await WriteAsync(0x0e, new byte[] { 0x00 });
        }

        /// <summary>
        /// 开启自动通知
        /// </summary>
        /// <returns></returns>
        public async Task<bool> GetContinuousDimmingConfiguration()
        {
            return await WriteAsync(0x00, new byte[] { 0x18 });
        }

        /// <summary>
        /// 开启自动通知
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SetContinuousDimmingConfiguration(ContinuousDimmingConfiguration continuousDimmingConfiguration)
        {
            return await WriteAsync(0x18, continuousDimmingConfiguration.ToByteArr());
        }

        /// <summary>
        /// 结果返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Characteristic_ValueUpdated(object? sender, CharacteristicUpdatedEventArgs e)
        {
            try
            {
                if (sender == null)
                {
                    return;
                }
                var characteristic = e.Characteristic;
                var receiveMessage = new ReceiveMessage(characteristic.Value, DeviceInfo.IsFDPDevice(Device.Id));
                bluetoothLog?.Add(new BluetoothMessageItem
                {
                    ResultCode = 0,
                    Sender = Device.Name,
                    Message = GetHexString(characteristic.Value),
                    Type = "Receive"
                });
                App.InfoLog.Info(string.Format("Type:[{0}]|Sender:[{1}]|Message:[{2}]", "Receive", Device.Name, GetHexString(characteristic.Value)));
                HandleDeviceParameters(receiveMessage);
                ValueUpdated?.Invoke(this, receiveMessage);
            }
            catch (Exception ex)
            {
                App.ErrorLog.Info(ex.Message);
            }
        }

        private void HandleDeviceParameters(ReceiveMessage receiveMessage)
        {
            if (receiveMessage == null)
            {
                return;
            }
            if (receiveMessage.GetMessageType() != (byte)MessageType.DeviceParameters)
            {
                return;
            }
            var deviceParameter = receiveMessage.GetResult() as DeviceParameters;
            if (deviceParameter == null)
            {
                return;
            }
            if (!DeviceInfo.DeviceParameterDic.ContainsKey(Device.Id))
            {
                DeviceInfo.DeviceParameterDic.TryAdd(Device.Id, deviceParameter);
                return;
            }
            DeviceInfo.DeviceParameterDic[Device.Id] = deviceParameter;
        }


        #region Test Mode
        public event EventHandler<RemainderEventArgs>? RemainderUpdated;

        /// <summary>
        /// 获取测试模式剩余时间
        /// </summary>
        /// <returns></returns>
        public async Task<bool> TestMode()
        {
            if(Remainder == 0)
            {
                return true;
            }
            var re = await WriteAsync(0x00, new byte[] { 0x04 });
            if (re)
            {
                Remainder = 0;
            }
            return re;
        }

        private Task? TestModeTask;

        private int Remainder = 0;

        /// <summary>
        /// 测试模式
        /// </summary>
        /// <returns></returns>
        public async Task<bool> TestMode(int seconds)
        {
            if ((seconds != 0) == (Remainder != 0))
            {
                return false;
            }
            var re = await WriteAsync(0x04, BitConverter.GetBytes(seconds));
            if (!re)
            {
                return false;
            }
            Remainder = seconds;
            TestModeTask = new Task(() =>
            {
                RemainderUpdated?.Invoke(this, new RemainderEventArgs
                {
                    DeviceId = Device.Id,
                    Remainder = this.Remainder
                });
                while (this.Remainder > 0)
                {
                    Thread.Sleep(1000);
                    if (this.Remainder >= 1)
                    {
                        this.Remainder--;
                    }
                    RemainderUpdated?.Invoke(this, new RemainderEventArgs
                    {
                        DeviceId = Device.Id,
                        Remainder = this.Remainder
                    });
                }
            });
            TestModeTask.Start();
            return true;
        }
        #endregion
    }

    public class RemainderEventArgs
    {
        public Guid DeviceId { get; set; }
        public int Remainder { get; set; }
    }
}
