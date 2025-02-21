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
using TiketManagementV2.Model;
using TiketManagementV2.Services;
using TiketManagementV2.View;

namespace TiketManagementV2.View
{
    public partial class VehicleManagementView : UserControl, INotifyPropertyChanged
    {
        private int _managementItemsToLoad = 20;
        private ObservableCollection<Vehicle> _managedFilteredVehicles;
        private bool _managementCanLoadMore;
        private string _managementSessionTime;
        private int _managementCurrent = 0;
        private ApiServices _service;
        private INotificationService _notificationService;

        private ObservableCollection<Vehicle> _managedVehicles;
        public ObservableCollection<Vehicle> ManagedVehicles
        {
            get { return _managedVehicles; }
            set { _managedVehicles = value; OnPropertyChanged(nameof(ManagedVehicles)); }
        }
        public ObservableCollection<Vehicle> ManagedFilteredVehicles
        {
            get { return _managedFilteredVehicles; }
            set { _managedFilteredVehicles = value; OnPropertyChanged(nameof(ManagedFilteredVehicles)); }
        }

        public bool ManagementCanLoadMore
        {
            get { return _managementCanLoadMore; }
            set { _managementCanLoadMore = value; OnPropertyChanged(nameof(ManagementCanLoadMore)); }
        }

        private string _managementSearchText;
        public string ManagementSearchText
        {
            get { return _managementSearchText; }
            set
            {
                _managementSearchText = value;
                OnPropertyChanged(nameof(ManagementSearchText));
                FilterManagedVehicles();
            }
        }

        public ICommand SearchCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }
        public ICommand AddCommand { get; private set; }
        public ICommand ImageCommand { get; private set; }

        public VehicleManagementView(INotificationService notificationService)
        {
            InitializeComponent();

            _notificationService = notificationService;
            _managementSessionTime = DateTime.Now.ToString("o");
            _service = new ApiServices();

            ManagedVehicles = new ObservableCollection<Vehicle>();
            ManagedFilteredVehicles = new ObservableCollection<Vehicle>(ManagedVehicles.Take(_managementItemsToLoad));
            ManagementCanLoadMore = ManagedVehicles.Count > _managementItemsToLoad;

            SearchCommand = new RelayCommandGeneric<Vehicle>(_ => FilterManagedVehicles());
            EditCommand = new RelayCommandGeneric<Vehicle>(EditVehicle);
            DeleteCommand = new RelayCommandGeneric<Vehicle>(DeleteVehicle);
            LoadMoreCommand = new RelayCommandGeneric<Vehicle>(_ => LoadMoreManagedVehicles());
            AddCommand = new RelayCommand(ExecuteAddCommand);
            ImageCommand = new RelayCommandGeneric<Vehicle>(ShowVehicleImages);

            ManagedVehicles.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(ManagedVehicles));
            };

            DataContext = this;
            LoadManagedVehicles();
        }

        private void ExecuteAddCommand(object obj)
        {
            var addVehicleView = new AddVehicleView();
            addVehicleView.ShowDialog();
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
                    refresh_token,
                    session_time = _managementSessionTime,
                    current = _managementCurrent
                };

                return await _service.PostWithHeaderAndBodyAsync("api/vehicle/get-vehicle", getVehicleDataHeader, getVehicleDataBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async void LoadManagedVehicles()
        {
            try
            {
                dynamic data = await GetManagedVehicleData();

                if (data == null || data.message == "You must log in to use this function" ||
                    data.message == "Invalid refresh token" || data.message == "You do not have permission to perform this action")
                {
                    _notificationService.ShowNotification("Error", data?.message ?? "Error loading data", NotificationType.Error);
                    return;
                }

                Properties.Settings.Default.access_token = data.authenticate.access_token;
                Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                Properties.Settings.Default.Save();

                if (data.message == "Input data error")
                {
                    foreach (dynamic item in data.errors)
                    {
                        _notificationService.ShowNotification("Input data error", (string)item.Value.msg, NotificationType.Warning);
                    }
                    return;
                }

                if (data.message == "Failed to get vehicle information")
                {
                    _notificationService.ShowNotification("Error", (string)data.message, NotificationType.Warning);
                    return;
                }

                foreach (dynamic item in data.result.vehicle)
                {
                    ManagedVehicles.Add(new Vehicle()
                    {
                        Id = item._id,
                        User = item.user.display_name,
                        Amenities = item.amenities,
                        LicensePlate = item.license_plate,
                        Rules = item.rules,
                        SeatType = (string)item.seat_type,
                        Seats = (int)item.seats,
                        VehicleType = (string)item.vehicle_type
                    });
                }
                FilterManagedVehicles();
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification("Error", ex.Message, NotificationType.Error);
            }
        }

        private void FilterManagedVehicles()
        {
            if (string.IsNullOrWhiteSpace(ManagementSearchText))
            {
                ManagedFilteredVehicles = new ObservableCollection<Vehicle>(ManagedVehicles.Take(_managementItemsToLoad));
            }
            else
            {
                var filteredList = ManagedVehicles.Where(v =>
                    v.VehicleType.IndexOf(ManagementSearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    v.LicensePlate.IndexOf(ManagementSearchText, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

                ManagedFilteredVehicles = new ObservableCollection<Vehicle>(filteredList);
            }

            ManagementCanLoadMore = ManagedFilteredVehicles.Count < ManagedVehicles.Count;
        }

        public void LoadMoreManagedVehicles()
        {
            int currentCount = ManagedFilteredVehicles.Count;
            if (currentCount < ManagedVehicles.Count)
            {
                var moreVehicles = ManagedVehicles.Skip(currentCount).Take(_managementItemsToLoad).ToList();
                foreach (var vehicle in moreVehicles)
                {
                    ManagedFilteredVehicles.Add(vehicle);
                }
            }
            ManagementCanLoadMore = ManagedFilteredVehicles.Count < ManagedVehicles.Count;
        }

        private async Task<dynamic> CensorManagedVehicleRegistration(string id, bool decision)
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

                return await _service.PutWithHeaderAndBodyAsync("api/vehicle/censor-vehicle", businessRegistrationHeader, businessRegistrationBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private void EditVehicle(object obj)
        {
            if (obj is Vehicle vehicle)
            {
                var editVehicle = new EditVehicleView();
                // You might need to pass the vehicle data to the edit view
                // editVehicle.DataContext = vehicle;
                editVehicle.ShowDialog();
            }
        }

        private async void DeleteVehicle(object obj)
        {
            if (obj is Vehicle vehicle)
            {
                await ProcessManagedVehicleDecision(vehicle, false);
            }
        }

        private async Task ProcessManagedVehicleDecision(Vehicle vehicle, bool isAccepted)
        {
            dynamic data = await CensorManagedVehicleRegistration(vehicle.Id, isAccepted);

            if (data == null)
            {
                _notificationService.ShowNotification("Error", "Failed to process vehicle", NotificationType.Error);
                return;
            }

            if (data.message == "You must log in to use this function" ||
                data.message == "Invalid refresh token" ||
                data.message == "You do not have permission to perform this action")
            {
                _notificationService.ShowNotification("Error", data.message, NotificationType.Error);

                if (data.authenticate != null)
                {
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();
                }
                return;
            }

            if (data.message == "Input data error")
            {
                foreach (dynamic items in data.errors)
                {
                    _notificationService.ShowNotification("Error", (string)items.Value.msg, NotificationType.Warning);
                }
            }
            else if (data.message == "Vehicle approved successfully!")
            {
                _notificationService.ShowNotification("Successfully", data.message, NotificationType.Success);
                ManagedVehicles.Remove(vehicle);
                FilterManagedVehicles();
            }

            if (data.authenticate != null)
            {
                Properties.Settings.Default.access_token = data.authenticate.access_token;
                Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                Properties.Settings.Default.Save();
            }
        }

        private void ShowVehicleImages(object obj)
        {
            if (obj is Vehicle vehicle)
            {
                var imageGalleryView = new ImageGalleryView();
                LoadVehicleImagesIntoGallery(vehicle.Id, imageGalleryView);
                imageGalleryView.ShowDialog();
            }
        }

        private async void LoadVehicleImagesIntoGallery(string vehicleId, ImageGalleryView galleryView)
        {
            try
            {
                var images = await GetVehicleImagesAsync(vehicleId);

                if (images != null && images.Count > 0)
                {
                    foreach (string imagePath in images)
                    {
                        galleryView.ImagePaths.Add(imagePath);
                    }

                    if (galleryView.ImagePaths.Count > 0)
                    {
                        galleryView.ThumbnailList.SelectedIndex = 0;
                    }
                }
                else
                {
                    _notificationService.ShowNotification("Warning", "No images found for this vehicle", NotificationType.Warning);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification("Error", "Failed to load vehicle images: " + ex.Message, NotificationType.Error);
            }
        }

        private async Task<List<string>> GetVehicleImagesAsync(string vehicleId)
        {
            try
            {
                Dictionary<string, string> header = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {Properties.Settings.Default.access_token}" }
                };

                var requestBody = new
                {
                    refresh_token = Properties.Settings.Default.refresh_token,
                    vehicle_id = vehicleId
                };

                dynamic response = await _service.PostWithHeaderAndBodyAsync("api/vehicle/get-vehicle-images", header, requestBody);

                if (response == null || response.message == "Failed to get vehicle images")
                {
                    return new List<string>();
                }

                if (response.authenticate != null)
                {
                    Properties.Settings.Default.access_token = response.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = response.authenticate.refresh_token;
                    Properties.Settings.Default.Save();
                }

                List<string> imagePaths = new List<string>();

                foreach (dynamic imageData in response.result.images)
                {
                    string imagePath = (string)imageData.path;
                    imagePaths.Add(imagePath);
                }

                return imagePaths;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new List<string>();
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(SearchTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            ManagementSearchText = SearchTextBox.Text;
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LoadMoreManagedVehicles();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Vehicle : INotifyPropertyChanged
    {
        private string id;
        private string user;
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
                OnPropertyChanged(nameof(Id));
            }
        }

        public string User
        {
            get => user;
            set
            {
                user = value;
                OnPropertyChanged(nameof(User));
            }
        }

        public string VehicleType
        {
            get
            {
                if (vehicleType == "0") return "Bus";
                if (vehicleType == "1") return "Train";
                if (vehicleType == "2") return "Plane";
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
                if (seatType == "0") return "Seating seat";
                if (seatType == "1") return "Sleeper seat";
                if (seatType == "2") return "Hybrid seat";
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
}