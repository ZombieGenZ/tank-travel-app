using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using TiketManagementV2.View;

namespace TiketManagementV2.Services
{
    public class NotificationService : INotificationService
    {
        public void ShowNotification(string header, string message, NotificationType type)
        {
            var notification = new NotificationMessage
            {
                Header = header,
                Message = message
            };

            switch (type)
            {
                case NotificationType.Success:
                    notification.ImagePath = "/Images/Success_Icon.png";
                    notification.Gradient = (LinearGradientBrush)Application.Current.Resources["SuccessGradient"];
                    notification.RecFill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CC355"));
                    break;

                case NotificationType.Error:
                    notification.ImagePath = "/Images/Error_Icon.png";
                    notification.Gradient = (LinearGradientBrush)Application.Current.Resources["RedGradient"];
                    notification.RecFill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F24A50"));
                    break;

                case NotificationType.Info:
                    notification.ImagePath = "/Images/Info_Icon.png";
                    notification.Gradient = (LinearGradientBrush)Application.Current.Resources["InfoGradient"];
                    notification.RecFill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3874CB"));
                    break;

                case NotificationType.Warning:
                    notification.ImagePath = "/Images/Warning_Icon.png";
                    notification.Gradient = (LinearGradientBrush)Application.Current.Resources["WarningGradient"];
                    notification.RecFill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA500"));
                    break;
            }

            notification.Show();
        }
    }
}
