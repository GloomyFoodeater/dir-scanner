using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfApp.Converters;

public class PercentageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double proportion = (double)value;
        return proportion > 0.01 ? (100 * proportion).ToString("N2") + "%" : "~0.01%";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}