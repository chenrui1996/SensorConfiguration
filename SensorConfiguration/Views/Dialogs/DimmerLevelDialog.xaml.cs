﻿using HandyControl.Controls;
using SensorConfiguration.Models;
using SensorConfiguration.ViewModel.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SensorConfiguration.Views.Dialogs
{
    /// <summary>
    /// PasswordDialogs.xaml 的交互逻辑
    /// </summary>
    public partial class DimmerLevelDialog : HandyControl.Controls.Window
    {

        public DimmerLevelDialog(BluetoothItem selectedBluetoothItem, ListViewModel listViewModel)
        {
            InitializeComponent();
            DataContext = new DimmerLeveDialogViewModel(selectedBluetoothItem, listViewModel);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
