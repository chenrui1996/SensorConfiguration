using SensorConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SensorConfiguration.SharedData
{
    public class LoggedDevices
    {
        public ObservableCollection<BluetoothItem> LoggedBluetooths = new ObservableCollection<BluetoothItem>();

        //public BluetoothItem SelectedLoggedBluetooth;
    }
}
