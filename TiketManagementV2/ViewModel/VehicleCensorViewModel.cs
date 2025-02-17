using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TiketManagementV2.Model;
using TiketManagementV2.Services;
using static TiketManagementV2.ViewModel.HomeViewModel;
using static TiketManagementV2.ViewModel.VehicleCensorViewModel;

namespace TiketManagementV2.ViewModel
{
    public class VehicleCensorViewModel : INotifyPropertyChanged
    {
        private int _itemsToLoad = 20;
        private ObservableCollection<Vehicle> _filteredVehicles;
        private bool _canLoadMore;
        private string _searchText;
        private string session_time;
        private int current = 0;
        private ApiServices _service;
        private INotificationService _notificationService;

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

        public ICommand SearchCommand { get; }
        public ICommand AcceptCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand LoadMoreCommand { get; }

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
                        return "Bus";
                    }
                    if (vehicleType == "1")
                    {
                        return "Train";
                    }
                    if (vehicleType == "2")
                    {
                        return "Plane";
                    }

                    return "Unknown";
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
                        return "Seating seat";
                    }
                    if (seatType == "1")
                    {
                        return "Sleeper seat";
                    }
                    if (seatType == "2")
                    {
                        return "Hybrid seat";
                    }
                    return "Unknown";
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

        public VehicleCensorViewModel(INotificationService notificationService)
        public VehicleCensorViewModel(INotificationService notificationService)
        {
            _notificationService = notificationService;
            _notificationService = notificationService;
            session_time = DateTime.Now.ToString("o");
            _service = new ApiServices();
            //Vehicles = new ObservableCollection<Vehicle>
            //{
            //    new Vehicle {VehicleType = "Train", LicensePlate = "ABC-123", SeatType = "Luxury", Seats = 50, Rules = "No smoking", Amenities = "Wifi"},
            //    new Vehicle {VehicleType = "Bus", LicensePlate = "DEF-456", SeatType = "Standard", Seats = 40, Rules = "No eating", Amenities = "TV"},
            //};

            Vehicles = new ObservableCollection<Vehicle>();

            FilteredVehicles = new ObservableCollection<Vehicle>(Vehicles.Take(_itemsToLoad));
            CanLoadMore = Vehicles.Count > _itemsToLoad;

            SearchCommand = new RelayCommand(_ => FilterVehicles());
            AcceptCommand = new RelayCommand(AcceptVehicle);
            RejectCommand = new RelayCommand(RejectVehicle);
            LoadMoreCommand = new RelayCommand(_ => LoadMore());

            LoadVehicle();
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

        private async void LoadVehicle()
        {
            try
            {
                dynamic data = await GetVehicleData();

                if (data == null)
                {
                    _notificationService.ShowNotification(
                        "Error",
                        data.message,
                        NotificationType.Error
                    );
                    return;
                }

                if (data.message == "You must log in to use this function" || data.message == "Invalid refresh token" || data.message == "You do not have permission to perform this action")
                {
                    _notificationService.ShowNotification(
                        "Error",
                        data.message,
                        NotificationType.Error
                    );
                    return;
                }

                Properties.Settings.Default.access_token = data.authenticate.access_token;
                Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                Properties.Settings.Default.Save();

                if (data.message == "Input data error")
                {
                    foreach (dynamic item in data.errors)
                    {
                        _notificationService.ShowNotification(
                            "Input data error",
                            (string)item.Value.msg,
                            NotificationType.Warning
                        );
                    }
                    return;
                }

                if (data.message == "Failed to get vehicle information")
                {
                    _notificationService.ShowNotification(
                        "Error",
                        (string)data.message,
                        NotificationType.Warning
                    );
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
                FilterVehicles();
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification(
                    "Error",
                    ex.Message,
                    NotificationType.Error
                );
                return;
                _notificationService.ShowNotification(
                    "Error",
                    ex.Message,
                    NotificationType.Error
                );
                return;
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

        private void LoadMore()
        {
            int currentCount = FilteredVehicles.Count;

            if (currentCount < Vehicles.Count)
            {
                var moreVehicles = Vehicles.Skip(currentCount).Take(_itemsToLoad).ToList();
                foreach (var vehicle in moreVehicles)
                {
                    FilteredVehicles.Add(vehicle);
                }
            }

            CanLoadMore = FilteredVehicles.Count < Vehicles.Count;
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
            if (obj is Vehicle vehicle)
            {
                dynamic data = await CensorVehicleRegistration(vehicle.Id, true);

                if (data.message == "You must log in to use this function" || data.message == "Invalid refresh token")
                {
                    _notificationService.ShowNotification(
                        "Error",
                        data.message,
                        NotificationType.Error
                    );
                    return;
                }

                if (data.message == "You do not have permission to perform this action")
                {
                    _notificationService.ShowNotification(
                        "Error",
                        data.message,
                        NotificationType.Error
                    );
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();
                    return;
                }

                if (data.message == "Input data error")
                {
                    foreach (dynamic items in data.errors)
                    {
                        _notificationService.ShowNotification(
                            "Error",
                            (string)items.Value.msg,
                             NotificationType.Warning
                        );
                    }

                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();
                    return;
                }

                if (data.message == "Failed to approve vehicle")
                {
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    _notificationService.ShowNotification(
                        "Error",
                        (string)data.message,
                        NotificationType.Warning
                    );

                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();
                    return;
                }

                if (data.message == "Vehicle approved successfully!")
                {
                    _notificationService.ShowNotification(
                        "Successfully",
                        (string)data.message,
                        NotificationType.Success
                    );

                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    Vehicles.Remove(vehicle);
                    FilterVehicles();
                }
            }
        }

        private async void RejectVehicle(object obj)
        {
            if (obj is Vehicle vehicle)
            {
                dynamic data = await CensorVehicleRegistration(vehicle.Id, false);

                if (data.message == "You must log in to use this function" || data.message == "Invalid refresh token")
                {
                    _notificationService.ShowNotification(
                        "Error",
                        data.message,
                        NotificationType.Error
                    );
                    return;
                }

                if (data.message == "You do not have permission to perform this action")
                {
                    _notificationService.ShowNotification(
                        "Error",
                        data.message,
                        NotificationType.Error
                    );
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();
                    return;
                }

                if (data.message == "Input data error")
                {
                    foreach (dynamic items in data.errors)
                    {
                        _notificationService.ShowNotification(
                            "Error",
                            (string)items.Value.msg,
                             NotificationType.Warning
                        );
                    }

                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();
                    return;
                }

                if (data.message == "Failed to approve vehicle")
                {
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    _notificationService.ShowNotification(
                        "Error",
                        (string)data.message,
                        NotificationType.Warning
                    );

                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();
                    return;
                }

                if (data.message == "Vehicle approved successfully!")
                {
                    _notificationService.ShowNotification(
                        "Successfully",
                        (string)data.message,
                        NotificationType.Success
                    );

                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    Vehicles.Remove(vehicle);
                    FilterVehicles();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
