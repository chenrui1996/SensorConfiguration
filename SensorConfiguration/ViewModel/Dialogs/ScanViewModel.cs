using CommunityToolkit.Mvvm.ComponentModel;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using SensorConfiguration.Constant;
using SensorConfiguration.Helper.BLE.Messages;
using SensorConfiguration.Helper.BLE;
using SensorConfiguration.Models;
using SensorConfiguration.Services;
using SensorConfiguration.SharedData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using static SensorConfiguration.Constant.Enums;
using System.Windows;
using Plugin.BLE;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Concurrent;
using System.Windows.Controls;
using Windows.Devices.Enumeration;
using Plugin.BLE.Abstractions;

namespace SensorConfiguration.ViewModel.Dialogs
{
    public class ScanViewModel : ObservableRecipient
    {
        #region loading

        private Visibility _loadingFlag = Visibility.Collapsed;

        public Visibility LoadingFlag
        {
            get { return _loadingFlag; }
            set
            {
                if (_loadingFlag != value)
                {
                    SetProperty(ref _loadingFlag, value);
                }
            }
        }

        private void ShowLoadingModal()
        {
            LoadingFlag = Visibility.Visible;
        }

        private void HideLoadingModal()
        {
            LoadingFlag = Visibility.Collapsed;
        }
        #endregion

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
        #endregion

        #region 扫描
        /// <summary>
        /// 扫描命令
        /// </summary>
        public ICommand? StartScanCommand { get; }

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
                if (Ble == null || Adapter == null)
                {
                    new DialogService().DisplayAlert("提示", "蓝牙未开启！", "OK");
                    return;
                }
                if (Ble.State != BluetoothState.On
                && Ble.State != BluetoothState.TurningOn)
                {
                    new DialogService().DisplayAlert("提示", "蓝牙未开启！", "OK");
                    return;
                }
                StopScan();
                UseableBluetooths.Clear();
                Adapter.ScanTimeout = 30000;
                Adapter.ScanMode = ScanMode.Balanced;
                Adapter.ScanMatchMode = ScanMatchMode.AGRESSIVE;
                Adapter.LowEnergyFlag = true;
                StartScanEnabled = true;
                await Adapter.StartScanningForDevicesAsync();
                StartScanEnabled = true;
            }
            catch (Exception e)
            {
                App.ErrorLog.Error(e);
                new DialogService().DisplayAlert("提示", e.Message, "OK");
            }
        }

        private object _lockObj = new object();

        private void DeviceDiscoveredHandle(object? s, DeviceEventArgs? a)
        {
            lock (_lockObj)
            {
                if (string.IsNullOrWhiteSpace(a.Device.Name))
                {
                    return;
                }
                if (a.Device.Name.StartsWith("Bluetooth"))
                {
                    return;
                }
                if (_deviceDic.ContainsKey(a.Device.Id))
                {
                    _deviceDic.TryRemove(a.Device.Id, out _);
                }
                _deviceDic.TryAdd(a.Device.Id, a.Device);
                var address = GetDeviceAddress(a.Device);
                if (UseableBluetooths.All(r => r.Address != address))
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

        public void StopScan()
        {
            try
            {
                if (Ble == null || Adapter == null)
                {
                    new DialogService().DisplayAlert("提示", "蓝牙未开启！", "OK");
                    return;
                }
                if (Adapter.IsScanning)
                {
                    Adapter.StopScanningForDevicesAsync();
                }
            }
            catch(Exception e)
            {
                App.ErrorLog.Error(e);
            }
        }

        public void ClearEvents()
        {
            if (Ble == null || Adapter == null)
            {
                return;
            }
            Adapter.DeviceDiscovered -= DeviceDiscoveredHandle;
            Adapter.DeviceActionParing -= OnDeviceActionParing;
            Adapter.DeviceBounded -= OnDeviceBounded;
            Adapter = null;
        }
        #endregion

        #region 连接
        /// <summary>
        /// 连接指令
        /// </summary>
        public ICommand? ConnectToDeviceCommand { get; }

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

        /// <summary>
        /// 连接并登录
        /// </summary>
        /// <param name="obj"></param>
        public async void ConnectToDevice()
        {
            try
            {
                if (SelectedUseableBluetooth != null && Adapter != null)
                {
                    if (!_deviceDic.ContainsKey(SelectedUseableBluetooth.Id))
                    {
                        new DialogService().DisplayAlert("提示", "设备查找失败", "OK");
                        return;
                    }
                    var selectDevice = _deviceDic[SelectedUseableBluetooth.Id];
                    ShowLoadingModal();
                    await Adapter.ConnectToDeviceAsync(selectDevice);
                    if (!Adapter.ConnectedDevices.Any(r => r.Id == SelectedUseableBluetooth.Id))
                    {
                        HideLoadingModal();
                        new DialogService().DisplayAlert("提示", "设备连接失败", "OK");
                        return;
                    }
                    //HideLoadingModal();
                }
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception e)
            {
                App.ErrorLog.Error(e);
                new DialogService().DisplayAlert("提示", e.Message, "OK");
                HideLoadingModal();
            }
        }

        private async void DisconnectDeviceNative(IDevice device)
        {
            if (Ble == null || Adapter == null)
            {
                new DialogService().DisplayAlert("提示", "蓝牙未开启！", "OK");
                return;
            }
            await Adapter.DisconnectDeviceAsync(device);

            BLEDeviceHelper.RemoveBLEDeviceHelper(device.Id);

            if (Adapter.ConnectedDevices.Any(r => r.Id == device.Id))
            {
                new DialogService().DisplayAlert("提示", "设备断连失败", "OK");
            }
        }

        public void OnDeviceActionParing(object? sender, DevicePairingRequestedEventArgs args)
        {
            var pin = PinHelper.GetPin(SelectedUseableBluetooth?.Address ?? "");
            args.Accept(pin);
        }

        public void OnDeviceBounded(object? obj, DeviceEventArgs args)
        {
            try
            {
                if (args.Device == null)
                {
                    return;
                }
                if (!_deviceDic.ContainsKey(args.Device.Id))
                {
                    HideLoadingModal();
                    new DialogService().DisplayAlert("提示", "设备查找失败", "OK");
                    return;
                }
                var selectDevice = _deviceDic[args.Device.Id];
                HandleLogin(selectDevice);
            }
            catch (Exception e)
            {
                App.ErrorLog.Error(e);
                new DialogService().DisplayAlert("提示", e.Message, "OK");
                HideLoadingModal();
            }

        }
        #endregion

        #region 登录
        /// <summary>
        /// 登录
        /// </summary>
        public async void HandleLogin(IDevice device)
        {
            var helper = BLEDeviceHelper.GetBLEDeviceHelper(device);
            if (helper == null)
            {
                HideLoadingModal();
                DisconnectDeviceNative(device);
                new DialogService().DisplayAlert("提示", "通讯错误", "OK");
                return;
            }
            var password = (DefultPasswordFlag || NewDeviceFlag)
                ? Common.DefultPassword
                : new DialogService().ShowPasswordDialog();
            if (string.IsNullOrWhiteSpace(password))
            {
                HideLoadingModal();
                DisconnectDeviceNative(device);
                new DialogService().DisplayAlert("提示", "密码为空", "OK");
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
                HideLoadingModal();
                DisconnectDeviceNative(device);
                new DialogService().DisplayAlert("提示", "登录失败已断连", "OK");
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
                await Task.Delay(30000, cancellationToken);
                DisconnectDeviceNative(helper.GetDevice());
                new DialogService().DisplayAlert("提示", "登录超时已断连", "OK");
                HideLoadingModal();
                return;
            }
            catch (TaskCanceledException)
            {

            }
            catch(Exception e)
            {
                App.ErrorLog.Error(e);
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
                if (Adapter == null)
                {
                    return;
                }
                if (message.GetMessageType() == (byte)MessageType.AuthenticationGranted)
                {
                    var helper = obj as BLEDeviceHelper;
                    if (helper == null)
                    {
                        DisconnectDeviceNative(helper.GetDevice());
                        new DialogService().DisplayAlert("提示", "登录失败已断连", "OK");
                        HideLoadingModal();
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
                        new DialogService().DisplayAlert("提示", "登录失败已断连", "OK");
                        HideLoadingModal();
                        return;
                    }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var loggedBluetooths = ((App)Application.Current).LoggedDevices.LoggedBluetooths;
                        loggedBluetooths.Clear();
                        foreach (var device in Adapter.ConnectedDevices)
                        {
                            loggedBluetooths.Add(new BluetoothItem
                            {
                                Id = device.Id,
                                Name = device.Name,
                                Rssi = device.Rssi,
                                Address = GetDeviceAddress(device)
                            });
                        }
                    });
                    helper.ValueUpdated -= LoginCallback;
                    await Task.Run(async () =>
                    {
                        Thread.Sleep(1000);
                        await helper.DeviceParameters();
                    });
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var temp = UseableBluetooths.FirstOrDefault(r => r.Id == helper.GetDevice()?.Id);
                        if (temp != null)
                        {
                            UseableBluetooths.Remove(temp);
                        }
                    });
                    // 设置新密码
                    if (NewDeviceFlag)
                    {
                        //new DialogService().ShowModifyPasswordModal(new BluetoothItem
                        //{
                        //    Id = helper.GetDevice().Id,
                        //    Name = helper.GetDevice().Name,
                        //    Rssi = helper.GetDevice().Rssi,
                        //    Address = GetDeviceAddress(helper.GetDevice().NativeDevice)
                        //}, false);
                    }
                    HideLoadingModal();
                }
            }
            catch (Exception e)
            {
                App.ErrorLog.Error(e);
                new DialogService().DisplayAlert("提示", e.Message, "OK");
                HideLoadingModal();
            }
        }
        #endregion

        public ScanViewModel()
        {
            try
            {
                StartScanEnabled = true;
                DefultPasswordEnabled = true;
                NewDeviceEnabled = true;
                //初始化蓝牙
                Ble = CrossBluetoothLE.Current;
                Adapter = CrossBluetoothLE.Current.Adapter;

                Adapter.DeviceDiscovered -= DeviceDiscoveredHandle;
                Adapter.DeviceActionParing -= OnDeviceActionParing;
                Adapter.DeviceBounded -= OnDeviceBounded;

                Adapter.DeviceDiscovered += DeviceDiscoveredHandle;
                Adapter.DeviceActionParing += OnDeviceActionParing;
                Adapter.DeviceBounded += OnDeviceBounded;

                //初始化扫描指令
                StartScanCommand = new RelayCommand(StartScan);
                //初始化连接指令
                ConnectToDeviceCommand = new RelayCommand(ConnectToDevice);
            }
            catch(Exception e)
            {
                App.ErrorLog.Error(e);
            }
        }
    }
}
