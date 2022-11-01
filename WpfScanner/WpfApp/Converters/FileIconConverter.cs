using System;
using System.Globalization;
using System.Windows.Data;
using WpfApp.Model;

namespace WpfApp.Converters;

public class FileIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return ((IconEnum)value) switch
        {
            IconEnum.Root => "Resources/root-ico.png",
            IconEnum.Directory => "Resources/directory-ico.png",
            _ => "Resources/file-ico.png"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}