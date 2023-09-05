using SensorConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SensorConfiguration.SharedData
{
    public class BluetoothLog
    {
        public static ObservableCollection<BluetoothMessageItem> BluetoothMessages = new ObservableCollection<BluetoothMessageItem>();
    }
}
