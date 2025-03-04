using SocketIO.Core;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TiketManagementV2.Model;
using TiketManagementV2.Services;
using TiketManagementV2.ViewModel;
using Newtonsoft.Json.Linq;
namespace TiketManagementV2.View
{
    /// <summary>
    /// Interaction logic for BusView.xaml
    /// </summary>
    public partial class BusView : Window, INotifyPropertyChanged
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
        public BusView(dynamic user)
        {
            InitializeComponent();
            LoadingControl.Visibility = Visibility.Hidden;
            var notificationService = new NotificationService();
            _user = user;
            var viewModel = new MainViewModel(notificationService, _user);
            DataContext = viewModel;
            _service = new ApiServices();
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
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
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

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginView loginView = new LoginView();
            loginView.Show();
            this.Close();
        }

        private void btnDropdown_Click(object sender, RoutedEventArgs e)
        {
            if (userDropdown.IsOpen == false)
                userDropdown.IsOpen = true;
            else userDropdown.IsOpen = false;
        }


        private void pnlControlBar_MouseEnter(object sender, MouseEventArgs e)
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
