using SensorConfiguration.Attributes;
using SensorConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using static SensorConfiguration.Constant.Enums;

namespace SensorConfiguration.Converter
{
    public class ListViewKeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var key = (string)value;
            if (string.IsNullOrEmpty(key))
            {
                return "";
            }
            try
            {
                string typeName = "Sensor_Configer.Models." + parameter;
                Type type = Type.GetType(typeName);
                if (type == null)
                {
                    return key;
                }
                PropertyInfo propertyInfo = type.GetProperty(key);
                if (propertyInfo == null)
                {
                    return key;
                }
                var attribute = propertyInfo.GetCustomAttribute<PropertyConfigAttribute>();
                if (attribute == null)
                {
                    return key;
                }
                return attribute.ShowName;
            }
            catch
            {
                return key;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ListViewKeyConverter is a one-way converter only.");
        }
    }

    public class ListViewValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var keyValue = value as KeyValue;
            if (keyValue == null)
            {
                return "";
            }
            try
            {
                string typeName = "Sensor_Configer.Models." + parameter;
                Type type = Type.GetType(typeName);
                if (type == null)
                {
                    return keyValue.Value;
                }
                PropertyInfo propertyInfo = type.GetProperty(keyValue.Key);
                if (propertyInfo == null)
                {
                    return keyValue.Value;
                }
                var attribute = propertyInfo.GetCustomAttribute<PropertyConfigAttribute>();
                if (attribute == null)
                {
                    return keyValue.Value;
                }
                switch (attribute.Type)
                {
                    case PropertyType.None:
                        return keyValue.Value;
                    case PropertyType.WithUnit:
                        if (keyValue.Value == "0" && parameter.ToString() != "DeviceParameters")
                        {
                            return "Disabled";
                        }
                        if ((!keyValue.DaliFlag || string.IsNullOrEmpty(attribute.DaliUnit)) && attribute.Unit == "Volts")
                        {
                            return decimal.Parse(keyValue.Value ?? "0") / 10 + attribute.Unit;
                        }
                        return keyValue.Value + " " + (keyValue.DaliFlag && !string.IsNullOrEmpty(attribute.DaliUnit) ? attribute.DaliUnit : attribute.Unit);
                    case PropertyType.Enum:
                        if(attribute.EnumType == null)
                        {
                            return keyValue.Value;
                        }
                        return Enum.ToObject(attribute.EnumType, int.Parse(keyValue.Value ?? "0"));
                    case PropertyType.StrEnum:
                        if(attribute.EnumType == null)
                        {
                            return keyValue.Value;
                        }
                        return GetEnumStringValue(attribute.EnumType, int.Parse(keyValue.Value ?? "0"));
                }
                return keyValue.Value;
            }
            catch
            {
                return keyValue.Value;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ListViewKeyConverter is a one-way converter only.");
        }
    }

    public class VoltsValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var temp = (int)value;
                return ((decimal)temp / 10).ToString();
            }
            catch
            {
                return value;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
