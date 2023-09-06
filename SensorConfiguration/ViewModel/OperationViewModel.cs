using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using SensorConfiguration.Constant;
using SensorConfiguration.Helper.BLE.Messages;
using SensorConfiguration.Helper.BLE;
using SensorConfiguration.Models;
using SensorConfiguration.Services;
using SensorConfiguration.SharedData;
using SensorConfiguration.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using static SensorConfiguration.Constant.Enums;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE;
using System.Windows;
using System.Collections.Concurrent;
using Plugin.BLE.Abstractions;

namespace SensorConfiguration.ViewModel
{
    public class OperationViewModel : ObservableRecipient
    {
        #region 设备信息
        /// <summary>
        /// 蓝牙
        /// </summary>
        IBluetoothLE Ble;

        /// <summary>
        /// 蓝牙适配器
        /// </summary>
        IAdapter Adapter;

        /// <summary>
        /// 设备信息
        /// </summary>
        private ConcurrentDictionary<Guid, IDevice> _deviceDic = DeviceInfo.DeviceDic;
        #endregion

        #region UI
        private bool _defultPasswordEnabled;

        public bool DefultPasswordEnabled
        {
            get { return _defultPasswordEnabled; }
            set
            {
                if (_defultPasswordEnabled != value)
                {
                    SetProperty(ref _defultPasswordEnabled, value);
                }
            }
        }

        private bool _newDeviceEnabled;

        public bool NewDeviceEnabled
        {
            get { return _newDeviceEnabled; }
            set
            {
                if (_newDeviceEnabled != value)
                {
                    SetProperty(ref _newDeviceEnabled, value);
                }
            }
        }

        private bool _startScanEnabled;

        public bool StartScanEnabled
        {
            get { return _startScanEnabled; }
            set
            {
                if (_startScanEnabled != value)
                {
                    SetProperty(ref _startScanEnabled, value);
                }
            }
        }

        private bool _connectEnabled;

        public bool ConnectEnabled
        {
            get { return _connectEnabled; }
            set
            {
                if (_connectEnabled != value)
                {
                    SetProperty(ref _connectEnabled, value);
                }
            }
        }

        private bool _disconnectEnabled;

        public bool DisconnectEnabled
        {
            get { return _disconnectEnabled; }
            set
            {
                if (_disconnectEnabled != value)
                {
                    SetProperty(ref _disconnectEnabled, value);
                }
            }
        }

        private bool _bluetoothViewEnabled;

        public bool BluetoothViewEnabled
        {
            get { return _bluetoothViewEnabled; }
            set
            {
                if (_bluetoothViewEnabled != value)
                {
                    SetProperty(ref _bluetoothViewEnabled, value);
                }
            }
        }

        /// <summary>
        /// 打开扫码创建
        /// </summary>
        public ICommand OpenSacnDialogCommand { get; }

        private void OpenSacnDialog()
        {
            new DialogService().ShowScanDialog();
        }
        #endregion

        #region 连接

        /// <summary>
        /// 断连指令
        /// </summary>
        public ICommand DisconnectDeviceCommand { get; }

        /// <summary>
        /// 断连
        /// </summary>
        /// <param name="obj"></param>
        public void DisconnectDevice()
        {
            try
            {
                if (SelectedLoggedBluetooth != null)
                {
                    if (!_deviceDic.ContainsKey(SelectedLoggedBluetooth.Id))
                    {
                        new DialogService().DisplayAlert("提示", "设备查找失败", "OK");
                        return;
                    }
                    var selectDevice = _deviceDic[SelectedLoggedBluetooth.Id];
                    DisconnectDeviceNative(selectDevice);
                    LoggedBluetooths.Clear();
                    foreach (var device in Adapter.ConnectedDevices)
                    {
                        LoggedBluetooths.Add(new BluetoothItem
                        {
                            Id = device.Id,
                            Name = device.Name,
                            Rssi = device.Rssi,
                            Address = GetDeviceAddress(device)
                        });
                    }
                }
            }
            catch (Exception e)
            {
                new DialogService().DisplayAlert("提示", e.Message, "OK");
            }
        }

        /// <summary>
        /// 获取MAC地址
        /// </summary>
        /// <param name="nativeDevice"></param>
        /// <returns></returns>
        private string GetDeviceAddress(object nativeDevice)
        {
            try
            {
                if (nativeDevice == null)
                {
                    return "";
                }
                Type type = nativeDevice.GetType();
                if (type == null)
                {
                    return "";
                }
                // 获取Name属性信息
                PropertyInfo? property = type.GetProperty("Address");
                if (property == null)
                {
                    return "";
                }
                // 获取属性值
                string value = (string)(property.GetValue(nativeDevice) ?? "");
                return value;
            }
            catch
            {
                return "";
            }

        }

        private async void DisconnectDeviceNative(IDevice device)
        {
            await Adapter.DisconnectDeviceAsync(device);

            BLEDeviceHelper.RemoveBLEDeviceHelper(device.Id);

            if (Adapter.ConnectedDevices.Any(r => r.Id == device.Id))
            {
                new DialogService().DisplayAlert("提示", "设备断连失败", "OK");
            }
        }
        #endregion

        #region 登录
        /// <summary>
        /// 已登录设备
        /// </summary>
        private ObservableCollection<BluetoothItem> loggedBluetooths 
            = new ObservableCollection<BluetoothItem>();

        //private BLEDeviceHelper helper;
        /// <summary>
        /// 已登录设备
        /// </summary>
        public ObservableCollection<BluetoothItem> LoggedBluetooths
        {
            get { return loggedBluetooths; }
            set
            {
                if (loggedBluetooths != value)
                {
                    SetProperty(ref loggedBluetooths, value);
                }
            }
        }

        private BluetoothItem? selectedLoggedBluetooth;

        /// <summary>
        /// 已选择的可用设备
        /// </summary>
        public BluetoothItem? SelectedLoggedBluetooth
        {
            get { return selectedLoggedBluetooth; }
            set
            {
                if (selectedLoggedBluetooth != value)
                {
                    SetProperty(ref selectedLoggedBluetooth, value);
                    foreach (var item in LoggedBluetooths)
                    {
                        item.IsSelected = item.Id == value?.Id;
                    }
                    DisconnectEnabled = selectedLoggedBluetooth != null;
                }
            }
        }
        #endregion

        public OperationViewModel()
        {
            //初始化页面
            StartScanEnabled = true;
            DefultPasswordEnabled = true;
            NewDeviceEnabled = true;
            BluetoothViewEnabled = true;
            OpenSacnDialogCommand = new RelayCommand(OpenSacnDialog);
            Trace.TraceImplementation += (s, a) =>
            {
                App.InfoLog.Info(s);
            };
            //初始化蓝牙
            Ble = CrossBluetoothLE.Current;
            Adapter = CrossBluetoothLE.Current.Adapter;
            LoggedBluetooths = new ObservableCollection<BluetoothItem>();
            //初始化登录指令
            DisconnectDeviceCommand = new RelayCommand(DisconnectDevice);
        }
    }
}
