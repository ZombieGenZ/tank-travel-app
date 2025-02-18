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
using System.Runtime.InteropServices;
using System.Runtime;
using System.Windows.Interop;
using TiketManagementV2.ViewModel;
using TiketManagementV2.Services;
using TiketManagementV2.Model;

namespace TiketManagementV2.View
{
    public partial class AdminView : Window
    {
        private ApiServices _service;
        private dynamic _user;
        public AdminView(dynamic user)
        {
            InitializeComponent();
            LoadingControl.Visibility = Visibility.Hidden;
            var notificationService = new NotificationService();
            _user = user;
            var viewModel = new MainViewModel(notificationService, _user);
            DataContext = viewModel;
            _service = new ApiServices();
        }
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);
        private void pnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void pnlControlBar_MouseEnter(object sender, MouseEventArgs e)
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        private async void btnClose_Click(object sender, RoutedEventArgs e)
        {
            bool remember = Properties.Settings.Default.remember_login;

            if (!remember)
            {
                await DeleteLogout();
            }

            Application.Current.Shutdown();
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else this.WindowState = WindowState.Normal;
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void btnDropdown_Click(object sender, RoutedEventArgs e)
        {
            userDropdown.IsOpen = true;
            btnDropdown.IsEnabled = false; // Khóa nút khi dropdown mở
        }

        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            userDropdown.IsOpen = false;
        }

        private async Task<bool> DeleteLogout()
        {
            try
            {
                var logoutBody = new
                {
                    refresh_token = Properties.Settings.Default.refresh_token
                };

                await _service.DeleteWithBodyAsync("api/users/logout", logoutBody);

                Properties.Settings.Default.access_token = "";
                Properties.Settings.Default.refresh_token = "";
                Properties.Settings.Default.Save();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        private async void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            await DeleteLogout();

            LoginView loginView = new LoginView();
            loginView.Show();
            this.Close();
        }

        private void userDropdown_Closed(object sender, EventArgs e)
        {
            btnDropdown.IsEnabled = true; // Mở khóa nút khi dropdown đóng
        }
    }
}
