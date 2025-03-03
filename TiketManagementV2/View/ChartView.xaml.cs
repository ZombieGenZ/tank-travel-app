using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;

namespace TiketManagementV2.View
{
    public partial class ChartView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ChartValues<double> _hiringSources;
        public ChartValues<double> HiringSources
        {
            get => _hiringSources;
            set
            {
                _hiringSources = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HiringSources)));
            }
        }

        private List<string> _sourceLabels;
        public List<string> SourceLabels
        {
            get => _sourceLabels;
            set
            {
                _sourceLabels = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceLabels)));
            }
        }

        public class RankingItem
        {
            public string Rank { get; set; }
            public string Name { get; set; }
            public ImageSource ProfileImage { get; set; }
        }

        private List<RankingItem> _rankingItems;
        public List<RankingItem> RankingItems
        {
            get => _rankingItems;
            set
            {
                _rankingItems = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RankingItems)));
            }
        }

        public ChartView()
        {
            try
            {
                InitializeComponent();
                this.DataContext = this;
                LoadSampleData();
                LoadRankingData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo: {ex.Message}\n{ex.StackTrace}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSampleData()
        {
            HiringSources = new ChartValues<double> { 50, 60, 55, 70, 65, 80, 75, 85, 90, 100, 75, 40 };
            SourceLabels = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        }

        private ImageSource LoadSafeImage(string resourcePath)
        {
            try
            {
                // Sử dụng Pack URI để tải tài nguyên từ assembly
                string packUri = $"pack://application:,,,/TiketManagementV2;component{resourcePath}";
                return new BitmapImage(new Uri(packUri));
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                Console.WriteLine($"Không thể tải hình ảnh: {resourcePath}. Lỗi: {ex.Message}");

                // Trả về hình ảnh mặc định hoặc tạo một placeholder
                return CreatePlaceholderImage();
            }
        }

        private ImageSource CreatePlaceholderImage()
        {
            // Tạo một ảnh đơn giản màu xám để sử dụng làm placeholder
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(Brushes.LightGray, null, new Rect(0, 0, 40, 40));
            }

            RenderTargetBitmap rtb = new RenderTargetBitmap(40, 40, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(drawingVisual);
            return rtb;
        }

        private void LoadRankingData()
        {
            try
            {
                // Tạo dữ liệu mẫu cho bảng xếp hạng
                RankingItems = new List<RankingItem>
                {
                    new RankingItem
                    {
                        Rank = "1",
                        Name = "Duy Anh",
                        ProfileImage = LoadSafeImage("/Images/profile1.png"),
                    },
                    new RankingItem
                    {
                        Rank = "2",
                        Name = "QuyÔngCuĐâi",
                        ProfileImage = LoadSafeImage("/Images/profile2.png"),
                    },
                    new RankingItem
                    {
                        Rank = "3",
                        Name = "--",
                        ProfileImage = LoadSafeImage("/Images/profile3.png"),
                    },
                    new RankingItem
                    {
                        Rank = "4",
                        Name = "T",
                        ProfileImage = LoadSafeImage("/Images/profile4.png"),
                    },
                    new RankingItem
                    {
                        Rank = "5",
                        Name = "Nguyễn Đình Nam",
                        ProfileImage = LoadSafeImage("/Images/profile5.png"),
                    },
                };

                // Gán nguồn dữ liệu cho ItemsControl
                RankingList.ItemsSource = RankingItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu bảng xếp hạng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}