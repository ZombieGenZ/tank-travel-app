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
using System.Collections.Generic;
using System.Threading.Tasks;
using TiketManagementV2.Model;
using TiketManagementV2.ViewModel;

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

        private string Password;
        public string password
        {
            get => Password;
            set
            {
                if (Password != value)
                {
                    Password = value;
                    OnPropertyChanged(nameof(password));
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

        private ApiServices _services;
        private MainViewModel _mainViewModel;
        private TextBlock _display_name;
        private AdminView _adminView;

        public ProfileView(dynamic user, CircularLoadingControl circularLoadingControl, MainViewModel mainView, TextBlock display_name, AdminView adminView)
        {
            InitializeComponent();
            _adminView = adminView;
            _display_name = display_name;
            _mainViewModel = mainView;
            _services = new ApiServices();
            _user = user;
            _circularLoadingControl = circularLoadingControl;
            _notificationService = new NotificationService();

            try
            {
                DisplayName = user.display_name;
                Email = user.email;
                Phone = user.phone;
                PathUser = user.avatar?.url;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi khởi tạo dữ liệu: {ex.Message}");
            }

            StoreOriginalValues();

            ChangeProfileImageCommand = new RelayCommand(ExecuteChangeProfileImage);

            DataContext = this;

            // Gắn sự kiện cho TextBox và PasswordBox
            DisplayNameTextBox.TextChanged += DisplayNameTextBox_TextChanged;
            EmailTextBox.TextChanged += EmailTextBox_TextChanged;
            PhoneTextBox.TextChanged += PhoneTextBox_TextChanged;
            EmailConfirmationCodeTextBox.TextChanged += EmailConfirmationCodeTextBox_TextChanged;
            OldPasswordTextBox.PasswordChanged += OldPasswordTextBox_PasswordChanged;
            PasswordTextBox.PasswordChanged += PasswordTextBox_PasswordChanged;
            ConfirmPasswordTextBox.PasswordChanged += ConfirmPasswordTextBox_PasswordChanged;
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
                    _notificationService.ShowNotification("Lỗi", $"Không thể thay đổi ảnh: {ex.Message}", NotificationType.Warning);
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
                        OldPasswordTextBox.Focus();
                    }
                }
            }
        }

        private async Task<dynamic> SendCode(string email, bool type)
        {
            try
            {
                string access_token = Properties.Settings.Default.access_token;
                string refresh_token = Properties.Settings.Default.refresh_token;

                Dictionary<string, string> userHeader = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {access_token}" }
                };
                var userBody = new
                {
                    refresh_token,
                    email
                };

                if (type)
                {
                    return await _services.PostWithHeaderAndBodyAsync("api/users/send-email-verify-change-email", userHeader, userBody);
                }
                else
                {
                    return await _services.PutWithHeaderAndBodyAsync("api/users/resend-email-verify-change-email", userHeader, userBody);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async void SendEmailCode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _circularLoadingControl.Visibility = Visibility.Visible;

                string email = EmailTextBox.Text;
                string btnContent = (string)EmailSendCodeButton.Content;

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(btnContent))
                {
                    _notificationService.ShowNotification("Lỗi", "Vui lòng điền đầy đủ thông tin", NotificationType.Warning);
                    return;
                }

                if (!IsValidEmail(email))
                {
                    _notificationService.ShowNotification("Lỗi", "Email không hợp lệ", NotificationType.Warning);
                    return;
                }

                bool isSend = btnContent == "Send Code";

                dynamic data = await SendCode(email.Trim(), isSend);

                if (data == null)
                {
                    _notificationService.ShowNotification("Lỗi", "Không thể kết nối đến máy chủ", NotificationType.Error);
                    return;
                }

                if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" || data.message == "Refresh token không hợp lệ")
                {
                    _notificationService.ShowNotification("Lỗi", (string)data.message, NotificationType.Error);
                    return;
                }

                Properties.Settings.Default.access_token = data.authenticate.access_token;
                Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                Properties.Settings.Default.Save();

                if (data.message == "Lỗi dữ liệu đầu vào")
                {
                    foreach (dynamic item in data.errors)
                    {
                        if ((string)item.Value.msg == "Email đã được gửi, vui lòng kiểm tra hộp thư của bạn")
                        {
                            _notificationService.ShowNotification("Lỗi", (string)item.Value.msg, NotificationType.Warning);
                            EmailSendCodeButton.Content = "Gửi lại?";
                            return;
                        }
                        else if ((string)item.Value.msg == "Mã xác minh email chưa được gửi")
                        {
                            _notificationService.ShowNotification("Lỗi", (string)item.Value.msg, NotificationType.Warning);
                            EmailSendCodeButton.Content = "Send Code";
                            return;
                        }

                        _notificationService.ShowNotification("Lỗi dữ liệu đầu vào", (string)item.Value.msg, NotificationType.Warning);
                    }

                    return;
                }

                if (data.message == "Mã xác minh email đã được gửi thành công! Vui lòng kiểm tra hộp thư của bạn" ||
                    data.message == "Mã xác minh email đã được gửi lại thành công! Vui lòng kiểm tra hộp thư của bạn")
                {
                    _notificationService.ShowNotification("Thành công", (string)data.message, NotificationType.Success);

                    EmailConfirmationCodeTextBox.Visibility = Visibility.Visible;
                    EmailConfirmationCodePlaceholder.Visibility = Visibility.Visible; // Hiển thị placeholder khi TextBox hiển thị
                    EmailConfirmButton.Visibility = Visibility.Visible;
                    EmailSendCodeButton.Content = "Gửi lại?";
                }
                else
                {
                    _notificationService.ShowNotification("Lỗi", (string)data.message, NotificationType.Error);
                }
            }
            catch (Exception err)
            {
                _notificationService.ShowNotification("Lỗi", err.Message, NotificationType.Error);
            }
            finally
            {
                _circularLoadingControl.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<dynamic> ChangeDisplayName(string newDisplayName)
        {
            try
            {
                string access_token = Properties.Settings.Default.access_token;
                string refresh_token = Properties.Settings.Default.refresh_token;

                Dictionary<string, string> userHeader = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {access_token}" }
                };
                var userBody = new
                {
                    refresh_token,
                    new_display_name = newDisplayName
                };

                return await _services.PutWithHeaderAndBodyAsync("api/users/change-display-name", userHeader, userBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task<dynamic> ChangeEmail(string newEmail, string verifyCode)
        {
            try
            {
                string access_token = Properties.Settings.Default.access_token;
                string refresh_token = Properties.Settings.Default.refresh_token;

                Dictionary<string, string> userHeader = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {access_token}" }
                };
                var userBody = new
                {
                    refresh_token,
                    new_email = newEmail,
                    email_verify_code = verifyCode
                };

                return await _services.PutWithHeaderAndBodyAsync("api/users/change-email", userHeader, userBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task<dynamic> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            try
            {
                string access_token = Properties.Settings.Default.access_token;
                string refresh_token = Properties.Settings.Default.refresh_token;

                Dictionary<string, string> userHeader = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {access_token}" }
                };
                var userBody = new
                {
                    refresh_token,
                    password = oldPassword,
                    new_password = newPassword,
                    comform_new_password = confirmPassword
                };

                return await _services.PutWithHeaderAndBodyAsync("api/users/change-password", userHeader, userBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task<bool> DeleteLogout()
        {
            try
            {
                var logoutBody = new
                {
                    refresh_token = Properties.Settings.Default.refresh_token
                };

                await _services.DeleteWithBodyAsync("api/users/logout", logoutBody);

                Properties.Settings.Default.access_token = "";
                Properties.Settings.Default.refresh_token = "";
                Properties.Settings.Default.Save();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        private async Task<dynamic> ChangePhone(string phone)
        {
            try
            {
                string access_token = Properties.Settings.Default.access_token;
                string refresh_token = Properties.Settings.Default.refresh_token;

                Dictionary<string, string> userHeader = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {access_token}" }
                };
                var userBody = new
                {
                    refresh_token,
                    new_phone = phone
                };

                return await _services.PutWithHeaderAndBodyAsync("api/users/change-phone", userHeader, userBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string editModeName)
            {
                if (editModeName == "DisplayNameEditMode")
                {
                    try
                    {
                        _circularLoadingControl.Visibility = Visibility.Visible;

                        string displayName = DisplayNameTextBox.Text;

                        if (string.IsNullOrWhiteSpace(displayName))
                        {
                            _notificationService.ShowNotification("Lỗi", "Vui lòng điền đầy đủ thông tin", NotificationType.Warning);
                            return;
                        }

                        dynamic data = await ChangeDisplayName(displayName.Trim());

                        if (data == null)
                        {
                            _notificationService.ShowNotification("Lỗi", "Không thể kết nối đến máy chủ", NotificationType.Error);
                            return;
                        }

                        if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" || data.message == "Refresh token không hợp lệ")
                        {
                            _notificationService.ShowNotification("Lỗi", (string)data.message, NotificationType.Error);
                            return;
                        }

                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();

                        if (data.message == "Lỗi dữ liệu đầu vào")
                        {
                            foreach (dynamic item in data.errors)
                            {
                                _notificationService.ShowNotification("Lỗi dữ liệu đầu vào", (string)item.Value.msg, NotificationType.Warning);
                            }
                            return;
                        }

                        if (data.message == "Thay đổi tên hiển thị thành công!")
                        {
                            _notificationService.ShowNotification("Thành công", (string)data.message, NotificationType.Success);
                            _display_name.Text = displayName;
                        }
                        else
                        {
                            _notificationService.ShowNotification("Lỗi", (string)data.message, NotificationType.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        _notificationService.ShowNotification("Error", ex.Message, NotificationType.Error);
                    }
                    finally
                    {
                        _circularLoadingControl.Visibility = Visibility.Collapsed;
                    }
                }
                else if (editModeName == "EmailEditMode")
                {
                    try
                    {
                        _circularLoadingControl.Visibility = Visibility.Visible;

                        string email = EmailTextBox.Text;
                        string verifyCode = EmailConfirmationCodeTextBox.Text;

                        if (string.IsNullOrWhiteSpace(email) ||
                            EmailConfirmationCodeTextBox.Visibility == Visibility.Collapsed ||
                            EmailConfirmationCodeTextBox.Visibility == Visibility.Hidden ||
                            string.IsNullOrWhiteSpace(verifyCode))
                        {
                            _notificationService.ShowNotification("Lỗi", "Vui lòng điền đầy đủ thông tin", NotificationType.Warning);
                            return;
                        }

                        dynamic data = await ChangeEmail(email.Trim(), verifyCode.Trim());

                        if (data == null)
                        {
                            _notificationService.ShowNotification("Lỗi", "Không thể kết nối đến máy chủ", NotificationType.Error);
                            return;
                        }

                        if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" || data.message == "Refresh token không hợp lệ")
                        {
                            _notificationService.ShowNotification("Lỗi", (string)data.message, NotificationType.Error);
                            return;
                        }

                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();

                        if (data.message == "Lỗi dữ liệu đầu vào")
                        {
                            foreach (dynamic item in data.errors)
                            {
                                _notificationService.ShowNotification("Lỗi dữ liệu đầu vào", (string)item.Value.msg, NotificationType.Warning);
                            }
                            return;
                        }

                        if (data.message == "Thay đổi email thành công! Vui lòng đăng nhập lại")
                        {
                            _notificationService.ShowNotification("Thành công", (string)data.message, NotificationType.Success);

                            await DeleteLogout();

                            LoginView loginView = new LoginView();
                            loginView.Show();
                            _adminView.Close();
                        }
                        else
                        {
                            _notificationService.ShowNotification("Lỗi", (string)data.message, NotificationType.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        _notificationService.ShowNotification("Error", ex.Message, NotificationType.Error);
                    }
                    finally
                    {
                        _circularLoadingControl.Visibility = Visibility.Collapsed;
                    }
                }
                else if (editModeName == "PhoneEditMode")
                {
                    try
                    {
                        _circularLoadingControl.Visibility = Visibility.Visible;

                        string phone = PhoneTextBox.Text;

                        if (string.IsNullOrWhiteSpace(phone))
                        {
                            _notificationService.ShowNotification("Lỗi", "Vui lòng điền đầy đủ thông tin", NotificationType.Warning);
                            return;
                        }

                        dynamic data = await ChangePhone(phone.Trim());

                        if (data == null)
                        {
                            _notificationService.ShowNotification("Lỗi", "Không thể kết nối đến máy chủ", NotificationType.Error);
                            return;
                        }

                        if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" || data.message == "Refresh token không hợp lệ")
                        {
                            _notificationService.ShowNotification("Lỗi", (string)data.message, NotificationType.Error);
                            return;
                        }

                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();

                        if (data.message == "Lỗi dữ liệu đầu vào")
                        {
                            foreach (dynamic item in data.errors)
                            {
                                _notificationService.ShowNotification("Lỗi dữ liệu đầu vào", (string)item.Value.msg, NotificationType.Warning);
                            }
                            return;
                        }

                        if (data.message == "Thay đổi số điện thoại thành công!")
                        {
                            _notificationService.ShowNotification("Thành công", (string)data.message, NotificationType.Success);
                        }
                        else
                        {
                            _notificationService.ShowNotification("Lỗi", (string)data.message, NotificationType.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        _notificationService.ShowNotification("Error", ex.Message, NotificationType.Error);
                    }
                    finally
                    {
                        _circularLoadingControl.Visibility = Visibility.Collapsed;
                    }
                }
                else if (editModeName == "PasswordEditMode")
                {
                    try
                    {
                        _circularLoadingControl.Visibility = Visibility.Visible;

                        string oldPassword = OldPasswordTextBox.Password;
                        string newPassword = PasswordTextBox.Password;
                        string confirmPassword = ConfirmPasswordTextBox.Password;

                        if (newPassword != confirmPassword)
                        {
                            _notificationService.ShowNotification("Lỗi", "Xác nhận mật khẩu phải trùng khớp với mật khẩu", NotificationType.Warning);
                            return;
                        }

                        dynamic data = await ChangePassword(oldPassword, newPassword, confirmPassword);

                        if (data == null)
                        {
                            _notificationService.ShowNotification("Lỗi", "Không thể kết nối đến máy chủ", NotificationType.Error);
                            return;
                        }

                        if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" || data.message == "Refresh token không hợp lệ")
                        {
                            _notificationService.ShowNotification("Lỗi", (string)data.message, NotificationType.Error);
                            return;
                        }

                        if (data.message == "Lỗi dữ liệu đầu vào")
                        {
                            foreach (dynamic item in data.errors)
                            {
                                _notificationService.ShowNotification("Lỗi dữ liệu đầu vào", (string)item.Value.msg, NotificationType.Warning);
                            }
                            return;
                        }

                        if (data.message == "Thay đổi mật khẩu thành công! Vui lòng đăng nhập lại")
                        {
                            _notificationService.ShowNotification("Thành công", (string)data.message, NotificationType.Success);

                            await DeleteLogout();
                            LoginView loginView = new LoginView();
                            loginView.Show();
                            _adminView.Close();
                        }
                        else
                        {
                            _notificationService.ShowNotification("Lỗi", (string)data.message, NotificationType.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        _notificationService.ShowNotification("Error", ex.Message, NotificationType.Error);
                    }
                    finally
                    {
                        _circularLoadingControl.Visibility = Visibility.Collapsed;
                    }
                }

                CloseEditMode(editModeName);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string editModeName)
            {
                CloseEditMode(editModeName);

                // Khôi phục giá trị gốc nếu cần
                if (editModeName == "DisplayNameEditMode") DisplayName = _originalDisplayName;
                else if (editModeName == "EmailEditMode") Email = _originalEmail;
                else if (editModeName == "PhoneEditMode") Phone = _originalPhone;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Không tự động đóng chế độ chỉnh sửa khi mất tiêu điểm
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

                // Đặt lại trạng thái cho EmailEditMode
                if (editModeName == "EmailEditMode")
                {
                    EmailConfirmationCodeTextBox.Visibility = Visibility.Collapsed;
                    EmailConfirmationCodePlaceholder.Visibility = Visibility.Collapsed; // Ẩn placeholder khi TextBox ẩn
                    EmailSendCodeButton.Content = "Send Code";
                    EmailSendCodeButton.Visibility = Visibility.Visible;
                    EmailConfirmButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void DisplayNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DisplayNamePlaceholder.Visibility = string.IsNullOrWhiteSpace(DisplayNameTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EmailPlaceholder.Visibility = string.IsNullOrWhiteSpace(EmailTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void PhoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PhonePlaceholder.Visibility = string.IsNullOrWhiteSpace(PhoneTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void EmailConfirmationCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EmailConfirmationCodePlaceholder.Visibility = string.IsNullOrWhiteSpace(EmailConfirmationCodeTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OldPasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            OldPasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(OldPasswordTextBox.Password) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(PasswordTextBox.Password) ? Visibility.Visible : Visibility.Collapsed;
            ValidatePasswords();
        }

        private void ConfirmPasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ConfirmPasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(ConfirmPasswordTextBox.Password) ? Visibility.Visible : Visibility.Collapsed;
            ValidatePasswords();
        }

        private void OldPasswordVisibleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OldPasswordTextBox.Password = (sender as TextBox)?.Text;
            OldPasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(OldPasswordVisibleTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void PasswordVisibleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PasswordTextBox.Password = (sender as TextBox)?.Text;
            PasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(PasswordVisibleTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            ValidatePasswords();
        }

        private void ConfirmPasswordVisibleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfirmPasswordTextBox.Password = (sender as TextBox)?.Text;
            ConfirmPasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(ConfirmPasswordVisibleTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            ValidatePasswords();
        }

        private void UpdatePlaceholderVisibility(TextBlock placeholder, string text)
        {
            if (placeholder != null)
            {
                placeholder.Visibility = string.IsNullOrEmpty(text) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private bool _passwordsMatch = false;

        private void ValidatePasswords()
        {
            string newPassword = PasswordTextBox.Visibility == Visibility.Visible ? PasswordTextBox.Password : (FindName("PasswordVisibleTextBox") as TextBox)?.Text;
            string confirmPassword = ConfirmPasswordTextBox.Visibility == Visibility.Visible ? ConfirmPasswordTextBox.Password : (FindName("ConfirmPasswordVisibleTextBox") as TextBox)?.Text;

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                PasswordValidationMessage.Text = string.Empty;
                PasswordValidationMessage.Visibility = Visibility.Collapsed;
                _passwordsMatch = false;
                return;
            }

            if (newPassword != confirmPassword)
            {
                PasswordValidationMessage.Text = "Xác nhận mật khẩu không khớp!";
                PasswordValidationMessage.Visibility = Visibility.Visible;
                _passwordsMatch = false;
            }
            else
            {
                PasswordValidationMessage.Text = string.Empty;
                PasswordValidationMessage.Visibility = Visibility.Collapsed;
                _passwordsMatch = true;
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

        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as Button;
            if (toggleButton == null) return;

            var iconBlock = toggleButton.Content as FontAwesome.Sharp.IconBlock;

            string targetId = toggleButton.Tag as string;

            if (targetId == "OldPassword")
            {
                var oldPasswordTextBox = FindName("OldPasswordTextBox") as PasswordBox;
                var oldPasswordVisibleTextBox = FindName("OldPasswordVisibleTextBox") as TextBox;

                if (oldPasswordTextBox.Visibility == System.Windows.Visibility.Visible)
                {
                    oldPasswordVisibleTextBox.Text = oldPasswordTextBox.Password;
                    oldPasswordTextBox.Visibility = System.Windows.Visibility.Collapsed;
                    oldPasswordVisibleTextBox.Visibility = System.Windows.Visibility.Visible;

                    if (iconBlock != null)
                    {
                        iconBlock.Icon = FontAwesome.Sharp.IconChar.EyeSlash;
                    }
                }
                else
                {
                    oldPasswordTextBox.Password = oldPasswordVisibleTextBox.Text;
                    oldPasswordVisibleTextBox.Visibility = System.Windows.Visibility.Collapsed;
                    oldPasswordTextBox.Visibility = System.Windows.Visibility.Visible;

                    if (iconBlock != null)
                    {
                        iconBlock.Icon = FontAwesome.Sharp.IconChar.Eye;
                    }
                }
            }
            else if (targetId == "Password")
            {
                var passwordTextBox = FindName("PasswordTextBox") as PasswordBox;
                var passwordVisibleTextBox = FindName("PasswordVisibleTextBox") as TextBox;

                if (passwordTextBox.Visibility == System.Windows.Visibility.Visible)
                {
                    passwordVisibleTextBox.Text = passwordTextBox.Password;
                    passwordTextBox.Visibility = System.Windows.Visibility.Collapsed;
                    passwordVisibleTextBox.Visibility = System.Windows.Visibility.Visible;

                    if (iconBlock != null)
                    {
                        iconBlock.Icon = FontAwesome.Sharp.IconChar.EyeSlash;
                    }
                }
                else
                {
                    passwordTextBox.Password = passwordVisibleTextBox.Text;
                    passwordVisibleTextBox.Visibility = System.Windows.Visibility.Collapsed;
                    passwordTextBox.Visibility = System.Windows.Visibility.Visible;

                    if (iconBlock != null)
                    {
                        iconBlock.Icon = FontAwesome.Sharp.IconChar.Eye;
                    }
                }
            }
            else if (targetId == "ConfirmPassword")
            {
                var confirmPasswordTextBox = FindName("ConfirmPasswordTextBox") as PasswordBox;
                var confirmPasswordVisibleTextBox = FindName("ConfirmPasswordVisibleTextBox") as TextBox;

                if (confirmPasswordTextBox.Visibility == System.Windows.Visibility.Visible)
                {
                    confirmPasswordVisibleTextBox.Text = confirmPasswordTextBox.Password;
                    confirmPasswordTextBox.Visibility = System.Windows.Visibility.Collapsed;
                    confirmPasswordVisibleTextBox.Visibility = System.Windows.Visibility.Visible;

                    if (iconBlock != null)
                    {
                        iconBlock.Icon = FontAwesome.Sharp.IconChar.EyeSlash;
                    }
                }
                else
                {
                    confirmPasswordTextBox.Password = confirmPasswordVisibleTextBox.Text;
                    confirmPasswordVisibleTextBox.Visibility = System.Windows.Visibility.Collapsed;
                    confirmPasswordTextBox.Visibility = System.Windows.Visibility.Visible;

                    if (iconBlock != null)
                    {
                        iconBlock.Icon = FontAwesome.Sharp.IconChar.Eye;
                    }
                }
            }

            ValidatePasswords();
        }
    }
}