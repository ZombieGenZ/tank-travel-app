using ControlzEx.Standard;
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
        private CircularLoadingControl _circularLoadingControl;
        public VehicleManagementView(INotificationService notificationService, CircularLoadingControl loading)
        {
            InitializeComponent();
            _circularLoadingControl = loading;
            //LoadingControl.Visibility = Visibility.Collapsed;
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

        private async void ExecuteAddCommand(object obj)
        {
            var addVehicleView = new AddVehicleView(this);
            addVehicleView.ShowDialog();
            await Reload();
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

                if (data.message == "Lấy thông tin phương tiện thất bại")
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

                _managementCurrent = data.result.current;

                if ((bool)data.result.continued)
                {
                    LoadMore.Visibility = Visibility.Visible;
                }
                else
                {
                    LoadMore.Visibility = Visibility.Collapsed;
                }

                FilterManagedVehicles();
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

        public async void LoadMoreManagedVehicles()
        {
            await LoadManagedVehicles();
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

        private async void EditVehicle(object obj)
        {
            if (obj is Vehicle vehicle)
            {
                var editVehicle = new EditVehicleView(vehicle);
                editVehicle.ShowDialog();

                await Reload();

            }
        }

        private async void DeleteVehicle(object obj)
        {
            if (obj is Vehicle vehicle)
            {
                var dialog = new ConfirmationDialogView();
                dialog.OnDialogClosed += async (isDelete) =>
                {
                    if (isDelete)
                    {
                        await LDeleteVehicle(vehicle);
                        Reload();
                    }
                };
                dialog.ShowDialog();
            }
        }

        private async Task<dynamic> DeleteVehicle(string id)
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
                };

                dynamic data = await _service.DeleteWithHeaderAndBodyAsync("api/vehicle/delete", vehicleHeader, vehicleBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task LDeleteVehicle(Vehicle vehicle)
        {
            try
            {
                _circularLoadingControl.Visibility = Visibility.Visible;

                dynamic data = await DeleteVehicle(vehicle.Id);

                if (data == null)
                {
                    _notificationService.ShowNotification(
                        "Lỗi!",
                        "Không thể kết nối đến máy chủ",
                        NotificationType.Warning
                    );
                    _circularLoadingControl.Visibility = Visibility.Collapsed;
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
                    _circularLoadingControl.Visibility = Visibility.Collapsed;
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
                    _circularLoadingControl.Visibility = Visibility.Collapsed;
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
                    _circularLoadingControl.Visibility = Visibility.Collapsed;
                    return;
                }

                if (data.message == "Xóa thông tin phương tiện thành công!")
                {
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    _notificationService.ShowNotification(
                        "Thành công",
                        (string)data.message,
                        NotificationType.Success
                    );
                    _circularLoadingControl.Visibility = Visibility.Collapsed;
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
                    _circularLoadingControl.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification(
                    "Lỗi!",
                    ex.Message,
                    NotificationType.Error
                );
                _circularLoadingControl.Visibility = Visibility.Collapsed;
                throw;
            }
            finally
            {
                _circularLoadingControl.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowVehicleImages(object obj)
        {
            if (obj is Vehicle vehicle)
            {
                var imageGalleryView = new ImageGalleryView(vehicle.Id);
                //LoadVehicleImagesIntoGallery(vehicle.Id, imageGalleryView);
                imageGalleryView.ShowDialog();
            }
        }

        //private async void LoadVehicleImagesIntoGallery(string vehicleId, ImageGalleryView galleryView)
        //{
        //    try
        //    {
        //        var images = await GetVehicleImagesAsync(vehicleId);

        //        if (images != null && images.Count > 0)
        //        {
        //            foreach (string imagePath in images)
        //            {
        //                galleryView.ImagePaths.Add(imagePath);
        //            }

        //            if (galleryView.ImagePaths.Count > 0)
        //            {
        //                galleryView.ThumbnailList.SelectedIndex = 0;
        //            }
        //        }
        //        else
        //        {
        //            _notificationService.ShowNotification("Warning", "No images found for this vehicle", NotificationType.Warning);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _notificationService.ShowNotification("Error", "Failed to load vehicle images: " + ex.Message, NotificationType.Error);
        //    }
        //}

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

        private async void Reload_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //LoadingControl.Visibility = Visibility.Visible;

                _managementSessionTime = DateTime.Now.ToString("o");
                _managementCurrent = 0;

                await Reload();
            }
            finally
            {
                //LoadingControl.Visibility = Visibility.Collapsed;
            }
        }

        public async Task Reload()
        {
            _managementSessionTime = DateTime.Now.ToString("o");
            _managementCurrent = 0;
            ManagedVehicles.Clear();

            await LoadManagedVehicles();
        }

        //private async void Search_OnClick(object sender, RoutedEventArgs e)
        //{
        //    if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
        //    {
        //        await FindManagedVehicles(SearchTextBox.Text);
        //    }
        //    else
        //    {
        //        await LoadManagedVehicles();
        //    }
        //}

        //private async Task<dynamic> FidnManagedVehicleData(string keyworld)
        //{
        //    try
        //    {
        //        _managementSessionTime = DateTime.Now.ToString("o");
        //        _managementCurrent = 0;

        //        string access_token = Properties.Settings.Default.access_token;
        //        string refresh_token = Properties.Settings.Default.refresh_token;

        //        Dictionary<string, string> getVehicleDataHeader = new Dictionary<string, string>()
        //        {
        //            { "Authorization", $"Bearer {access_token}" }
        //        };
        //        var getVehicleDataBody = new
        //        {
        //            refresh_token,
        //            session_time = _managementSessionTime,
        //            current = _managementCurrent,
        //            keywords = keyworld
        //        };

        //        return await _service.PostWithHeaderAndBodyAsync("api/vehicle/find-vehicle", getVehicleDataHeader, getVehicleDataBody);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //        return null;
        //    }
        //}

        //public async Task FindManagedVehicles(string keyword)
        //{
        //    try
        //    {
        //        _circularLoadingControl.Visibility = Visibility.Visible;

        //        dynamic data = await FidnManagedVehicleData(keyword);

        //        if (data == null || data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" ||
        //            data.message == "Refresh token không hợp lệ" ||
        //            data.message == "Bạn không có quyền thực hiện hành động này")
        //        {
        //            _notificationService.ShowNotification("Lỗi", (string)data.message,
        //                NotificationType.Error);
        //            return;
        //        }

        //        Properties.Settings.Default.access_token = data.authenticate.access_token;
        //        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
        //        Properties.Settings.Default.Save();

        //        if (data.message == "Lỗi dữ liệu đầu vào")
        //        {
        //            foreach (dynamic item in data.errors)
        //            {
        //                _notificationService.ShowNotification("Lỗi dữ liệu đầu vào", (string)item.Value.msg,
        //                    NotificationType.Warning);
        //            }

        //            return;
        //        }

        //        if (data.message == "Tìm kiếm thông tin phương tiện thất bại")
        //        {
        //            _notificationService.ShowNotification("Lỗi", (string)data.message, NotificationType.Warning);
        //            return;
        //        }

        //        if (data.result.message == "Không tìm thấy kết quả phù hợp")
        //        {
        //            // hiển thị không tìm thấy kết quả nào phù hợp
        //            return;
        //        }

        //        foreach (dynamic item in data.result.vehicle)
        //        {
        //            ManagedVehicles.Add(new Vehicle()
        //            {
        //                Id = item._id,
        //                User = item.user.display_name,
        //                Amenities = item.amenities,
        //                LicensePlate = item.license_plate,
        //                Rules = item.rules,
        //                SeatType = (string)item.seat_type,
        //                Seats = (int)item.seats,
        //                VehicleType = (string)item.vehicle_type
        //            });
        //        }

        //        _managementCurrent = data.result.current;

        //        if ((bool)data.result.continued)
        //        {
        //            LoadMore.Visibility = Visibility.Visible;
        //        }
        //        else
        //        {
        //            LoadMore.Visibility = Visibility.Collapsed;
        //        }

        //        FilterManagedVehicles();
        //    }
        //    catch (Exception ex)
        //    {
        //        _notificationService.ShowNotification("Error", ex.Message, NotificationType.Error);
        //    }
        //    finally
        //    {
        //        _circularLoadingControl.Visibility = Visibility.Collapsed;
        //    }
        //}
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
                if (vehicleType == "0") return "Xe khách";
                if (vehicleType == "1") return "Tàu hỏa";
                if (vehicleType == "2") return "Máy bay";
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
                if (seatType == "0") return "Ghế ngồi";
                if (seatType == "1") return "Giường nằm";
                if (seatType == "2") return "Ghế vừa nằm vừa ngồi";
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
}