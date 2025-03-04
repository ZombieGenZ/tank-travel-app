using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TicketManagementV2.Converters
{
    public class RankBorderConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is int rank && parameter is string converterParameter)
            {
                switch (converterParameter)
                {
                    case "BorderBrush" when rank == 1:
                        return new LinearGradientBrush
                        {
                            StartPoint = new System.Windows.Point(0, 0),
                            EndPoint = new System.Windows.Point(1, 1),
                            GradientStops = new GradientStopCollection
                            {
                                new GradientStop(Colors.Gold, 0),
                                new GradientStop(Colors.Orange, 1)
                            }
                        };
                    case "BorderBrush" when rank == 2:
                        return new LinearGradientBrush
                        {
                            StartPoint = new System.Windows.Point(0, 0),
                            EndPoint = new System.Windows.Point(1, 1),
                            GradientStops = new GradientStopCollection
                            {
                                new GradientStop(Colors.Silver, 0),
                                new GradientStop(Color.FromRgb(192, 192, 192), 1)
                            }
                        };
                    case "BorderBrush" when rank == 3:
                        return new LinearGradientBrush
                        {
                            StartPoint = new System.Windows.Point(0, 0),
                            EndPoint = new System.Windows.Point(1, 1),
                            GradientStops = new GradientStopCollection
                            {
                                new GradientStop(Color.FromRgb(205, 127, 50), 0),
                                new GradientStop(Color.FromRgb(165, 110, 48), 1)
                            }
                        };
                    case "BorderBrush":
                        return new SolidColorBrush(Color.FromRgb(58, 49, 118)); // Default blue

                    case "BorderThickness" when rank <= 3:
                        return 3.0;
                    case "BorderThickness":
                        return 2.0;
                }
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}