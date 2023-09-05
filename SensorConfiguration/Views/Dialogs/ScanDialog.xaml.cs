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
    /// ScanDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ScanDialog : Window
    {
        public ScanDialog()
        {
            InitializeComponent();
            DataContext = new ScanViewModel();
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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            var scanViewModel = DataContext as ScanViewModel;
            scanViewModel?.StopScan();
        }
    }
}
