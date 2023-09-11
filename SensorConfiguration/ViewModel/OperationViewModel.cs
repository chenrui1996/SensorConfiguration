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
        IBluetoothLE? Ble;

        /// <summary>
        /// 蓝牙适配器
        /// </summary>
        IAdapter? Adapter;

        /// <summary>
        /// 设备信息
        /// </summary>
        private ConcurrentDictionary<Guid, IDevice> _deviceDic = DeviceInfo.DeviceDic;
        #endregion

        #region UI
        public void ExecuteItemSelected(object? selectedItem)
        {
            if (selectedItem == null)
            {
                return;
            }
            var item = (BluetoothItem)selectedItem;
            if (SelectedLoggedBluetooth == item)
            {
                SelectedLoggedBluetooth = null;
            }
            else
            {
                SelectedLoggedBluetooth = item;
            }
        }

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
                    if (Adapter == null)
                    {
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
                App.ErrorLog.Error(e);
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
            catch(Exception e)
            {
                App.ErrorLog.Error(e);
                return "";
            }

        }

        private async void DisconnectDeviceNative(IDevice device)
        {
            if (Adapter == null)
            {
                return;
            }

            await Adapter.DisconnectDeviceAsync(device);

            BLEDeviceHelper.RemoveBLEDeviceHelper(device.Id);

            if (Adapter.ConnectedDevices.Any(r => r.Id == device.Id))
            {
                new DialogService().DisplayAlert("提示", "设备断连失败", "OK");
            }
        }
        #endregion

        #region 登录
        private ObservableCollection<BluetoothItem>? loggedBluetooths;

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
                    DisableAutoNotification(selectedLoggedBluetooth);

                    SetProperty(ref selectedLoggedBluetooth, value);
                    foreach (var item in LoggedBluetooths)
                    {
                        item.IsSelected = item.Id == value?.Id;
                    }

                    DisconnectEnabled = selectedLoggedBluetooth != null;
                    HandleDeviceParameters(selectedLoggedBluetooth);
                    EnableAutoNotification(selectedLoggedBluetooth);
                }
            }
        }
        #endregion

        #region 设备信息
        /// <summary>
        /// 设备信息
        /// </summary>
        private ConcurrentDictionary<Guid, DeviceParameters> _deviceParameterDic = DeviceInfo.DeviceParameterDic;

        private ConcurrentDictionary<Guid, string> _devicePasswordDic = DeviceInfo.DevicePasswordDic;

        private void HandleDeviceParameters(BluetoothItem? selectedLoggedBluetooth)
        {
            if (selectedLoggedBluetooth == null)
            {
                Password = "";
                InitDeviceParametersKeyValue(new DeviceParameters());
                return;
            }
            if(!_deviceParameterDic.ContainsKey(selectedLoggedBluetooth.Id))
            {
                InitDeviceParametersKeyValue(new DeviceParameters());
                return;
            }
            var deviceParameter = _deviceParameterDic[selectedLoggedBluetooth.Id];
            InitDeviceParametersKeyValue(deviceParameter);

            if (!_devicePasswordDic.ContainsKey(selectedLoggedBluetooth.Id))
            {
                Password = "";
                return;
            }
            Password = _devicePasswordDic[selectedLoggedBluetooth.Id];
        }

        /// <summary>
        /// 初始化配置信息列表
        /// </summary>
        /// <param name="configurationInfo"></param>
        /// <param name="daliFlag"></param>
        private void InitDeviceParametersKeyValue(DeviceParameters deviceParameters)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (deviceParameters == null)
                {
                    return;
                }
                var properties = typeof(DeviceParameters).GetProperties();
                foreach (var property in properties)
                {
                    var deviceParameter = DeviceParameters.FirstOrDefault(r => r.Key == property.Name);
                    if (deviceParameter != null)
                    {
                        deviceParameter.KeyValue = new KeyValue
                        {
                            Key = property.Name,
                            Value = property.GetValue(deviceParameters)?.ToString()
                        };
                        continue;
                    }
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
            });
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

        private string? _deviceName;

        public string DeviceName
        {
            get { return _deviceName; }
            set
            {
                SetProperty(ref _deviceName, value);
            }
        }

        private string? _password;

        public string Password
        {
            get { return _password; }
            set
            {
                SetProperty(ref _password, value);
            }
        }

        #endregion

        #region 配置信息
        /// <summary>
        /// 初始化配置信息列表
        /// </summary>
        /// <param name="configurationInfo"></param>
        /// <param name="daliFlag"></param>
        private void InitConfigurationKeyValue(ConfigurationInfo configurationInfo, bool daliFlag = false)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (configurationInfo == null)
                {
                    return;
                }

                var properties = typeof(ConfigurationInfo).GetProperties();
                foreach (var property in properties)
                {
                    var attribute = property.GetCustomAttribute<PropertyConfigAttribute>();
                    if (!daliFlag && attribute != null && attribute.Group == "DALI Specific")
                    {
                        continue;
                    }
                    var configuration = ConfigurationInfos.FirstOrDefault(r => r.Key == property.Name);
                    if (configuration != null)
                    {
                        configuration.KeyValue = new KeyValue
                        {
                            Key = property.Name,
                            DaliFlag = daliFlag,
                            Value = property.GetValue(configurationInfo)?.ToString()
                        };
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
            });
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

        #region 连续测光配置
        /// <summary>
        /// 初始化配置信息列表
        /// </summary>
        /// <param name="configurationInfo"></param>
        /// <param name="daliFlag"></param>
        private void InitContinuousDimmingKeyValue(ContinuousDimmingConfiguration continuousDimmingConfig)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (continuousDimmingConfig == null)
                {
                    return;
                }
                EnableStatus = continuousDimmingConfig.EnableStatus == 1;
                DayConfigurations.Clear();
                NightConfigurations.Clear();

                var properties = typeof(ContinuousDimmingConfiguration).GetProperties();
                foreach (var property in properties)
                {
                    var attribute = property.GetCustomAttribute<PropertyConfigAttribute>();
                    if (attribute == null)
                    {
                        continue;
                    }
                    if (!attribute.IsShow)
                    {
                        continue;
                    }
                    var listViewModel = new ListViewModel
                    {
                        Key = property.Name,
                        KeyValue = new KeyValue
                        {
                            Key = property.Name,
                            Value = property.GetValue(continuousDimmingConfig)?.ToString()
                        }
                    };
                    if (attribute.Group == "Day")
                    {
                        var configuration = DayConfigurations.FirstOrDefault(r => r.Key == property.Name);
                        if (configuration != null)
                        {
                            configuration.KeyValue = listViewModel.KeyValue;
                            continue;
                        }
                        DayConfigurations.Add(listViewModel);
                    }
                    if (attribute.Group == "Night")
                    {
                        var configuration = DayConfigurations.FirstOrDefault(r => r.Key == property.Name);
                        if (configuration != null)
                        {
                            configuration.KeyValue = listViewModel.KeyValue;
                            continue;
                        }
                        NightConfigurations.Add(listViewModel);
                    }
                }
            });
        }

        private ObservableCollection<ListViewModel>? dayConfigurations;

        public ObservableCollection<ListViewModel> DayConfigurations
        {
            get { return dayConfigurations; }
            set
            {
                if (dayConfigurations != value)
                {
                    SetProperty(ref dayConfigurations, value);
                }
            }
        }

        private ObservableCollection<ListViewModel>? nightConfigurations;

        public ObservableCollection<ListViewModel> NightConfigurations
        {
            get { return nightConfigurations; }
            set
            {
                if (nightConfigurations != value)
                {
                    SetProperty(ref nightConfigurations, value);
                }
            }
        }

        private bool enableStatus;

        public bool EnableStatus
        {
            get { return enableStatus; }
            set
            {
                if (enableStatus != value)
                {
                    SetProperty(ref enableStatus, value);
                }
            }
        }

        /// <summary>
        /// 打开配置页面
        /// </summary>
        public ICommand ContinuousDimmingConfigurationButtonCommand { get; }

        /// <summary>
        /// 打开配置页面
        /// </summary>
        /// <param name="listViewModel"></param>
        private async void OpenContinuousDimmingConfigurationPopup(ListViewModel? listViewModel)
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
                new DialogService().ShowConfiguationModal(listViewModel, typeof(ContinuousDimmingConfiguration));
            }
            catch (Exception e)
            {
                await new DialogService().DisplayAlertAsync("错误", e.Message, "确定");
            }
        }

        public ICommand CommitContinuousDimmingConfigurationCommand { get; }

        private async void CommitContinuousDimmingConfiguration()
        {
            if (SelectedLoggedBluetooth == null)
            {
                return;
            }
            try
            {
                if (!_deviceDic.ContainsKey(SelectedLoggedBluetooth.Id))
                {
                    await new DialogService().DisplayAlertAsync("提示", "设备查找失败", "OK");
                    return;
                }
                var selectDevice = _deviceDic[SelectedLoggedBluetooth.Id];
                var helper = BLEDeviceHelper.GetBLEDeviceHelper(selectDevice);
                var configurationInfo = new ContinuousDimmingConfiguration();
                if (!EnableStatus)
                {
                    configurationInfo.EnableStatus = 0;
                    var re1 = await helper.SetContinuousDimmingConfiguration(configurationInfo);
                    if (!re1)
                    {
                        await new DialogService().DisplayAlertAsync("提示", "上传失败", "确定");
                        return;
                    }
                    await new DialogService().DisplayAlertAsync("提示", "上传成功", "确定");
                    new DialogService().BackBeforPage();
                    return;
                }
                configurationInfo.EnableStatus = 1;
                var properties = typeof(ContinuousDimmingConfiguration).GetProperties();
                foreach (var property in properties)
                {
                    var attribute = property.GetCustomAttribute<PropertyConfigAttribute>();
                    if (attribute == null)
                    {
                        continue;
                    }
                    if (!attribute.IsShow)
                    {
                        continue;
                    }
                    if (attribute.Group == "Day")
                    {
                        var val = DayConfigurations.FirstOrDefault(f => f.Key == property.Name)?.KeyValue?.Value ?? "0";
                        property.SetValue(configurationInfo, Convert.ToByte(val));
                        continue;
                    }
                    if (attribute.Group == "Night")
                    {
                        var val = NightConfigurations.FirstOrDefault(f => f.Key == property.Name)?.KeyValue?.Value ?? "0";
                        property.SetValue(configurationInfo, Convert.ToByte(val));
                        continue;
                    }
                }
                var re = await helper.SetContinuousDimmingConfiguration(configurationInfo);
                if (!re)
                {
                    await new DialogService().DisplayAlertAsync("提示", "上传失败", "确定");
                    return;
                }
                await new DialogService().DisplayAlertAsync("提示", "上传成功", "确定");
                new DialogService().BackBeforPage();
            }
            catch (Exception e)
            {
                await new DialogService().DisplayAlertAsync("错误", e.Message, "确定");
            }
        }
        #endregion

        #region 控制
        private void InitControlInfosKeyValue()
        {
            ControlInfos = new ObservableCollection<ListViewModel>
            {
                new ListViewModel
                {
                    Key = "Test Mode",
                    KeyValue = new KeyValue
                    {
                        Key = "Test Mode",
                        Value = "OFF"
                    }
                },

                new ListViewModel
                {
                    Key = "LoadLevel",
                    KeyValue = new KeyValue
                    {
                        Key = "LoadLevel",
                        Value = "0"
                    }
                },

                new ListViewModel
                {
                    Key = "LightSensorLevel",
                    KeyValue = new KeyValue
                    {
                        Key = "LightSensorLevel",
                        Value = "0"
                    }
                }
            };
        }

        private ObservableCollection<ListViewModel>? controlInfos;

        public ObservableCollection<ListViewModel> ControlInfos
        {
            get { return controlInfos; }
            set
            {
                if (controlInfos != value)
                {
                    SetProperty(ref controlInfos, value);
                }
            }
        }

        /// <summary>
        /// 打开配置页面
        /// </summary>
        public ICommand ControlButtonCommand { get; }

        /// <summary>
        /// 打开配置页面
        /// </summary>
        /// <param name="listViewModel"></param>
        private async void OpenControlPopup(ListViewModel? listViewModel)
        {
            try
            {
                if (listViewModel == null)
                {
                    return;
                }
                if (listViewModel.KeyValue == null)
                {
                    return;
                }
                if (SelectedLoggedBluetooth == null)
                {
                    return;
                }
                switch (listViewModel.Key)
                {
                    case "Test Mode":
                        new DialogService().ShowTestModeModal(SelectedLoggedBluetooth);
                        break;
                    case "LoadLevel":
                        if (listViewModel.KeyValue.Value == "0")
                        {
                            break;
                        }
                        new DialogService().ShowDimmerLevelModal(SelectedLoggedBluetooth, listViewModel);
                        break;
                }

            }
            catch (Exception e)
            {
                await new DialogService().DisplayAlertAsync("错误", e.Message, "确定");
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void UpdateControlInfoKeyValue(string key, string value)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var keyValue = ControlInfos.FirstOrDefault(r => r.Key == key);
                if (keyValue == null)
                {
                    return;
                }
                keyValue.KeyValue = new KeyValue
                {
                    Key = key,
                    Value = value
                };
            });
        }
        #endregion

        #region 获取信息
        /// <summary>
        /// 开启自动通知
        /// </summary>
        /// <param name="bluetoothItem"></param>
        private async void EnableAutoNotification(BluetoothItem? bluetoothItem)
        {
            try
            {
                if (bluetoothItem == null)
                {
                    DeviceName = "";
                    UpdateControlInfoKeyValue("LoadLevel", "0");
                    UpdateControlInfoKeyValue("LightSensorLevel", "0");
                    InitConfigurationKeyValue(new ConfigurationInfo());
                    InitContinuousDimmingKeyValue(new ContinuousDimmingConfiguration());
                    return;
                }
                if (!_deviceDic.ContainsKey(bluetoothItem.Id))
                {
                    await new DialogService().DisplayAlertAsync("提示", "设备查找失败", "OK");
                    DeviceName = "";
                    UpdateControlInfoKeyValue("LoadLevel", "0");
                    UpdateControlInfoKeyValue("LightSensorLevel", "0");
                    InitConfigurationKeyValue(new ConfigurationInfo());
                    InitContinuousDimmingKeyValue(new ContinuousDimmingConfiguration());
                    return;
                }
                var selectDevice = _deviceDic[bluetoothItem.Id];
               var helper = BLEDeviceHelper.GetBLEDeviceHelper(selectDevice);
                if (helper == null)
                {
                    await new DialogService().DisplayAlertAsync("提示", "设备查找失败", "OK");
                    DeviceName = "";
                    UpdateControlInfoKeyValue("LoadLevel", "0");
                    UpdateControlInfoKeyValue("LightSensorLevel", "0");
                    InitConfigurationKeyValue(new ConfigurationInfo());
                    InitContinuousDimmingKeyValue(new ContinuousDimmingConfiguration());
                    return;
                }

                helper.ValueUpdated -= AutoNotifyCallback;
                helper.RemainderUpdated -= RemainderUpdatedCallback;

                helper.ValueUpdated += AutoNotifyCallback;
                helper.RemainderUpdated += RemainderUpdatedCallback;

                var getDeviceNameResult = await helper.GetDeviceName();
                if (!getDeviceNameResult)
                {
                    await new DialogService().DisplayAlertAsync("提示", "设备名称获取失败", "OK");
                }
                var getConfigResult = await helper.RequestInformation();
                if (!getConfigResult)
                {
                    await new DialogService().DisplayAlertAsync("提示", "设备配置获取失败", "OK");
                }
                var getCDConfigResult = await helper.GetContinuousDimmingConfiguration();
                if (!getCDConfigResult)
                {
                    await new DialogService().DisplayAlertAsync("提示", "持续灯光配置获取失败", "OK");
                }
                var re = await helper.EnableAutoNotification();
                if (!re)
                {
                    await new DialogService().DisplayAlertAsync("提示", "开启通知失败", "OK");
                }
            }
            catch (Exception e)
            {
                App.ErrorLog.Error(e);
                await new DialogService().DisplayAlertAsync("提示", e.Message, "OK");
                DeviceName = "";
                UpdateControlInfoKeyValue("LoadLevel", "0");
                UpdateControlInfoKeyValue("LightSensorLevel", "0");
                InitConfigurationKeyValue(new ConfigurationInfo());
                InitContinuousDimmingKeyValue(new ContinuousDimmingConfiguration());
                return;
            }
        }

        /// <summary>
        /// 自动通知反馈
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="message"></param>
        private void AutoNotifyCallback(object? obj, ReceiveMessage message)
        {
            try
            {
                if (obj == null)
                {
                    return;
                }
                var helper = obj as BLEDeviceHelper;
                if (helper == null)
                {
                    return;
                }
                if (DeviceInfo.IsFDPDevice(helper.GetDevice().Id))
                {
                    if (message.GetMessageType() == 0x16)
                    {
                        var configurationInfo = message.GetResult() as ConfigurationInfo;
                        if (configurationInfo == null)
                        {
                            return;
                        }
                        InitConfigurationKeyValue(configurationInfo, true);
                    }
                    return;
                }
                switch (message.GetMessageType())
                {
                    case (byte)MessageType.DeviceName:
                        DeviceName = (string)(message.GetResult() ?? "");
                        break;
                    case (byte)MessageType.ConfigurationInfo:
                        var configurationInfo = message.GetResult() as ConfigurationInfo;
                        if (configurationInfo == null)
                        {
                            break;
                        }
                        InitConfigurationKeyValue(configurationInfo);
                        break;
                    case (byte)MessageType.ContinuousDimmingConfiguration:
                        var continuousDimmingConfiguration = message.GetResult() as ContinuousDimmingConfiguration;
                        if (continuousDimmingConfiguration == null)
                        {
                            break;
                        }
                        InitContinuousDimmingKeyValue(continuousDimmingConfiguration);
                        break;
                    case (byte)MessageType.LoadLevel:
                        UpdateControlInfoKeyValue("LoadLevel", ((byte)(message.GetResult() ?? "0")).ToString());
                        break;
                    case (byte)MessageType.LightSensorLevel:
                        UpdateControlInfoKeyValue("LightSensorLevel", ((ushort)(message.GetResult() ?? "0")).ToString());
                        break;
                }
            }
            catch (Exception e)
            {
                App.ErrorLog.Error(e);
                return;
            }
        }

        /// <summary>
        /// 关闭自动通知
        /// </summary>
        /// <param name="bluetoothItem"></param>
        private async void DisableAutoNotification(BluetoothItem? bluetoothItem)
        {
            if (bluetoothItem == null)
            {
                return;
            }
            BLEDeviceHelper? helper = null;
            try
            {
                if (!_deviceDic.ContainsKey(bluetoothItem.Id))
                {
                    await new DialogService().DisplayAlertAsync("提示", "设备查找失败", "OK");
                }
                var selectDevice = _deviceDic[bluetoothItem.Id];
                helper = BLEDeviceHelper.GetBLEDeviceHelper(selectDevice);
                helper.ValueUpdated -= AutoNotifyCallback;
                helper.RemainderUpdated -= RemainderUpdatedCallback;
                var re = await helper.DisableAutoNotification();
                if (!re)
                {
                    await new DialogService().DisplayAlertAsync("提示", "关闭通知失败", "OK");
                }
                UpdateControlInfoKeyValue("LoadLevel", "0");
                UpdateControlInfoKeyValue("LightSensorLevel", "0");
            }
            catch (Exception e)
            {
                await new DialogService().DisplayAlertAsync("提示", e.Message, "OK");
            }
        }

        /// <summary>
        /// 更新测试模式剩余时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        private void RemainderUpdatedCallback(object? sender, RemainderEventArgs arg)
        {
            if (arg.DeviceId == selectedLoggedBluetooth?.Id)
            {
                if (arg.Remainder == 0)
                {
                    UpdateControlInfoKeyValue("Test Mode", " off");
                    return;
                }
                UpdateControlInfoKeyValue("Test Mode", arg.Remainder + " Seconds");
            }
        }
        #endregion

        public OperationViewModel()
        {
            try
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
                DeviceName = "";
                Password = "";
                DeviceParameters = new ObservableCollection<ListViewModel>();
                InitDeviceParametersKeyValue(new DeviceParameters());
                //初始化配置信息列表
                ConfigurationInfos = new ObservableCollection<ListViewModel>();
                InitConfigurationKeyValue(new ConfigurationInfo());
                //初始化配置命令
                ConfigurationButtonCommand = new RelayCommand<ListViewModel>(OpenConfigurationPopup);
                CommitConfigurationCommand = new RelayCommand(CommitConfiguration);
                //初始化持续调光配置信息列表
                DayConfigurations = new ObservableCollection<ListViewModel>();
                NightConfigurations = new ObservableCollection<ListViewModel>();
                InitContinuousDimmingKeyValue(new ContinuousDimmingConfiguration());
                //初始化持续调光配置命令
                ContinuousDimmingConfigurationButtonCommand = new RelayCommand<ListViewModel>(OpenContinuousDimmingConfigurationPopup);
                CommitContinuousDimmingConfigurationCommand = new RelayCommand(CommitContinuousDimmingConfiguration);
                //初始化控制列表
                InitControlInfosKeyValue();
                //初始化控制命令
                ControlButtonCommand = new RelayCommand<ListViewModel>(OpenControlPopup);
            }
            catch (Exception e)
            {
                App.ErrorLog.Error(e);
            }
        }
    }
}
