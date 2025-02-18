using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TiketManagementV2.ViewModel;

namespace TiketManagementV2.Converters
{
    public class LogTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is LogType logType))
                return "#95a5a6";

            switch (logType)
            {
                case LogType.Info:
                    return "#3498db";    // Blue
                case LogType.Success:
                    return "#2ecc71";    // Green
                case LogType.Warning:
                    return "#f1c40f";    // Yellow
                case LogType.Error:
                    return "#e74c3c";    // Red
                default:
                    return "#95a5a6";    // Gray (default)
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
