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
using SensorConfiguration.Attributes;

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
        private ObservableCollection<BluetoothItem> loggedBluetooths;


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
                    HandleDeviceParameters(selectedLoggedBluetooth);
                }
            }
        }
        #endregion

        #region 设备信息
        /// <summary>
        /// 设备信息
        /// </summary>
        private ConcurrentDictionary<Guid, DeviceParameters> _deviceParameterDic = DeviceInfo.DeviceParameterDic;

        private void HandleDeviceParameters(BluetoothItem? selectedLoggedBluetooth)
        {
            if (selectedLoggedBluetooth == null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    InitDeviceParametersKeyValue(new DeviceParameters());
                });
                return;
            }
            if(!_deviceParameterDic.ContainsKey(selectedLoggedBluetooth.Id))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    InitDeviceParametersKeyValue(new DeviceParameters());
                });
                return;
            }
            var deviceParameter = _deviceParameterDic[selectedLoggedBluetooth.Id];
            Application.Current.Dispatcher.Invoke(() =>
            {
                InitDeviceParametersKeyValue(deviceParameter);
            });
        }

        /// <summary>
        /// 初始化配置信息列表
        /// </summary>
        /// <param name="configurationInfo"></param>
        /// <param name="daliFlag"></param>
        private void InitDeviceParametersKeyValue(DeviceParameters deviceParameters)
        {
            if (deviceParameters == null)
            {
                return;
            }

            DeviceParameters.Clear();

            var properties = typeof(DeviceParameters).GetProperties();
            foreach (var property in properties)
            {
                DeviceParameters.Add(new ListViewModel
                {
                    Key = property.Name,
                    KeyValue = new KeyValue
                    {
                        Key = property.Name,
                        Value = property.GetValue(deviceParameters)?.ToString()
                    }
                });
            }
        }

        private ObservableCollection<ListViewModel>? deviceParameters;

        public ObservableCollection<ListViewModel> DeviceParameters
        {
            get { return deviceParameters; }
            set
            {
                SetProperty(ref deviceParameters, value);
            }
        }
        #endregion

        #region 配置信息
        /// <summary>
        /// 初始化配置信息列表
        /// </summary>
        /// <param name="configurationInfo"></param>
        /// <param name="daliFlag"></param>
        private void InitKeyValue(ConfigurationInfo configurationInfo, bool daliFlag = false)
        {
            if (configurationInfo == null)
            {
                return;
            }

            ConfigurationInfos.Clear();

            var properties = typeof(ConfigurationInfo).GetProperties();
            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<PropertyConfigAttribute>();
                if (!daliFlag && attribute != null && attribute.Group == "DALI Specific")
                {
                    continue;
                }
                ConfigurationInfos.Add(new ListViewModel
                {
                    Key = property.Name,
                    KeyValue = new KeyValue
                    {
                        Key = property.Name,
                        DaliFlag = daliFlag,
                        Value = property.GetValue(configurationInfo)?.ToString()
                    }
                });
            }
        }

        private ObservableCollection<ListViewModel>? configurationInfos;

        public ObservableCollection<ListViewModel> ConfigurationInfos
        {
            get { return configurationInfos; }
            set
            {
                SetProperty(ref configurationInfos, value);
            }
        }
        #endregion

        #region 配置
        /// <summary>
        /// 打开配置页面
        /// </summary>
        public ICommand ConfigurationButtonCommand { get; }

        /// <summary>
        /// 打开配置页面
        /// </summary>
        /// <param name="listViewModel"></param>
        private async void OpenConfigurationPopup(ListViewModel? listViewModel)
        {
            try
            {
                if (SelectedLoggedBluetooth == null)
                {
                    return;
                }
                if (listViewModel == null)
                {
                    return;
                }
                new DialogService().ShowConfiguationModal(listViewModel, typeof(ConfigurationInfo));
            }
            catch (Exception e)
            {
                await new DialogService().DisplayAlertAsync("错误", e.Message, "确定");
            }
        }

        public ICommand CommitConfigurationCommand { get; }

        private async void CommitConfiguration()
        {
            if (SelectedLoggedBluetooth == null)
            {
                return;
            }
            BLEDeviceHelper? helper = null;
            try
            {
                if (!_deviceDic.ContainsKey(SelectedLoggedBluetooth.Id))
                {
                    await new DialogService().DisplayAlertAsync("提示", "设备查找失败", "OK");
                    return;
                }
                var selectDevice = _deviceDic[SelectedLoggedBluetooth.Id];
                helper = BLEDeviceHelper.GetBLEDeviceHelper(selectDevice);
                var daliFlag = DeviceInfo.IsFDPDevice(helper.GetDevice().Id);
                var configurationInfo = new ConfigurationInfo();
                var properties = typeof(ConfigurationInfo).GetProperties();
                foreach (var property in properties)
                {
                    var attribute = property.GetCustomAttribute<PropertyConfigAttribute>();
                    if (!daliFlag && attribute != null && attribute.Group == "DALI Specific")
                    {
                        continue;
                    }
                    var val = ConfigurationInfos.FirstOrDefault(f => f.Key == property.Name)?.KeyValue?.Value ?? "0";
                    property.SetValue(configurationInfo, Convert.ToByte(val));
                }
                var re = await helper.RequestInformation(configurationInfo);
                if (!re)
                {
                    await new DialogService().DisplayAlertAsync("提示", "上传失败", "确定");
                    return;
                }
                await new DialogService().DisplayAlertAsync("提示", "上传成功", "确定");
                //SelectedLoggedBluetooth = null;
            }
            catch (Exception e)
            {
                await new DialogService().DisplayAlertAsync("错误", e.Message, "确定");
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
            loggedBluetooths = ((App)Application.Current).LoggedDevices.LoggedBluetooths;
            //初始化登录指令
            DisconnectDeviceCommand = new RelayCommand(DisconnectDevice);
            //初始化设备信息
            DeviceParameters = new ObservableCollection<ListViewModel>();
            InitDeviceParametersKeyValue(new DeviceParameters());
            //初始化配置信息列表
            ConfigurationInfos = new ObservableCollection<ListViewModel>();
            InitKeyValue(new ConfigurationInfo());
            //初始化配置命令
            ConfigurationButtonCommand = new RelayCommand<ListViewModel>(OpenConfigurationPopup);
            CommitConfigurationCommand = new RelayCommand(CommitConfiguration);
        }
    }
}
