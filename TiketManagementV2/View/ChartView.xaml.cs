using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;
using TiketManagementV2.Controls;
using TiketManagementV2.Model;
using TiketManagementV2.Services;
using TiketManagementV2.ViewModel;
using static TiketManagementV2.View.AccountView;

namespace TiketManagementV2.View
{
    public partial class ChartView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ChartValues<double> _hiringSources;
        public ChartValues<double> HiringSources
        {
            get => _hiringSources;
            set
            {
                _hiringSources = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HiringSources)));
            }
        }

        private List<string> _sourceLabels;
        public List<string> SourceLabels
        {
            get => _sourceLabels;
            set
            {
                _sourceLabels = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceLabels)));
            }
        }

        public class RankingItem
        {
            public string Rank { get; set; }
            public string Name { get; set; }
            public ImageSource ProfileImage { get; set; }
            public string Revenue { get; set; }
        }

        private List<RankingItem> _rankingItems;
        public List<RankingItem> RankingItems
        {
            get => _rankingItems;
            set
            {
                _rankingItems = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RankingItems)));
            }
        }

        private ApiServices _service;
        private INotificationService _notificationService;
        public Func<double, string> YFormatter { get; set; }
        private CircularLoadingControl _circularLoadingControl;
        public ChartView(CircularLoadingControl loading)
        {
            try
            {
                InitializeComponent();
                _circularLoadingControl = loading;
                _service = new ApiServices();
                _notificationService = new NotificationService();
                this.DataContext = this;
                LoadAllDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo: {ex.Message}\n{ex.StackTrace}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadAllDataAsync()
        {
            try
            {
                _circularLoadingControl.Visibility = Visibility.Visible;

                var tasks = new[]
                {
                    LoadChart(),
                    LoadCompareDeals(),
                    LoadCompareTicket(),
                    LoadCompareRevenue(),
                    LoadRankingData()
                };

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification(
                    "Error",
                    "Lỗi khi tải dử liệu: " + ex.Message,
                    NotificationType.Error
                );
            }
            finally
            {
                _circularLoadingControl.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<dynamic> GetChartData()
        {
            try
            {
                string access_token = Properties.Settings.Default.access_token;
                string refresh_token = Properties.Settings.Default.refresh_token;

                Dictionary<string, string> statisticalHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {access_token}" }
                };
                var getVehicleDataBody = new
                {
                    refresh_token
                };

                return await _service.PostWithHeaderAndBodyAsync("api/statistical/chart/revenue", statisticalHeader, getVehicleDataBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task LoadChart()
        {
            try
            {
                dynamic data = await GetChartData();

                if (data == null)
                {
                    _notificationService.ShowNotification("Lỗi", "Không thể kết nối đến máy chủ",
                        NotificationType.Error);
                    return;
                }

                if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" ||
                    data.message == "Refresh token không hợp lệ" ||
                    data.message == "Bạn không có quyền thực hiện hành động này")
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

                if (data.message == "Lấy thông tin thống kê thất bại")
                {
                    _notificationService.ShowNotification("Error", (string)data.message, NotificationType.Warning);
                    return;
                }

                List<string> names = new List<string>();
                ChartValues<double> values = new ChartValues<double>();
                
                foreach (dynamic item in data.result)
                {
                    names.Add((string)item.date);
                    values.Add((double)item.totalRevenue);
                }

                SourceLabels = new List<string>(names);
                HiringSources = new ChartValues<double>(values);

                double maxValue = HiringSources.Max();
                double interval = maxValue / 4;

                YFormatter = value =>
                {
                    if (Math.Abs(value % interval) < interval * 0.1 || Math.Abs(value) < 0.1)
                        return value.ToString("0");
                    return null;
                };
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification("Error", ex.Message, NotificationType.Error);
            }
        }

        private async Task LoadCompareDeals()
        {
            try
            {
                dynamic data = await GetCompareDealsData();

                if (data == null)
                {
                    _notificationService.ShowNotification("Lỗi", "Không thể kết nối đến máy chủ",
                        NotificationType.Error);
                    return;
                }

                if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" ||
                    data.message == "Refresh token không hợp lệ" ||
                    data.message == "Bạn không có quyền thực hiện hành động này")
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

                if (data.message == "Lấy thông tin thống kê thất bại")
                {
                    _notificationService.ShowNotification("Error", (string)data.message, NotificationType.Warning);
                    return;
                }

                double value = (double)data.result;

                if (value >= 0)
                {
                    txtHoaDon.Text = $"+{value}%";
                    txtHoaDon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#4CAF50");
                    imgHoaDon.Source = new BitmapImage(new Uri("pack://application:,,,/Images/increase.png", UriKind.Absolute));
                }
                else
                {
                    txtHoaDon.Text = $"{value}%";
                    txtHoaDon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#F44336");
                    imgHoaDon.Source = new BitmapImage(new Uri("pack://application:,,,/Images/decrease.png", UriKind.Absolute));
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification("Error", ex.Message, NotificationType.Error);
            }
        }

        private async Task LoadCompareRevenue()
        {
            try
            {
                dynamic data = await GetCompareRevenueData();

                if (data == null)
                {
                    _notificationService.ShowNotification("Lỗi", "Không thể kết nối đến máy chủ",
                        NotificationType.Error);
                    return;
                }

                if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" ||
                    data.message == "Refresh token không hợp lệ" ||
                    data.message == "Bạn không có quyền thực hiện hành động này")
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

                if (data.message == "Lấy thông tin thống kê thất bại")
                {
                    _notificationService.ShowNotification("Error", (string)data.message, NotificationType.Warning);
                    return;
                }

                double value = (double)data.result;

                if (value >= 0)
                {
                    txtDoanhThu.Text = $"+{value}%";
                    txtDoanhThu.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#4CAF50");
                    imgDoanhThu.Source = new BitmapImage(new Uri("pack://application:,,,/Images/increase.png", UriKind.Absolute));
                }
                else
                {
                    txtDoanhThu.Text = $"{value}%";
                    txtDoanhThu.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#F44336");
                    imgDoanhThu.Source = new BitmapImage(new Uri("pack://application:,,,/Images/decrease.png", UriKind.Absolute));
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification("Error", ex.Message, NotificationType.Error);
            }
        }

        private async Task LoadCompareTicket()
        {
            try
            {
                dynamic data = await GetCompareTicketData();

                if (data == null)
                {
                    _notificationService.ShowNotification("Lỗi", "Không thể kết nối đến máy chủ",
                        NotificationType.Error);
                    return;
                }

                if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" ||
                    data.message == "Refresh token không hợp lệ" ||
                    data.message == "Bạn không có quyền thực hiện hành động này")
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

                if (data.message == "Lấy thông tin thống kê thất bại")
                {
                    _notificationService.ShowNotification("Error", (string)data.message, NotificationType.Warning);
                    return;
                }

                double value = (double)data.result;

                if (value >= 0)
                {
                    txtVeDaDat.Text = $"+{value}%";
                    txtVeDaDat.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#4CAF50");
                    imgVeDaDat.Source = new BitmapImage(new Uri("pack://application:,,,/Images/increase.png", UriKind.Absolute));
                }
                else
                {
                    txtVeDaDat.Text = $"{value}%";
                    txtVeDaDat.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#F44336");
                    imgVeDaDat.Source = new BitmapImage(new Uri("pack://application:,,,/Images/decrease.png", UriKind.Absolute));
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification("Error", ex.Message, NotificationType.Error);
            }
        }

        private async Task<dynamic> GetCompareDealsData()
        {
            try
            {
                string access_token = Properties.Settings.Default.access_token;
                string refresh_token = Properties.Settings.Default.refresh_token;

                Dictionary<string, string> statisticalHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {access_token}" }
                };
                var getVehicleDataBody = new
                {
                    refresh_token
                };

                return await _service.PostWithHeaderAndBodyAsync("api/statistical/compare/deals", statisticalHeader, getVehicleDataBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task<dynamic> GetCompareRevenueData()
        {
            try
            {
                string access_token = Properties.Settings.Default.access_token;
                string refresh_token = Properties.Settings.Default.refresh_token;

                Dictionary<string, string> statisticalHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {access_token}" }
                };
                var getVehicleDataBody = new
                {
                    refresh_token
                };

                return await _service.PostWithHeaderAndBodyAsync("api/statistical/compare/revenue", statisticalHeader, getVehicleDataBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task<dynamic> GetCompareTicketData()
        {
            try
            {
                string access_token = Properties.Settings.Default.access_token;
                string refresh_token = Properties.Settings.Default.refresh_token;

                Dictionary<string, string> statisticalHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {access_token}" }
                };
                var getVehicleDataBody = new
                {
                    refresh_token
                };

                return await _service.PostWithHeaderAndBodyAsync("api/statistical/compare/ticket", statisticalHeader, getVehicleDataBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task<dynamic> GetRankData()
        {
            try
            {
                string access_token = Properties.Settings.Default.access_token;
                string refresh_token = Properties.Settings.Default.refresh_token;

                Dictionary<string, string> statisticalHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {access_token}" }
                };
                var getVehicleDataBody = new
                {
                    refresh_token
                };

                return await _service.PostWithHeaderAndBodyAsync("api/statistical/top/revenue", statisticalHeader, getVehicleDataBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private ImageSource LoadSafeImage(string resourcePath)
        {
            try
            {
                // Sử dụng Pack URI để tải tài nguyên từ assembly
                string packUri = $"pack://application:,,,/TiketManagementV2;component{resourcePath}";
                return new BitmapImage(new Uri(packUri));
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                Console.WriteLine($"Không thể tải hình ảnh: {resourcePath}. Lỗi: {ex.Message}");

                // Trả về hình ảnh mặc định hoặc tạo một placeholder
                return CreatePlaceholderImage();
            }
        }

        private ImageSource CreatePlaceholderImage()
        {
            // Tạo một ảnh đơn giản màu xám để sử dụng làm placeholder
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(Brushes.LightGray, null, new Rect(0, 0, 40, 40));
            }

            RenderTargetBitmap rtb = new RenderTargetBitmap(40, 40, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(drawingVisual);
            return rtb;
        }

        private async Task LoadRankingData()
        {
            try
            {
                dynamic data = await GetRankData();

                if (data == null)
                {
                    _notificationService.ShowNotification("Lỗi", "Không thể kết nối đến máy chủ",
                        NotificationType.Error);
                    return;
                }

                if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" ||
                    data.message == "Refresh token không hợp lệ" ||
                    data.message == "Bạn không có quyền thực hiện hành động này")
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

                if (data.message == "Lấy thông tin thống kê thất bại")
                {
                    _notificationService.ShowNotification("Error", (string)data.message, NotificationType.Warning);
                    return;
                }

                int count = 1;
                RankingItems = new List<RankingItem>();

                foreach (dynamic item in data.result)
                {
                    CultureInfo culture = new CultureInfo("vi-VN");
                    string formattedAmount = ((double)item.totalRevenue).ToString("#,##0 \u20ab", culture);

                    RankingItems.Add(new RankingItem()
                    {
                        Rank = count.ToString(),
                        Name = (string)item.display_name,
                        ProfileImage = new BitmapImage(new Uri((string)item.avatar.url, UriKind.Absolute)),
                        Revenue = formattedAmount
                    });
                    count++;
                }

                RankingList.ItemsSource = RankingItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu bảng xếp hạng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}