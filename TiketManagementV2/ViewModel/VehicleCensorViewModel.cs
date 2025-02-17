using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TiketManagementV2.Model;

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

        public ObservableCollection<Vehicle> Vehicles { get; set; }
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

        public class Vehicle
        {
            public string VehicleType { get; set; }
            public string LicensePlate { get; set; }
            public string SeatType { get; set; }
            public int Seats { get; set; }
            public string Rules { get; set; }
            public string Amenities { get; set; }
        }

        public VehicleCensorViewModel()
        {
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

            }
            catch (Exception ex)
            {

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

        private void AcceptVehicle(object obj)
        {
            if (obj is Vehicle vehicle)
            {
                // Xử lý chấp nhận phương tiện
                Vehicles.Remove(vehicle);
                FilterVehicles();
            }
        }

        private void RejectVehicle(object obj)
        {
            if (obj is Vehicle vehicle)
            {
                // Xử lý từ chối phương tiện
                Vehicles.Remove(vehicle);
                FilterVehicles();
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
