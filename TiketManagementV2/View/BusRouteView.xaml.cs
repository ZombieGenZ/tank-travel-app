using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TiketManagementV2.Commands;
using TiketManagementV2.Model;
using TiketManagementV2.Services;
using TiketManagementV2.ViewModel;

namespace TiketManagementV2.View
{
    /// <summary>
    /// Interaction logic for BusRouteView.xaml
    /// </summary>
    public partial class BusRouteView : UserControl, INotifyPropertyChanged
    {
        private int _itemsToLoad = 20;
        private ObservableCollection<BusRoute> _filteredBusRoutes;
        private bool _canLoadMore;
        private string _managementSessionTime;
        private ApiServices _service;
        private INotificationService _notificationService;
        private ObservableCollection<BusRoute> BusRoutes;
        public ObservableCollection<BusRoute> busRoutes
        {
            get { return busRoutes; }
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

        public class BusRoute : INotifyPropertyChanged
        {
            private string id;
            private string vehicle;
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
            public string Vehicle
            {
                get
                {
                    if (vehicle == "0")
                    {
                        return "Bus";
                    }
                    if (vehicle == "1")
                    {
                        return "Train";
                    }
                    if (vehicle == "2")
                    {
                        return "Plane";
                    }

                    return "Unknown";
                }
                set
                {
                    vehicle = value;
                    OnPropertyChanged(nameof(Vehicle));
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

        public BusRouteView()
        {
            InitializeComponent();
            _notificationService = new NotificationService();
            BusRoutes = new ObservableCollection<BusRoute>
            {
                new BusRoute {Vehicle = "0", StartPoint = "Doe", EndPoint = "123456789",DepartureTime = "11-11-2000", ArrivalTime = "11-11-2000", Price = 500000, Quantity = 50, Sold = 0},

            };

            filteredBusRoutes = new ObservableCollection<BusRoute>(BusRoutes.Take(_itemsToLoad));
            CanLoadMore = BusRoutes.Count > _itemsToLoad;

            EditCommand = new RelayCommandGeneric<BusRoute>(EditBusRoute);
            BanCommand = new RelayCommandGeneric<BusRoute>(RemoveBusRoute);
            LoadMoreCommand = new RelayCommandGeneric<BusRoute>(_ => LoadMore());
            AddCommand = new RelayCommand(ExecuteAddCommand);

        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is BusRouteViewModel viewModel)
            {
                viewModel.LoadMore();
            }
        }

        private void ExecuteAddCommand(object obj)
        {
            var addBusRouteView = new AddBusRouteView();
            addBusRouteView.Show();
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
