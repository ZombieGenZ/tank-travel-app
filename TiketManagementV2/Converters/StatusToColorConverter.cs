using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace TiketManagementV2.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = (string)value;
            if (status == "Pending")
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB800"));
            if (status == "Approved")
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28A745"));
            if (status == "Rejected")
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC3545"));
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
