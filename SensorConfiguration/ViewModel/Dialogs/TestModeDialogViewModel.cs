using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.BLE.Abstractions.Contracts;
using SensorConfiguration.Helper.BLE;
using SensorConfiguration.Models;
using SensorConfiguration.Services;
using SensorConfiguration.SharedData;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SensorConfiguration.ViewModel.Dialogs
{
    public class TestModeDialogViewModel : ObservableRecipient
    {
        #region 设备信息
        /// <summary>
        /// 设备信息
        /// </summary>
        private ConcurrentDictionary<Guid, IDevice> _deviceDic = DeviceInfo.DeviceDic;

        private BluetoothItem _selectedLoggedBluetooth;
        #endregion

        #region 属性
        private int testSeconds;

        /// <summary>
        /// 属性值
        /// </summary>
        public int TestSeconds
        {
            get { return testSeconds; }
            set
            {
                if (testSeconds != value)
                {
                    SetProperty(ref testSeconds, value);

                }
            }
        }

        private int remainders;

        /// <summary>
        /// 属性值
        /// </summary>
        public int Remainders
        {
            get 
            {
                return remainders;
            }
            set
            {
                if (remainders != value)
                {
                    SetProperty(ref remainders, value);
                }
            }
        }
        #endregion

        #region 指令
        private BLEDeviceHelper? helper;

        private void InitBLEDeviceHelper()
        {
            try
            {
                if (!_deviceDic.ContainsKey(_selectedLoggedBluetooth.Id))
                {
                    return;
                }
                var selectDevice = _deviceDic[_selectedLoggedBluetooth.Id];
                helper = BLEDeviceHelper.GetBLEDeviceHelper(selectDevice);
                helper.RemainderUpdated += RemainderUpdatedCallback;
            }
            catch
            {
                if (helper != null)
                {
                    helper.RemainderUpdated -= RemainderUpdatedCallback;
                }
            }
        }

        /// <summary>
        /// 加
        /// </summary>
        public ICommand AddTestSecondsCommand { get; }

        private async void AddProperty()
        {
            await Task.Run(() =>
            {
                if (TestSeconds < 300)
                {
                    TestSeconds++;
                }
            });
        }
        /// <summary>
        /// 减
        /// </summary>
        public ICommand SubTestSecondsCommand { get; }

        private async void SubProperty()
        {
            await Task.Run(() =>
            {
                if (TestSeconds > 30)
                {
                    TestSeconds--;
                }
            });
        }

        public ICommand StartTestModeCommand { get; }

        private async void StartTestMode()
        {
            try
            {
                if (helper == null)
                {
                    return;
                }
                var re = await helper.TestMode(TestSeconds);
                if (!re)
                {
                    await new DialogService().DisplayAlertAsync("提示", "操作失败", "OK");
                }
            }
            catch (Exception e)
            {
                await new DialogService().DisplayAlertAsync("提示", e.Message, "OK");
            }
        }

        private void RemainderUpdatedCallback(object? sender, RemainderEventArgs arg)
        {
            if (sender == null)
            {
                return;
            }
            if (arg.DeviceId == _selectedLoggedBluetooth?.Id)
            {
                Remainders = arg.Remainder;
            }
        }

        public ICommand StopTestModeCommand { get; }

        private async void StopTestMode()
        {
            try
            {
                if (helper == null)
                {
                    return;
                }
                var re = await helper.TestMode(0);
                if (!re)
                {
                    await new DialogService().DisplayAlertAsync("提示", "操作失败", "OK");
                }
            }
            catch (Exception e)
            {
                await new DialogService().DisplayAlertAsync("提示", e.Message, "OK");
            }
        }
        #endregion

        public TestModeDialogViewModel(BluetoothItem selectedBluetoothItem)
        {
            _selectedLoggedBluetooth = selectedBluetoothItem;
            Remainders = 0;
            TestSeconds = 30;
            InitBLEDeviceHelper();
            AddTestSecondsCommand = new RelayCommand(AddProperty);
            SubTestSecondsCommand = new RelayCommand(SubProperty);
            StartTestModeCommand = new RelayCommand(StartTestMode);
            StopTestModeCommand = new RelayCommand(StopTestMode);
        }

    }
}
