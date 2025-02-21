using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.IO;
using System.Timers;
using System.Windows.Data;
using static TiketManagementV2.View.FileUploadInfo;
using System.Collections.Generic;
using System.Linq;
using TiketManagementV2.Model;

namespace TiketManagementV2.View
{
    public class FileUploadInfo : INotifyPropertyChanged
    {
        private string _fileName;
        private double _progress;
        private string _progressText;
        private Timer _timer;
        private Random _random = new Random();

        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }

        public double Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged(nameof(Progress));
                ProgressText = $"{Math.Round(_progress, 0)}% done";
            }
        }

        public string ProgressText
        {
            get { return _progressText; }
            set
            {
                _progressText = value;
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        public FileUploadInfo(string fileName)
        {
            FileName = fileName;
            Progress = 0;
            ProgressText = "0% done";

            // Start a timer to simulate upload progress
            _timer = new Timer(100);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Simulate upload progress
            if (Progress < 100)
            {
                // Random increment between 1 and 5
                double increment = _random.Next(1, 6);
                Progress = Math.Min(Progress + increment, 100);

                // When complete
                if (Progress >= 100)
                {
                    _timer.Stop();
                    ProgressText = "Upload Complete";

                    // Notify the view that upload is complete
                    UploadComplete?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void CancelUpload()
        {
            _timer.Stop();
            UploadCancelled?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler UploadComplete;
        public event EventHandler UploadCancelled;
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class SeatTypeItem
    {
        public string Name { get; set; }
    }

    public class VehicleTypeItem
    {
        public string Name { get; set; }
    }
    public partial class AddVehicleView : Window, INotifyPropertyChanged
    {
        private List<Tuple<string, byte[]>> fileList = new List<Tuple<string, byte[]>>();
        public ObservableCollection<SeatTypeItem> SeatTypes { get; set; }
        public ObservableCollection<VehicleTypeItem> VehicleTypes { get; set; }

        private bool _isDraggingOver = false;
        private string _selectedImagePath;
        private bool _hasUploadedFiles;
        private ApiServices _apiServices;

        public ObservableCollection<string> ImageFiles { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<FileUploadInfo> UploadingFiles { get; set; } = new ObservableCollection<FileUploadInfo>();

        public string SelectedImagePath
        {
            get { return _selectedImagePath; }
            set
            {
                _selectedImagePath = value;
                OnPropertyChanged(nameof(SelectedImagePath));
            }
        }

        public bool HasUploadedFiles
        {
            get { return _hasUploadedFiles; }
            set
            {
                _hasUploadedFiles = value;
                OnPropertyChanged(nameof(HasUploadedFiles));
            }
        }

        public AddVehicleView()
        {
            _apiServices = new ApiServices();

            InitializeComponent();
            SeatTypes = new ObservableCollection<SeatTypeItem>();
            VehicleTypes = new ObservableCollection<VehicleTypeItem>();

            DataContext = this;
            ImageFiles.CollectionChanged += (s, e) => HasUploadedFiles = ImageFiles.Count > 0;
        }

        

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (cmbEditVehicle.SelectedItem is ComboBoxItem comboBoxItem)
            {
                string vehicleType = comboBoxItem.Content.ToString();
                // Process the form submission
                MessageBox.Show($"Vehicle added successfully: {vehicleType} with {ImageFiles.Count} images");
            }
            else
            {
                MessageBox.Show("Please select a vehicle type");
            }
        }

        private void ChangeProfileImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "PNG and JPG Files|*.png;*.jpg",
                Title = "Chọn ảnh xem trước cho phương tiện",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var filePath in openFileDialog.FileNames)
                {
                    byte[] fileData = File.ReadAllBytes(filePath);
                    string fileName = Path.GetFileName(filePath);

                    SimulateFileUpload(fileName, fileData);
                }
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is string fileName)
            {
                ImageFiles.Remove(fileName);

                var fileToRemove = fileList.FirstOrDefault(f => f.Item1 == fileName);
                if (fileToRemove != null)
                {
                    fileList.Remove(fileToRemove);
                }

                if (ImageFiles.Count > 0)
                {
                    SelectedImagePath = ImageFiles[0];
                }
                else
                {
                    SelectedImagePath = string.Empty;
                }
            }
        }

        private void CancelUpload_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string fileName)
            {
                var fileToCancel = GetUploadingFileByName(fileName);
                if (fileToCancel != null)
                {
                    fileToCancel.CancelUpload();

                    var fileToRemove = fileList.FirstOrDefault(f => f.Item1 == fileName);
                    if (fileToRemove != null)
                    {
                        fileList.Remove(fileToRemove);
                    }
                }
            }
        }

        private FileUploadInfo GetUploadingFileByName(string fileName)
        {
            foreach (var file in UploadingFiles)
            {
                if (file.FileName == fileName)
                {
                    return file;
                }
            }
            return null;
        }

        private void SimulateFileUpload(string fileName, byte[] fileData)
        {
            if (ImageFiles.Contains(fileName) || HasUploadingFile(fileName))
            {
                return;
            }

            var fileUpload = new FileUploadInfo(fileName);
            fileUpload.UploadComplete += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    UploadingFiles.Remove(fileUpload);
                    ImageFiles.Add(fileName);
                    if (string.IsNullOrEmpty(SelectedImagePath))
                    {
                        SelectedImagePath = fileName;
                    }
                });
            };

            fileUpload.UploadCancelled += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    UploadingFiles.Remove(fileUpload);
                });
            };

            Application.Current.Dispatcher.Invoke(() =>
            {
                UploadingFiles.Add(fileUpload);
                fileList.Add(new Tuple<string, byte[]>(fileName, fileData));
            });
        }

        private bool HasUploadingFile(string fileName)
        {
            foreach (var file in UploadingFiles)
            {
                if (file.FileName == fileName)
                {
                    return true;
                }
            }
            return false;
        }

        // Drag & Drop Handling
        private void dragDropArea_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                _isDraggingOver = true;
                dragDropArea.BorderBrush = new SolidColorBrush(Colors.LightBlue);
                dragDropArea.Background = new SolidColorBrush(Color.FromArgb(60, 100, 149, 237));
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void dragDropArea_DragLeave(object sender, DragEventArgs e)
        {
            _isDraggingOver = false;
            dragDropArea.BorderBrush = new SolidColorBrush(Color.FromRgb(68, 68, 68));
            dragDropArea.Background = new SolidColorBrush(Color.FromRgb(42, 39, 49));
            e.Handled = true;
        }

        private void dragDropArea_Drop(object sender, DragEventArgs e)
        {
            _isDraggingOver = false;
            dragDropArea.BorderBrush = new SolidColorBrush(Color.FromRgb(68, 68, 68));
            dragDropArea.Background = new SolidColorBrush(Color.FromRgb(42, 39, 49));

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string filePath in files)
                {
                    string extension = Path.GetExtension(filePath).ToLower();
                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".bmp" || extension == ".gif")
                    {
                        byte[] fileData = File.ReadAllBytes(filePath);

                        string fileName = Path.GetFileName(filePath);

                        fileList.Add(new Tuple<string, byte[]>(fileName, fileData));

                        SimulateFileUpload(fileName, fileData);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}