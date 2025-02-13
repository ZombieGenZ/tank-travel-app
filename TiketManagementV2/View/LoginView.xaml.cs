using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
        private ApiServices _service;

        public LoginView()
        {
            InitializeComponent();
            checkviewModel = new LoginViewModel(new NotificationService()); // Inject NotificationService
            notificationService = new NotificationService();  // Initialize the field
            var viewModel = new MainViewModel(notificationService);
            _service = new ApiServices();
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
                var loginBody = new
                {
                    email = tenDangNhap,
                    password = matKhau
                };

                dynamic data = await _service.PostWithBodyAsync("api/users/login-manage", loginBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task<dynamic> GetUserData(string access_token, string refresh_token)
        {
            try
            {
                _service = new ApiServices();

                Dictionary<string, string> getUserDataHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {access_token}" }
                };
                var getUserDataBody = new
                {
                    refresh_token = refresh_token
                };

                dynamic data = await _service.PostWithHeaderAndBodyAsync("api/users/get-user-infomation", getUserDataHeader, getUserDataBody);

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

                if (data.message == "Login failed")
                {
                    notificationService.ShowNotification(
                        "Error",
                        data.message,
                        NotificationType.Error
                    );
                    return;
                }

                if (data.message == "Login successful!")
                {
                    string access_token = data.authenticate.access_token;
                    string refresh_token = data.authenticate.refresh_token;

                    Properties.Settings.Default.access_token = access_token;
                    Properties.Settings.Default.refresh_token = refresh_token;
                    Properties.Settings.Default.Save();

                    dynamic userData = await GetUserData(access_token, refresh_token);

                    if (userData == null)
                    {
                        notificationService.ShowNotification(
                            "Error",
                            "Error connecting to server!",
                            NotificationType.Error
                        );
                        return;
                    }

                    access_token = userData.authenticate.access_token;
                    refresh_token = userData.authenticate.refresh_token;

                    Properties.Settings.Default.access_token = access_token;
                    Properties.Settings.Default.refresh_token = refresh_token;
                    Properties.Settings.Default.Save();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Window nextView;

                        if (userData.user.permission == 2)
                        {
                            nextView = new AdminView(); // Giao diện dành cho Admin
                            nextView.Show();
                        }
                        else if (userData.user.permission == 1)
                        {
                            nextView = new BusView(); // Giao diện dành cho Bus Operator
                            nextView.Show();
                        }
                        else
                        {
                            notificationService.ShowNotification(
                                "Error",
                                "YOU ARE A USER? GET OUT",
                                NotificationType.Warning
                            );
                        }

                        // Đóng cửa sổ đăng nhập
                        foreach (Window window in Application.Current.Windows)
                        {
                            if (window is LoginView)
                            {
                                window.Close();
                                break;
                            }
                        }
                    });

                    notificationService.ShowNotification(
                        "Success",
                        (string)data.message,
                        NotificationType.Success
                    );
                }

                notificationService.ShowNotification(
                    "Error",
                    "Error connecting to server!",
                    NotificationType.Error
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
