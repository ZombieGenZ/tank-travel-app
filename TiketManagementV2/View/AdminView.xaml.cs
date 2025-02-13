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

namespace TiketManagementV2.View
{
    /// <summary>
    /// Interaction logic for AdminView.xaml
    /// </summary>


    public partial class AdminView : Window
    {

        public AdminView(dynamic user)
        {
            InitializeComponent();
            var notificationService = new NotificationService();
            var viewModel = new MainViewModel(notificationService);
            DataContext = viewModel;

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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
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
            if (userDropdown.IsOpen == false)
                userDropdown.IsOpen = true;
            else userDropdown.IsOpen = false;
        }

        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            // Xử lý sự kiện khi click vào nút Profile
            // Ví dụ: Mở trang profile
            userDropdown.IsOpen = false;
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginView loginView = new LoginView();
            loginView.Show();
            this.Close();
        }


    }
}
