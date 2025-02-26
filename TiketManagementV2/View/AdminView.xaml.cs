using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using TiketManagementV2.Services;
using TiketManagementV2.ViewModel;
using TiketManagementV2.Model;
using SocketIO.Core;
using System.ComponentModel;

namespace TiketManagementV2.View
{
    public partial class AdminView : Window, INotifyPropertyChanged
    {
        private ApiServices _service;
        private dynamic _user;
        private SocketIOClient.SocketIO socket;
        private INotificationService _notificationService;
        private bool _hasNewNotifications;
        public bool HasNewNotifications
        {
            get { return _hasNewNotifications; }
            set
            {
                _hasNewNotifications = value;
                OnPropertyChanged(nameof(HasNewNotifications));
            }
        }

        public AdminView(dynamic user)
        {
            InitializeComponent();
            LoadingControl.Visibility = Visibility.Hidden;
            _notificationService = new NotificationService();
            _user = user;
            var viewModel = new MainViewModel(_notificationService, _user, LoadingControl);
            DataContext = viewModel;
            _service = new ApiServices();

            InitializeSocketIO();
        }

        private void InitializeSocketIO()
        {
            try
            {
                string serverUrl = Properties.Settings.Default.host;
                socket = new SocketIOClient.SocketIO(serverUrl, new SocketIOOptions { EIO = EngineIO.V3 });

                socket.OnError += (sender, e) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _notificationService.ShowNotification("Lỗi", "Không thể kết nối đến máy chủ: " + e,
                            NotificationType.Error);
                    });
                };

                socket.On("new-private-notificaton", response =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            dynamic data = response.GetValue<object>(0);
                            string jsonString = data.GetRawText();
                            var jsonObject = JObject.Parse(jsonString);

                            string sender = jsonObject["sender"].ToString();
                            string message = jsonObject["message"].ToString();

                            _notificationService.ShowNotification(sender, message, NotificationType.Info);

                            if (DataContext is MainViewModel viewModel)
                            {
                                viewModel.HasNewNotifications = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing notification: {ex.Message}");
                        }
                    });
                });

                Task.Run(async () =>
                {
                    await socket.ConnectAsync();
                    await socket.EmitAsync("connect-user-realtime", Properties.Settings.Default.refresh_token);
                });
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification("Lỗi kết nối",
                    $"Không thể khởi tạo kết nối Socket.IO: {ex.Message}",
                    NotificationType.Error);
            }
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);
        private void pnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void pnlControlBar_MouseEnter(object sender, MouseEventArgs e)
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        private async void btnClose_Click(object sender, RoutedEventArgs e)
        {
            bool remember = Properties.Settings.Default.remember_login;

            if (!remember)
            {
                await DeleteLogout();
            }

            Application.Current.Shutdown();
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else this.WindowState = WindowState.Normal;
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void btnDropdown_Click(object sender, RoutedEventArgs e)
        {
            userDropdown.IsOpen = true;
            btnDropdown.IsEnabled = false; // Khóa nút khi dropdown mở
        }

        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            userDropdown.IsOpen = false;
        }

        private async Task<bool> DeleteLogout()
        {
            try
            {
                var logoutBody = new
                {
                    refresh_token = Properties.Settings.Default.refresh_token
                };

                await _service.DeleteWithBodyAsync("api/users/logout", logoutBody);

                Properties.Settings.Default.access_token = "";
                Properties.Settings.Default.refresh_token = "";
                Properties.Settings.Default.Save();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        private async void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            await DeleteLogout();

            LoginView loginView = new LoginView();
            loginView.Show();
            this.Close();
        }

        private void userDropdown_Closed(object sender, EventArgs e)
        {
            btnDropdown.IsEnabled = true; // Mở khóa nút khi dropdown đóng
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
