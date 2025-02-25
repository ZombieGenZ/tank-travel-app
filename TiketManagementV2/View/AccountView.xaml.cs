using System;
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

        public ICommand LoadMoreCommand { get; }


        private async void ExecuteBanCommand(object obj)
        {
            var banaccountview = new BanAccountView();
            banaccountview.ShowDialog();
            _circularLoadingControl.Visibility = Visibility.Visible;
            await Task.Delay(1000);
            //Reload();
        }
        private async void ExecuteUnBanCommand(object obj)
        {
            var unbanaccount = new ConfirmationDialogView();
            unbanaccount.ShowDialog();
            _circularLoadingControl.Visibility= Visibility.Visible;

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
            //AccountSearchText = SearchTextBox.Text;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
