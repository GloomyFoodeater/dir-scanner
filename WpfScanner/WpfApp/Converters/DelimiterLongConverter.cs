using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfApp.Converters;

public class DelimiterLongConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => $"{(long)value:n0}";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}