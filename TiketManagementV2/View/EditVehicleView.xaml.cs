using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using TiketManagementV2.Model;
using TiketManagementV2.Services;

namespace TiketManagementV2.View
{
    /// <summary>
    /// Interaction logic for EditVehicleView.xaml
    /// </summary>
    public partial class EditVehicleView : Window
    {
        private INotificationService _notificationService;
        private ApiServices _apiServices;
        public ObservableCollection<SeatTypeItem> SeatTypes { get; set; }
        public ObservableCollection<VehicleTypeItem> VehicleTypes { get; set; }
        private ApiServices _service;
        private Vehicle _vehicle;
        public EditVehicleView(Vehicle vehicle)
        {
            InitializeComponent();
            _service = new ApiServices();
            _apiServices = new ApiServices();
            SeatTypes = new ObservableCollection<SeatTypeItem>();
            VehicleTypes = new ObservableCollection<VehicleTypeItem>();
            LoadingControl.Visibility = Visibility.Collapsed;
            _notificationService = new NotificationService();
            _vehicle = vehicle;

            LoadData();

            if (vehicle.SeatType == "Ghế ngồi")
            {
                cmbSeatType.SelectedIndex = 0;
            }
            else if (vehicle.SeatType == "Giường nằm")
            {
                cmbSeatType.SelectedIndex = 1;
            }
            else if (vehicle.SeatType == "Ghế vừa nằm vừa ngồi")
            {
                cmbSeatType.SelectedIndex = 2;
            }
            else
            {
                cmbSeatType.SelectedIndex = -1;
            }

            if (vehicle.VehicleType == "Xe khách")
            {
                cmbVehicle.SelectedIndex = 0;
            }
            else if (vehicle.VehicleType == "Tàu hỏa")
            {
                cmbVehicle.SelectedIndex = 1;
            }
            else if (vehicle.VehicleType == "Máy bay")
            {
                cmbVehicle.SelectedIndex = 2;
            }
            else
            {
                cmbVehicle.SelectedIndex = -1;
            }

            txtSeats.Text = vehicle.Seats.ToString();
            txtRules.Text = vehicle.Rules;
            txtAmenities.Text = vehicle.Amenities;
            txtLic.Text = vehicle.LicensePlate;
        }

        private async Task<dynamic> GetVehicleType()
        {
            try
            {
                Dictionary<string, string> statisticsHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {Properties.Settings.Default.access_token}" }
                };
                var statisticsBody = new
                {
                    refresh_token = Properties.Settings.Default.refresh_token,
                };

                dynamic data = await _apiServices.PostWithHeaderAndBodyAsync("api/vehicle/get-vehicle-type", statisticsHeader, statisticsBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task<dynamic> GetSeatype()
        {
            try
            {
                Dictionary<string, string> statisticsHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {Properties.Settings.Default.access_token}" }
                };
                var statisticsBody = new
                {
                    refresh_token = Properties.Settings.Default.refresh_token,
                };

                dynamic data = await _apiServices.PostWithHeaderAndBodyAsync("api/vehicle/get-seat-type", statisticsHeader, statisticsBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task LoadVehicleType()
        {
            try
            {
                LoadingControl.Visibility = Visibility.Visible;

                dynamic data = await GetVehicleType();

                if (data == null)
                {
                    _notificationService.ShowNotification(
                        "Lỗi!",
                        "Không thể kết nối đến máy chủ",
                        NotificationType.Error
                    );
                    return;
                }

                if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" ||
                    data.message == "Refresh token không hợp lệ")
                {
                    _notificationService.ShowNotification(
                        "Lỗi!",
                        (string)data.message,
                        NotificationType.Error
                    );
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
                        NotificationType.Error
                    );
                    return;
                }

                foreach (dynamic item in data.vehicleType)
                {
                    VehicleTypes.Add(new VehicleTypeItem()
                    {
                        Id = item.value,
                        Name = item.display,
                    });
                }

                cmbVehicle.ItemsSource = VehicleTypes;
            }
            finally
            {
                LoadingControl.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadSeaType()
        {
            try
            {
                LoadingControl.Visibility = Visibility.Visible;

                dynamic data = await GetSeatype();

                if (data == null)
                {
                    _notificationService.ShowNotification(
                        "Lỗi!",
                        "Không thể kết nối đến máy chủ",
                        NotificationType.Error
                    );
                    return;
                }

                if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" ||
                    data.message == "Refresh token không hợp lệ")
                {
                    _notificationService.ShowNotification(
                        "Lỗi!",
                        (string)data.message,
                        NotificationType.Error
                    );
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
                        NotificationType.Error
                    );
                    return;
                }

                foreach (dynamic item in data.seatType)
                {
                    SeatTypes.Add(new SeatTypeItem()
                    {
                        Id = item.value,
                        Name = item.display,
                    });
                }

                cmbSeatType.ItemsSource = SeatTypes;
            }
            finally
            {
                LoadingControl.Visibility = Visibility.Collapsed;
            }
        }

        private async void LoadData()
        {
            try
            {
                LoadingControl.Visibility = Visibility.Visible;

                Task loadVehicleTypeTask = LoadVehicleType();
                Task loadSeaTypeTask = LoadSeaType();

                await Task.WhenAll(loadVehicleTypeTask, loadSeaTypeTask);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification(
                    "Lỗi!",
                    "Có lỗi xảy ra khi tải dữ liệu",
                    NotificationType.Error
                );
            }
            finally
            {
                LoadingControl.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<dynamic> ChangeVehicle(string id, int vehicle_type, int seat_type, int seats, string rules, string amenities, string license_plate)
        {
            try
            {
                Dictionary<string, string> vehicleHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {Properties.Settings.Default.access_token}" }
                };

                var vehicleBody = new
                {
                    refresh_token = Properties.Settings.Default.refresh_token,
                    vehicle_id = id,
                    vehicle_type,
                    seat_type,
                    seats,
                    rules,
                    amenities,
                    license_plate
                };

                dynamic data = await _service.PutWithHeaderAndBodyAsync("api/vehicle/update", vehicleHeader, vehicleBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadingControl.Visibility = Visibility.Visible;

                VehicleTypeItem type = (VehicleTypeItem)cmbVehicle.SelectedItem;
                SeatTypeItem seattype = (SeatTypeItem)cmbSeatType.SelectedItem;
                string seatt = txtSeats.Text;
                int seat;
                string rule = txtRules.Text.Trim();
                string ame = txtAmenities.Text.Trim();
                string lic = txtLic.Text.Trim();

                if (type == null || seattype == null || !int.TryParse(seatt, out seat) || string.IsNullOrWhiteSpace(rule) ||
                    string.IsNullOrWhiteSpace(ame) || string.IsNullOrWhiteSpace(lic))
                {
                    _notificationService.ShowNotification(
                        "Lỗi!",
                        "Vui lòng điền đẩy đủ thông tin",
                        NotificationType.Warning
                    );
                    LoadingControl.Visibility = Visibility.Collapsed;
                    return;
                }

                dynamic data = await ChangeVehicle(_vehicle.Id, type.Id, seattype.Id, seat, rule, ame, lic);

                if (data == null)
                {
                    _notificationService.ShowNotification(
                        "Lỗi!",
                        "Không thể kết nối đến máy chủ",
                        NotificationType.Warning
                    );
                    LoadingControl.Visibility = Visibility.Collapsed;
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
                    LoadingControl.Visibility = Visibility.Collapsed;
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
                    LoadingControl.Visibility = Visibility.Collapsed;
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
                    LoadingControl.Visibility = Visibility.Collapsed;
                    return;
                }

                if (data.message == "Cập nhật thông tin phương tiện thành công!")
                {
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    _notificationService.ShowNotification(
                        "Thành công",
                        (string)data.message,
                        NotificationType.Success
                    );
                    LoadingControl.Visibility = Visibility.Collapsed;
                    Close();
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
                    LoadingControl.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification(
                    "Lỗi!",
                    ex.Message,
                    NotificationType.Error
                );
                LoadingControl.Visibility = Visibility.Collapsed;
                throw;
            }
            finally
            {
                LoadingControl.Visibility = Visibility.Collapsed;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
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
