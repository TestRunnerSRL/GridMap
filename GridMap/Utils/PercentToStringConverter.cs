using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GridMap
{
    public class PercentToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"{(float)value * 100:f0}%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var input = value as string;
            float zoom;
            if (float.TryParse(input.TrimEnd('%').Trim(), out zoom))
            {
                return zoom / 100.0;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }
    }
}
