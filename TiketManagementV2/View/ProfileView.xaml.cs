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
using static MaterialDesignThemes.Wpf.Theme;

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

        private async Task<dynamic> SendCode(string email, bool type)
        {
            try
            {
                string access_token = Properties.Settings.Default.access_token;
                string refresh_token = Properties.Settings.Default.refresh_token;

                Dictionary<string, string> userHeader = new Dictionary<string, string>()
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
                    _notificationService.ShowNotification("Lỗi", "Không thể kết nối đến máy chủ",
                        NotificationType.Error);
                    return;
                }

                if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" ||
                    data.message == "Refresh token không hợp lệ")
                {
                    _notificationService.ShowNotification("Lỗi", (string)data.message,
                        NotificationType.Error);
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
                            _notificationService.ShowNotification("Lỗi", (string)item.Value.msg,
                                NotificationType.Warning);
                            EmailSendCodeButton.Content = "Gửi lại?";
                            return;
                        }
                        else if ((string)item.Value.msg == "Mã xác minh email chưa được gửi")
                        {
                            _notificationService.ShowNotification("Lỗi", (string)item.Value.msg,
                                NotificationType.Warning);
                            EmailSendCodeButton.Content = "Send Code";
                            return;
                        }

                        _notificationService.ShowNotification("Lỗi dữ liệu đầu vào", (string)item.Value.msg,
                            NotificationType.Warning);
                    }

                    return;
                }

                if (data.message == "Mã xác minh email đã được gửi thành công! Vui lòng kiểm tra hộp thư của bạn" ||
                    data.message == "Mã xác minh email đã được gửi lại thành công! Vui lòng kiểm tra hộp thư của bạn")
                {
                    _notificationService.ShowNotification("Thành công", (string)data.message,
                        NotificationType.Success);

                    EmailConfirmationCodeTextBox.Visibility = Visibility.Visible;

                    EmailConfirmButton.Visibility = Visibility.Visible;

                    EmailSendCodeButton.Content = "Gửi lại?";
                }
                else
                {
                    _notificationService.ShowNotification("Lỗi", (string)data.message,
                        NotificationType.Error);
                }
            }
            catch (Exception err)
            {
                _notificationService.ShowNotification("Lỗi", err.Message, NotificationType.Error);
                _circularLoadingControl.Visibility = Visibility.Collapsed;
                return;
            }
            finally
            {
                _circularLoadingControl.Visibility = Visibility.Collapsed;
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

        private async Task<dynamic> ChangeDisplayName(string newDisplayName)
        {
            try
            {
                string access_token = Properties.Settings.Default.access_token;
                string refresh_token = Properties.Settings.Default.refresh_token;

                Dictionary<string, string> userHeader = new Dictionary<string, string>()
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

                Dictionary<string, string> userHeader = new Dictionary<string, string>()
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

                Dictionary<string, string> userHeader = new Dictionary<string, string>()
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

                Dictionary<string, string> userHeader = new Dictionary<string, string>()
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
                            _notificationService.ShowNotification("Lỗi", "Vui lòng điền đầy đủ thông tin",
                                NotificationType.Warning);
                            return;
                        }

                        dynamic data = await ChangeDisplayName(displayName.Trim());

                        if (data == null)
                        {
                            _notificationService.ShowNotification("Lỗi", "Không thể kết nối đến máy chủ",
                                NotificationType.Error);
                            return;
                        }

                        if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" ||
                            data.message == "Refresh token không hợp lệ")
                        {
                            _notificationService.ShowNotification("Lỗi", (string)data.message,
                                NotificationType.Error);
                            return;
                        }

                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();

                        if (data.message == "Lỗi dữ liệu đầu vào")
                        {
                            foreach (dynamic item in data.errors)
                            {
                                _notificationService.ShowNotification("Lỗi dữ liệu đầu vào", (string)item.Value.msg,
                                    NotificationType.Warning);
                            }

                            return;
                        }

                        if (data.message == "Thay đổi tên hiển thị thành công!")
                        {
                            _notificationService.ShowNotification("Thành công", (string)data.message,
                                NotificationType.Success);
                            //_mainViewModel.CurrentUserAccount.DisplayName = displayName;
                            _display_name.Text = displayName;
                        }
                        else
                        {
                            _notificationService.ShowNotification("Lỗi", (string)data.message,
                                NotificationType.Error);
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
                            _notificationService.ShowNotification("Lỗi", "Vui lòng điền đầy đủ thông tin",
                                NotificationType.Warning);
                            return;
                        }

                        dynamic data = await ChangeEmail(email.Trim(), verifyCode.Trim());

                        if (data == null)
                        {
                            _notificationService.ShowNotification("Lỗi", "Không thể kết nối đến máy chủ",
                                NotificationType.Error);
                            return;
                        }

                        if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" ||
                            data.message == "Refresh token không hợp lệ")
                        {
                            _notificationService.ShowNotification("Lỗi", (string)data.message,
                                NotificationType.Error);
                            return;
                        }

                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();

                        if (data.message == "Lỗi dữ liệu đầu vào")
                        {
                            foreach (dynamic item in data.errors)
                            {
                                _notificationService.ShowNotification("Lỗi dữ liệu đầu vào", (string)item.Value.msg,
                                    NotificationType.Warning);
                            }

                            return;
                        }

                        if (data.message == "Thay đổi email thành công! Vui lòng đăng nhập lại")
                        {
                            _notificationService.ShowNotification("Thành công", (string)data.message,
                                NotificationType.Success);

                            await DeleteLogout();

                            LoginView loginView = new LoginView();
                            loginView.Show();
                            _adminView.Close();
                        }
                        else
                        {
                            _notificationService.ShowNotification("Lỗi", (string)data.message,
                                NotificationType.Error);
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
                            _notificationService.ShowNotification("Lỗi", "Vui lòng điền đầy đủ thông tin",
                                NotificationType.Warning);
                            return;
                        }

                        dynamic data = await ChangePhone(phone.Trim());

                        if (data == null)
                        {
                            _notificationService.ShowNotification("Lỗi", "Không thể kết nối đến máy chủ",
                                NotificationType.Error);
                            return;
                        }

                        if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" ||
                            data.message == "Refresh token không hợp lệ")
                        {
                            _notificationService.ShowNotification("Lỗi", (string)data.message,
                                NotificationType.Error);
                            return;
                        }

                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();

                        if (data.message == "Lỗi dữ liệu đầu vào")
                        {
                            foreach (dynamic item in data.errors)
                            {
                                _notificationService.ShowNotification("Lỗi dữ liệu đầu vào", (string)item.Value.msg,
                                    NotificationType.Warning);
                            }

                            return;
                        }

                        if (data.message == "Thay đổi số điện thoại thành công!")
                        {
                            _notificationService.ShowNotification("Thành công", (string)data.message,
                                NotificationType.Success);
                        }
                        else
                        {
                            _notificationService.ShowNotification("Lỗi", (string)data.message,
                                NotificationType.Error);
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
                            _notificationService.ShowNotification("Lỗi", "Xác nhận mật khẩu phải trùng khớp với mật khẩu",
                                NotificationType.Warning);
                            return;
                        }

                        dynamic data = await ChangePassword(oldPassword, newPassword, confirmPassword);

                        if (data == null)
                        {
                            _notificationService.ShowNotification("Lỗi", "Không thể kết nối đến máy chủ",
                                NotificationType.Error);
                            return;
                        }

                        if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" ||
                            data.message == "Refresh token không hợp lệ")
                        {
                            _notificationService.ShowNotification("Lỗi", (string)data.message,
                                NotificationType.Error);
                            return;
                        }

                        if (data.message == "Lỗi dữ liệu đầu vào")
                        {
                            foreach (dynamic item in data.errors)
                            {
                                _notificationService.ShowNotification("Lỗi dữ liệu đầu vào", (string)item.Value.msg,
                                    NotificationType.Warning);
                            }
                            return;
                        }

                        if (data.message == "Thay đổi mật khẩu thành công! Vui lòng đăng nhập lại")
                        {
                            _notificationService.ShowNotification("Thành công", (string)data.message,
                                NotificationType.Success);

                            await DeleteLogout();
                            LoginView loginView = new LoginView();
                            loginView.Show();
                            _adminView.Close();
                        }
                        else
                        {
                            _notificationService.ShowNotification("Lỗi", (string)data.message,
                                NotificationType.Error);
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

        private bool _passwordsMatch = false;

        private void ValidatePasswords()
        {
            string newPassword = PasswordTextBox.Visibility == Visibility.Visible ?
                                 PasswordTextBox.Password :
                                 PasswordVisibleTextBox.Text;

            string confirmPassword = ConfirmPasswordTextBox.Visibility == Visibility.Visible ?
                                     ConfirmPasswordTextBox.Password :
                                     ConfirmPasswordVisibleTextBox.Text;

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                PasswordValidationMessage.Text = string.Empty;
                PasswordValidationMessage.Visibility = Visibility.Collapsed;
                _passwordsMatch = false;
                return;
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
            // Cast sender to the correct button type
            var toggleButton = sender as System.Windows.Controls.Button; // Sử dụng namespace đầy đủ
            if (toggleButton == null) return;

            // Find the icon within the button
            var iconBlock = toggleButton.Content as FontAwesome.Sharp.IconBlock;

            // Determine which password field pair this button is associated with
            string targetId = toggleButton.Tag as string;

            if (targetId == "Password")
            {
                if (PasswordTextBox.Visibility == System.Windows.Visibility.Visible)
                {
                    PasswordVisibleTextBox.Text = PasswordTextBox.Password;
                    PasswordTextBox.Visibility = System.Windows.Visibility.Collapsed;
                    PasswordVisibleTextBox.Visibility = System.Windows.Visibility.Visible;

                    if (iconBlock != null)
                    {
                        iconBlock.Icon = FontAwesome.Sharp.IconChar.EyeSlash;
                    }
                }
                else
                {
                    PasswordTextBox.Password = PasswordVisibleTextBox.Text;
                    PasswordVisibleTextBox.Visibility = System.Windows.Visibility.Collapsed;
                    PasswordTextBox.Visibility = System.Windows.Visibility.Visible;

                    if (iconBlock != null)
                    {
                        iconBlock.Icon = FontAwesome.Sharp.IconChar.Eye;
                    }
                }
            }
            else if (targetId == "ConfirmPassword")
            {
                if (ConfirmPasswordTextBox.Visibility == System.Windows.Visibility.Visible)
                {
                    ConfirmPasswordVisibleTextBox.Text = ConfirmPasswordTextBox.Password;
                    ConfirmPasswordTextBox.Visibility = System.Windows.Visibility.Collapsed;
                    ConfirmPasswordVisibleTextBox.Visibility = System.Windows.Visibility.Visible;

                    if (iconBlock != null)
                    {
                        iconBlock.Icon = FontAwesome.Sharp.IconChar.EyeSlash;
                    }
                }
                else
                {
                    ConfirmPasswordTextBox.Password = ConfirmPasswordVisibleTextBox.Text;
                    ConfirmPasswordVisibleTextBox.Visibility = System.Windows.Visibility.Collapsed;
                    ConfirmPasswordTextBox.Visibility = System.Windows.Visibility.Visible;

                    if (iconBlock != null)
                    {
                        iconBlock.Icon = FontAwesome.Sharp.IconChar.Eye;
                    }
                }
            }
            else if (targetId == "OldPassword")
            {
                if (OldPasswordTextBox.Visibility == System.Windows.Visibility.Visible)
                {
                    var visibleTextBox = FindName("OldPasswordVisibleTextBox") as System.Windows.Controls.TextBox;
                    if (visibleTextBox != null)
                    {
                        visibleTextBox.Text = OldPasswordTextBox.Password;
                        OldPasswordTextBox.Visibility = System.Windows.Visibility.Collapsed;
                        visibleTextBox.Visibility = System.Windows.Visibility.Visible;

                        if (iconBlock != null)
                        {
                            iconBlock.Icon = FontAwesome.Sharp.IconChar.EyeSlash;
                        }
                    }
                }
                else
                {
                    var visibleTextBox = FindName("OldPasswordVisibleTextBox") as System.Windows.Controls.TextBox;
                    if (visibleTextBox != null)
                    {
                        OldPasswordTextBox.Password = visibleTextBox.Text;
                        visibleTextBox.Visibility = System.Windows.Visibility.Collapsed;
                        OldPasswordTextBox.Visibility = System.Windows.Visibility.Visible;

                        if (iconBlock != null)
                        {
                            iconBlock.Icon = FontAwesome.Sharp.IconChar.Eye;
                        }
                    }
                }
            }

            ValidatePasswords();
        }


        private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ValidatePasswords();

        }

        private void PasswordVisibleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidatePasswords();

        }

        private void ConfirmPasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ValidatePasswords();

        }

        private void ConfirmPasswordVisibleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidatePasswords();

        }
    }
}