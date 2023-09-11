using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SensorConfiguration.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static SensorConfiguration.Constant.Enums;

namespace SensorConfiguration.ViewModel
{
    public class LogViewModel : ObservableRecipient
    {
        private string? _deviceName;

        public string DeviceName
        {
            get { return _deviceName; }
            set
            {
                if (_deviceName != value)
                {
                    SetProperty(ref _deviceName, value);
                }
            }
        }

        public ICommand OpenDialogCommand { get; }

        public  void OpenDialog()
        {
            Window1 modalDialog = new Window1();
            modalDialog.ShowDialog();
        }


        public LogViewModel()
        {
            DeviceName = "Seven Chen";
            OpenDialogCommand = new RelayCommand(OpenDialog);
        }
    }
}
