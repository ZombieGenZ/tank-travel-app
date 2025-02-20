using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using IOPath = System.IO.Path;

namespace TiketManagementV2.View
{
    /// <summary>
    /// Interaction logic for ImageGalleryView.xaml
    /// </summary>
    public partial class ImageGalleryView : Window
    {
        public ObservableCollection<string> ImagePaths { get; set; }
        private string imagesFolder;

        public ImageGalleryView()
        {
            InitializeComponent();
            DataContext = this;

            // Lấy đường dẫn thư mục gốc của ứng dụng
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            imagesFolder = System.IO.Path.Combine(baseDirectory, "Images");

            // Kiểm tra và tạo thư mục Images nếu chưa tồn tại
            if (!System.IO.Directory.Exists(imagesFolder))
            {
                System.IO.Directory.CreateDirectory(imagesFolder);
            }

            // Khởi tạo danh sách đường dẫn ảnh với đường dẫn tuyệt đối
            ImagePaths = new ObservableCollection<string>();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ThumbnailList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThumbnailList.SelectedItem is string selectedImagePath)
            {
                LoadImageToMainDisplay(selectedImagePath);
                ThumbnailList.ScrollIntoView(selectedImagePath);
                ThumbnailList.Focus(); // Đặt focus vào ListBox
            }
        }

        private void LoadImageToMainDisplay(string imagePath)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.EndInit();
                MainImage.Source = bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChooseImageButton_Click(object sender, RoutedEventArgs e)
        {
            // Tạo OpenFileDialog để chọn ảnh
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Chọn ảnh",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string sourceFilePath in openFileDialog.FileNames)
                {
                    try
                    {
                        // Tạo tên file đích dựa trên thời gian hiện tại để tránh trùng tên
                        string fileName = $"image_{DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")}_{System.IO.Path.GetFileName(sourceFilePath)}";
                        string destinationPath = System.IO.Path.Combine(imagesFolder, fileName);

                        // Copy file ảnh vào thư mục Images của ứng dụng
                        File.Copy(sourceFilePath, destinationPath);

                        // Thêm ảnh mới vào danh sách
                        ImagePaths.Add(destinationPath);

                        // Hiển thị ảnh đầu tiên được chọn
                        if (ImagePaths.Count == 1 || sourceFilePath == openFileDialog.FileNames[0])
                        {
                            LoadImageToMainDisplay(destinationPath);
                            ThumbnailList.SelectedIndex = ImagePaths.Count - 1;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
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