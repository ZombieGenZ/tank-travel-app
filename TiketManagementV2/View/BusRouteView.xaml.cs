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
                    id = value;
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

        private void ExecuteAddCommand(object obj)
        {
            var addBusRouteView = new AddBusRouteView();
            addBusRouteView.Show();
        }

        public BusRouteView()
        {
            InitializeComponent();
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

                return await _service.PostWithHeaderAndBodyAsync("api/vehicle/get-bus-route", getBusRouteDataHeader, getBusRouteDataBody);
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
                //_circularLoadingControl.Visibility = Visibility.Visible;

                dynamic data = await GetBusRoute();

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

                foreach (dynamic item in data.result.get_bus_route)
                {
                    busRoutes.Add(new BusRoute()
                    {
                        Id = item._id,
                        Plate = item.vehicle_id,
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

                FilterManagedVehicles();
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
        private void RemoveBusRoute(BusRoute busRoute)
        {
            if (busRoute != null)
            {
                filteredBusRoutes.Remove(busRoute);
                BusRoutes.Remove(busRoute);
                CanLoadMore = filteredBusRoutes.Count < BusRoutes.Count;
            }
        }

        private void EditBusRoute(object obj)
        {
            var editBusRouteView = new EditBusRouteView(_notificationService);
            editBusRouteView.Show();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
