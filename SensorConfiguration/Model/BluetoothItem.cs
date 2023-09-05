using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace SensorConfiguration.Models
{
    public class BluetoothItem : ObservableObject
    {
        public Guid Id { set; get; }
        public string? Name { get; set; }
        public int Rssi { get; set; }

        public string? Address { get; set; }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set 
            {
                if (isSelected != value)
                {
                    SetProperty(ref isSelected, value);
                }
            }
        }
    }
}
