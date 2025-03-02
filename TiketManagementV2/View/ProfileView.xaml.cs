using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using TiketManagementV2.Commands;
using TiketManagementV2.Controls;
using TiketManagementV2.Helpers;
using TiketManagementV2.Services;
using Microsoft.Win32;

namespace TiketManagementV2.View
{
    public partial class ProfileView : UserControl, INotifyPropertyChanged
    {
        private dynamic _user;
        private CircularLoadingControl _circularLoadingControl;
        private INotificationService _notificationService;

        // User account properties
        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (_displayName != value)
                {
                    _displayName = value;
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged(nameof(Email));
                }
            }
        }

        private string _phone;
        public string Phone
        {
            get => _phone;
            set
            {
                if (_phone != value)
                {
                    _phone = value;
                    OnPropertyChanged(nameof(Phone));
                }
            }
        }

        private string _pathUser;
        public string PathUser
        {
            get => _pathUser;
            set
            {
                if (_pathUser != value)
                {
                    _pathUser = value;
                    OnPropertyChanged(nameof(PathUser));
                }
            }
        }

        private string _originalDisplayName;
        private string _originalEmail;
        private string _originalPhone;
        private string _originalPathUser;
        private string _password; 

        // Commands
        public ICommand ChangeProfileImageCommand { get; private set; }

        public ProfileView(dynamic user, CircularLoadingControl circularLoadingControl)
        {
            InitializeComponent();

            _user = user;
            _circularLoadingControl = circularLoadingControl;
            _notificationService = new NotificationService();
            try
            {
                DisplayName = user.display_name;
                Email = user.email;
                Phone = user.phone;
                PathUser = user.avatar?.url; // Sử dụng null conditional operator để tránh null reference
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi khởi tạo dữ liệu: {ex.Message}");
            }

            StoreOriginalValues();

            ChangeProfileImageCommand = new RelayCommand(ExecuteChangeProfileImage);

            DataContext = this;
        }

        private void StoreOriginalValues()
        {
            _originalDisplayName = DisplayName;
            _originalEmail = Email;
            _originalPhone = Phone;
            _originalPathUser = PathUser;
        }

        private void ExecuteChangeProfileImage(object obj)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*",
                Title = "Chọn ảnh đại diện"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    PathUser = openFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    _notificationService.ShowNotification("Lỗi", "Không thể thay đổi ảnh: {ex.Message}", NotificationType.Warning);
                }
            }
        }

        private void CloseAllEditModes()
        {
            DisplayNameReadOnly.Visibility = Visibility.Visible;
            DisplayNameEditMode.Visibility = Visibility.Collapsed;
            DisplayNameButtons.Visibility = Visibility.Collapsed;
            DisplayNameEditButton.Visibility = Visibility.Visible;

            EmailReadOnly.Visibility = Visibility.Visible;
            EmailEditMode.Visibility = Visibility.Collapsed;
            EmailButtons.Visibility = Visibility.Collapsed;
            EmailEditButton.Visibility = Visibility.Visible;

            PhoneReadOnly.Visibility = Visibility.Visible;
            PhoneEditMode.Visibility = Visibility.Collapsed;
            PhoneButtons.Visibility = Visibility.Collapsed;
            PhoneEditButton.Visibility = Visibility.Visible;

            PasswordReadOnly.Visibility = Visibility.Visible;
            PasswordEditMode.Visibility = Visibility.Collapsed;
            PasswordButtons.Visibility = Visibility.Collapsed;
            PasswordEditButton.Visibility = Visibility.Visible;
        }


        private void TextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string editModeName)
            {
                CloseAllEditModes();

                var editModeElement = FindName(editModeName) as UIElement;
                var readOnlyElement = sender as UIElement;

                var buttonsName = editModeName.Replace("EditMode", "Buttons");
                var buttonsElement = FindName(buttonsName) as UIElement;

                if (editModeElement != null && readOnlyElement != null && buttonsElement != null)
                {
                    readOnlyElement.Visibility = Visibility.Collapsed;
                    editModeElement.Visibility = Visibility.Visible;
                    buttonsElement.Visibility = Visibility.Visible;

                    if (editModeName == "DisplayNameEditMode")
                    {
                        DisplayNameTextBox.Focus();
                        DisplayNameTextBox.SelectAll();
                    }
                    else if (editModeName == "EmailEditMode")
                    {
                        EmailTextBox.Focus();
                        EmailTextBox.SelectAll();
                    }
                    else if (editModeName == "PhoneEditMode")
                    {
                        PhoneTextBox.Focus();
                        PhoneTextBox.SelectAll();
                    }
                    else if (editModeName == "PasswordEditMode")
                    {
                        PasswordTextBox.Focus();
                    }
                }
            }
        }

        private void SendEmailCode_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                _notificationService.ShowNotification("Lỗi", "Email không được để trống", NotificationType.Warning);
                return;
            }

            if (!IsValidEmail(EmailTextBox.Text))
            {
                _notificationService.ShowNotification("Lỗi", "Email không hợp lệ", NotificationType.Warning);
                return;
            }

            try
            {


                EmailConfirmationCodeTextBox.Visibility = Visibility.Visible;

                EmailSendCodeButton.Visibility = Visibility.Collapsed;
                EmailConfirmButton.Visibility = Visibility.Visible;
                _notificationService.ShowNotification("Thành công", "Mã xác nhận đã được gửi đến email của bạn", NotificationType.Info);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification("Lỗi", "Không thể gửi mã xác nhận", NotificationType.Warning);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string editModeName)
            {
                CloseAllEditModes();

                var editModeElement = FindName(editModeName) as UIElement;

                var buttonsName = editModeName.Replace("EditMode", "Buttons");
                var buttonsElement = FindName(buttonsName) as UIElement;

                var readOnlyName = editModeName.Replace("EditMode", "ReadOnly");
                var readOnlyElement = FindName(readOnlyName) as UIElement;

                var editButtonName = editModeName.Replace("EditMode", "EditButton");
                var editButtonElement = FindName(editButtonName) as UIElement;

                if (editModeElement != null && readOnlyElement != null && buttonsElement != null && editButtonElement != null)
                {
                    readOnlyElement.Visibility = Visibility.Collapsed;
                    editButtonElement.Visibility = Visibility.Collapsed;
                    editModeElement.Visibility = Visibility.Visible;
                    buttonsElement.Visibility = Visibility.Visible;

                    if (editModeName == "EmailEditMode")
                    {
                        EmailConfirmationCodeTextBox.Visibility = Visibility.Collapsed;
                        EmailSendCodeButton.Visibility = Visibility.Visible;
                        EmailConfirmButton.Visibility = Visibility.Collapsed;
                    }

                    if (editModeName == "DisplayNameEditMode")
                    {
                        DisplayNameTextBox.Focus();
                        DisplayNameTextBox.SelectAll();
                    }
                    else if (editModeName == "EmailEditMode")
                    {
                        EmailTextBox.Focus();
                        EmailTextBox.SelectAll();
                    }
                    else if (editModeName == "PhoneEditMode")
                    {
                        PhoneTextBox.Focus();
                        PhoneTextBox.SelectAll();
                    }
                    else if (editModeName == "PasswordEditMode")
                    {
                        PasswordTextBox.Focus();
                    }
                }
            }
        }
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string editModeName)
            {
                if (editModeName == "DisplayNameEditMode")
                {
                    if (string.IsNullOrWhiteSpace(DisplayNameTextBox.Text))
                    {
                        _notificationService.ShowNotification("Lỗi", "Họ tên không được để trống", NotificationType.Warning);
                        return;
                    }
                    DisplayName = DisplayNameTextBox.Text;
                }
                else if (editModeName == "EmailEditMode")
                {
                    if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
                    {
                        _notificationService.ShowNotification("Lỗi", "Email không được để trống", NotificationType.Warning);
                        return;
                    }

                    if (!IsValidEmail(EmailTextBox.Text))
                    {
                        _notificationService.ShowNotification("Lỗi", "Email không hợp lệ", NotificationType.Warning);
                        return;
                    }

                    if (EmailConfirmationCodeTextBox.Visibility == Visibility.Visible &&
                        string.IsNullOrWhiteSpace(EmailConfirmationCodeTextBox.Text))
                    {
                        _notificationService.ShowNotification("Lỗi", "Vui lòng nhập mã xác nhận", NotificationType.Warning);
                        return;
                    }



                    Email = EmailTextBox.Text;
                }
                else if (editModeName == "PhoneEditMode")
                {
                    Phone = PhoneTextBox.Text;
                }
                else if (editModeName == "PasswordEditMode")
                {
                    // Validate password
                    if (PasswordTextBox.Password.Length < 6)
                    {
                        _notificationService.ShowNotification("Lỗi", "Mật khẩu phải có ít nhất 6 ký tự", NotificationType.Warning);
                        return;
                    }

                    if (PasswordTextBox.Password != ConfirmPasswordTextBox.Password)
                    {
                        _notificationService.ShowNotification("Lỗi", "Mật khẩu xác nhận không khớp", NotificationType.Warning);
                        return;
                    }

                    _password = PasswordTextBox.Password;
                }

                CloseEditMode(editModeName);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string editModeName)
            {
                CloseEditMode(editModeName);
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Không tự động đóng chế độ chỉnh sửa khi hộp văn bản mất tiêu điểm
            // Điều này cho phép nhấp vào các nút xác nhận/hủy
        }

        private void CloseEditMode(string editModeName)
        {
            string readOnlyName = editModeName.Replace("EditMode", "ReadOnly");
            string buttonsName = editModeName.Replace("EditMode", "Buttons");
            string editButtonName = editModeName.Replace("EditMode", "EditButton");

            var editModeElement = FindName(editModeName) as UIElement;
            var readOnlyElement = FindName(readOnlyName) as UIElement;
            var buttonsElement = FindName(buttonsName) as UIElement;
            var editButtonElement = FindName(editButtonName) as UIElement;

            if (editModeElement != null && readOnlyElement != null && buttonsElement != null && editButtonElement != null)
            {
                editModeElement.Visibility = Visibility.Collapsed;
                readOnlyElement.Visibility = Visibility.Visible;
                buttonsElement.Visibility = Visibility.Collapsed;
                editButtonElement.Visibility = Visibility.Visible;
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}