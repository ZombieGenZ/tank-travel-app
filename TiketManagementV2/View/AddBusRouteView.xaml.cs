using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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
using TiketManagementV2.Controls;
using TiketManagementV2.Model;
using TiketManagementV2.Services;
using TiketManagementV2.ViewModel;

namespace TiketManagementV2.View
{

    public partial class AddBusRouteView : Window, INotifyPropertyChanged
    {
        private INotificationService _notificationService;

        public class SeatTypeItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public override string ToString()
            {
                return Name;
            }
        }

        public class PlateItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public override string ToString()
            {
                return Name;
            }
        }

        public class LocationItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public override string ToString()
            {
                return Name;
            }
        }

        public ObservableCollection<SeatTypeItem> SeatTypes { get; set; }
        public ObservableCollection<PlateItem> PlateItems { get; set; }
        public ObservableCollection<LocationItem> StartPointItems { get; set; }
        public ObservableCollection<LocationItem> EndPointItems { get; set; }
        public ApiServices _service;

        public AddBusRouteView()
        {
            InitializeComponent();
            _service = new ApiServices();
            var viewmodel = new BusRouteViewModel();
            DataContext = viewmodel;
            _notificationService = new NotificationService();

            SeatTypes = new ObservableCollection<SeatTypeItem>();
            PlateItems = new ObservableCollection<PlateItem>();
            StartPointItems = new ObservableCollection<LocationItem>();
            EndPointItems = new ObservableCollection<LocationItem>();

            var selectedDate = DateTime.Now;

            txtSelectedDateTime.Text = selectedDate.ToString("HH:mm dd/MM/yyyy");

            txtSelectedArrivalDateTime.Text = selectedDate.ToString("HH:mm dd/MM/yyyy");

            LoadManagedVehicles();
            LoadLocation();
        }

        private async Task<dynamic> GetManagedVehicleData()
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
                    refresh_token
                };

                return await _service.PostWithHeaderAndBodyAsync("api/vehicle/get-vehicle-list", getVehicleDataHeader, getVehicleDataBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task<dynamic> GetLocationData()
        {
            try
            {
                return await _service.GetLocationAsync("https://provinces.open-api.vn/api/?depth=1");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task LoadLocation()
        {
            dynamic data = await GetLocationData();

            foreach (dynamic location in data)
            {
                string local = (string)location.name;
                string locall = local.Replace("Tỉnh ", "").Replace("Thành phố ", "TP ");
                StartPointItems.Add(new LocationItem()
                {
                    Id = locall,
                    Name = local
                });
                EndPointItems.Add(new LocationItem()
                {
                    Id = locall,
                    Name = local
                });
            }

            cmbStartPoint.ItemsSource = StartPointItems;
            cmbEndPoint.ItemsSource = EndPointItems;
        }

        public async Task LoadManagedVehicles()
        {
            try
            {
                //_circularLoadingControl.Visibility = Visibility.Visible;

                dynamic data = await GetManagedVehicleData();

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

                if (data.message == "Lấy danh sách phương tiện thất bại")
                {
                    _notificationService.ShowNotification("Error", (string)data.message, NotificationType.Warning);
                    return;
                }

                if (data.result.message == "Không tìm thấy kết quả phù hợp")
                {
                    // hiển thị không tìm thấy kết quả phù hợp
                    return;
                }

                foreach (dynamic item in data.result.vehicle)
                {
                    PlateItems.Add(new PlateItem()
                    {
                        Id = item._id,
                        Name = item.license_plate
                    });
                }

                cmbPlate.ItemsSource = PlateItems;
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

        private void BtnSelectDateTime_Click(object sender, RoutedEventArgs e)
        {
            var selectedDate = Calendar.SelectedDate ?? DateTime.Now;

            var time = Clock.Time;

            var combinedDateTime = new DateTime(
                selectedDate.Year,
                selectedDate.Month,
                selectedDate.Day,
                time.Hour,
                time.Minute,
                0 
            );

            txtSelectedDateTime.Text = combinedDateTime.ToString("HH:mm dd/MM/yyyy");
        }

        private void BtnSelectArrivalDateTime_Click(object sender, RoutedEventArgs e)
        {
            var selectedDate = ArrivalCalendar.SelectedDate ?? DateTime.Now;

            var time = ArrivalClock.Time;

            var combinedDateTime = new DateTime(
                selectedDate.Year,
                selectedDate.Month,
                selectedDate.Day,
                time.Hour,
                time.Minute,
                0
            );

            txtSelectedArrivalDateTime.Text = combinedDateTime.ToString("HH:mm dd/MM/yyyy");
        }

        private async void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            dynamic plate = cmbPlate.SelectedItem;
            dynamic start = cmbStartPoint.SelectedItem;
            dynamic end = cmbEndPoint.SelectedItem;
            dynamic startTimeStr = txtSelectedDateTime.Text;
            dynamic endTimeStr = txtSelectedArrivalDateTime.Text;
            DateTime startTime = new DateTime();
            DateTime endTime = new DateTime();
            DateTime now = DateTime.Now;
            string pricee = txtPrice.Text;
            string quatee = txtQuantity.Text;

            int price = 0;
            int quate = 0;

            if (plate == null || start == null || end == null || !DateTime.TryParseExact(startTimeStr,
                    "HH:mm dd/MM/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out startTime) || !DateTime.TryParseExact(endTimeStr,
                    "HH:mm dd/MM/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out endTime) || !int.TryParse(pricee, out price) || !int.TryParse(quatee, out quate))
            {
                _notificationService.ShowNotification(
                    "Lỗi",
                    "Vui lòng điền đầy đủ thông tin",
                    NotificationType.Warning
                );
                return;
            }

            if (now > startTime || now > endTime)
            {
                _notificationService.ShowNotification(
                    "Lỗi",
                    "Thời gian khởi hành và thời gian tới dự kiến phải lớn hơn thời gian hiện tại",
                    NotificationType.Warning
                );
                return;
            }

            if (startTime > endTime)
            {
                _notificationService.ShowNotification(
                    "Lỗi",
                    "Thời gian khởi hành không được bé hơn thời gian đến dự kiến",
                    NotificationType.Warning
                );
                return;
            }

            if (start.Name == end.Name)
            {
                _notificationService.ShowNotification(
                    "Lỗi",
                    "Điểm khởi hành và điểm đến không được trùng nhau",
                    NotificationType.Warning
                );
                return;
            }

            if (quate < 0)
            {
                _notificationService.ShowNotification(
                    "Lỗi",
                    "Số lượng phải lớn hơn 0",
                    NotificationType.Warning
                );
                return;
            }

            if (price < 0)
            {
                _notificationService.ShowNotification(
                    "Lỗi",
                    "Giá vé phải lớn hơn 0",
                    NotificationType.Warning
                );
                return;
            }

            dynamic data = await CreateBusRoute((string)plate.Id, (string)start.Name, (string)end.Name,
                startTime.ToString("o"),
                endTime.ToString("o"), price, quate);

            if (data == null)
            {
                _notificationService.ShowNotification(
                    "Lỗi",
                    "Không thể kết nối đến máy chủ",
                    NotificationType.Error
                );
                return;
            }

            if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" ||
                data.message == "Refresh token không hợp lệ")
            {
                _notificationService.ShowNotification(
                    "Lỗi",
                    (string)data.message,
                    NotificationType.Error
                );
                return;
            }

            Properties.Settings.Default.access_token = data.authenticate.access_token;
            Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
            Properties.Settings.Default.Save();

            if (data.message == "Bạn không có quyền thực hiện hành động này")
            {
                _notificationService.ShowNotification(
                    "Lỗi",
                    (string)data.message,
                    NotificationType.Error
                );
                return;
            }

            if (data.message == "Lỗi dữ liệu đầu vào")
            {
                foreach (dynamic item in data.errors)
                {
                    _notificationService.ShowNotification("Lỗi dữ liệu đầu vào", (string)item.Value.msg,
                        NotificationType.Warning);
                }

                return;
            }

            if (data.message == "Thêm thông tin tuyến thành công!")
            {
                _notificationService.ShowNotification(
                    "Thành công",
                    (string)data.message,
                    NotificationType.Success
                );
                Close();
                return;
            }
            else
            {
                _notificationService.ShowNotification(
                    "Lỗi",
                    (string)data.message,
                    NotificationType.Error
                );
                return;
            }
        }

        private async Task<dynamic> CreateBusRoute(string vehicle_id, string start_point, string end_point, string departure_time, string arrival_time, int price, int quantity)
        {
            try
            {
                Dictionary<string, string> busRouteHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {Properties.Settings.Default.access_token}" }
                };
                var busRouteBody = new
                {
                    refresh_token = Properties.Settings.Default.refresh_token,
                    vehicle_id,
                    start_point,
                    end_point,
                    departure_time,
                    arrival_time,
                    price,
                    quantity
                };

                dynamic data = await _service.PostWithHeaderAndBodyAsync("api/bus-route/create", busRouteHeader, busRouteBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private bool IsValidDateTime(string input, out string formattedDateTime)
        {
            formattedDateTime = string.Empty;

            if (DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateTime))
            {
                formattedDateTime = parsedDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                return true;
            }

            return false;
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
