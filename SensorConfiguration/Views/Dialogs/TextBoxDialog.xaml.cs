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
    public partial class TextBoxDialog : HandyControl.Controls.Window
    {
        public string? Result { get; private set; }

        public TextBoxDialog(string title, string lable, string defult = "")
        {
            InitializeComponent();
            this.title_lable.Content = title;
            this.textbox_lable.Content = lable;
            this.textbox.Text = defult;
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
            Result = this.textbox.Text;
            Close();
        }
    }
}
