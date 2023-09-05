using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Extensions;
using Microsoft.Toolkit.Uwp.Connectivity;
using Windows.System;

namespace Plugin.BLE.UWP
{
    public class Device : DeviceBase<ObservableBluetoothLEDevice>
    {
        public Device(Adapter adapter, BluetoothLEDevice nativeDevice, int rssi, Guid id, DispatcherQueue dq, IReadOnlyList<AdvertisementRecord> advertisementRecords = null, bool isConnectable = true) 
            : base(adapter, new ObservableBluetoothLEDevice(nativeDevice.DeviceInformation, dq)) 
        {
            Rssi = rssi;
            Id = id;
            Name = nativeDevice.Name;
            AdvertisementRecords = advertisementRecords;
            IsConnectable = isConnectable;
            InitAddress(nativeDevice.BluetoothAddress);
        }

        public string? Address { set; get; }

        private void InitAddress(ulong bluetoothAddress)
        {
            try
            {
                var macWithoutColons = bluetoothAddress.ToString("x");
                macWithoutColons = macWithoutColons.PadLeft(12, '0'); //ensure valid length
                var deviceGuid = new byte[16];
                Array.Clear(deviceGuid, 0, 16);
                var macBytes = Enumerable.Range(0, macWithoutColons.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(macWithoutColons.Substring(x, 2), 16))
                    .ToArray();
                Address = BitConverter.ToString(macBytes, 0).ToUpper();
            }
            catch
            {
                Address = "";
            }
        }

        internal void Update(short btAdvRawSignalStrengthInDBm, IReadOnlyList<AdvertisementRecord> advertisementData)
        {
            this.Rssi = btAdvRawSignalStrengthInDBm;
            this.AdvertisementRecords = advertisementData;
        }

        public override Task<bool> UpdateRssiAsync()
        {
            //No current method to update the Rssi of a device
            //In future implementations, maybe listen for device's advertisements

            Trace.Message("Request RSSI not supported in UWP");

            return Task.FromResult(true);
        }

        protected override async Task<IReadOnlyList<IService>> GetServicesNativeAsync()
        {
            if (NativeDevice?.BluetoothLEDevice == null)
                return new List<IService>();

            var result = await NativeDevice.BluetoothLEDevice.GetGattServicesAsync(BleImplementation.CacheModeGetServices);
            result?.ThrowIfError();

            return result?.Services?
                .Select(nativeService => new Service(nativeService, this))
                .Cast<IService>()
                .ToList() ?? new List<IService>();
        }

        protected override async Task<IService> GetServiceNativeAsync(Guid id)
        {
            var result = await NativeDevice.BluetoothLEDevice.GetGattServicesForUuidAsync(id, BleImplementation.CacheModeGetServices);
            result.ThrowIfError();

            var nativeService = result.Services?.FirstOrDefault();
            return nativeService != null ? new Service(nativeService, this) : null;
        }

        protected override DeviceState GetState()
        {
            if (NativeDevice.IsConnected)
            {
                return DeviceState.Connected;
            }

            return NativeDevice.IsPaired ? DeviceState.Limited : DeviceState.Disconnected;
        }

        protected override Task<int> RequestMtuNativeAsync(int requestValue)
        {
            Trace.Message("Request MTU not supported in UWP");
            return Task.FromResult(-1);
        }

        protected override bool UpdateConnectionIntervalNative(ConnectionInterval interval)
        {
            Trace.Message("Update Connection Interval not supported in UWP");
            return false;
        }

        public override void Dispose()
        {
            NativeDevice.Services.ToList().ForEach(s => 
            {
                s?.Service?.Session?.Dispose();
                s?.Service?.Dispose();
            });

            NativeDevice.BluetoothLEDevice?.Dispose();            
            GC.Collect();
        }

        public override bool IsConnectable { get; protected set; }

        public override bool SupportsIsConnectable { get => true; }

        public override async Task<bool> CreateBound()
        {
            return await Task.Run(() =>
            {
                return true;
            });
        }

        public override bool SetPairingConfirmationNative(bool confirm)
        {
            return true;
        }

        public override bool SetPin(byte[] pin)
        {
            return true;
        }
    }
}
