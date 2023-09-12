using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SensorConfiguration.Attributes;
using SensorConfiguration.Constant;
using SensorConfiguration.Models;
using SensorConfiguration.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SensorConfiguration.ViewModel.Dialogs
{
    public class ConfigurationDialogViewModel : ObservableRecipient
    {
        #region 属性
        private string? _title;

        /// <summary>
        /// 显示单位
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    SetProperty(ref _title, value);
                }
            }
        }

        private ListViewModel? _listViewModel { get; set; }

        private int _propertyValue;

        /// <summary>
        /// 属性值
        /// </summary>
        public int PropertyValue
        {
            get { return _propertyValue; }
            set
            {
                if (_propertyValue != value)
                {
                    SetProperty(ref _propertyValue, value);
                    
                }
            }
        }

        private string? _unit;

        /// <summary>
        /// 显示单位
        /// </summary>
        public string Unit
        {
            get { return _unit; }
            set
            {
                if (_unit != value)
                {
                    SetProperty(ref _unit, value);
                }
            }
        }


        private int _maxPropertyValue;

        /// <summary>
        /// 最大属性值
        /// </summary>
        public int MaxPropertyValue
        {
            get { return _maxPropertyValue; }
            set
            {
                if (_maxPropertyValue != value)
                {
                    SetProperty(ref _maxPropertyValue, value);
                }
            }
        }

        private int _minPropertyValue;

        /// <summary>
        /// 最小属性值
        /// </summary>
        public int MinPropertyValue
        {
            get { return _minPropertyValue; }
            set
            {
                if (_minPropertyValue != value)
                {
                    SetProperty(ref _minPropertyValue, value);
                }
            }
        }

        private Visibility _normalLabelVisiable;

        /// <summary>
        /// 普通值显示标志
        /// </summary>
        public Visibility NormalLabelVisiable
        {
            get { return _normalLabelVisiable; }
            set
            {
                if (_normalLabelVisiable != value)
                {
                    SetProperty(ref _normalLabelVisiable, value);
                }
            }
        }

        private Visibility _enumLabelVisiable;

        /// <summary>
        /// 枚举显示标志
        /// </summary>
        public Visibility EnumLabelVisiable
        {
            get { return _enumLabelVisiable; }
            set
            {
                if (_enumLabelVisiable != value)
                {
                    SetProperty(ref _enumLabelVisiable, value);
                }
            }
        }


        private List<EnumKeyValue>? _enumValueList;

        /// <summary>
        /// 枚举值列表
        /// </summary>
        public List<EnumKeyValue> EnumValueList
        {
            get { return _enumValueList; }
            set
            {
                if (_enumValueList != value)
                {
                    SetProperty(ref _enumValueList, value);
                }
            }
        }

        private EnumKeyValue? _selectedEnumItem;

        /// <summary>
        /// 已选择枚举值
        /// </summary>
        public EnumKeyValue SelectedEnumItem
        {
            get { return _selectedEnumItem; }
            set
            {
                if (_selectedEnumItem != value)
                {
                    SetProperty(ref _selectedEnumItem, value);
                    if (PropertyValue != _selectedEnumItem.EnumValue)
                    {
                        PropertyValue = _selectedEnumItem.EnumValue;
                    }
                }
            }
        }
        private Visibility _testButtonVisiable;

        /// <summary>
        /// 测试按钮显示标志
        /// </summary>
        public Visibility TestButtonVisiable
        {
            get { return _testButtonVisiable; }
            set
            {
                if (_testButtonVisiable != value)
                {
                    SetProperty(ref _testButtonVisiable, value);
                }
            }
        }

        private Visibility _percentVisiable;

        /// <summary>
        /// 百分号
        /// </summary>
        public Visibility PercentVisiable
        {
            get { return _percentVisiable; }
            set
            {
                if (_percentVisiable != value)
                {
                    SetProperty(ref _percentVisiable, value);
                }
            }
        }

        private Visibility _voltsVisiable;

        /// <summary>
        /// 电压 需要/10
        /// </summary>
        public Visibility VoltsVisiable
        {
            get { return _voltsVisiable; }
            set
            {
                if (_voltsVisiable != value)
                {
                    SetProperty(ref _voltsVisiable, value);
                }
            }
        }
        #endregion

        #region 指令
        /// <summary>
        /// 保存配置
        /// </summary>
        public ICommand CommitButtonCommand { get; }

        private async void Commit(object? o)
        {
            await Task.Run(async () =>
            {
                try
                {
                    if (_listViewModel == null || _listViewModel.KeyValue == null)
                    {
                        await new DialogService().DisplayAlertAsync("错误", "初始化失败", "确认");
                        return;
                    }
                    if (_listViewModel.KeyValue.Value != _propertyValue.ToString())
                    {
                        var daliFlag = _listViewModel.KeyValue.DaliFlag;
                        _listViewModel.KeyValue = new KeyValue
                        {
                            Key = _listViewModel.Key,
                            Value = _propertyValue.ToString(),
                            DaliFlag = daliFlag
                        };
                    }
                    if (o == null)
                    {
                        return;
                    }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        (o as Window)?.Close();
                    });
                }
                catch (Exception e)
                {
                    await new DialogService().DisplayAlertAsync("错误", e.Message, "确认");
                }
            });
        }

        /// <summary>
        /// 取消保存
        /// </summary>
        public ICommand CancelButtonCommand { get; }

        private async void Cancel(object? o)
        {
            try
            {
                if (o == null)
                {
                    return;
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    (o as Window)?.Close();
                });
            }
            catch (Exception e)
            {
                await new DialogService().DisplayAlertAsync("错误", e.Message, "确认");
            }
        }

        /// <summary>
        /// 加
        /// </summary>
        public ICommand AddPropertyButtonCommand { get; }

        private async void AddProperty()
        {
            await Task.Run(() =>
            {
                if (PropertyValue < MaxPropertyValue)
                {
                    PropertyValue++;
                }
            });
        }
        /// <summary>
        /// 减
        /// </summary>
        public ICommand SubPropertyButtonCommand { get; }

        private async void SubProperty()
        {
            await Task.Run(() =>
            {
                if (PropertyValue > MinPropertyValue)
                {
                    PropertyValue--;
                }
            });
        }
        #endregion

        private void Init(ListViewModel listViewModel, Type type)
        {
            try
            {
                this._listViewModel = listViewModel;
                EnumValueList = new List<EnumKeyValue>();
                var key = listViewModel.Key;
                if (key == null)
                {
                    return;
                }
                //Type type = typeof(ConfigurationInfo);
                if (type == null)
                {
                    return;
                }
                PropertyInfo? propertyInfo = type.GetProperty(key);
                if (propertyInfo == null)
                {
                    return;
                }
                var attribute = propertyInfo.GetCustomAttribute<PropertyConfigAttribute>();
                if (attribute == null)
                {
                    return;
                }
                Title = string.Format("{0}({1})", attribute.ShowName, attribute.Group);
                if (_listViewModel == null 
                    || _listViewModel.KeyValue == null 
                    || _listViewModel.KeyValue.Value == null)
                {
                    return;
                }
                PropertyValue = int.Parse(_listViewModel.KeyValue.Value);
                if (attribute.Type == Enums.PropertyType.Enum)
                {
                    NormalLabelVisiable = Visibility.Collapsed;
                    EnumLabelVisiable = Visibility.Visible;
                    MaxPropertyValue = 100;
                    MinPropertyValue = 0;
                    var typeOdEnum = attribute.EnumType;
                    if (typeOdEnum == null)
                    {
                        return;
                    }
                    foreach (var item in typeOdEnum.GetEnumValues())
                    {
                        var value = (int)item;
                        var enumKeyValue = new EnumKeyValue
                        {
                            EnumKey = item.ToString(),
                            EnumValue = (int)item
                        };
                        if (value == PropertyValue)
                        {
                            SelectedEnumItem = enumKeyValue;
                        }
                        EnumValueList.Add(enumKeyValue);
                    }
                    return;
                }
                if (attribute.Type == Enums.PropertyType.StrEnum)
                {
                    NormalLabelVisiable = Visibility.Collapsed;
                    EnumLabelVisiable = Visibility.Visible;
                    MaxPropertyValue = 100;
                    MinPropertyValue = 0;
                    var typeOdEnum = attribute.EnumType;
                    if (typeOdEnum == null)
                    {
                        return;
                    }
                    foreach (var item in typeOdEnum.GetEnumValues())
                    {
                        var value = (int)item;
                        var enumKeyValue = new EnumKeyValue
                        {
                            EnumKey = Enums.GetEnumStringValue(typeOdEnum, value),
                            EnumValue = (int)item
                        };
                        if (value == PropertyValue)
                        {
                            SelectedEnumItem = enumKeyValue;
                        }
                        EnumValueList.Add(enumKeyValue);
                    }
                    return;
                }
                NormalLabelVisiable = Visibility.Visible;
                EnumLabelVisiable = Visibility.Collapsed;
                MaxPropertyValue = attribute.MaxValue;
                MinPropertyValue = attribute.MinValue;
                TestButtonVisiable = Visibility.Collapsed;
                Unit = _listViewModel.KeyValue.DaliFlag && !string.IsNullOrEmpty(attribute.DaliUnit)
                    ? (attribute.DaliUnit ?? "")
                    : (attribute.Unit ?? "");
                if (Unit == "Volts")
                {
                    VoltsVisiable = Visibility.Visible;
                    PercentVisiable = Visibility.Collapsed;
                }
                else
                {
                    VoltsVisiable = Visibility.Collapsed;
                    PercentVisiable = Visibility.Visible;
                }
            }
            catch
            {

            }
           
        }

        public ConfigurationDialogViewModel(ListViewModel listViewModel, Type type)
        {
            Init(listViewModel, type);
            AddPropertyButtonCommand = new RelayCommand(AddProperty);
            SubPropertyButtonCommand = new RelayCommand(SubProperty);

            CommitButtonCommand = new RelayCommand<object>(Commit);
            CancelButtonCommand = new RelayCommand<object>(Cancel);
        }
    }
}
