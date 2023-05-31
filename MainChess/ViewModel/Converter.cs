using System;
using System.Globalization;
using System.Windows.Data;

namespace MainChess.ViewModel
{
    public class Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int v && (v % 2 == 0 ^ v / 8 % 2 == 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null!;
        }
    }
}