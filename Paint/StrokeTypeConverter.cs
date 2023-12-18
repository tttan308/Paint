using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Paint
{
    public class StrokeTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dashes = value as DoubleCollection;
            if (dashes == null || dashes.Count == 0)
                return "Solid";
            if (dashes.Count == 2)
                return "Dash";
            if (dashes.Count == 4)
                return "Dot";
            if (dashes.Count == 6)
                return "DashDotDot";
            return "Custom";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
