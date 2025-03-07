﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TiketManagementV2.Commands;
using TiketManagementV2.Controls;
using TiketManagementV2.Model;
using TiketManagementV2.Services;
using static TiketManagementV2.View.AccountView;

namespace TiketManagementV2.View
{
    /// <summary>
    /// Interaction logic for AccountView.xaml
    /// </summary>
    public partial class AccountView : UserControl, INotifyPropertyChanged
    {
        private int _itemsToLoad = 2;
        private ObservableCollection<Account> _filteredAccounts;
        private bool _canLoadMore;
        private string _SessionTime;
        private ApiServices _service;
        private INotificationService _notificationService;
        private ObservableCollection<Account> Accounts;
        private int _Current = 0;
        private string _searchText = string.Empty;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                FilterAccounts();
            }
        }

        public ObservableCollection<Account> accounts
        {
            get { return Accounts; }
            set { Accounts = value; OnPropertyChanged(nameof(accounts)); }
        }
        public ObservableCollection<Account> filteredBusRoutes
        {
            get { return _filteredAccounts; }
            set { _filteredAccounts = value; OnPropertyChanged(nameof(_filteredAccounts)); }
        }

        public bool CanLoadMore
        {
            get { return _canLoadMore; }
            set { _canLoadMore = value; OnPropertyChanged(nameof(CanLoadMore)); }
        }

        public class Account : INotifyPropertyChanged
        {
            private string id;
            private string role;
            private string name;
            private string email;
            private string phone;
            private string status;
            private dynamic banned;
            private int permission;


            public dynamic Permission
            {
                get => permission;
                set
                {
                    permission = value;
                    OnPropertyChanged(nameof(Permission));
                }
            }

            public dynamic Banned
            {
                get => banned;
                set
                {
                    banned = value;
                    OnPropertyChanged(nameof(Banned));
                }
            }

            public bool IsBanned
            {
                get => banned != null;
            }

            public string Id
            {
                get => id;
                set
                {
                    id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }

            public string Role
            {
                get {
                    return permission == 0 ? "Người dùng" : permission == 1 ? "Doanh nghiệp" : "Quản trị viên";
                }
                set
                {
                    role = value;
                    OnPropertyChanged(nameof(Role));
                }
            }

            public string Name
            {
                get => name;
                set
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
            public string Email
            {
                get => email;
                set
                {
                    email = value;
                    OnPropertyChanged(nameof(email));
                }
            }
            public string Phone
            {
                get => phone;
                set
                {
                    phone = value;
                    OnPropertyChanged(nameof(Phone));
                }
            }
            public string Status
            {
                get => banned != null ? "Banned" : "Active";
                set
                {
                    status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public ICommand BanCommand { get; }
        public ICommand UnBanCommand { get; }
        public ICommand SendMailCommand { get; }
        public ICommand LoadMoreCommand { get; }

        public ICommand SeachCommand { get; }

        private void ExecuteSearchCommand(object obj)
        {
            FilterAccounts();
        }
        private async void ExecuteBanCommand(object obj)
        {
            if (obj is Account account)
            {
                var banaccountview = new BanAccountView(account.Id);
                banaccountview.ShowDialog();
                await Task.Delay(1000);
                await Reload();
            }
        }
        private async void ExecuteSendMailCommand(object obj)
        {
            if (obj is Account account)
            {
                var sendmail = new SendMailView(account.Id);
                sendmail.ShowDialog();
            }
        }
        private async Task<dynamic> UnBan(string user_id)
        {
            try
            {
                Dictionary<string, string> accountHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {Properties.Settings.Default.access_token}" }
                };
                var accountBody = new
                {
                    refresh_token = Properties.Settings.Default.refresh_token,
                    user_id
                };

                dynamic data = await _service.PutWithHeaderAndBodyAsync("api/account-management/unban-account", accountHeader, accountBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async void LUnban(string user_id)
        {
            try
            {
                _circularLoadingControl.Visibility = Visibility.Visible;

                dynamic data = await UnBan(user_id);

                if (data == null)
                {
                    _notificationService.ShowNotification(
                        "Lỗi!",
                        "Không thể kết nối đến máy chủ",
                        NotificationType.Warning
                    );
                    _circularLoadingControl.Visibility = Visibility.Collapsed;
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
                    _circularLoadingControl.Visibility = Visibility.Collapsed;
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
                    _circularLoadingControl.Visibility = Visibility.Collapsed;
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
                    _circularLoadingControl.Visibility = Visibility.Collapsed;
                    return;
                }

                if (data.message == "Mở khóa tài khoản thành công!")
                {
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    _notificationService.ShowNotification(
                        "Thành công",
                        (string)data.message,
                        NotificationType.Success
                    );
                    _circularLoadingControl.Visibility = Visibility.Collapsed;
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
                    _circularLoadingControl.Visibility = Visibility.Collapsed;
                }
            }
            finally
            {
                _circularLoadingControl.Visibility = Visibility.Collapsed;
            }
        }

        private async void ExecuteUnBanCommand(object obj)
        {
            if (obj is Account account)
            {
                var dialog = new ConfirmationDialogView();
                dialog.OnDialogClosed += async (isDelete) =>
                {
                    if (isDelete)
                    {
                        LUnban(account.Id);
                        await Task.Delay(1000);
                        await Reload();
                    }
                };
                dialog.ShowDialog();
            }

        }
        private CircularLoadingControl _circularLoadingControl;

        public AccountView(CircularLoadingControl loading)
        {
            InitializeComponent();
            DataContext = this;
            _circularLoadingControl = loading;
            _notificationService = new NotificationService();
            _SessionTime = DateTime.Now.ToString("o");
            _service = new ApiServices();

            //busrouteSearchText = string.Empty;

            Accounts = new ObservableCollection<Account>();
            BanCommand = new RelayCommandGeneric<Account>(ExecuteBanCommand);
            UnBanCommand = new RelayCommandGeneric<Account>(ExecuteUnBanCommand);
            SendMailCommand = new RelayCommandGeneric<Account>(ExecuteSendMailCommand);
            SeachCommand = new RelayCommand(ExecuteSearchCommand);

            accounts.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(accounts));
            };

            LoadAccount();
        }

        private async Task<dynamic> GetAccount()
        {
            try
            {
                string access_token = Properties.Settings.Default.access_token;
                string refresh_token = Properties.Settings.Default.refresh_token;

                Dictionary<string, string> getAccountDataHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {access_token}" }
                };
                var getAccountDataBody = new
                {
                    refresh_token,
                    session_time = _SessionTime,
                    current = _Current
                };

                return await _service.PostWithHeaderAndBodyAsync("api/account-management/get-account", getAccountDataHeader, getAccountDataBody);
                FilterAccounts();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public async Task LoadAccount()
        {
            try
            {
                _circularLoadingControl.Visibility = Visibility.Visible;

                dynamic data = await GetAccount();

                if (data == null)
                {
                    _notificationService.ShowNotification("Lỗi", "Lỗi kết nối đến máy chủ",
                        NotificationType.Error);
                    return;
                }

                if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" ||
                data.message == "Refresh token không hợp lệ" ||
                data.message == "Bạn không có quyền thực hiện hành động này")
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

                if (data.message == "Lấy danh sách thông tin tài khoản thất bại")
                {
                    _notificationService.ShowNotification("Error", (string)data.message, NotificationType.Warning);
                    return;
                }

                if (data.result.message == "Không tìm thấy kết quả phù hợp")
                {
                    // hiển thị không tìm thấy kết quả phù hợp
                    return;
                }

                foreach (dynamic item in data.result.account)
                {
                    accounts.Add(new Account()
                    {
                        Id = item._id,
                        //Role = item.
                        Name = item.display_name,
                        Email = item.email,
                        Phone = item.phone,
                        Banned = item.penalty,
                        Permission = item.permission,
                    });
                }

                _Current = data.result.current;

                if ((bool)data.result.continued)
                {

                    LoadMoreAcc.Visibility = Visibility.Visible;
                }
                else
                {
                    LoadMoreAcc.Visibility = Visibility.Collapsed;
                }

                dgv.ItemsSource = accounts;

                //FilterManagedVehicles();
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
        private void btnReload_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(SearchTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            SearchText = SearchTextBox.Text;
        }

        private void FilterAccounts()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // Nếu ô tìm kiếm trống, hiển thị tất cả tài khoản
                dgv.ItemsSource = accounts;
            }
            else
            {
                // Lọc tài khoản dựa trên văn bản tìm kiếm (không phân biệt hoa thường)
                var filtered = accounts.Where(a =>
                    (a.Name != null && a.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (a.Email != null && a.Email.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (a.Phone != null && a.Phone.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (a.Role != null && a.Role.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (a.Status != null && a.Status.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();

                dgv.ItemsSource = filtered;
            }

            // Hiển thị thông báo nếu không có kết quả
            txtkco.Visibility = dgv.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task Reload()
        {
            _SessionTime = DateTime.Now.AddSeconds(10).ToString("o");
            _Current = 0;
            accounts.Clear();

            await LoadAccount();
        }

        private async void Reload_OnClick(object sender, RoutedEventArgs e)
        {
            await Reload();
        }
    }
}
