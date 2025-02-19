using System;
using System.Collections.Generic;
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

    public partial class AddBusRouteView : Window
    {
        private INotificationService _notificationService;

        public AddBusRouteView(INotificationService notificationService)
        {
            InitializeComponent();
            var viewmodel = new BusRouteViewModel();

            DataContext = viewmodel;
            _notificationService = notificationService;
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string vehicle = cmbVehicle.Text.Trim();
            string startPoint = txtStartPoint.Text.Trim();
            string endPoint = txtEndPoint.Text.Trim();
            string departureTimeInput = txtDepartureTime.Text.Trim();
            string arrivalTimeInput = txtArrivalTime.Text.Trim();
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

            if (string.IsNullOrWhiteSpace(startPoint))
            {
                _notificationService.ShowNotification(
                    "Validation Error",
                    "Start point cannot be empty",
                    NotificationType.Error
                );
                return;
            }
            if (string.IsNullOrWhiteSpace(endPoint))
            {
                _notificationService.ShowNotification(
                    "Validation Error",
                    "End point cannot be empty",
                    NotificationType.Error
                );
                return;
            }

            if (!IsValidDateTime(departureTimeInput, out string formattedDepartureTime))
            {
                _notificationService.ShowNotification(
                    "Validation Error",
                    "Invalid departure time format. Please use yyyy-MM-dd HH:mm:ss",
                    NotificationType.Error
                );
                return;
            }

            if (!IsValidDateTime(arrivalTimeInput, out string formattedArrivalTime))
            {
                _notificationService.ShowNotification(
                    "Validation Error",
                    "Invalid arrival time format. Please use yyyy-MM-dd HH:mm:ss",
                    NotificationType.Error
                );
                return;
            }

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

            txtDepartureTime.Text = formattedDepartureTime;
            txtArrivalTime.Text = formattedArrivalTime;

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
    }
}
