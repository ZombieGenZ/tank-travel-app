﻿using Newtonsoft.Json.Linq;
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
using System.Windows.Markup;
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
            _service = new ApiServices();
            InitializeComponent();
            LoadingControl.Visibility = Visibility.Collapsed;
            checkviewModel = new LoginViewModel(new NotificationService()); // Inject NotificationService
            notificationService = new NotificationService();  // Initialize the field
            var viewModel = new MainViewModel(notificationService);
            DataContext = viewModel;

            InitializeLoginCheck();
        }

        private void InitializeLoginCheck()
        {
            //Properties.Settings.Default.access_token = "";
            //Properties.Settings.Default.refresh_token = "";
            //Properties.Settings.Default.Save();


            LoadingControl.Visibility = Visibility.Visible;
            Task.Run(async () =>
            {
                try
                {
                    await CheckLogin();
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        notificationService?.ShowNotification(
                            "Lỗi!",
                            "Không thể kết nối đến máy chủ",
                            NotificationType.Error
                        );
                        LoadingControl.Visibility = Visibility.Collapsed;
                    });
                }
            });
        }

        private string FormatDate(DateTime date)
        {
            var second = date.Second.ToString("D2");
            var minute = date.Minute.ToString("D2");
            var hour = date.Hour.ToString("D2");
            var day = date.Day.ToString("D2");
            var month = date.Month.ToString("D2");
            var year = date.Year;

            return $"{hour}:{minute}:{second} {day}/{month}/{year}";
        }

        private async Task CheckLogin()
        {
            bool rememberLogin = Properties.Settings.Default.remember_login;
            string access_token = Properties.Settings.Default.access_token;
            string refresh_token = Properties.Settings.Default.refresh_token;

            if (string.IsNullOrWhiteSpace(access_token) ||
                string.IsNullOrWhiteSpace(refresh_token) ||
                !rememberLogin)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadingControl.Visibility = Visibility.Collapsed;
                });
                return;
            }

            dynamic userData = await GetUserData(access_token, refresh_token);

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (userData == null)
                {
                    notificationService.ShowNotification(
                        "Lỗi",
                        "Không thể kết nối đến máy chủ",
                        NotificationType.Error
                    );
                    LoadingControl.Visibility = Visibility.Collapsed;
                    return;
                }

                if (userData.user.penalty != null)
                {
                    LoadingControl.Visibility = Visibility.Collapsed;
                    return;
                }

                access_token = userData.authenticate.access_token;
                refresh_token = userData.authenticate.refresh_token;

                Properties.Settings.Default.access_token = access_token;
                Properties.Settings.Default.refresh_token = refresh_token;
                Properties.Settings.Default.Save();

                Window nextView;

                if (userData.user.permission == 2)
                {
                    nextView = new AdminView(userData.user);
                    nextView.Show();
                }
                else if (userData.user.permission == 1)
                {
                    nextView = new BusView(userData.user);
                    nextView.Show();
                }
                else
                {
                    LoadingControl.Visibility = Visibility.Collapsed;
                    return;
                }

                // Close login window
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is LoginView)
                    {
                        window.Close();
                        break;
                    }
                }
            });
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
            LoadingControl.Visibility = Visibility.Visible;
            try
            {
                string username = txtUser.Text.Trim();
                string password = txtPass.Password.Trim();

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    notificationService.ShowNotification(
                        "Lỗi kiểu dử liệu đầu vào",
                        "Vui lòng điền đầy đủ thông tin",
                        NotificationType.Warning
                    );
                    txtUser.Focus();
                    LoadingControl.Visibility = Visibility.Collapsed;
                    return;
                }

                if (!ValidationHelper.IsValidEmail(username))
                {
                    notificationService.ShowNotification(
                        "Lỗi kiểu dử liệu đầu vào",
                        "Địa chỉ eamil không đúng định dạn",
                        NotificationType.Warning
                    );
                    LoadingControl.Visibility = Visibility.Collapsed;
                    return;
                }

                dynamic data = await GetLogin(username, password);

                if (data == null)
                {
                    notificationService.ShowNotification(
                        "Lỗi",
                        "Không thể kết nối đến máy chủ",
                        NotificationType.Error
                    );
                    LoadingControl.Visibility = Visibility.Collapsed;
                    return;
                }

                if (data.message == "Vui lòng thay đổi mật khẩu tạm thời trước khi đăng nhập")
                {
                    LoadingControl.Visibility = Visibility.Collapsed;
                    ChangePasswordView view = new ChangePasswordView(txtUser.Text, txtPass.Password);
                    view.ShowDialog();

                    txtUser.Text = "";
                    txtPass.Password = "";

                    return;
                }

                if (data.message == "Lỗi dữ liệu đầu vào")
                {
                    foreach (dynamic item in data.errors)
                    {
                       notificationService.ShowNotification(
                            "Lỗi kiểu dử liệu đầu vào",
                            (string)item.Value.msg,
                            NotificationType.Warning
                        );
                    }
                    LoadingControl.Visibility = Visibility.Collapsed;
                    return;
                }

                if (data.message == "Đăng nhập thành công!")
                {
                    string access_token = data.authenticate.access_token;
                    string refresh_token = data.authenticate.refresh_token;

                    Properties.Settings.Default.remember_login = RememberLogin.IsChecked ?? false;
                    Properties.Settings.Default.access_token = access_token;
                    Properties.Settings.Default.refresh_token = refresh_token;
                    Properties.Settings.Default.Save();

                    dynamic userData = await GetUserData(access_token, refresh_token);

                    if (userData == null)
                    {
                        notificationService.ShowNotification(
                            "Lỗi",
                            "Không thể kết nối đến máy chủ",
                            NotificationType.Error
                        );
                        LoadingControl.Visibility = Visibility.Collapsed;
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
                            nextView = new AdminView(userData.user); // Giao diện dành cho Admin
                            nextView.Show();
                        }
                        else if (userData.user.permission == 1)
                        {
                            nextView = new BusView(userData.user); // Giao diện dành cho Bus Operator
                            nextView.Show();
                        }
                        else
                        {
                            notificationService.ShowNotification(
                                "Error",
                                "Bạn không có quyền thực hiện hành động này",
                                NotificationType.Warning
                            );
                            LoadingControl.Visibility = Visibility.Collapsed;
                            return;
                        }

                        LoadingControl.Visibility = Visibility.Collapsed;

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
                        "Thành công!",
                        (string)data.message,
                        NotificationType.Success
                    );
                }
            }
            catch
            {
                notificationService.ShowNotification(
                    "Lỗi",
                    "Không thể kết nối đến máy chủ",
                    NotificationType.Error
                );
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            ResetPasswordView resetPasswordView = new ResetPasswordView();
            resetPasswordView.ShowDialog();
        }
    }
}
