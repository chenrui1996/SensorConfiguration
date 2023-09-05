using CommunityToolkit.Mvvm.ComponentModel;

namespace SensorConfiguration.Models
{
    public class ListViewModel : ObservableObject
    {
        public string? Key { set; get; }

        private KeyValue? _keyValue;
        public KeyValue? KeyValue
        {
            get { return _keyValue; }
            set
            {
                if (_keyValue != value)
                {
                    SetProperty(ref _keyValue, value);
                }
            }
        }
    }
    public class KeyValue : ObservableObject
    {
        public bool DaliFlag { set; get; } = false;

        private string? _key;

        public string? Key
        {
            get { return _key; }
            set
            {
                if (_key != value)
                {
                    SetProperty(ref _key, value);
                }
            }
        }

        private string? _value;

        public string? Value
        {
            get { return _value; }
            set 
            {
                if (_value != value)
                {
                    SetProperty(ref _value, value);
                }
            }
        }
    }
}
