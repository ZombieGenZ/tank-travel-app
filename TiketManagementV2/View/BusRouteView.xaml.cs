using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
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
using static TiketManagementV2.View.BusCensorView;

namespace TiketManagementV2.View
{
    /// <summary>
    /// Interaction logic for BusRouteView.xaml
    /// </summary>
    public partial class BusRouteView : UserControl, INotifyPropertyChanged
    {
        private int _itemsToLoad = 2;
        private ObservableCollection<BusRoute> _filteredBusRoutes;
        private bool _canLoadMore;
        private string _SessionTime;
        private ApiServices _service;
        private INotificationService _notificationService;
        private ObservableCollection<BusRoute> BusRoutes;
        private int _Current = 0;

        public ObservableCollection<BusRoute> busRoutes
        {
            get { return BusRoutes; }
            set { BusRoutes = value; OnPropertyChanged(nameof(busRoutes)); }
        }
        public ObservableCollection<BusRoute> filteredBusRoutes
        {
            get { return _filteredBusRoutes; }
            set { _filteredBusRoutes = value; OnPropertyChanged(nameof(_filteredBusRoutes)); }
        }

        public bool CanLoadMore
        {
            get { return _canLoadMore; }
            set { _canLoadMore = value; OnPropertyChanged(nameof(CanLoadMore)); }
        }

        private string _SearchText;
        public string SearchText
        {
            get { return _SearchText; }
            set
            {
                _SearchText = value;
                OnPropertyChanged(nameof(SearchText));
                FilterManagedVehicles();
            }
        }

        public class BusRoute : INotifyPropertyChanged
        {
            private string id;
            private string plate;
            private string startPoint;
            private string endPoint;
            private string departureTime;
            private string arrivalTime;
            private int price;
            private int quantity;
            private int sold;

            public string Id
            {
                get => id;
                set
                {
                    id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }

            public string Plate
            {
                get => plate;
                set
                {
                    plate = value;
                    OnPropertyChanged(nameof(Plate));
                }
            }

            public string StartPoint
            {
                get => startPoint;
                set
                {
                    startPoint = value;
                    OnPropertyChanged(nameof(StartPoint));
                }
            }
            public string EndPoint
            {
                get => endPoint;
                set
                {
                    endPoint = value;
                    OnPropertyChanged(nameof(EndPoint));
                }
            }
            public string DepartureTime
            {
                get => departureTime;
                set
                {
                    departureTime = value;
                    OnPropertyChanged(nameof(DepartureTime));
                }
            }
            public string ArrivalTime
            {
                get => arrivalTime;
                set
                {
                    arrivalTime = value;
                    OnPropertyChanged(nameof(ArrivalTime));
                }
            }
            public int Price
            {
                get => price;
                set
                {
                    price = value;
                    OnPropertyChanged(nameof(Price));
                }
            }
            public int Quantity
            {
                get => quantity;
                set
                {
                    quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                }
            }
            public int Sold
            {
                get => sold;
                set
                {
                    sold = value;
                    OnPropertyChanged(nameof(Sold));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ICommand EditCommand { get; }
        public ICommand BanCommand { get; }
        public ICommand LoadMoreCommand { get; }

        public ICommand AddCommand { get; }

        private async void ExecuteAddCommand(object obj)
        {
            var addBusRouteView = new AddBusRouteView();
            addBusRouteView.ShowDialog();
            _circularLoadingControl.Visibility = Visibility.Visible;
            await Task.Delay(1000);
            Reload();
        }

        private CircularLoadingControl _circularLoadingControl;

        public BusRouteView(CircularLoadingControl loading)
        {
            InitializeComponent();
            _circularLoadingControl = loading;
            _notificationService = new NotificationService();
            _SessionTime = DateTime.Now.ToString("o");
            _service = new ApiServices();

            BusRoutes = new ObservableCollection<BusRoute>();

            filteredBusRoutes = new ObservableCollection<BusRoute>(BusRoutes.Take(_itemsToLoad));
            CanLoadMore = BusRoutes.Count > _itemsToLoad;

            EditCommand = new RelayCommandGeneric<BusRoute>(EditBusRoute);
            BanCommand = new RelayCommandGeneric<BusRoute>(RemoveBusRoute);
            LoadMoreCommand = new RelayCommandGeneric<BusRoute>(_ => LoadMore());
            AddCommand = new RelayCommand(ExecuteAddCommand);
            DataContext = this;

            busRoutes.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(busRoutes));
            };

            LoadManagedBusRoute();

        }

        private async void RemoveBusRoute(BusRoute busRoute)
        {
            if (busRoute != null)
            {
                var dialog = new ConfirmationDialogView();
                dialog.OnDialogClosed += async (isDelete) =>
                {
                    if (isDelete)
                    {
                        await LDeleteBusRoute(busRoute);
                        Reload();
                    }
                };
                dialog.ShowDialog();
            }
        }

        private async Task<dynamic> DeleteBusRoute(string id)
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
                    bus_route_id = id,
                };

                dynamic data = await _service.DeleteWithHeaderAndBodyAsync("api/bus-route/delete", busRouteHeader, busRouteBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task LDeleteBusRoute(BusRoute vehicle)
        {
            try
            {
                _circularLoadingControl.Visibility = Visibility.Visible;

                dynamic data = await DeleteBusRoute(vehicle.Id);

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

                if (data.message == "Xóa thông tin tuyến thành công!")
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

        private async void Reload()
        {
            _SessionTime = DateTime.Now.AddSeconds(10).ToString("o");
            _Current = 0;
            busRoutes.Clear();

            await LoadManagedBusRoute();
        }

        private async Task<dynamic> GetBusRoute()
        {
            try
            {
                string access_token = Properties.Settings.Default.access_token;
                string refresh_token = Properties.Settings.Default.refresh_token;

                Dictionary<string, string> getBusRouteDataHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {access_token}" }
                };
                var getBusRouteDataBody = new
                {
                    refresh_token,
                    session_time = _SessionTime,
                    current = _Current
                };

                return await _service.PostWithHeaderAndBodyAsync("api/bus-route/get-bus-route", getBusRouteDataHeader, getBusRouteDataBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public async Task LoadManagedBusRoute()
        {
            try
            {
                _circularLoadingControl.Visibility = Visibility.Visible;

                dynamic data = await GetBusRoute();

                if (data == null)
                {
                    _notificationService.ShowNotification("Lỗi", "Lỗi kết nối đến áy chủ",
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

                if (data.message == "Lấy thông tin tuyến thất bại")
                {
                    _notificationService.ShowNotification("Error", (string)data.message, NotificationType.Warning);
                    return;
                }

                if (data.result.message == "Không tìm thấy kết quả phù hợp")
                {
                    // hiển thị không tìm thấy kết quả phù hợp
                    return;
                }

                foreach (dynamic item in data.result.busRoute)
                {
                    busRoutes.Add(new BusRoute()
                    {
                        Id = item._id,
                        Plate = item.vehicle.license_plate,
                        StartPoint = item.start_point,
                        EndPoint = item.end_point,
                        DepartureTime = item.departure_time,
                        ArrivalTime = item.arrival_time,
                        Price = item.price,
                        Quantity = item.quantity
                    });
                }

                _Current = data.result.current;

                if ((bool)data.result.continued)
                {

                    LoadBusRoute.Visibility = Visibility.Visible;
                }
                else
                {
                    LoadBusRoute.Visibility = Visibility.Collapsed;
                }

                dgv.ItemsSource = busRoutes;

                //FilterManagedVehicles();
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
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                filteredBusRoutes = new ObservableCollection<BusRoute>(busRoutes.Take(_itemsToLoad));
            }
            else
            {
                //var filteredList = SearchText.Where(v =>
                //    v.VehicleType.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                //    v.LicensePlate.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

                //_filteredBusRoutes = new ObservableCollection<BusRoute>(filteredList);
            }

            CanLoadMore = _filteredBusRoutes.Count < BusRoutes.Count;
        }
        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is BusRouteViewModel viewModel)
            {
                viewModel.LoadMore();
            }
        }


        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(SearchTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        public void LoadMore()
        {
            int currentCount = filteredBusRoutes.Count;

            if (currentCount < BusRoutes.Count)
            {
                var moreBusRoutes = BusRoutes.Skip(currentCount).Take(_itemsToLoad).ToList();
                foreach (var busRoute in moreBusRoutes)
                {
                    filteredBusRoutes.Add(busRoute);
                }
            }

            CanLoadMore = filteredBusRoutes.Count < BusRoutes.Count;
        }

        private async void EditBusRoute(object obj)
        {
            if (obj is BusRoute busRoute)
            {
                var editBusRouteView = new EditBusRouteView(busRoute, _notificationService);
                editBusRouteView.ShowDialog();
                _circularLoadingControl.Visibility = Visibility.Visible;
                await Task.Delay(1000);
                Reload();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            Reload();
        }
    }
}
