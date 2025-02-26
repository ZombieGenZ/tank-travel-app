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
using TiketManagementV2.Controls;
using TiketManagementV2.Model;
using TiketManagementV2.Services;

namespace TiketManagementV2.View
{
    /// <summary>
    /// Interaction logic for BanAccountView.xaml
    /// </summary>
    public partial class BanAccountView : Window
    {
        private INotificationService _notificationService;
        private ApiServices _service;
        private string _user_id;
        public BanAccountView(string user_id)
        {
            InitializeComponent();
            _user_id = user_id;
            _service = new ApiServices();
            _notificationService = new NotificationService();
            var selectedDate = DateTime.Now;
            txtSelectedDateTime.Text = selectedDate.ToString("HH:mm dd/MM/yyyy");
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private async void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //_circularLoadingControl.Visibility = Visibility.Visible;

                string reason = txtReason.Text;
                DateTime endTime = new DateTime();
                string endTimeStr = txtSelectedDateTime.Text;

                if (string.IsNullOrWhiteSpace(reason))
                {
                    reason = "Bạn đã vi phạm điều khoản dịch vụ";
                }

                if (!DateTime.TryParseExact(endTimeStr,
                        "HH:mm dd/MM/yyyy",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out endTime))
                {
                    _notificationService.ShowNotification("Lỗi", "Vui lòng điền đẩy đủ thông tin",
                        NotificationType.Warning);
                    return;
                }

                dynamic data = await Banned(reason, endTime.ToUniversalTime().ToString("o"));

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

                if (data.message == "Khóa tài khoản thành công!")
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

        private async Task<dynamic> Banned(string reason, string expired_at)
        {
            try
            {
                Dictionary<string, string> accountHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {Properties.Settings.Default.access_token}" }
                };
                var accountBody = new
                {
                    refresh_token = Properties.Settings.Default.refresh_token,
                    user_id = _user_id,
                    reason,
                    expired_at
                };

                dynamic data = await _service.PutWithHeaderAndBodyAsync("api/account-management/ban-account", accountHeader, accountBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private void BtnSelectDateTime_Click(object sender, RoutedEventArgs e)
        {
            var selectedDate = Calendar.SelectedDate ?? DateTime.Now;

            var time = Clock.Time;

            var combinedDateTime = new DateTime(
                selectedDate.Year,
                selectedDate.Month,
                selectedDate.Day,
                time.Hour,
                time.Minute,
                0
            );

            txtSelectedDateTime.Text = combinedDateTime.ToString("HH:mm dd/MM/yyyy");
        }
    }
}
