using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TiketManagementV2.Controls
{
    public partial class CircularLoadingControl : UserControl
    {
        private Storyboard spinAnimation;
        private TextBlock[] loadingTextBlocks;

        public CircularLoadingControl()
        {
            InitializeComponent();
            InitializeAnimation();
        }

        private void InitializeAnimation()
        {
            spinAnimation = (Storyboard)FindResource("SpinAnimation");

            // Khởi tạo và thiết lập text blocks
            string loadingText = "Loading";
            loadingTextBlocks = new TextBlock[loadingText.Length];
            StackPanel textContainer = new StackPanel { Orientation = Orientation.Horizontal };

            for (int i = 0; i < loadingText.Length; i++)
            {
                var textBlock = new TextBlock
                {
                    Text = loadingText[i].ToString(),
                    FontSize = 16,
                    Foreground = System.Windows.Media.Brushes.White,
                    Margin = new Thickness(1, 0, 1, 0)
                };

                // Thiết lập transform
                textBlock.RenderTransformOrigin = new Point(0.5, 0.5);
                textBlock.RenderTransform = new TranslateTransform();

                // Tạo animation cho mỗi chữ
                var storyboard = new Storyboard();
                var animation = new DoubleAnimation
                {
                    From = 0,
                    To = -8,
                    Duration = new Duration(TimeSpan.FromSeconds(0.6)),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever,
                    BeginTime = TimeSpan.FromSeconds(i * 0.1)
                };

                Storyboard.SetTarget(animation, textBlock);
                Storyboard.SetTargetProperty(animation,
                    new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));

                storyboard.Children.Add(animation);
                textBlock.Loaded += (s, e) => storyboard.Begin();

                loadingTextBlocks[i] = textBlock;
                textContainer.Children.Add(textBlock);
            }

            // Thêm text container vào Grid
            MainGrid.Children.Add(textContainer);

            // Thiết lập vị trí cho text container
            textContainer.VerticalAlignment = VerticalAlignment.Center;
            textContainer.HorizontalAlignment = HorizontalAlignment.Center;
            textContainer.Margin = new Thickness(0, 90, 0, 0); // Đặt text xuống dưới vòng loading

            // Start/Stop animations
            Loaded += (s, e) =>
            {
                spinAnimation.Begin();
            };

            Unloaded += (s, e) =>
            {
                spinAnimation.Stop();
            };
        }

        // Dependency property for background opacity
        public double OverlayOpacity
        {
            get { return (double)GetValue(OverlayOpacityProperty); }
            set { SetValue(OverlayOpacityProperty, value); }
        }

        public static readonly DependencyProperty OverlayOpacityProperty =
            DependencyProperty.Register("OverlayOpacity", typeof(double),
                typeof(CircularLoadingControl), new PropertyMetadata(0.6));
    }
}