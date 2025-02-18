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
        private readonly INotificationService _notificationService;
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
                    "Error",
                    "Invalid email format! Please enter a valid email.",
                    NotificationType.Error
                );
                return;
            }

            // Kiểm tra Start Point và End Point
            if (string.IsNullOrWhiteSpace(startPoint))
            {
                MessageBox.Show("Start Point không được để trống.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(endPoint))
            {
                MessageBox.Show("End Point không được để trống.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Kiểm tra thời gian
            if (!IsValidDateTime(departureTimeInput, out string formattedDepartureTime) ||
                !IsValidDateTime(arrivalTimeInput, out string formattedArrivalTime))
            {
                MessageBox.Show("Thời gian không hợp lệ! Vui lòng nhập đúng định dạng yyyy-MM-dd HH:mm:ss.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Kiểm tra giá (Price)
            if (!decimal.TryParse(priceInput, out decimal price) || price <= 0)
            {
                MessageBox.Show("Giá phải là số hợp lệ và lớn hơn 0.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Kiểm tra số lượng (Quantity)
            if (!int.TryParse(quantityInput, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Số lượng phải là số nguyên dương.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Gán lại giá trị đã được format vào TextBox
            txtEditDepartureTime.Text = formattedDepartureTime;
            txtEditArrivalTime.Text = formattedArrivalTime;

            // Tiếp tục xử lý lưu thông tin
            MessageBox.Show("Dữ liệu hợp lệ! Tiến hành lưu thông tin.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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

        }
    }
}
