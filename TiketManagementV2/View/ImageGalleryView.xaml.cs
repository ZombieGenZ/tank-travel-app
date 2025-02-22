using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace TiketManagementV2.View
{
    public partial class ImageGalleryView : Window
    {
        public ObservableCollection<string> ImagePaths { get; set; }

        public ImageGalleryView(Vehicle vehicle)
        {
            InitializeComponent();
            DataContext = this;

            // Sử dụng Pack URI format cho project resources
            ImagePaths = new ObservableCollection<string>
            {
                "pack://application:,,,/Images/bell_icon.png",
                "pack://application:,,,/Images/Close_Icon.png",
                "pack://application:,,,/Images/DefaultAvatar.png",
                "pack://application:,,,/Images/ducanh.jpg",
                "pack://application:,,,/Images/Error_Icon.png",
                "pack://application:,,,/Images/key-icon.png",
                "pack://application:,,,/Images/management.jpg",
                "pack://application:,,,/Images/success_icon.png",
                "pack://application:,,,/Images/TANK_logo_rmbg.ico",
                "pack://application:,,,/Images/TANK_logo_rmbg.png",
                "pack://application:,,,/Images/user-icon.png",
                "pack://application:,,,/Images/Warning_Icon.png"
            };

            if (ImagePaths.Count > 0)
            {
                LoadImageToMainDisplay(ImagePaths[0]);
                ThumbnailList.SelectedIndex = 0;
            }
        }

        private void ThumbnailList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThumbnailList.SelectedItem is string selectedImagePath)
            {
                LoadImageToMainDisplay(selectedImagePath);
                ThumbnailList.ScrollIntoView(selectedImagePath);
                ThumbnailList.Focus();
            }
        }

        private void LoadImageToMainDisplay(string imagePath)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(imagePath);
                bitmap.EndInit();
                MainImage.Source = bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Down || e.Key == Key.Right)
            {
                if (ThumbnailList.SelectedIndex < ImagePaths.Count - 1)
                {
                    ThumbnailList.SelectedIndex++;
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Up || e.Key == Key.Left)
            {
                if (ThumbnailList.SelectedIndex > 0)
                {
                    ThumbnailList.SelectedIndex--;
                }
                e.Handled = true;
            }
        }
    }
}