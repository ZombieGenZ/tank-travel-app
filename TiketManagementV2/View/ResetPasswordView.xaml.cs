using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TiketManagementV2.Model;
using TiketManagementV2.Services;
using TiketManagementV2.Helpers;

namespace TiketManagementV2.View
{
    public partial class ResetPasswordView : Window
    {
        private INotificationService _notificationService;

        private ApiServices _service;

        public ResetPasswordView()
        {
            _service = new ApiServices();
            InitializeComponent();
            _notificationService = new NotificationService();

        }

        private async Task<dynamic> GetResetPassWord(string tenDangNhap)
        {
            try
            {
                var RsPassBody = new
                {
                    email = tenDangNhap
                };

                dynamic data = await _service.PostWithBodyAsync("api/users/send-email-forgot-password", RsPassBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            LoadingControl.Visibility = Visibility.Visible;
            string confirmEmail = txtConfirmEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(confirmEmail))
            {
                _notificationService.ShowNotification(
                    "Lỗi kiểu dử liệu đầu vào",
                    "Vui lòng điền đầy đủ thông tin",
                    NotificationType.Warning
                );
                txtConfirmEmail.Focus();
                LoadingControl.Visibility = Visibility.Collapsed;
                return;
            }

            if (!ValidationHelper.IsValidEmail(confirmEmail))
            {
                _notificationService.ShowNotification(
                    "Lỗi kiểu dử liệu đầu vào",
                    "Địa chỉ eamil không đúng định dạn",
                    NotificationType.Warning
                );
                LoadingControl.Visibility = Visibility.Collapsed;
                return;
            }

            dynamic data = await GetResetPassWord(confirmEmail);

            if (data == null)
            {
                _notificationService.ShowNotification(
                    "Lỗi",
                    "Không thể kết nối đến máy chủ",
                    NotificationType.Error
                );
                LoadingControl.Visibility = Visibility.Collapsed;
                return;
            }
            if (data.message == "Lỗi dữ liệu đầu vào")
            {
                _notificationService.ShowNotification(
                    "Lỗi",
                    "Lỗi kiểu dữ liệu đầu vào",
                    NotificationType.Warning
                    );
                LoadingControl.Visibility= Visibility.Collapsed;
                return;
            }
            if (data.message == "Email yêu cầu đặt lại mật khẩu đã được gửi thành công! Vui lòng kiểm tra hộp thư của bạn")
            {
                _notificationService.ShowNotification(
                    "Success",
                    (string)data.message,
                    NotificationType.Success
                );
                LoadingControl.Visibility = Visibility.Hidden;
                this.Close();
                return;
            }

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
