using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using SensorConfiguration.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace SensorConfiguration.ViewModel
{
    public class MainWindowViewModel : ObservableRecipient
    {
        #region
        private Page _currentPage = new Views.Index();
        public Page CurrentPage
        {
            get { return _currentPage; }
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged(nameof(CurrentPage));
                }
            }
        }

        private string pageTitle = "首页";

        public string PageTitle
        {
            get { return pageTitle; }
            set
            {
                if (pageTitle != value)
                {
                    pageTitle = value;
                    OnPropertyChanged(nameof(pageTitle));
                }
            }
        }

        #endregion

        #region 指令
        private SideMenuItem? _selectedItem { set; get; }

        public ICommand SelectCmd { get; }

        private void SelectMethod(SideMenuItem? item)
        {
            try
            {
                if (item == null)
                {
                    CurrentPage = new Views.Index();
                    PageTitle = "首页";
                    if (_selectedItem != null)
                    {
                        _selectedItem.IsSelected = false;
                    }
                    _selectedItem = null;
                    return;
                }
                if (string.IsNullOrEmpty(item.Name))
                {
                    return;
                }
                string typeName = "SensorConfiguration.Views." + item.Name;
                var type = Type.GetType(typeName); // 获取类型
                if (type == null)
                {
                    return;
                }
                var instance = Activator.CreateInstance(type) as Page;
                if (instance == null)
                {
                    return;
                }
                CurrentPage = instance;
                PageTitle = item.ToolTip.ToString() ?? "无标题";
                _selectedItem = item;
            }
            catch
            {

            }
        }
        #endregion

        public MainWindowViewModel()
        {
            SelectCmd = new RelayCommand<SideMenuItem>(SelectMethod);
        }
    }
}
