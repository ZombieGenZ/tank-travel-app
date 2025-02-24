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


        public EditBusRouteView()
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