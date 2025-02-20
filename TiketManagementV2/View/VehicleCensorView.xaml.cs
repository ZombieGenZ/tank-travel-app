using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
using TiketManagementV2.Commands;
using TiketManagementV2.Controls;
using TiketManagementV2.Model;
using TiketManagementV2.Services;
using TiketManagementV2.ViewModel;
using static TiketManagementV2.ViewModel.HomeViewModel;

namespace TiketManagementV2.View
{
    /// <summary>
    /// Interaction logic for VehicleCensorView.xaml
    /// </summary>
    public partial class VehicleCensorView : UserControl, INotifyPropertyChanged
    {
        private int _itemsToLoad = 20;
        private ObservableCollection<Vehicle> _filteredVehicles;
        private bool _canLoadMore;
        private string _searchText;
        private string session_time;
        private int current = 0;
        private ApiServices _service;
        private INotificationService _notificationService;
        private CircularLoadingControl _circularLoadingControl;
        private bool _hasItems;
        public bool HasItems
        {
            get => Vehicles?.Any() ?? false;
            private set
            {
                _hasItems = value;
                OnPropertyChanged(nameof(HasItems));
            }
        }

        private ObservableCollection<Vehicle> _vehicles;
        public ObservableCollection<Vehicle> Vehicles
        {
            get { return _vehicles; }
            set { _vehicles = value; OnPropertyChanged(nameof(Vehicles)); }
        }

        public ObservableCollection<Vehicle> FilteredVehicles
        {
            get { return _filteredVehicles; }
            set { _filteredVehicles = value; OnPropertyChanged(nameof(FilteredVehicles)); }
        }

        public bool CanLoadMore
        {
            get { return _canLoadMore; }
            set { _canLoadMore = value; OnPropertyChanged(nameof(CanLoadMore)); }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                FilterVehicles();
            }
        }

        public ICommand SearchCommand { get; private set; }
        public ICommand AcceptCommand { get; private set; }
        public ICommand RejectCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }

        public class Vehicle : INotifyPropertyChanged
        {
            private string id;
            private string vehicleType;
            private string licensePlate;
            private string seatType;
            private int seats;
            private string rules;
            private string amenities;

            public string Id
            {
                get => id;
                set
                {
                    id = value;
                    OnPropertyChanged(nameof(id));
                }
            }

            public string VehicleType
            {
                get
                {
                    if (vehicleType == "0")
                    {
                        return "Xe khách";
                    }
                    if (vehicleType == "1")
                    {
                        return "Tài hỏa";
                    }
                    if (vehicleType == "2")
                    {
                        return "Máy bay";
                    }

                    return "Không xác định";
                }
                set
                {
                    vehicleType = value;
                    OnPropertyChanged(nameof(VehicleType));
                }
            }

            public string LicensePlate
            {
                get => licensePlate;
                set
                {
                    licensePlate = value;
                    OnPropertyChanged(nameof(LicensePlate));
                }
            }

            public string SeatType
            {
                get
                {
                    if (seatType == "0")
                    {
                        return "Ghế ngồi";
                    }
                    if (seatType == "1")
                    {
                        return "Giường nằm";
                    }
                    if (seatType == "2")
                    {
                        return "Ghế vừa ngồi vừa nằm";
                    }
                    return "Không xác định";
                }
                set
                {
                    seatType = value;
                    OnPropertyChanged(nameof(SeatType));
                }
            }

            public int Seats
            {
                get => seats;
                set
                {
                    seats = value;
                    OnPropertyChanged(nameof(Seats));
                }
            }

            public string Rules
            {
                get => rules;
                set
                {
                    rules = value;
                    OnPropertyChanged(nameof(Rules));
                }
            }

            public string Amenities
            {
                get => amenities;
                set
                {
                    amenities = value;
                    OnPropertyChanged(nameof(Amenities));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public VehicleCensorView(INotificationService notificationService, CircularLoadingControl loading)
        {
            InitializeComponent();

            _circularLoadingControl = loading;

            _notificationService = notificationService;
            session_time = DateTime.Now.ToString("o");
            _service = new ApiServices();

            Vehicles = new ObservableCollection<Vehicle>();
            FilteredVehicles = new ObservableCollection<Vehicle>();

            Vehicles.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasItems));
            };

            // Set up commands
            SearchCommand = new RelayCommandGeneric<Vehicle>(_ => FilterVehicles());
            AcceptCommand = new RelayCommandGeneric<Vehicle>(AcceptVehicle);
            RejectCommand = new RelayCommandGeneric<Vehicle>(RejectVehicle);
            LoadMoreCommand = new RelayCommandGeneric<Vehicle>(_ => LoadMore());

            // Set the DataContext to this
            DataContext = this;

            StartLoadingVehicle();
        }

        private async void StartLoadingVehicle()
        {
            try
            {
                _circularLoadingControl.Visibility = Visibility.Visible;
                await LoadVehicle();
            }
            finally
            {
                _circularLoadingControl.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<dynamic> GetVehicleData()
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
                    session_time,
                    current
                };

                dynamic data = await _service.PostWithHeaderAndBodyAsync("api/vehicle/get-vehicle-registration", getVehicleDataHeader, getVehicleDataBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task LoadVehicle()
        {
            try
            {
                dynamic data = await GetVehicleData();

                if (data == null)
                {
                    _notificationService.ShowNotification(
                        "Lỗi",
                        data.message,
                        NotificationType.Error
                    );
                    return;
                }

                if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" || data.message == "Refresh token không hợp lệ")
                {
                    _notificationService.ShowNotification(
                        "Lỗi",
                        data.message,
                        NotificationType.Error
                    );
                    return;
                }

                if (data.message == "Bạn không có quyền thực hiện hành động này")
                {
                    _notificationService.ShowNotification(
                        "Lỗi",
                        data.message,
                        NotificationType.Error
                    );

                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    return;
                }

                if (data.message == "Lỗi dữ liệu đầu vào")
                {
                    foreach (dynamic item in data.errors)
                    {
                        _notificationService.ShowNotification(
                            "Lỗi dữ liệu đầu vào",
                            (string)item.Value.msg,
                            NotificationType.Warning
                        );
                    }

                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    return;
                }

                if (data.message == "Lấy thông tin phương tiện thất bại")
                {
                    _notificationService.ShowNotification(
                        "Lỗi",
                        (string)data.message,
                        NotificationType.Warning
                    );
                    return;
                }

                if (data.result.message == "Không tìm thấy kết quả phù hợp")
                {
                    txtkco.Visibility = Visibility.Visible;
                    return;
                }

                foreach (dynamic item in data.result.vehicle)
                {
                    Vehicles.Add(new Vehicle()
                    {
                        Id = item._id,
                        Amenities = item.amenities,
                        LicensePlate = item.license_plate,
                        Rules = item.rules,
                        SeatType = (string)item.seat_type,
                        Seats = (int)item.seats,
                        VehicleType = item.vehicle_type
                    });
                }

                current = data.result.current;
                CanLoadMore = data.result.continued;

                //FilterVehicles();
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification(
                    "Lỗi",
                    ex.Message,
                    NotificationType.Error
                );
            }
        }

        private void FilterVehicles()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredVehicles = new ObservableCollection<Vehicle>(Vehicles.Take(_itemsToLoad));
            }
            else
            {
                var filteredList = Vehicles.Where(v =>
                    v.VehicleType.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    v.LicensePlate.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

                FilteredVehicles = new ObservableCollection<Vehicle>(filteredList);
            }

            CanLoadMore = FilteredVehicles.Count < Vehicles.Count;
        }

        private async void LoadMore()
        {
            try
            {
                _circularLoadingControl.Visibility = Visibility.Visible;

                await LoadVehicle();
            }
            finally
            {
                _circularLoadingControl.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<dynamic> CensorVehicleRegistration(string id, bool decision)
        {
            try
            {
                Dictionary<string, string> businessRegistrationHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {Properties.Settings.Default.access_token}" }
                };
                var businessRegistrationBody = new
                {
                    refresh_token = Properties.Settings.Default.refresh_token,
                    vehicle_id = id,
                    decision = decision
                };

                dynamic data = await _service.PutWithHeaderAndBodyAsync("api/vehicle/censor-vehicle", businessRegistrationHeader, businessRegistrationBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async void AcceptVehicle(object obj)
        {
            try
            {
                _circularLoadingControl.Visibility = Visibility.Visible;

                if (obj is Vehicle vehicle)
                {
                    dynamic data = await CensorVehicleRegistration(vehicle.Id, true);

                    if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" || data.message == "Refresh token không hợp lệ")
                    {
                        _notificationService.ShowNotification(
                            "Lỗi",
                            data.message,
                            NotificationType.Error
                        );
                        return;
                    }

                    if (data.message == "Bạn không có quyền thực hiện hành động này")
                    {
                        _notificationService.ShowNotification(
                            "Lỗi",
                            data.message,
                            NotificationType.Error
                        );
                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();
                        return;
                    }

                    if (data.message == "Lỗi dữ liệu đầu vào")
                    {
                        foreach (dynamic items in data.errors)
                        {
                            _notificationService.ShowNotification(
                                "Lỗi",
                                (string)items.Value.msg,
                                 NotificationType.Warning
                            );
                        }

                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();
                        return;
                    }

                    if (data.message == "Kiểm duyệt phương tiện thành công!")
                    {
                        _notificationService.ShowNotification(
                            "Thành công",
                            (string)data.message,
                            NotificationType.Success
                        );

                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();

                        Vehicles.Remove(vehicle);
                        //FilterVehicles();

                        if (Vehicles.Count < 1)
                        {
                            txtkco.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        _notificationService.ShowNotification(
                            "Lỗi",
                            (string)data.message,
                            NotificationType.Warning
                        );

                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();
                    }
                }
            }
            finally
            {
                _circularLoadingControl.Visibility = Visibility.Collapsed;
            }
        }

        private async void RejectVehicle(object obj)
        {
            try
            {
                _circularLoadingControl.Visibility = Visibility.Visible;

                if (obj is Vehicle vehicle)
                {
                    dynamic data = await CensorVehicleRegistration(vehicle.Id, false);

                    if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" || data.message == "Refresh token không hợp lệ")
                    {
                        _notificationService.ShowNotification(
                            "Lỗi",
                            data.message,
                            NotificationType.Error
                        );
                        return;
                    }

                    if (data.message == "Bạn không có quyền thực hiện hành động này")
                    {
                        _notificationService.ShowNotification(
                            "Lỗi",
                            data.message,
                            NotificationType.Error
                        );
                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();
                        return;
                    }

                    if (data.message == "Lỗi dữ liệu đầu vào")
                    {
                        foreach (dynamic items in data.errors)
                        {
                            _notificationService.ShowNotification(
                                "Lỗi",
                                (string)items.Value.msg,
                                 NotificationType.Warning
                            );
                        }

                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();
                        return;
                    }

                    if (data.message == "Kiểm duyệt phương tiện thành công!")
                    {
                        _notificationService.ShowNotification(
                            "Thành công",
                            (string)data.message,
                            NotificationType.Success
                        );

                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();

                        Vehicles.Remove(vehicle);
                        //FilterVehicles();

                        if (Vehicles.Count < 1)
                        {
                            txtkco.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        _notificationService.ShowNotification(
                            "Lỗi",
                            (string)data.message,
                            NotificationType.Warning
                        );

                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();
                        return;
                    }
                }
            }
            finally
            {
                _circularLoadingControl.Visibility = Visibility.Collapsed;
            }
        }

        //private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(SearchTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;

        //    // Update the SearchText property
        //    SearchText = SearchTextBox.Text;
        //}

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LoadMore();
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void btnReload_Click()
        {

        }
    }
}