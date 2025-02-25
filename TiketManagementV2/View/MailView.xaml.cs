using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIO.Core;
using SocketIOClient;
using TiketManagementV2.Controls;
using TiketManagementV2.Model;
using TiketManagementV2.Services;

namespace TiketManagementV2.View
{
    /// <summary>
    /// Interaction logic for MailView.xaml
    /// </summary>
    public partial class MailView : Window
    {
        private SocketIOClient.SocketIO socket;
        public ObservableCollection<Notification> Notifications { get; set; }

        private string _sessionTime;
        private int _current;
        private ApiServices _service;
        private INotificationService _notificationService;
        public MailView()
        {
            InitializeComponent();
            _sessionTime = DateTime.Now.ToString("o");
            _current = 0;
            _notificationService = new NotificationService();
            _service = new ApiServices();
            Notifications = new ObservableCollection<Notification>();

            LoadNotification();
            InitializeSocketIO();
            DataContext = this;
        }

        private void InitializeSocketIO()
        {
            try
            {
                string serverUrl = Properties.Settings.Default.host;

                socket = new SocketIOClient.SocketIO(serverUrl, new SocketIOOptions
                {
                    EIO = EngineIO.V3
                });

                socket.OnError += (sender, e) =>
                {
                    Console.WriteLine("Socket.IO Connection Error: " + e);
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

                            Notification notification = new Notification
                            {
                                Title = sender,
                                Message = message,
                                Time = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy")
                            };

                            Notifications.Insert(0, notification);
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

        private async Task<dynamic> GetNotificationData()
        {
            try
            {
                string access_token = Properties.Settings.Default.access_token;
                string refresh_token = Properties.Settings.Default.refresh_token;

                Dictionary<string, string> getNotificationHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {access_token}" }
                };
                var getNotificationDataBody = new
                {
                    refresh_token,
                    session_time = _sessionTime,
                    current = _current
                };

                return await _service.PostWithHeaderAndBodyAsync("api/notification-private/get-notification", getNotificationHeader, getNotificationDataBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public async Task LoadNotification()
        {
            try
            {
                //_circularLoadingControl.Visibility = Visibility.Visible;

                dynamic data = await GetNotificationData();

                if (data == null || data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" ||
                    data.message == "Refresh token không hợp lệ")
                {
                    _notificationService.ShowNotification("Lỗi", (string)data.message,
                        NotificationType.Error);
                    return;
                }

                Properties.Settings.Default.access_token = data.authenticate.access_token;
                Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                Properties.Settings.Default.Save();

                if (data.message == "Lỗi dữ liệu đầu vào")
                {
                    foreach (dynamic item in data.errors)
                    {
                        _notificationService.ShowNotification("Lỗi dữ liệu đầu vào", (string)item.Value.msg,
                            NotificationType.Warning);
                    }

                    return;
                }

                if (data.message == "Lấy thông báo thất bại")
                {
                    _notificationService.ShowNotification("Lỗi", (string)data.message, NotificationType.Warning);
                    return;
                }

                if (data.result.message == "Không tìm thấy thông báo nào phù hợp")
                {
                    // hiển thị không tìm thấy kết quả phù hợp
                    return;
                }

                foreach (dynamic item in data.result.notification)
                {
                    Notifications.Add(new Notification()
                    {
                        Title = item.sender,
                        Message = item.message,
                        Time = ((DateTime)item.created_at).ToString("HH:mm:ss dd/MM/yyyy")
                    });
                }

                _current = data.result.current;

                if ((bool)data.result.continued)
                {
                    //LoadMore.Visibility = Visibility.Visible;
                }
                else
                {
                    //LoadMore.Visibility = Visibility.Collapsed;
                }

                NotificationsList.ItemsSource = Notifications;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _notificationService.ShowNotification("Error", ex.Message, NotificationType.Error);
            }
            finally
            {
                //_circularLoadingControl.Visibility = Visibility.Collapsed;
            }
        }

        public class Notification : INotifyPropertyChanged
        {
            private string _title;
            private string _message;
            private string _time;

            public string Title
            {
                get => _title;
                set
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }

            public string Message
            {
                get => _message;
                set
                {
                    _message = value;
                    OnPropertyChanged(nameof(Message));
                }
            }

            public string Time
            {
                get => _time;
                set
                {
                    _time = value;
                    OnPropertyChanged(nameof(Time));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            // Ngắt kết nối socket trước khi đóng window
            if (socket != null && socket.Connected)
            {
                socket.DisconnectAsync();
            }
            this.Close();
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