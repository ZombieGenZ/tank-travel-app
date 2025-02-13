using System;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using TiketManagementV2.View;
using TiketManagementV2.Helpers;
using TiketManagementV2.Services;
using System.Windows;
using TiketManagementV2.Model;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using System.Windows.Markup;

namespace TiketManagementV2.ViewModel
{
    public class LoginViewModel
    {
        private readonly INotificationService notificationService;

        public LoginViewModel(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        public void Login(string username, string password)
        {
            // Kiểm tra email
            if (!ValidationHelper.IsValidEmail(username))
            {
                notificationService.ShowNotification(
                    "Error",
                    "Invalid email format! Please enter a valid email.",
                    NotificationType.Error
                );
                return;
            }

            // Kiểm tra mật khẩu
            if (!ValidationHelper.IsValidPassword(password))
            {
                notificationService.ShowNotification(
                    "Error",
                    "Password cannot be empty!",
                    NotificationType.Error
                );
                return;
            }

            KiemTraDangNhap(username, password, out string role);

            if (!string.IsNullOrEmpty(role))  // Kiểm tra role để xác định đăng nhập thành công
            {
                // Hiển thị thông báo theo vai trò
                string message = role == "admin"
                    ? "Welcome, Admin! You have full access."
                    : "Welcome, Bus Operator! You can manage transport schedules.";

                notificationService.ShowNotification("Success", message, NotificationType.Success);

                // Chuyển đến giao diện phù hợp
                NavigateToRoleView(role);
                
            }
            else
            {
                notificationService.ShowNotification("Error", "Incorrect username or password!", NotificationType.Error);
            }
        }


        private async Task<dynamic> GetLogin(string tenDangNhap, string matKhau)
        {
            ApiServices service = new ApiServices();

            var loginBody = new
            {
                email = tenDangNhap,
                password = matKhau
            };

            dynamic data = await service.PostWithBodyAsync("api/users/login-manage", loginBody);

            return data;
        }
        private void KiemTraDangNhap(string tenDangNhap, string matKhau, out string role)
        {
            //dynamic data = await GetLogin(tenDangNhap, matKhau);



            role = string.Empty;

            if (tenDangNhap == "admin@example.com" && matKhau == "admin123")
            {
                role = "admin";
            }
            else if (tenDangNhap == "user@example.com" && matKhau == "user123")
            {
                role = "bus";
            }
        }

        private void NavigateToRoleView(string role)
        {
            //Application.Current.Dispatcher.Invoke(() =>
            //{
            //    Window nextView;

            //    if (role == "admin")
            //    {
            //        nextView = new AdminView(); // Giao diện dành cho Admin
            //    }
            //    else
            //    {
            //        nextView = new BusView(); // Giao diện dành cho Bus Operator
            //    }

            //    nextView.Show();

            //    // Đóng cửa sổ đăng nhập
            //    foreach (Window window in Application.Current.Windows)
            //    {
            //        if (window is LoginView)
            //        {
            //            window.Close();
            //            break;
            //        }
            //    }
            //});
        }
    }
}
