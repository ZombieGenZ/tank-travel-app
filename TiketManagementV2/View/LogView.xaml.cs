using Newtonsoft.Json.Linq;
using SocketIO.Core;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TiketManagementV2.Services;
using TiketManagementV2.ViewModel;

namespace TiketManagementV2.View
{
    /// <summary>
    /// Interaction logic for LogView.xaml
    /// </summary>
    public partial class LogView : UserControl, INotifyPropertyChanged
    {
        private SocketIOClient.SocketIO socket;
        public class LogItem
        {
            public LogType LogType { get; set; }
            public string Message { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public enum LogType
        {
            Success,
            Warning,
            Error
        }

        private ObservableCollection<LogItem> _logItems;
        public ObservableCollection<LogItem> LogItems
        {
            get => _logItems;
            set
            {
                _logItems = value;
                //OnPropertyChanged();
                OnPropertyChanged(nameof(HasLogItems));
            }
        }

        public bool HasLogItems => LogItems?.Count > 0;
        private INotificationService _notificationService;
        public LogView()
        {
            InitializeComponent();
            DataContext = this;

            LogItems = new ObservableCollection<LogItem>();
            LogItems.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(LogItems));
            };
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

                socket.On("new-system-log", response =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            dynamic data = response.GetValue<object>(0);
                            string jsonString = data.GetRawText();
                            var jsonObject = JObject.Parse(jsonString);

                            int logType = (int)jsonObject["log_type"];
                            string content = jsonObject["content"].ToString();
                            DateTime time = (DateTime)jsonObject["time"];

                            LogItems.Add(new LogItem()
                            {
                                LogType = logType == 0 ? LogType.Success : logType == 1 ? LogType.Warning : LogType.Error,
                                Message = content,
                                Timestamp = time
                            });
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
                    await socket.EmitAsync("connect-admin-realtime", Properties.Settings.Default.refresh_token);
                });
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification("Lỗi kết nối",
                    $"Không thể khởi tạo kết nối Socket.IO: {ex.Message}",
                    NotificationType.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
