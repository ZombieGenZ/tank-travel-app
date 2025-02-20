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
using System.Globalization;
using TiketManagementV2.ViewModel;
using static TiketManagementV2.ViewModel.MainViewModel;
using TiketManagementV2.Controls;

namespace TiketManagementV2.View
{
    public partial class HomeView : UserControl
    {
        private MainViewModel _mainViewModel;
        private DispatcherTimer timer;
        private Popup currentPopup;
        private CircularLoadingControl _circularLoadingControl;
        //private readonly NotificationService notificationService;
        //private ApiServices _service;
        //private dynamic _user;

        public HomeView(dynamic user, MainViewModel mainViewModel, CircularLoadingControl loading)
        {
            InitializeComponent();
            _circularLoadingControl = loading;
            //_service = new ApiServices();
            //_user = user;
            ClockText.Text = DateTime.Now.ToString("HH:mm:ss - dd/MM/yyyy");
            StartClock();
            //notificationService = new NotificationService();  // Initialize the field

            var viewModel = new HomeViewModel(user, mainViewModel, _circularLoadingControl);
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
            _mainViewModel.CurrentView = new BusCensorView(_mainViewModel._notificationService);
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            if (currentPopup != null)
            {
                currentPopup.IsOpen = false;
                currentPopup = null;
            }
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            var textBlock = sender as TextBlock;
            if (textBlock != null && textBlock.Text.Length > 0)
            {
                // Kiểm tra xem text có bị cắt không
                var formattedText = new FormattedText(
                    textBlock.Text,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                    textBlock.FontSize,
                    textBlock.Foreground,
                    VisualTreeHelper.GetDpi(textBlock).PixelsPerDip);

                // Chỉ hiện popup nếu text bị cắt
                if (formattedText.Width > textBlock.ActualWidth)
                {
                    var popup = new Popup
                    {
                        StaysOpen = true,
                        AllowsTransparency = true,
                        Placement = PlacementMode.Mouse,
                        // Điều chỉnh vị trí popup
                        HorizontalOffset = 5,
                        VerticalOffset = 5
                    };

                    var border = new Border();
                    border.Style = (Style)FindResource("DetailPopupStyle");

                    var popupText = new TextBlock
                    {
                        Text = textBlock.Text,
                        Foreground = Brushes.White,
                        TextWrapping = TextWrapping.Wrap,
                        MaxWidth = 280 // Để lại margin cho border
                    };

                    border.Child = popupText;
                    popup.Child = border;

                    // Đóng popup cũ
                    if (currentPopup != null)
                    {
                        currentPopup.IsOpen = false;
                    }

                    popup.IsOpen = true;
                    currentPopup = popup;
                }
            }
        }

        private void btnSeeMoreVehicle_Click(object sender, RoutedEventArgs e)
        {
            _mainViewModel.CurrentView = new VehicleCensorView(_mainViewModel._notificationService, _circularLoadingControl);
        }
    }
}
