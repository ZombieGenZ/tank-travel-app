using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TiketManagementV2.ViewModel;

namespace TiketManagementV2.Converters
{
    public class LogTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is LogType logType))
                return FontAwesome.Sharp.IconChar.Circle;

            switch (logType)
            {
                case LogType.Info:
                    return FontAwesome.Sharp.IconChar.InfoCircle;
                case LogType.Success:
                    return FontAwesome.Sharp.IconChar.CheckCircle;
                case LogType.Warning:
                    return FontAwesome.Sharp.IconChar.ExclamationTriangle;
                case LogType.Error:
                    return FontAwesome.Sharp.IconChar.TimesCircle;
                default:
                    return FontAwesome.Sharp.IconChar.Circle;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
