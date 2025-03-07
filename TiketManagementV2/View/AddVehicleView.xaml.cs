﻿using System;
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
using System.Threading.Tasks;
using TiketManagementV2.Services;

namespace TiketManagementV2.View
{
    public class SeatTypeItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }

    public class VehicleTypeItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
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
        private INotificationService _notificationService;

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

        private VehicleManagementView _wd;
        public AddVehicleView(VehicleManagementView wd)
        {
            InitializeComponent();
            _wd = wd;
            _apiServices = new ApiServices();
            _notificationService = new NotificationService();
            LoadingControl.Visibility = Visibility.Collapsed;
            SeatTypes = new ObservableCollection<SeatTypeItem>();
            VehicleTypes = new ObservableCollection<VehicleTypeItem>();

            DataContext = this;
            ImageFiles.CollectionChanged += (s, e) => HasUploadedFiles = ImageFiles.Count > 0;

            LoadData();
        }

        private async Task<dynamic> GetVehicleType()
        {
            try
            {
                Dictionary<string, string> statisticsHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {Properties.Settings.Default.access_token}" }
                };
                var statisticsBody = new
                {
                    refresh_token = Properties.Settings.Default.refresh_token,
                };

                dynamic data = await _apiServices.PostWithHeaderAndBodyAsync("api/vehicle/get-vehicle-type", statisticsHeader, statisticsBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task<dynamic> GetSeatype()
        {
            try
            {
                Dictionary<string, string> statisticsHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {Properties.Settings.Default.access_token}" }
                };
                var statisticsBody = new
                {
                    refresh_token = Properties.Settings.Default.refresh_token,
                };

                dynamic data = await _apiServices.PostWithHeaderAndBodyAsync("api/vehicle/get-seat-type", statisticsHeader, statisticsBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task LoadVehicleType()
        {
            try
            {
                LoadingControl.Visibility = Visibility.Visible;

                dynamic data = await GetVehicleType();

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

                foreach (dynamic item in data.vehicleType)
                {
                    VehicleTypes.Add(new VehicleTypeItem()
                    {
                        Id = item.value,
                        Name = item.display,
                    });
                }

                cmbVehicle.ItemsSource = VehicleTypes;
            }
            finally
            {
                LoadingControl.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadSeaType()
        {
            try
            {
                LoadingControl.Visibility = Visibility.Visible;

                dynamic data = await GetSeatype();

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

                foreach (dynamic item in data.seatType)
                {
                    SeatTypes.Add(new SeatTypeItem()
                    {
                        Id = item.value,
                        Name = item.display,
                    });
                }

                cmbSeatType.ItemsSource = SeatTypes;
            }
            finally
            {
                LoadingControl.Visibility = Visibility.Collapsed;
            }
        }

        private async void LoadData()
        {
            try
            {
                LoadingControl.Visibility = Visibility.Visible;

                Task loadVehicleTypeTask = LoadVehicleType();
                Task loadSeaTypeTask = LoadSeaType();

                await Task.WhenAll(loadVehicleTypeTask, loadSeaTypeTask);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification(
                    "Lỗi!",
                    "Có lỗi xảy ra khi tải dữ liệu",
                    NotificationType.Error
                );
            }
            finally
            {
                LoadingControl.Visibility = Visibility.Collapsed;
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

        private async void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadingControl.Visibility = Visibility.Visible;

                VehicleTypeItem type = (VehicleTypeItem)cmbVehicle.SelectedItem;
                SeatTypeItem seattype = (SeatTypeItem)cmbSeatType.SelectedItem;
                string seatt = txtSeats.Text;
                int seat;
                string rule = txtRules.Text.Trim();
                string ame = txtAmenities.Text.Trim();
                string lic = txtLicensePlate.Text.Trim();

                if (type == null || seattype == null || !int.TryParse(seatt, out seat) || string.IsNullOrWhiteSpace(rule) ||
                    string.IsNullOrWhiteSpace(ame) || string.IsNullOrWhiteSpace(lic))
                {
                    _notificationService.ShowNotification(
                        "Lỗi!",
                        "Vui lòng điền đẩy đủ thông tin",
                        NotificationType.Warning
                    );
                    LoadingControl.Visibility = Visibility.Collapsed;
                    return;
                }

                dynamic data = await CreateVehicle(type.Id, seattype.Id, seat, rule, ame, lic);

                if (data == null)
                {
                    _notificationService.ShowNotification(
                        "Lỗi!",
                        "Không thể kết nối đến máy chủ",
                        NotificationType.Warning
                    );
                    LoadingControl.Visibility = Visibility.Collapsed;
                    return;
                }

                if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" ||
                    data.message == "Refresh token không hợp lệ")
                {
                    _notificationService.ShowNotification(
                        "Lỗi!",
                        (string)data.message,
                        NotificationType.Warning
                    );
                    LoadingControl.Visibility = Visibility.Collapsed;
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
                        NotificationType.Warning
                    );
                    LoadingControl.Visibility = Visibility.Collapsed;
                    return;
                }

                if (data.message == "Lỗi dữ liệu đầu vào")
                {
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    foreach (dynamic item in data.errors)
                    {
                        _notificationService.ShowNotification(
                            "Lỗi kiểu dử liệu đầu vào",
                            (string)item.Value.msg,
                            NotificationType.Warning
                        );
                    }
                    LoadingControl.Visibility = Visibility.Collapsed;
                    return;
                }

                if (data.message == "Tạo thông tin phương tiện thành công!")
                {
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    _notificationService.ShowNotification(
                        "Thành công",
                        (string)data.message,
                        NotificationType.Success
                    );
                    LoadingControl.Visibility = Visibility.Collapsed;
                    Close();
                }
                else
                {
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    _notificationService.ShowNotification(
                        "Lõi!",
                        (string)data.message,
                        NotificationType.Error
                    );
                    LoadingControl.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification(
                    "Lỗi!",
                    ex.Message,
                    NotificationType.Error
                );
                LoadingControl.Visibility = Visibility.Collapsed;
                throw;
            }
            finally
            {
                LoadingControl.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<dynamic> CreateVehicle(int vehicle_type, int seat_type, int seats, string rules, string amenities, string license_plate)
        {
            try
            {
                Dictionary<string, string> statisticsHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {Properties.Settings.Default.access_token}" }
                };
                var statisticsBody = new
                {
                    refresh_token = Properties.Settings.Default.refresh_token,
                    vehicle_type,
                    seat_type,
                    seats,
                    rules,
                    amenities,
                    license_plate
                };

                dynamic data = await _apiServices.UploadMultipleImagesWithPostAsync("api/vehicle/create", statisticsHeader, fileList, statisticsBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private void ChangeProfileImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png",
                Title = "Select Vehicle Images",
                Multiselect = true
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

                        // Validate file type
                        if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                        {
                            MessageBox.Show("Only JPG, JPEG and PNG files are allowed.");
                            continue;
                        }

                        // Validate file size (e.g., max 5MB)
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

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is string fileName)
            {
                Application.Current.Dispatcher.Invoke(() =>
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
                });
            }
        }

        private void CancelUpload_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string fileName)
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