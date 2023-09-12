using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;

using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Extensions;
using Microsoft.Toolkit.Uwp.Connectivity;
using Windows.System;
using Windows.Devices.Enumeration;

namespace Plugin.BLE.UWP
{
    public class Adapter : AdapterBase
    {
        private BluetoothLEHelper? _bluetoothHelper;
        private BluetoothLEAdvertisementWatcher _bleWatcher;
        private DispatcherQueue _dq;
        public Adapter()
        {
            Trace.Message("Starting Adapter.");
            _dq = DispatcherQueue.GetForCurrentThread();
            _bleWatcher = new BluetoothLEAdvertisementWatcher();
            //_bleWatcher.Received += DeviceFoundAsync;
        }

        public Adapter(BluetoothLEHelper bluetoothHelper)
        {
            Trace.Message("Starting Adapter.");
            _bluetoothHelper = bluetoothHelper;
            _dq = DispatcherQueue.GetForCurrentThread();
            _bleWatcher = new BluetoothLEAdvertisementWatcher();
            //_bleWatcher.Received += DeviceFoundAsync;
        }

        protected override Task StartScanningForDevicesNativeAsync(ScanFilterOptions scanFilterOptions, bool allowDuplicatesKey, CancellationToken scanCancellationToken)
        {
            var serviceUuids = scanFilterOptions?.ServiceUuids;
            var hasFilter = serviceUuids?.Any() ?? false;

            //_bleWatcher = new BluetoothLEAdvertisementWatcher 
            //{
            //    ScanningMode = ScanMode.ToNative() 
            //};
            Trace.Message("Starting a scan for devices.");
            if (hasFilter && serviceUuids != null)
            {
                //adds filter to native scanner if serviceUuids are specified
                foreach (var uuid in serviceUuids)
                {
                    _bleWatcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(uuid);
                }

                Trace.Message($"ScanFilters: {string.Join(", ", serviceUuids)}");
            }
            _bleWatcher.Received += DeviceFoundAsync;
            _bleWatcher.Start();
            Trace.Message("Started the scan for devices");
            return Task.FromResult(true);
        }

        protected override void StopScanNative()
        {
            Trace.Message($"BleWatcher Status {0}", _bleWatcher.Status.ToString());
            if (_bleWatcher != null && _bleWatcher.Status == BluetoothLEAdvertisementWatcherStatus.Started)
            {
                Trace.Message("Stopping the scan for devices");
                _bleWatcher.Received -= DeviceFoundAsync;
                _bleWatcher.Stop();
                Trace.Message("Stopped the scan for devices");
            }
        }

        protected override async Task ConnectToDeviceNativeAsync(IDevice device, ConnectParameters connectParameters, CancellationToken cancellationToken)
        {
            Trace.Message($"Connecting to device with ID:  {device.Id}");

            if (!(device.NativeDevice is ObservableBluetoothLEDevice nativeDevice))
                return;

            nativeDevice.PropertyChanged -= Device_ConnectionStatusChanged;
            nativeDevice.PropertyChanged += Device_ConnectionStatusChanged;

            ConnectedDeviceRegistry[device.Id.ToString()] = device;

            await nativeDevice.DeviceInfo.Pairing.UnpairAsync();
            // 配对设置
            DeviceInformationCustomPairing pairing = nativeDevice.DeviceInfo.Pairing.Custom;
            
            // 配置自定义配对
            pairing.PairingRequested -= Device_PairingRequested;
            pairing.PairingRequested += Device_PairingRequested;

            var result = await pairing.PairAsync(DevicePairingKinds.ProvidePin);
            if (result.Status != DevicePairingResultStatus.Paired &&
                result.Status != DevicePairingResultStatus.AlreadyPaired)
            {
                throw new Exception(result.Status.ToString());
            }

            await nativeDevice.ConnectAsync();

            HandleBounddDevice(device, DeviceBondState.Bonded);

        }

        private void Device_ConnectionStatusChanged(object? sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (sender == null)
            {
                return;
            }
            if (!(sender is ObservableBluetoothLEDevice nativeDevice) || nativeDevice.BluetoothLEDevice == null)
            {
                return;
            }

            if (propertyChangedEventArgs.PropertyName != nameof(nativeDevice.IsConnected))
            {
                return;
            }

            var address = ParseDeviceId(nativeDevice.BluetoothLEDevice.BluetoothAddress).ToString();
            if (nativeDevice.IsConnected && ConnectedDeviceRegistry.TryGetValue(address, out var connectedDevice))
            {
                HandleConnectedDevice(connectedDevice);
                return;
            }

            if (!nativeDevice.IsConnected && ConnectedDeviceRegistry.TryRemove(address, out var disconnectedDevice))
            {
                HandleDisconnectedDevice(true, disconnectedDevice);
            }
        }

        private void Device_PairingRequested(DeviceInformationCustomPairing sender, DevicePairingRequestedEventArgs args)
        {
            HandleDeviceParing(sender, args);
        }

        protected override void DisconnectDeviceNative(IDevice device)
        {
            // Windows doesn't support disconnecting, so currently just dispose of the device
            Trace.Message($"Disconnected from device with ID:  {device.Id}");

            if (device.NativeDevice is ObservableBluetoothLEDevice nativeDevice)
            {
                ((Device)device).ClearServices();
                device.Dispose();
            }
        }

        public override async Task<IDevice> ConnectToKnownDeviceAsync(Guid deviceGuid, ConnectParameters connectParameters = default, CancellationToken cancellationToken = default)
        {
            //convert GUID to string and take last 12 characters as MAC address
            var guidString = deviceGuid.ToString("N").Substring(20);
            var bluetoothAddress = Convert.ToUInt64(guidString, 16);
            var nativeDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(bluetoothAddress);
            var knownDevice = new Device(this, nativeDevice, 0, deviceGuid, _dq);

            await ConnectToDeviceAsync(knownDevice, cancellationToken: cancellationToken);
            return knownDevice;
        }

        public override IReadOnlyList<IDevice> GetSystemConnectedOrPairedDevices(Guid[] services = null)
        {
            //currently no way to retrieve paired and connected devices on windows without using an
            //async method. 
            Trace.Message("Returning devices connected by this app only");
            return ConnectedDevices;
        }

        /// <summary>
        /// Parses a given advertisement for various stored properties
        /// Currently only parses the manufacturer specific data
        /// </summary>
        /// <param name="adv">The advertisement to parse</param>
        /// <returns>List of generic advertisement records</returns>
        public static List<AdvertisementRecord> ParseAdvertisementData(BluetoothLEAdvertisement adv)
        {
            var advList = adv.DataSections;
            return advList.Select(data => new AdvertisementRecord((AdvertisementRecordType)data.DataType, data.Data?.ToArray())).ToList();
        }

        /// <summary>
        /// Handler for devices found when duplicates are not allowed
        /// </summary>
        /// <param name="watcher">The bluetooth advertisement watcher currently being used</param>
        /// <param name="btAdv">The advertisement recieved by the watcher</param>
        private async void DeviceFoundAsync(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs btAdv)
        {
            if (string.IsNullOrWhiteSpace(btAdv.Advertisement.LocalName))
            {
                //筛选不符合条件的广播
                //以免Start Stop时阻塞等待时间结束导致线程卡死
                //2023.09.05
                return;
            }
            var deviceId = ParseDeviceId(btAdv.BluetoothAddress);

            if (DiscoveredDevicesRegistry.TryGetValue(deviceId, out var device))
            {
                //Trace.Message("AdvertisdedPeripheral: {0} Id: {1}, Rssi: {2}", device.Name, device.Id, btAdv.RawSignalStrengthInDBm);
                (device as Device)?.Update(btAdv.RawSignalStrengthInDBm, ParseAdvertisementData(btAdv.Advertisement));
                this.HandleDiscoveredDevice(device);
            }
            else
            {
                //Trace.Message(string.Format("DiscoveredPeripheral: {0} Id: {1}, Rssi: {2}", btAdv.Advertisement.LocalName, btAdv.BluetoothAddress, btAdv.RawSignalStrengthInDBm));
                //device = new Device(this, bluetoothLeDevice, btAdv.RawSignalStrengthInDBm, deviceId, _dq, ParseAdvertisementData(btAdv.Advertisement), btAdv.IsConnectable);
                //Trace.Message("DiscoveredPeripheral: {0} Id: {1}, Rssi: {2}", device.Name, device.Id, btAdv.RawSignalStrengthInDBm);
                //this.HandleDiscoveredDevice(device);
                var bluetoothLeDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(btAdv.BluetoothAddress);
                if (bluetoothLeDevice != null) //make sure advertisement bluetooth address actually returns a device
                {
                    device = new Device(this, bluetoothLeDevice, btAdv.RawSignalStrengthInDBm, deviceId, _dq, ParseAdvertisementData(btAdv.Advertisement), btAdv.IsConnectable);
                    //Trace.Message("DiscoveredPeripheral: {0} Id: {1}, Rssi: {2}", device.Name, device.Id, btAdv.RawSignalStrengthInDBm);
                    this.HandleDiscoveredDevice(device);
                }
            }
        }

        /// <summary>
        /// Method to parse the bluetooth address as a hex string to a UUID
        /// </summary>
        /// <param name="bluetoothAddress">BluetoothLEDevice native device address</param>
        /// <returns>a GUID that is padded left with 0 and the last 6 bytes are the bluetooth address</returns>
        private static Guid ParseDeviceId(ulong bluetoothAddress)
        {
            var macWithoutColons = bluetoothAddress.ToString("x");
            macWithoutColons = macWithoutColons.PadLeft(12, '0'); //ensure valid length
            var deviceGuid = new byte[16];
            Array.Clear(deviceGuid, 0, 16);
            var macBytes = Enumerable.Range(0, macWithoutColons.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(macWithoutColons.Substring(x, 2), 16))
                .ToArray();
            macBytes.CopyTo(deviceGuid, 10);
            return new Guid(deviceGuid);
        }

        public override IReadOnlyList<IDevice> GetKnownDevicesByIds(Guid[] ids)
        {
            // TODO: implement this
            return new List<IDevice>();
        }

        protected override string GetHostNameNaive()
        {
           return "Windows's Bluetooth";
        }

        protected override Task<bool> TurnOnNaive()
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> TurnOffNaive()
        {
            throw new NotImplementedException();
        }
        public override IReadOnlyList<IDevice> GetBondedDevices()
        {
            return GetSystemConnectedOrPairedDevices();
        }
    }
}