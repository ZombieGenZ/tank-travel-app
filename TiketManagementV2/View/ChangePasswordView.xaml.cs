using System;
using System.Collections.Generic;
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
using TiketManagementV2.Model;
using TiketManagementV2.Services;
using TiketManagementV2.ViewModel;

namespace TiketManagementV2.View
{
    /// <summary>
    /// Interaction logic for ChangePasswordView.xaml
    /// </summary>
    public partial class ChangePasswordView : Window
    {
        private readonly NotificationService notificationService;
        private string email;
        private string pass;
        private ApiServices _service;

        public ChangePasswordView(string email, string pass)
        {
            _service = new ApiServices();
            InitializeComponent();
            LoadingControl.Visibility = Visibility.Hidden;
            notificationService = new NotificationService();  // Initialize the field
            var viewModel = new MainViewModel(notificationService);
            DataContext = viewModel;
            this.email = email;
            this.pass = pass;
        }

        private async Task<dynamic> PutChangeTempPass(string email, string pass, string npass, string ncomformPass)
        {
            try
            {
                var changeBody = new
                {
                    email = email,
                    password = pass,
                    new_password = npass,
                    comform_new_password = ncomformPass
                };

                dynamic data = await _service.PutWithBodyAsync("api/users/change-password-temporary", changeBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private async void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            LoadingControl.Visibility = Visibility.Visible;
            if (string.IsNullOrEmpty(txtNewPassword.Password))
            {
                notificationService.ShowNotification(
                    "Warning",
                    "Please enter your new password!",
                    NotificationType.Warning
                );
                txtNewPassword.Focus();
                LoadingControl.Visibility = Visibility.Hidden;
                return;
            }

            if (string.IsNullOrEmpty(txtConfirmPassword.Password))
            {
                notificationService.ShowNotification(
                    "Warning",
                    "Please confirm your new password!",
                    NotificationType.Warning
                );
                txtConfirmPassword.Focus();
                LoadingControl.Visibility = Visibility.Hidden;
                return;
            }

            if (txtNewPassword.Password != txtConfirmPassword.Password)
            {
                notificationService.ShowNotification(
                    "Warning",
                    "New password and confirm password do not match!",
                    NotificationType.Warning
                );
                txtConfirmPassword.Password = "";
                txtConfirmPassword.Focus();
                LoadingControl.Visibility = Visibility.Hidden;
                return;
            }

            dynamic data = await PutChangeTempPass(email, pass, txtNewPassword.Password, txtConfirmPassword.Password);

            if (data == null)
            {
                notificationService.ShowNotification(
                    "Error",
                    "Error connecting to server!",
                    NotificationType.Error
                );
                LoadingControl.Visibility = Visibility.Hidden;
                return;
            }

            if (data.message == "Input data error")
            {
                foreach (dynamic item in data.errors)
                {
                    notificationService.ShowNotification(
                         "Warning",
                         (string)item.Value.msg,
                         NotificationType.Warning
                     );
                }
                LoadingControl.Visibility = Visibility.Hidden;
                return;
            }

            if (data.message == "Temporary password changed successfully! Please log in again")
            {
                notificationService.ShowNotification(
                    "Success",
                    (string)data.message,
                    NotificationType.Success
                );
                LoadingControl.Visibility = Visibility.Hidden;
                this.Close();
                return;
            }
        }
    }
}
