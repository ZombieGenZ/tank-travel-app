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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TiketManagementV2.View
{
    /// <summary>
    /// Interaction logic for NotificationMessage.xaml
    /// </summary>
    public partial class NotificationMessage : Window
    {
        Rect _ScreenArea = SystemParameters.WorkArea;
        private static List<NotificationMessage> _openNotifications = new List<NotificationMessage>();


        public string Header { get; set; }
        public string Message { get; set; }
        public string ImagePath { get; set; }
        public LinearGradientBrush Gradient { get; set; }
        public SolidColorBrush RecFill { get; set; }
        public NotificationMessage()
        {
            InitializeComponent();
            this.DataContext = this;
            _Border.MouseEnter += _Border_MouseEnter;
            _Border.MouseLeave += _Border_MouseLeave;
        }

        private void _Border_MouseLeave(object sender, MouseEventArgs e)
        {
            Storyboard fadeOUt = (Storyboard)this.Resources["CloseButtonFadeOutAnimation"];
            fadeOUt.Begin();
        }

        private void _Border_MouseEnter(object sender, MouseEventArgs e)
        {
            Storyboard fadeIn = (Storyboard)this.Resources["CloseButtonFadeInAnimation"];
            fadeIn.Begin();
        }
        private void _Close_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            double notificationHeight = this.Height + 10; // Khoảng cách giữa các thông báo
            double startY = _ScreenArea.Bottom - notificationHeight; // Bắt đầu từ góc dưới cùng

            // Đảm bảo thông báo mới không đè lên nhau
            foreach (var notification in _openNotifications)
            {
                startY -= notificationHeight;
            }

            // vị trí cho cửa sổ
            this.Left = _ScreenArea.Right - this.Width;
            this.Top = startY;

            // Thêm vào danh sách
            _openNotifications.Add(this);

            // Slide in window animation
            Storyboard slidein = (Storyboard)this.Resources["WindowSlideInAnimation"];
            slidein.Begin();
        }

        private void WindowSlideInAnimation_Completed(object sender, EventArgs e)
        {
            // Slide in Complete then decrease rectangle length
            this.Left = _ScreenArea.Right - this.Width - 10;
            Storyboard decreaseWidth = (Storyboard)this.Resources["RectangleWidthDecreaseAnimation"];
            decreaseWidth.Begin();
        }
        private void Storyboard_Completed(object sender, EventArgs e)
        {
            // after decrease width of rectangle slide out window
            Storyboard SlideOut = (Storyboard)this.Resources["WindowSlideOutAnimation"];
            this.Left = _ScreenArea.Right - this.Width;
            SlideOut.Begin();
        }
        private void WindowSlideOutAnimation_Completed(object sender, EventArgs e)
        {
            double closedNotificationTop = this.Top;
            _openNotifications.Remove(this);
            this.Close();
            foreach (var notification in _openNotifications)
            {
                if (notification.Top < closedNotificationTop)
                {
                    DoubleAnimation moveDown = new DoubleAnimation
                    {
                        To = notification.Top + this.Height + 10,
                        Duration = TimeSpan.FromMilliseconds(300),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };

                    notification.BeginAnimation(Window.TopProperty, moveDown);
                }
            }
        }
    }
}
