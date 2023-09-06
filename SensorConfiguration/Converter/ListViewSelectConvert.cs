using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SensorConfiguration.Converter
{
    public class BoolToColorConverter : IValueConverter
    {
        public Brush? TrueColor { get; set; }
        public Brush? FalseColor { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? TrueColor : FalseColor;
            }
            return FalseColor; // Default color if not a boolean
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("BoolToColorConverter is a one-way converter only.");
        }
    }

    public class BoolToTextColorConverter : IValueConverter
    {
        public Brush? TrueColor { get; set; }
        public Brush? FalseColor { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? TrueColor : FalseColor;
            }
            return FalseColor; // Default color if not a boolean
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("BoolToColorConverter is a one-way converter only.");
        }
    }
}
