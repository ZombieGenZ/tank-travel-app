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
using TiketManagementV2.Commands;
using TiketManagementV2.Controls;
using TiketManagementV2.Model;
using TiketManagementV2.Services;
using TiketManagementV2.ViewModel;

namespace TiketManagementV2.View
{
    public partial class EditBusRouteView : Window
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


        private BusRouteView.BusRoute _busRoute;
        public EditBusRouteView(BusRouteView.BusRoute busRoute, INotificationService notificationService)
        {
            InitializeComponent();
            _service = new ApiServices();
            var viewmodel = new BusRouteViewModel();
            DataContext = viewmodel;
            _busRoute = busRoute;
            _notificationService = notificationService;

            SeatTypes = new ObservableCollection<SeatTypeItem>();
            PlateItems = new ObservableCollection<PlateItem>();
            StartPointItems = new ObservableCollection<LocationItem>();
            EndPointItems = new ObservableCollection<LocationItem>();

            DateTime selectedDate = DateTime.Now;

            txtSelectedDateTime.Text = selectedDate.ToString("HH:mm dd/MM/yyyy");

            txtSelectedArrivalDateTime.Text = selectedDate.ToString("HH:mm dd/MM/yyyy");

            LoadAllDataAsync();

            //DateTime startTime = new DateTime();
            //DateTime endTime = new DateTime();

            
        }

        private async Task LoadAllDataAsync()
        {
            try
            {
                _circularLoadingControl.Visibility = Visibility.Visible;

                var tasks = new[]
                {
                    LoadManagedVehicles(),
                    LoadLocation()
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

            LocationItem sItem = StartPointItems.FirstOrDefault(l => l.Id == _busRoute.StartPoint);
            LocationItem eItem = EndPointItems.FirstOrDefault(l => l.Id == _busRoute.EndPoint);

            cmbStartPoint.SelectedValue = sItem.Id;
            cmbEndPoint.SelectedValue = eItem.Id;
        }

        public async Task LoadManagedVehicles()
        {
            try
            {
                _circularLoadingControl.Visibility = Visibility.Visible;

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

                PlateItem pItem = PlateItems.FirstOrDefault(l => l.Name == _busRoute.Plate);

                cmbPlate.SelectedValue = pItem.Id;
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification("Error", ex.Message, NotificationType.Error);
            }
            finally
            {
                _circularLoadingControl.Visibility = Visibility.Collapsed;
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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            
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
    }
}