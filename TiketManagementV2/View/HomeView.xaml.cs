using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    public partial class HomeView : UserControl
    {
        private MainViewModel _mainViewModel;
        private DispatcherTimer timer;
        private Popup currentPopup;
        //private readonly NotificationService notificationService;
        //private ApiServices _service;
        //private dynamic _user;

        public HomeView(dynamic user, MainViewModel mainViewModel)
        {
            InitializeComponent();
            //_service = new ApiServices();
            //_user = user;
            ClockText.Text = DateTime.Now.ToString("HH:mm:ss - dd/MM/yyyy");
            StartClock();
            //notificationService = new NotificationService();  // Initialize the field

            var viewModel = new HomeViewModel(user, mainViewModel);
            DataContext = viewModel;
            _mainViewModel = mainViewModel;
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

        private void btnSeeMoreBus_Click(object sender, RoutedEventArgs e)
        {
            _mainViewModel.CurrentView = new CensorView();
        }

        private void TextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var textBlock = sender as TextBlock;
            if (textBlock != null)
            {
                // Đóng popup cũ nếu đang mở
                if (currentPopup != null)
                {
                    currentPopup.IsOpen = false;
                }

                // Tạo popup mới
                var popup = new Popup
                {
                    StaysOpen = false,
                    AllowsTransparency = true,
                    Placement = PlacementMode.Mouse
                };

                // Tạo nội dung cho popup
                var border = new Border
                {
                    Background = (Brush)FindResource("#2D2179"),
                    BorderBrush = (Brush)FindResource("#413293"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(12),
                    MaxWidth = 300
                };

                var popupText = new TextBlock
                {
                    Text = textBlock.Text,
                    Foreground = Brushes.White,
                    TextWrapping = TextWrapping.Wrap
                };

                border.Child = popupText;
                popup.Child = border;

                // Mở popup
                popup.IsOpen = true;
                currentPopup = popup;

                // Đóng popup khi click ra ngoài
                popup.MouseLeave += (s, args) =>
                {
                    popup.IsOpen = false;
                    currentPopup = null;
                };
            }
        }
    }
}
