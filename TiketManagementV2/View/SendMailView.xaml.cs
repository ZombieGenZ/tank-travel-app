using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace TiketManagementV2.View
{
    /// <summary>
    /// Interaction logic for SendMailView.xaml
    /// </summary>
    public partial class SendMailView : Window
    {
        private ApiServices _service;
        private string _user_id;
        private INotificationService _notificationService;
        public SendMailView(string user_id)
        {
            InitializeComponent();
            _service = new ApiServices();
            _notificationService = new NotificationService();
            _user_id = user_id;
        }

        private async Task<dynamic> SendMail(string message)
        {
            try
            {
                Dictionary<string, string> mailHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {Properties.Settings.Default.access_token}" }
                };
                var mailBody = new
                {
                    refresh_token = Properties.Settings.Default.refresh_token,
                    user_id = _user_id,
                    message
                };

                dynamic data = await _service.PostWithHeaderAndBodyAsync("api/account-management/send-notification", mailHeader, mailBody);

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
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private async void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //_circularLoadingControl.Visibility = Visibility.Visible;

                string message = txtMessage.Text;

                if (string.IsNullOrWhiteSpace(message))
                {
                    _notificationService.ShowNotification("Lỗi", "Vui lòng điền đẩy đủ thông tin",
                        NotificationType.Warning);
                    return;
                }

                dynamic data = await SendMail(message);

                if (data == null)
                {
                    _notificationService.ShowNotification(
                        "Lỗi!",
                        "Không thể kết nối đến máy chủ",
                        NotificationType.Warning
                    );
                    //_circularLoadingControl.Visibility = Visibility.Collapsed;
                    return;
                }

                if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" ||
                    data.message == "Refresh token không hợp lệ")
                {
                    _notificationService.ShowNotification(
                        "Lỗi!",
                        (string)data.message,
                        NotificationType.Warning
                    );
                    //_circularLoadingControl.Visibility = Visibility.Collapsed;
                    return;
                }

                if (data.message == "Bạn không có quyền thực hiện hành động này")
                {
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    _notificationService.ShowNotification(
                        "Lỗi!",
                        (string)data.message,
                        NotificationType.Warning
                    );
                    //_circularLoadingControl.Visibility = Visibility.Collapsed;
                    return;
                }

                if (data.message == "Lỗi dữ liệu đầu vào")
                {
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    foreach (dynamic item in data.errors)
                    {
                        _notificationService.ShowNotification(
                            "Lỗi kiểu dử liệu đầu vào",
                            (string)item.Value.msg,
                            NotificationType.Warning
                        );
                    }
                    //_circularLoadingControl.Visibility = Visibility.Collapsed;
                    return;
                }

                if (data.message == "Gửi thông báo thành công!")
                {
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    _notificationService.ShowNotification(
                        "Thành công",
                        (string)data.message,
                        NotificationType.Success
                    );
                    Close();
                    //_circularLoadingControl.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    _notificationService.ShowNotification(
                        "Lõi!",
                        (string)data.message,
                        NotificationType.Error
                    );
                    //_circularLoadingControl.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification(
                    "Lỗi!",
                    ex.Message,
                    NotificationType.Error
                );
                //_circularLoadingControl.Visibility = Visibility.Collapsed;
                throw;
            }
            finally
            {
                //_circularLoadingControl.Visibility = Visibility.Collapsed;
            }
        }
    }
}
