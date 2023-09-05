using System;
using System.Collections.Generic;
using System.Text;
using static SensorConfiguration.Constant.Enums;

namespace SensorConfiguration.Attributes
{
    public class PropertyConfigAttribute : Attribute
    {
        public PropertyConfigAttribute(string showName)
        {
            Type = PropertyType.None;
            ShowName = showName;
        }

        public PropertyConfigAttribute(string group, string showName)
        {
            Type = PropertyType.None;
            ShowName = showName;
            Group = group;
        }
        public PropertyConfigAttribute(string showName, PropertyType type, int minValue, int maxValue, string unit, string daliUnit = "", bool isShow = true)
        {
            ShowName = showName;
            Type = type;
            Unit = unit;
            MaxValue = maxValue;
            MinValue = minValue;
            DaliUnit = daliUnit;
            IsShow = isShow;
        }

        public PropertyConfigAttribute(string group, string showName, PropertyType type, int minValue, int maxValue, string unit, string daliUnit = "", bool isShow = true)
        {
            ShowName = showName;
            Type = type;
            Unit = unit;
            Group = group;
            MaxValue = maxValue;
            MinValue = minValue;
            DaliUnit = daliUnit;
            IsShow = isShow;
        }

        public PropertyConfigAttribute(string showName, PropertyType type, Type enumName, bool isShow = true)
        {
            ShowName = showName;
            Type = type;
            EnumType = enumName;
            IsShow = isShow;
        }

        public PropertyConfigAttribute(string group, string showName, PropertyType type, Type enumName, bool isShow = true)
        {
            ShowName = showName;
            Type = type;
            EnumType = enumName;
            Group = group;
            IsShow = isShow;
        }

        public string? Group { get; set; }
        public string? ShowName { get; set; }
        public PropertyType? Type { get; set; }
        public string? Unit { get; set; }
        public string? DaliUnit { get; set; }
        public Type? EnumType { get; set; }
        public int MaxValue { get; set; }
        public int MinValue { get; set; }
        public bool IsShow { get; set; }
    }
}
