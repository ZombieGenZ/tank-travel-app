using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using TiketManagementV2.Controls;
using TiketManagementV2.Model;
using TiketManagementV2.Services;

namespace TiketManagementV2.View
{
    public partial class ImageGalleryView : Window, INotifyPropertyChanged
    {
        private ObservableCollection<string> _imagePaths;
        public ObservableCollection<string> ImagePaths
        {
            get { return _imagePaths; }
            set
            {
                _imagePaths = value;
                OnPropertyChanged(nameof(ImagePaths));
            }
        }

        private ApiServices _service;
        private INotificationService _notificationService;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ImageGalleryView(string id)
        {
            InitializeComponent();
            DataContext = this;
            _service = new ApiServices();
            _notificationService = new NotificationService();

            ImagePaths = new ObservableCollection<string>();

            LoadManagedVehicles(id);
        }

        private async Task<dynamic> GetManagedVehicleData(string vehicle_id)
        {
            try
            {
                string access_token = Properties.Settings.Default.access_token;
                string refresh_token = Properties.Settings.Default.refresh_token;

                Dictionary<string, string> getVehicleDataHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {access_token}" }
                };
                var getVehicleDataBody = new
                {
                    refresh_token,
                    vehicle_id
                };

                return await _service.PostWithHeaderAndBodyAsync("api/vehicle/get-vehicle-preview", getVehicleDataHeader, getVehicleDataBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public async Task LoadManagedVehicles(string vehicle_id)
        {
            try
            {
                //_circularLoadingControl.Visibility = Visibility.Visible;

                dynamic data = await GetManagedVehicleData(vehicle_id);

                if (data == null || data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" ||
                    data.message == "Refresh token không hợp lệ" ||
                    data.message == "Bạn không có quyền thực hiện hành động này")
                {
                    _notificationService.ShowNotification("Error", data?.message ?? "Error loading data",
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

                if (data.message == "Lấy thông tin phương tiện thất bại")
                {
                    _notificationService.ShowNotification("Error", (string)data.message, NotificationType.Warning);
                    return;
                }

                foreach (string item in data.result)
                {
                    ImagePaths.Add(item);
                }

                if (ImagePaths.Count > 0)
                {
                    LoadImageToMainDisplay(ImagePaths[0]);
                    ThumbnailList.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification("Error", ex.Message, NotificationType.Error);
            }
            finally
            {
                //_circularLoadingControl.Visibility = Visibility.Collapsed;
            }
        }

        private async void LoadImageToMainDisplay(string imageUrl)
        {
            try
            {
                var bitmap = new BitmapImage();

                using (var client = new HttpClient())
                {
                    var imageData = await client.GetByteArrayAsync(imageUrl);

                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = new MemoryStream(imageData);
                    bitmap.EndInit();
                    bitmap.Freeze(); // Tối ưu hiệu suất

                    MainImage.Source = bitmap;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThumbnailList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThumbnailList.SelectedItem is string selectedImageUrl)
            {
                LoadImageToMainDisplay(selectedImageUrl);
                ThumbnailList.ScrollIntoView(selectedImageUrl);
                ThumbnailList.Focus();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Down || e.Key == Key.Right)
            {
                if (ThumbnailList.SelectedIndex < ImagePaths.Count - 1)
                {
                    ThumbnailList.SelectedIndex++;
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Up || e.Key == Key.Left)
            {
                if (ThumbnailList.SelectedIndex > 0)
                {
                    ThumbnailList.SelectedIndex--;
                }
                e.Handled = true;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}