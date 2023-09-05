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

        #region 扫描
        /// <summary>
        /// 扫描命令
        /// </summary>
        public ICommand StartScanCommand { get; }

        /// <summary>
        /// 可用设备
        /// </summary>
        private ObservableCollection<BluetoothItem> _useableBluetooths 
            = new ObservableCollection<BluetoothItem>();

        /// <summary>
        /// 可用设备
        /// </summary>
        public ObservableCollection<BluetoothItem> UseableBluetooths
        {
            get { return _useableBluetooths; }
            set
            {
                if (_useableBluetooths != value)
                {
                    SetProperty(ref _useableBluetooths, value);
                }
            }
        }

        /// <summary>
        /// 已选择的可用设备
        /// </summary>
        private BluetoothItem? _selectedUseableBluetooth;

        /// <summary>
        /// 已选择的可用设备
        /// </summary>
        public BluetoothItem? SelectedUseableBluetooth
        {
            get => _selectedUseableBluetooth;
            set
            {
                if (_selectedUseableBluetooth != value)
                {
                    SetProperty(ref _selectedUseableBluetooth, value);
                    foreach (var item in UseableBluetooths)
                    {
                        item.IsSelected = item.Id == value?.Id;
                    }
                    ConnectEnabled = _selectedUseableBluetooth != null;
                }
            }
        }

        /// <summary>
        /// 开始扫描
        /// </summary>
        public async void StartScan()
        {
            try
            {
                if (Ble.State != BluetoothState.On
                && Ble.State != BluetoothState.TurningOn)
                {
                    await new DialogService().DisplayAlertAsync("提示", "蓝牙未开启！", "OK");
                    return;
                }
                if (Adapter.IsScanning)
                {
                    await new DialogService().DisplayAlertAsync("提示", "正在扫描中！", "OK");
                    return;
                }
                UseableBluetooths.Clear();
                Adapter.DeviceDiscovered += (s, a) =>
                {
                    if (string.IsNullOrWhiteSpace(a.Device.Name))
                    {
                        return;
                    }
                    if (!_deviceDic.ContainsKey(a.Device.Id))
                    {
                        _deviceDic.TryAdd(a.Device.Id, a.Device);
                    }
                    var address = GetDeviceAddress(a.Device);
                    if (UseableBluetooths.All(r => r.Id != a.Device.Id))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            UseableBluetooths.Add(new BluetoothItem
                            {
                                Id = a.Device.Id,
                                Name = a.Device.Name,
                                Rssi = a.Device.Rssi,
                                Address = address
                            });
                        });
                    }
                };
                Adapter.ScanTimeout = 30000;
                Adapter.ScanMode = ScanMode.Balanced;
                Adapter.ScanMatchMode = ScanMatchMode.AGRESSIVE;
                Adapter.LowEnergyFlag = true;
                StartScanEnabled = false;
                await Adapter.StartScanningForDevicesAsync();
                StartScanEnabled = true;
            }
            catch (Exception e)
            {
                await new DialogService().DisplayAlertAsync("提示", e.Message, "OK");
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
        #endregion

        #region 连接
        /// <summary>
        /// 连接指令
        /// </summary>
        public ICommand ConnectToDeviceCommand { get; }

        /// <summary>
        /// 断连指令
        /// </summary>
        public ICommand DisconnectDeviceCommand { get; }

        private bool _defultPasswordFlag;

        public bool DefultPasswordFlag
        {
            get { return _defultPasswordFlag; }
            set
            {
                if (_defultPasswordFlag != value)
                {
                    SetProperty(ref _defultPasswordFlag, value);
                    NewDeviceEnabled = !_defultPasswordFlag;
                }
            }
        }

        private bool _newDeviceFlag;

        public bool NewDeviceFlag
        {
            get { return _newDeviceFlag; }
            set
            {
                if (_newDeviceFlag != value)
                {
                    SetProperty(ref _newDeviceFlag, value);
                    DefultPasswordEnabled = !_newDeviceFlag;
                }
            }
        }

        private CancellationTokenSource? _cancellationTokenSource;
        /// <summary>
        /// 连接并登录
        /// </summary>
        /// <param name="obj"></param>
        public async void ConnectToDevice()
        {
            try
            {
                if (SelectedUseableBluetooth != null)
                {
                    if (!_deviceDic.ContainsKey(SelectedUseableBluetooth.Id))
                    {
                        await new DialogService().DisplayAlertAsync("提示", "设备查找失败", "OK");
                        return;
                    }
                    var selectDevice = _deviceDic[SelectedUseableBluetooth.Id];
                    new DialogService().ShowLoadingModal();
                    await Adapter.ConnectToDeviceAsync(selectDevice);
                    if (!Adapter.ConnectedDevices.Any(r => r.Id == SelectedUseableBluetooth.Id))
                    {
                        new DialogService().HideLoadingModal();
                        await new DialogService().DisplayAlertAsync("提示", "设备连接失败", "OK");
                        return;
                    }
                    _cancellationTokenSource = new CancellationTokenSource();
                    CancellationToken cancellationToken = _cancellationTokenSource.Token;
                    await Task.Delay(10000, cancellationToken);
                    DisconnectDeviceNative(selectDevice);
                    await new DialogService().DisplayAlertAsync("提示", "登录超时已断连", "OK");
                    new DialogService().HideLoadingModal();
                }
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception e)
            {
                await new DialogService().DisplayAlertAsync("提示", e.Message, "OK");
                new DialogService().HideLoadingModal();
            }
        }

        /// <summary>
        /// 断连
        /// </summary>
        /// <param name="obj"></param>
        public async void DisconnectDevice()
        {
            try
            {
                if (SelectedLoggedBluetooth != null)
                {
                    if (!_deviceDic.ContainsKey(SelectedLoggedBluetooth.Id))
                    {
                        await new DialogService().DisplayAlertAsync("提示", "设备查找失败", "OK");
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
                            Address = GetDeviceAddress(device.NativeDevice)
                        });
                    }
                }

            }
            catch (Exception e)
            {
                await new DialogService().DisplayAlertAsync("提示", e.Message, "OK");
            }
        }

        private async void DisconnectDeviceNative(IDevice device)
        {
            await Adapter.DisconnectDeviceAsync(device);

            BLEDeviceHelper.RemoveBLEDeviceHelper(device.Id);

            if (Adapter.ConnectedDevices.Any(r => r.Id == device.Id))
            {
                await new DialogService().DisplayAlertAsync("提示", "设备断连失败", "OK");
            }
        }

        public async void OnDeviceActionParing(object? obj, DeviceEventArgs args)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
            if (SelectedUseableBluetooth == null)
            {
                return;
            }
            if (!_deviceDic.ContainsKey(SelectedUseableBluetooth.Id))
            {
                new DialogService().HideLoadingModal();
                await new DialogService().DisplayAlertAsync("提示", "设备查找失败", "OK");
                return;
            }
            var selectDevice = _deviceDic[SelectedUseableBluetooth.Id];
            var pin = PinHelper.GetBytePin(SelectedUseableBluetooth?.Address ?? "");
            selectDevice.SetPin(pin);
        }
        public async void OnDeviceBounded(object? obj, DeviceEventArgs args)
        {
            if (SelectedUseableBluetooth == null)
            {
                return;
            }
            if (!_deviceDic.ContainsKey(SelectedUseableBluetooth.Id))
            {
                new DialogService().HideLoadingModal();
                await new DialogService().DisplayAlertAsync("提示", "设备查找失败", "OK");
                return;
            }
            var selectDevice = _deviceDic[SelectedUseableBluetooth.Id];
            HandleLogin(selectDevice);
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

        /// <summary>
        /// 登录
        /// </summary>
        public async void HandleLogin(IDevice device)
        {
            var helper = BLEDeviceHelper.GetBLEDeviceHelper(device);
            var password = (DefultPasswordFlag || NewDeviceFlag) ? Common.DefultPassword : await new DialogService().DisplayPromptAsync("登录", "输入密码", "确认", "取消");
            if (string.IsNullOrWhiteSpace(password))
            {
                new DialogService().HideLoadingModal();
                DisconnectDeviceNative(device);
                await new DialogService().DisplayAlertAsync("提示", "登录失败已断连", "OK");
                return;
            }
            helper.ValueUpdated += LoginCallback;
            await Task.Run(() =>
            {
                Thread.Sleep(1000);
            });
            var re = await helper.AuthenticationRequest(password);
            if (!re)
            {
                new DialogService().HideLoadingModal();
                DisconnectDeviceNative(device);
                await new DialogService().DisplayAlertAsync("提示", "登录失败已断连", "OK");
                return;
            }
            TimeOutCancel(helper);
        }

        /// <summary>
        /// 超时断联
        /// </summary>
        /// <param name="helper"></param>
        private async void TimeOutCancel(BLEDeviceHelper helper)
        {
            try
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                CancellationToken cancellationToken = cancellationTokenSource.Token;
                helper.CancellationTokenSource = cancellationTokenSource;
                await Task.Delay(10000, cancellationToken);
                DisconnectDeviceNative(helper.GetDevice());
                await new DialogService().DisplayAlertAsync("提示", "登录超时已断连", "OK");
                new DialogService().HideLoadingModal();
                return;
            }
            catch (TaskCanceledException)
            {

            }
            catch
            {

            }
        }


        /// <summary>
        /// 登录反馈
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="message"></param>
        private async void LoginCallback(object? obj, ReceiveMessage message)
        {
            try
            {
                if (obj == null)
                {
                    return;
                }
                if (message.GetMessageType() == (byte)MessageType.AuthenticationGranted)
                {
                    var helper = obj as BLEDeviceHelper;
                    if (helper == null)
                    {
                        DisconnectDeviceNative(helper.GetDevice());
                        await new DialogService().DisplayAlertAsync("提示", "登录失败已断连", "OK");
                        new DialogService().HideLoadingModal();
                        return;
                    }
                    if (helper.CancellationTokenSource != null)
                    {
                        helper.CancellationTokenSource.Cancel();
                        helper.CancellationTokenSource.Dispose();
                    }
                    var re = (bool)(message.GetResult() ?? false);
                    if (!re)
                    {
                        DisconnectDeviceNative(helper.GetDevice());
                        await new DialogService().DisplayAlertAsync("提示", "登录失败已断连", "OK");
                        new DialogService().HideLoadingModal();
                        return;
                    }

                    LoggedBluetooths.Clear();
                    foreach (var device in Adapter.ConnectedDevices)
                    {
                        LoggedBluetooths.Add(new BluetoothItem
                        {
                            Id = device.Id,
                            Name = device.Name,
                            Rssi = device.Rssi,
                            Address = GetDeviceAddress(device.NativeDevice)
                        });
                    }

                    helper.ValueUpdated -= LoginCallback;
                    await Task.Run(async () =>
                    {
                        Thread.Sleep(1000);
                        await helper.DeviceParameters();
                    });

                    var temp = UseableBluetooths.FirstOrDefault(r => r.Id == helper.GetDevice()?.Id);
                    if (temp != null)
                    {
                        UseableBluetooths.Remove(temp);
                    }
                    // 设置新密码
                    if (NewDeviceFlag)
                    {
                        new DialogService().ShowModifyPasswordModal(new BluetoothItem
                        {
                            Id = helper.GetDevice().Id,
                            Name = helper.GetDevice().Name,
                            Rssi = helper.GetDevice().Rssi,
                            Address = GetDeviceAddress(helper.GetDevice().NativeDevice)
                        }, false);
                    }
                    new DialogService().HideLoadingModal();
                }
            }
            catch (Exception e)
            {
                await new DialogService().DisplayAlertAsync("提示", e.Message, "OK");
                new DialogService().HideLoadingModal();
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
            //初始化蓝牙
            Ble = CrossBluetoothLE.Current;
            Adapter = CrossBluetoothLE.Current.Adapter;
            Adapter.DeviceActionParing += OnDeviceActionParing;
            Adapter.DeviceBounded += OnDeviceBounded;
            //初始化集合
            UseableBluetooths = new ObservableCollection<BluetoothItem>();
            LoggedBluetooths = new ObservableCollection<BluetoothItem>();
            //初始化扫描指令
            StartScanCommand = new RelayCommand(StartScan);
            //初始化连接指令
            ConnectToDeviceCommand = new RelayCommand(ConnectToDevice);
            //初始化登录指令
            DisconnectDeviceCommand = new RelayCommand(DisconnectDevice);
        }
    }
}
