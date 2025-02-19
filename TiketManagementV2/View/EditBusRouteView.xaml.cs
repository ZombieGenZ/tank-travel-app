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
    public partial class EditBusRouteView : Window
    {
        private INotificationService _notificationService;
        public EditBusRouteView(INotificationService notificationService)
        {
            InitializeComponent();
            var viewmodel = new BusRouteViewModel();

            DataContext = viewmodel;
            _notificationService = notificationService;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            // Lấy giá trị từ các input
            string vehicle = cmbEditVehicle.Text.Trim();
            string startPoint = txtEditStartPoint.Text.Trim();
            string endPoint = txtEditEndPoint.Text.Trim();
            string departureTimeInput = txtEditDepartureTime.Text.Trim();
            string arrivalTimeInput = txtEditArrivalTime.Text.Trim();
            string priceInput = txtEditPrice.Text.Trim();
            string quantityInput = txtEditQuantity.Text.Trim();

            // Kiểm tra Vehicle
            if (string.IsNullOrEmpty(vehicle))
            {
                _notificationService.ShowNotification(
                    "Validation Error",
                    "Please select a vehicle type",
                    NotificationType.Error
                );
                return;
            }

            // Kiểm tra Start Point và End Point
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

            // Kiểm tra thời gian
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

            // Kiểm tra số lượng (Quantity)
            if (!int.TryParse(quantityInput, out int quantity) || quantity <= 0)
            {
                _notificationService.ShowNotification(
                    "Validation Error",
                    "Quantity must be a positive integer",
                    NotificationType.Error
                );
                return;
            }

            // Gán lại giá trị đã được format vào TextBox
            txtEditDepartureTime.Text = formattedDepartureTime;
            txtEditArrivalTime.Text = formattedArrivalTime;

            // Nếu tất cả validation đều pass, hiển thị thông báo thành công
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
    }
}