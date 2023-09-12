using HandyControl.Controls;
using HandyControl.Tools.Extension;
using SensorConfiguration.Models;
using SensorConfiguration.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SensorConfiguration.Services
{
    public class DialogService
    {
        public async Task DisplayAlertAsync(string title, string message, string cancel)
        {
            await Task.Run(() =>
            {
                HandyControl.Controls.MessageBox.Show(message, title, MessageBoxButton.OKCancel);
            });

        }

        public void DisplayAlert(string title, string message, string cancel)
        {
            HandyControl.Controls.MessageBox.Show(message, title, MessageBoxButton.OKCancel);
        }

        public void ShowConfiguationModal(ListViewModel listViewModel, Type type)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ConfigurationDialog modalDialog = new ConfigurationDialog(listViewModel, type);
                modalDialog.ShowDialog();
            });
        }

        public void ShowTestModeModal(BluetoothItem selectedBluetoothItem)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TestModeDialog modalDialog = new TestModeDialog(selectedBluetoothItem);
                modalDialog.ShowDialog();
            });
        }

        public void ShowDimmerLevelModal(BluetoothItem selectedBluetoothItem, ListViewModel listViewModel)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                DimmerLevelDialog modalDialog = new DimmerLevelDialog(selectedBluetoothItem, listViewModel);
                modalDialog.ShowDialog();
            });
        }

        public void ShowScanDialog()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ScanDialog modalDialog = new ScanDialog();
                modalDialog.ShowDialog();
                //ScanDialog dialog = DialogManager.GetHcDialog<ScanDialog>();
                //dialog.ShowDialog();
            });
        }

        public string ShowPasswordDialog()
        {
            PasswordDialog modalDialog = new PasswordDialog();
            modalDialog.ShowDialog();
            return modalDialog.Result;
        }

        public string ShowTextBoxDialog(string title, string lable, string defult = "")
        {
            var textBoxDialog = new TextBoxDialog(title, lable, defult);
            textBoxDialog.ShowDialog();
            return textBoxDialog.Result;
        }
    }
}
