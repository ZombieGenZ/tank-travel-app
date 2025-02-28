using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using TiketManagementV2.Model;
using TiketManagementV2.Services;

namespace TiketManagementV2.View
{
    public partial class GlobalNotificationView : Window, INotifyPropertyChanged
    {
        private List<Tuple<string, byte[]>> fileList = new List<Tuple<string, byte[]>>();
        private bool _isDraggingOver = false;
        private bool _hasUploadedFiles;
        private ApiServices _apiServices;
        private INotificationService _notificationService;

        public ObservableCollection<string> ImageFiles { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<FileUploadInfo> UploadingFiles { get; set; } = new ObservableCollection<FileUploadInfo>();

        public bool HasUploadedFiles
        {
            get { return _hasUploadedFiles; }
            set
            {
                _hasUploadedFiles = value;
                OnPropertyChanged(nameof(HasUploadedFiles));
            }
        }

        public GlobalNotificationView()
        {
            InitializeComponent();
            _apiServices = new ApiServices();
            _notificationService = new NotificationService();
            DataContext = this;
            ImageFiles.CollectionChanged += (s, e) => HasUploadedFiles = ImageFiles.Count > 0;
            LoadNotification();
        }

        private async void LoadNotification()
        {
            try
            {
                // Loading

                dynamic data = await GetNotificationGlobal();

                if (data == null)
                {
                    _notificationService.ShowNotification("Lỗi", "Không thể kết nối đến máy chủ", NotificationType.Error);
                    return;
                }

                if (data.message == "Lấy thông báo thất bại")
                {
                    return;
                }

                txtTitle.Text = (string)data.title;
                txtNotification.Text = (string)data.description;
            }
            finally
            {
                // Loading
            }
        }

        private async Task<dynamic> GetNotificationGlobal()
        {
            try
            {
                return await _apiServices.GetAsync("api/notification-global/get-notification");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task<dynamic> SetNotificationGlobal(string title, string description)
        {
            try
            {
                Dictionary<string, string> notificationHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {Properties.Settings.Default.access_token}" }
                };
                var notificationBody = new
                {
                    refresh_token = Properties.Settings.Default.refresh_token,
                    title,
                    description
                };

                return await _apiServices.UploadSingleImageWithPutAsync("api/notification-global/set-notification", notificationHeader, fileList[0], notificationBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
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

        private async void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string title = txtTitle.Text.Trim();
                string description = txtNotification.Text.Trim();

                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description) || fileList.Count < 1)
                {
                    _notificationService.ShowNotification(
                        "Lỗi!",
                        "Vui lòng điền đầy đủ thông tin",
                        NotificationType.Warning
                    );
                    return;
                }

                dynamic data = await SetNotificationGlobal(title, description);

                if (data == null)
                {
                    _notificationService.ShowNotification(
                        "Lỗi!",
                        "Không thể kết nối đến máy chủ",
                        NotificationType.Error
                    );
                    return;
                }

                if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" ||
                    data.message == "Refresh token không hợp lệ")
                {
                    _notificationService.ShowNotification(
                        "Lỗi!",
                        (string)data.message,
                        NotificationType.Error
                    );
                    return;
                }

                if (data.message == "Bạn không có quyền thực hiện hành động này")
                {
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    _notificationService.ShowNotification(
                        "Lỗi!",
                        (string)data.message,
                        NotificationType.Error
                    );
                    return;
                }

                if (data.message == "Đặt thông báo thành công!")
                {
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    _notificationService.ShowNotification(
                        "Thành công",
                        (string)data.message,
                        NotificationType.Success
                    );
                    Close();
                }
                else
                {
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    _notificationService.ShowNotification(
                        "Lỗi!",
                        (string)data.message,
                        NotificationType.Error
                    );
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification(
                    "Lỗi!",
                    ex.Message,
                    NotificationType.Error
                );
            }
        }

        private void btnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            fileList.Clear();
            ImageFiles.Clear();

            foreach (var file in UploadingFiles.ToList())
            {
                file.CancelUpload();
            }
            UploadingFiles.Clear();

            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png",
                Title = "Select Notification Images",
                Multiselect = false 
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var filePath in openFileDialog.FileNames)
                {
                    try
                    {
                        byte[] fileData = File.ReadAllBytes(filePath);
                        string fileName = Path.GetFileName(filePath);
                        string extension = Path.GetExtension(filePath).ToLower();

                        if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                        {
                            MessageBox.Show("Only JPG, JPEG and PNG files are allowed.");
                            continue;
                        }

                        if (fileData.Length > 5 * 1024 * 1024)
                        {
                            MessageBox.Show($"File {fileName} is too large. Maximum size is 5MB.");
                            continue;
                        }

                        SimulateFileUpload(fileName, fileData);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error reading file: {ex.Message}");
                    }
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.DataContext is string fileName)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ImageFiles.Remove(fileName);
                    var fileToRemove = fileList.FirstOrDefault(f => f.Item1 == fileName);
                    if (fileToRemove != null)
                    {
                        fileList.Remove(fileToRemove);
                    }
                });
            }
        }

        private void CancelUpload_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.CommandParameter is string fileName)
            {
                Application.Current.Dispatcher.Invoke(() =>
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
                });
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

            fileUpload.StartUpload();
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
            dragDropArea.BorderBrush = new SolidColorBrush(Colors.DarkGray);
            dragDropArea.Background = new SolidColorBrush(Colors.Transparent);
            e.Handled = true;
        }

        private void dragDropArea_Drop(object sender, DragEventArgs e)
        {
            _isDraggingOver = false;
            dragDropArea.BorderBrush = new SolidColorBrush(Colors.DarkGray);
            dragDropArea.Background = new SolidColorBrush(Colors.Transparent);

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    fileList.Clear();
                    ImageFiles.Clear();

                    foreach (var file in UploadingFiles.ToList())
                    {
                        file.CancelUpload();
                    }
                    UploadingFiles.Clear();

                    string filePath = files[0];
                    string extension = Path.GetExtension(filePath).ToLower();
                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                    {
                        try
                        {
                            byte[] fileData = File.ReadAllBytes(filePath);

                            if (fileData.Length > 5 * 1024 * 1024)
                            {
                                MessageBox.Show($"File {Path.GetFileName(filePath)} quá lớn. Kích thước tối đa là 5MB.");
                                return;
                            }

                            string fileName = Path.GetFileName(filePath);
                            SimulateFileUpload(fileName, fileData);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Lỗi khi đọc file: {ex.Message}");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Chỉ chấp nhận file định dạng JPG, JPEG và PNG.");
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

    public class FileUploadInfo : INotifyPropertyChanged
    {
        private System.Timers.Timer _timer;
        private bool _isCancelled;
        private double _progress;
        private string _fileName;

        public event EventHandler UploadComplete;
        public event EventHandler UploadCancelled;
        public event PropertyChangedEventHandler PropertyChanged;

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
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        public string ProgressText => $"{Progress:F0}%";

        public FileUploadInfo(string fileName)
        {
            FileName = fileName;
            Progress = 0;
            _isCancelled = false;
            _timer = new System.Timers.Timer(50);
            _timer.Elapsed += TimerElapsed;
        }

        public void StartUpload()
        {
            _timer.Start();
        }

        public void CancelUpload()
        {
            _isCancelled = true;
            _timer.Stop();
            UploadCancelled?.Invoke(this, EventArgs.Empty);
        }

        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_isCancelled) return;

            Progress += 5;

            if (Progress >= 100)
            {
                _timer.Stop();
                Progress = 100;
                UploadComplete?.Invoke(this, EventArgs.Empty);
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}