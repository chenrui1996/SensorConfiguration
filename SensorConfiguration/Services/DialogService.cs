using HandyControl.Controls;
using HandyControl.Tools.Extension;
using SensorConfiguration.Models;
using SensorConfiguration.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorConfiguration.Services
{
    public class DialogService
    {
        public async Task DisplayAlertAsync(string title, string message, string cancel)
        {
            await Task.Run(() =>
            {
                PopupWindow.ShowDialog(message, title, true);
            });

        }

        public void DisplayAlert(string title, string message, string cancel)
        {
            PopupWindow.ShowDialog(message, title, true);
        }

        public async Task<string> DisplayPromptAsync(string title, string message, string accept, string cancel)
        {
            return await Task.Run(() =>
            {
                return "";
            });
            //return await Application.Current.MainPage.DisplayPromptAsync(title, message, accept, cancel);
        }

        public async void ShowLoadingModal()
        {
            await Task.Run(() =>
            {
                return "";
            });
            //var loadingPage = new LoadingPage
            //{
            //    CloseWhenBackgroundIsClicked = false
            //};
            //await PopupNavigation.Instance.PushAsync(loadingPage);
        }

        public async void HideLoadingModal()
        {
            await Task.Run(() =>
            {
                return "";
            });
            //var popPage = PopupNavigation.Instance.PopupStack;
            //if (!popPage.Any())
            //{
            //    return;
            //}
            //await PopupNavigation.Instance.PopAsync();
        }

        public async void ShowConfiguationModal(ListViewModel listViewModel, Type type)
        {
            await Task.Run(() =>
            {
                return "";
            });
            //var loadingPage = new ConfigurationPropup(listViewModel, type)
            //{
            //    CloseWhenBackgroundIsClicked = true
            //};
            //await PopupNavigation.Instance.PushAsync(loadingPage);
        }

        public async void HideConfiguationModal()
        {
            await Task.Run(() =>
            {
                return "";
            });
            //var popPage = PopupNavigation.Instance.PopupStack;
            //if (!popPage.Any())
            //{
            //    return;
            //}
            //await PopupNavigation.Instance.PopAsync();
        }

        public async void ShowTestModeModal(BluetoothItem selectedBluetoothItem)
        {
            await Task.Run(() =>
            {
                return "";
            });
            //var loadingPage = new TestModePropup(selectedBluetoothItem)
            //{
            //    CloseWhenBackgroundIsClicked = true
            //};
            //await PopupNavigation.Instance.PushAsync(loadingPage);
        }

        public async void HideTestModeModal()
        {
            await Task.Run(() =>
            {
                return "";
            });
            //var popPage = PopupNavigation.Instance.PopupStack;
            //if (!popPage.Any())
            //{
            //    return;
            //}
            //await PopupNavigation.Instance.PopAsync();
        }

        public async void ShowDimmerLevelModal(BluetoothItem selectedBluetoothItem, ListViewModel listViewModel)
        {
            await Task.Run(() =>
            {
                return "";
            });
            //var loadingPage = new DimmerLevelPropup(selectedBluetoothItem, listViewModel)
            //{
            //    CloseWhenBackgroundIsClicked = true
            //};
            //await PopupNavigation.Instance.PushAsync(loadingPage);
        }

        public async void HideDimmerLevelModal()
        {
            await Task.Run(() =>
            {
                return "";
            });
            //var popPage = PopupNavigation.Instance.PopupStack;
            //if (!popPage.Any())
            //{
            //    return;
            //}
            //await PopupNavigation.Instance.PopAsync();
        }

        public async void ShowDeviceParameterModal(DeviceParameters deviceParameters)
        {
            await Task.Run(() =>
            {
                return "";
            });
            //var loadingPage = new DeviceParametersPropup(deviceParameters)
            //{
            //    CloseWhenBackgroundIsClicked = true
            //};
            //await PopupNavigation.Instance.PushAsync(loadingPage);
        }

        public async void HideDeviceParameterModal()
        {
            await Task.Run(() =>
            {
                return "";
            });
            //var popPage = PopupNavigation.Instance.PopupStack;
            //if (!popPage.Any())
            //{
            //    return;
            //}
            //await PopupNavigation.Instance.PopAsync();
        }

        public async void ShowDeviceNameModal(BluetoothItem selectedBluetoothItem, ListViewModel listViewModel)
        {
            await Task.Run(() =>
            {
                return "";
            });
            //var loadingPage = new DeviceNamePropup(selectedBluetoothItem, listViewModel)
            //{
            //    CloseWhenBackgroundIsClicked = true
            //};
            //await PopupNavigation.Instance.PushAsync(loadingPage);
        }

        public async void HideDeviceNameModal()
        {
            await Task.Run(() =>
            {
                return "";
            });
            //var popPage = PopupNavigation.Instance.PopupStack;
            //if (!popPage.Any())
            //{
            //    return;
            //}
            //await PopupNavigation.Instance.PopAsync();
        }

        public async void ShowModifyPasswordModal(BluetoothItem selectedBluetoothItem, bool canCancel)
        {
            await Task.Run(() =>
            {
                return "";
            });
            //var popPage = new ModifyPasswordPropup(selectedBluetoothItem)
            //{
            //    CloseWhenBackgroundIsClicked = canCancel
            //};
            //await PopupNavigation.Instance.PushAsync(popPage);
        }

        public async void HideModifyPasswordModal()
        {
            await Task.Run(() =>
            {
                return "";
            });
            //var popPage = PopupNavigation.Instance.PopupStack;
            //if (!popPage.Any())
            //{
            //    return;
            //}
            //await PopupNavigation.Instance.PopAsync();
        }

        public async void ToContinuousDimmingPage(BluetoothItem selectedLoggedBluetooth, ContinuousDimmingConfiguration continuousDimmingConfig)
        {
            await Task.Run(() =>
            {
                return "";
            });
            //var page = new ContinuousDimming(selectedLoggedBluetooth, continuousDimmingConfig);
            //await Application.Current.MainPage.Navigation.PushAsync(page);
        }

        public async void BackBeforPage()
        {
            await Task.Run(() =>
            {
                return "";
            });
            //var popPage = Application.Current.MainPage.Navigation.NavigationStack;
            //if (!popPage.Any())
            //{
            //    return;
            //}
            //await Application.Current.MainPage.Navigation.PopAsync();
        }

        public void ShowScanDialog()
        {
            ScanDialog modalDialog = new ScanDialog();
            //new PopupWindow().Show(modalDialog, true);
            modalDialog.ShowDialog();
        }
    }
}
