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
using System.Windows;
using System.Windows.Input;

namespace SensorConfiguration.ViewModel.Dialogs
{
    public class DimmerLeveDialogViewModel : ObservableRecipient
    {
        #region 设备信息
        /// <summary>
        /// 设备信息
        /// </summary>
        private ConcurrentDictionary<Guid, IDevice> _deviceDic = DeviceInfo.DeviceDic;

        private BluetoothItem _selectedLoggedBluetooth;
        #endregion

        #region 属性
        private int dimmerLevel;

        /// <summary>
        /// 属性值
        /// </summary>
        public int DimmerLevel
        {
            get { return dimmerLevel; }
            set
            {
                if (dimmerLevel != value)
                {
                    SetProperty(ref dimmerLevel, value);

                }
            }
        }

        private int flashTime;

        /// <summary>
        /// 属性值
        /// </summary>
        public int FlashTime
        {
            get 
            {
                return flashTime;
            }
            set
            {
                if (flashTime != value)
                {
                    SetProperty(ref flashTime, value);
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
            }
            catch
            {

            }
        }

        /// <summary>
        /// 加
        /// </summary>
        public ICommand AddDimmerLevelCommand { get; }

        private async void AddDimmerLevel()
        {
            await Task.Run(() =>
            {
                if (DimmerLevel < 100)
                {
                    DimmerLevel++;
                }
            });
        }
        /// <summary>
        /// 减
        /// </summary>
        public ICommand SubDimmerLevelCommand { get; }

        private async void SubDimmerLevel()
        {
            await Task.Run(() =>
            {
                if (DimmerLevel > 1)
                {
                    DimmerLevel--;
                }
            });
        }

        /// <summary>
        /// 加
        /// </summary>
        public ICommand AddFlashTimeCommand { get; }

        private async void AddFlashTime()
        {
            await Task.Run(() =>
            {
                if (FlashTime < 10)
                {
                    FlashTime++;
                }
            });
        }
        /// <summary>
        /// 减
        /// </summary>
        public ICommand SubFlashTimeCommand { get; }

        private async void SubFlashTime()
        {
            await Task.Run(() =>
            {
                if (FlashTime > 1)
                {
                    FlashTime--;
                }
            });
        }

        public ICommand CommitDimmerLevelCommand { get; }

        private async void CommitDimmerLevel(object? o)
        {
            try
            {
                if (helper == null)
                {
                    return;
                }
                var re = await helper.SetDimmerLevel((byte)DimmerLevel);
                if (!re)
                {
                    await new DialogService().DisplayAlertAsync("提示", "操作失败", "OK");
                }
                if (o == null)
                {
                    return;
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    (o as Window)?.Close();
                });
            }
            catch (Exception e)
            {
                await new DialogService().DisplayAlertAsync("提示", e.Message, "OK");
            }
        }

        public ICommand CancelDimmerLevelCommand { get; }

        private async void CancelDimmerLevel(object? o)
        {
            try
            {
                if (o == null)
                {
                    return;
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    (o as Window)?.Close();
                });
            }
            catch (Exception e)
            {
                await new DialogService().DisplayAlertAsync("提示", e.Message, "OK");
            }
        }

        public ICommand TestDimmerLevelCommand { get; }

        private async void TestDimmerLevel()
        {
            try
            {
                if (helper == null)
                {
                    return;
                }
                var re = await helper.LoadFlash((byte)FlashTime);
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

        public DimmerLeveDialogViewModel(BluetoothItem selectedBluetoothItem, ListViewModel listViewModel)
        {
            _selectedLoggedBluetooth = selectedBluetoothItem;
            if (listViewModel.KeyValue == null)
            {
                DimmerLevel = 100;
            }
            else
            {
                DimmerLevel = Convert.ToInt32(listViewModel.KeyValue.Value);
            }
            FlashTime = 1;
            InitBLEDeviceHelper();
            AddDimmerLevelCommand = new RelayCommand(AddDimmerLevel);
            SubDimmerLevelCommand = new RelayCommand(SubDimmerLevel);
            AddFlashTimeCommand = new RelayCommand(AddFlashTime);
            SubFlashTimeCommand = new RelayCommand(SubFlashTime);
            CommitDimmerLevelCommand = new RelayCommand<object>(CommitDimmerLevel);
            CancelDimmerLevelCommand = new RelayCommand<object>(CancelDimmerLevel);
            TestDimmerLevelCommand = new RelayCommand(TestDimmerLevel);
        }

    }
}
