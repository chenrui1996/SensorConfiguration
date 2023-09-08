using HandyControl.Controls;
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
    public partial class PasswordDialog : HandyControl.Controls.Window
    {
        public string? Result { get; private set; }

        public PasswordDialog()
        {
            InitializeComponent();
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

        private void Confim_Click(object sender, RoutedEventArgs e)
        {
            Result = this.passwordBox.Password;
            Close();
        }
    }
}
