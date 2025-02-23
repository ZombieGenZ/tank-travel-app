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
using TiketManagementV2.Services;
using TiketManagementV2.ViewModel;

namespace TiketManagementV2.View
{

    public partial class AddBusRouteView : Window, INotifyPropertyChanged
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
            public int Id { get; set; }
            public string Name { get; set; }
            public override string ToString()
            {
                return Name;
            }
        }

        public class StartPointItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public override string ToString()
            {
                return Name;
            }
        }

        public class EndPointItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public override string ToString()
            {
                return Name;
            }
        }

        public ObservableCollection<SeatTypeItem> SeatTypes { get; set; }
        public ObservableCollection<PlateItem> PlateItems { get; set; }
        public ObservableCollection<StartPointItem> StartPointItems { get; set; }
        public ObservableCollection<EndPointItem> EndPointItems { get; set; }


        public AddBusRouteView()
        {
            InitializeComponent();
            var viewmodel = new BusRouteViewModel();
            DataContext = viewmodel;
            _notificationService = new NotificationService();

            SeatTypes = new ObservableCollection<SeatTypeItem>();
            PlateItems = new ObservableCollection<PlateItem>();
            StartPointItems = new ObservableCollection<StartPointItem>();
            EndPointItems = new ObservableCollection<EndPointItem>();

            var selectedDate = DateTime.Now;

            txtSelectedDateTime.Text = selectedDate.ToString("yyyy-MM-dd HH:mm");

            txtSelectedArrivalDateTime.Text = selectedDate.ToString("yyyy-MM-dd HH:mm");
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

            txtSelectedDateTime.Text = "yyyy-MM-dd HH:mm";
            txtSelectedDateTime.Text = combinedDateTime.ToString("yyyy-MM-dd HH:mm");
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

            txtSelectedArrivalDateTime.Text = combinedDateTime.ToString("yyyy-MM-dd HH:mm");
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string vehicle = cmbVehicle.Text.Trim();
            //string startPoint = txtStartPoint.Text.Trim();
            //string endPoint = txtEndPoint.Text.Trim();
            //string departureTimeInput = txtDepartureTime.Text.Trim();
            //string arrivalTimeInput = txtArrivalTime.Text.Trim();
            string priceInput = txtPrice.Text.Trim();
            string quantityInput = txtQuantity.Text.Trim();

            if (string.IsNullOrEmpty(vehicle))
            {
                _notificationService.ShowNotification(
                    "Validation Error",
                    "Please select a vehicle type",
                    NotificationType.Error
                );
                return;
            }

            //if (string.IsNullOrWhiteSpace(startPoint))
            //{
            //    _notificationService.ShowNotification(
            //        "Validation Error",
            //        "Start point cannot be empty",
            //        NotificationType.Error
            //    );
            //    return;
            //}
            //if (string.IsNullOrWhiteSpace(endPoint))
            //{
            //    _notificationService.ShowNotification(
            //        "Validation Error",
            //        "End point cannot be empty",
            //        NotificationType.Error
            //    );
            //    return;
            //}

            //if (!IsValidDateTime(departureTimeInput, out string formattedDepartureTime))
            //{
            //    _notificationService.ShowNotification(
            //        "Validation Error",
            //        "Invalid departure time format. Please use yyyy-MM-dd HH:mm:ss",
            //        NotificationType.Error
            //    );
            //    return;
            //}

            //if (!IsValidDateTime(arrivalTimeInput, out string formattedArrivalTime))
            //{
            //    _notificationService.ShowNotification(
            //        "Validation Error",
            //        "Invalid arrival time format. Please use yyyy-MM-dd HH:mm:ss",
            //        NotificationType.Error
            //    );
            //    return;
            //}

            // Kiểm tra giá (Price)
            if (!decimal.TryParse(priceInput, out decimal price) || price <= 0)
            {
                _notificationService.ShowNotification(
                    "Validation Error",
                    "Price must be a positive number",
                    NotificationType.Error
                );
                return;
            }

            if (!int.TryParse(quantityInput, out int quantity) || quantity <= 0)
            {
                _notificationService.ShowNotification(
                    "Validation Error",
                    "Quantity must be a positive integer",
                    NotificationType.Error
                );
                return;
            }

            //txtDepartureTime.Text = formattedDepartureTime;
            //txtArrivalTime.Text = formattedArrivalTime;

            _notificationService.ShowNotification(
                "Success",
                "Bus route updated successfully",
                NotificationType.Success
            );
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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
