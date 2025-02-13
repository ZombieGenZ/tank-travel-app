using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiketManagementV2.Services
{
    public enum NotificationType
    {
        Success,  // Thành công
        Error,    // Lỗi
        Info,     // Thông tin
        Warning   // Cảnh báo
    }

    public interface INotificationService
    {
        void ShowNotification(string header, string message, NotificationType type);
    }
}
