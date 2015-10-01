using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MeshInspector.Converters
{
    /// <summary>
    /// convert for xaml bindings
    /// convert a bool to a colorbrush
    /// </summary>
    public class BoolColorConverter : IValueConverter
    {
        internal static readonly SolidColorBrush ms_brushText = new SolidColorBrush(Colors.White);
        internal static readonly SolidColorBrush ms_brushInvalidText = new SolidColorBrush(Colors.Red);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool current = value as bool? ?? false;
            return current ? BoolColorConverter.ms_brushInvalidText : BoolColorConverter.ms_brushText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }
}
