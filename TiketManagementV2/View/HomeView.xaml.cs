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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TiketManagementV2.Model;
using TiketManagementV2.Services;
using TiketManagementV2.ViewModel;
using static TiketManagementV2.ViewModel.MainViewModel;

namespace TiketManagementV2.View
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        private DispatcherTimer timer;
        //private readonly NotificationService notificationService;
        //private ApiServices _service;
        //private dynamic _user;

        public HomeView(dynamic user)
        {
            InitializeComponent();
            //_service = new ApiServices();
            //_user = user;
            ClockText.Text = DateTime.Now.ToString("HH:mm:ss - dd/MM/yyyy");
            StartClock();
            //notificationService = new NotificationService();  // Initialize the field
            var viewModel = new HomeViewModel(user);
            DataContext = viewModel;
        }

        private void StartClock()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // Cập nhật mỗi giây
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            ClockText.Text = DateTime.Now.ToString("HH:mm:ss - dd/MM/yyyy");
        }
    }
}
