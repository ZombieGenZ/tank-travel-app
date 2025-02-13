using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TiketManagementV2.Helpers;
using TiketManagementV2.Model;
using TiketManagementV2.Services;
using TiketManagementV2.ViewModel;

namespace TiketManagementV2.View
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        private readonly NotificationService notificationService;
        private readonly LoginViewModel checkviewModel;


        public LoginView()
        {
            InitializeComponent();
            checkviewModel = new LoginViewModel(new NotificationService()); // Inject NotificationService
            notificationService = new NotificationService();  // Initialize the field
            var viewModel = new MainViewModel(notificationService);
            DataContext = viewModel;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private async Task<dynamic> GetLogin(string tenDangNhap, string matKhau)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            int? role = null;
            try
            {
                string username = txtUser.Text.Trim();
                string password = txtPass.Password.Trim();

                if (string.IsNullOrWhiteSpace(username))
                {
                    notificationService.ShowNotification(
                        "Error",
                        "Username cannot be empty!",
                        NotificationType.Error
                    );
                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    notificationService.ShowNotification(
                        "Error",
                        "Password cannot be empty!",
                        NotificationType.Error
                    );
                    return;
                }

                if (!ValidationHelper.IsValidEmail(username))
                {
                    notificationService.ShowNotification(
                        "Error",
                        "Invalid email format! Please enter a valid email.",
                        NotificationType.Error
                    );
                    return;
                }

                dynamic data = await GetLogin(username, password);

                if (data == null)
                {
                    notificationService.ShowNotification(
                        "Error",
                        "Error connecting to server!",
                        NotificationType.Error
                    );
                    return;
                }

                if (data.message == "Input data error")
                {
                    foreach (dynamic item in data.errors)
                    {
                       notificationService.ShowNotification(
                            "Error",
                            (string)item.Value.msg,
                            NotificationType.Warning
                        );
                    }
                    return;
                }

                if (data.messgae == "Login failed")
                {
                    notificationService.ShowNotification(
                        "Error",
                        data.message,
                        NotificationType.Error
                    );
                    return;
                }

                notificationService.ShowNotification(
                    "Success",
                    (string)data.message,
                    NotificationType.Success
                );
            }
            catch (Exception ex)
            {
                notificationService.ShowNotification(
                    "Error",
                    "Error connecting to server!",
                    NotificationType.Error
                );
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


    }
}
