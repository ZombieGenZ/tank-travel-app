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
using System.Xml.Linq;
using TiketManagementV2.Commands;
using TiketManagementV2.Controls;
using TiketManagementV2.Model;
using TiketManagementV2.Services;
using TiketManagementV2.ViewModel;
using static TiketManagementV2.View.BusCensorView;

namespace TiketManagementV2.View
{
    /// <summary>
    /// Interaction logic for BusCensorView.xaml
    /// </summary>
    public partial class BusCensorView : UserControl, INotifyPropertyChanged
    {
        private int _itemsToLoad = 2;
        private ObservableCollection<BusCensorViewModel.User> _filteredUsers;
        private bool _canLoadMore;
        private int current = 0;
        private string session_time;
        private ApiServices _service;
        private INotificationService _notificationService;
        private bool _hasItems;
        public bool HasItems
        {
            get => Buss?.Any() ?? false;
            private set
            {
                _hasItems = value;
                OnPropertyChanged(nameof(HasItems));
            }
        }
        public ObservableCollection<Bus> buss { get; set; }
        public ObservableCollection<Bus> Buss
        {
            get { return buss; }
            set { buss = value; OnPropertyChanged(nameof(Buss)); }
        }

        public bool CanLoadMore
        {
            get { return _canLoadMore; }
            set { _canLoadMore = value; OnPropertyChanged(nameof(CanLoadMore)); }
        }

        public class Bus : INotifyPropertyChanged
        {
            private string id { get; set; }
            private string name { get; set; }
            private string email { get; set; }
            private string phone { get; set; }

            public string Id
            {
                get => id;
                set
                {
                    id = value;
                    OnPropertyChanged(nameof(Id));
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
                    OnPropertyChanged(nameof(Email));
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

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ICommand AcceptCommand { get; }
        public ICommand RejectCommand { get; }
        private CircularLoadingControl _circularLoading;
        public BusCensorView(INotificationService notificationService, CircularLoadingControl loading)
        {
            InitializeComponent();
            DataContext = this;
            Buss = new ObservableCollection<Bus>();
            _service = new ApiServices();
            _notificationService = notificationService;
            _circularLoading = loading;

            Buss.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasItems));
            };

            session_time = DateTime.Now.ToString("o");

            AcceptCommand = new RelayCommandGeneric<Bus>(AcceptBus);
            RejectCommand = new RelayCommandGeneric<Bus>(RejectBus);
            LoadBusData();
        }

        private async Task<dynamic> GetBusData()
        {
            try
            {
                string access_token = Properties.Settings.Default.access_token;
                string refresh_token = Properties.Settings.Default.refresh_token;

                Dictionary<string, string> getVehicleDataHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {access_token}" }
                };
                var getVehicleDataBody = new
                {
                    refresh_token,
                    session_time,
                    current
                };

                dynamic data = await _service.PostWithHeaderAndBodyAsync("api/business-registration/get-business-registration", getVehicleDataHeader, getVehicleDataBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task LoadBusData()
        {
            try
            {
                _circularLoading.Visibility = Visibility.Visible;

                dynamic data = await GetBusData();

                if (data == null)
                {
                    _notificationService.ShowNotification(
                        "Lỗi",
                        data.message,
                        NotificationType.Error
                    );
                    return;
                }

                if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" || data.message == "Refresh token không hợp lệ")
                {
                    _notificationService.ShowNotification(
                        "Lỗi",
                        data.message,
                        NotificationType.Error
                    );
                    return;
                }

                if (data.message == "Bạn không có quyền thực hiện hành động này")
                {
                    _notificationService.ShowNotification(
                        "Lỗi",
                        data.message,
                        NotificationType.Error
                    );

                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    return;
                }

                if (data.message == "Lỗi dữ liệu đầu vào")
                {
                    foreach (dynamic item in data.errors)
                    {
                        _notificationService.ShowNotification(
                            "Lỗi dữ liệu đầu vào",
                            (string)item.Value.msg,
                            NotificationType.Warning
                        );
                    }

                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    return;
                }

                if (data.message == "Lấy thông tin đăng ký doanh nghiệp thất bại")
                {
                    _notificationService.ShowNotification(
                        "Lỗi",
                        (string)data.message,
                        NotificationType.Warning
                    );
                    return;
                }

                if (data.result.message == "Không tìm thấy kết quả phù hợp")
                {
                    txtkco.Visibility = Visibility.Visible;
                    return;
                }

                foreach (dynamic item in data.result.business_registration)
                {
                    Buss.Add(new Bus()
                    {
                        Id = item._id,
                        Name = item.name,
                        Email = item.email,
                        Phone = item.phone
                    });
                }

                current = data.result.current;
                CanLoadMore = data.result.continued;


                DgvBus.ItemsSource = Buss;
                //FilterVehicles();
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification(
                    "Lỗi",
                    ex.Message,
                    NotificationType.Error
                );
            }
            finally
            {
                _circularLoading.Visibility = Visibility.Collapsed;
            }
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LoadMore();
        }

        public async void LoadMore()
        {
            try
            {
                _circularLoading.Visibility = Visibility.Visible;

                await LoadBusData();
            }
            finally
            {
                _circularLoading.Visibility = Visibility.Collapsed;
            }
        }

        private async void AcceptBus(Bus bus)
        {
            try
            {
                _circularLoading.Visibility = Visibility.Visible;
                if (bus != null)
                {
                    dynamic data = await CensorBusRegistration(bus.Id, true);

                    if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" || data.message == "Refresh token không hợp lệ")
                    {
                        _notificationService.ShowNotification(
                            "Lỗi",
                            data.message,
                            NotificationType.Error
                        );
                        return;
                    }

                    if (data.message == "Bạn không có quyền thực hiện hành động này")
                    {
                        _notificationService.ShowNotification(
                            "Lỗi",
                            data.message,
                            NotificationType.Error
                        );
                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();
                        return;
                    }

                    if (data.message == "Lỗi dữ liệu đầu vào")
                    {
                        foreach (dynamic items in data.errors)
                        {
                            _notificationService.ShowNotification(
                                "Lỗi",
                                (string)items.Value.msg,
                                 NotificationType.Warning
                            );
                        }

                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();
                        return;
                    }

                    if (data.message == "Kiếm duyệt đăng ký doanh nghiệp thành công!")
                    {
                        _notificationService.ShowNotification(
                            "Thành công",
                            (string)data.message,
                            NotificationType.Success
                        );

                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();

                        Buss.Remove(bus);

                        if (Buss.Count < 1)
                        {
                            txtkco.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        _notificationService.ShowNotification(
                            "Lỗi",
                            (string)data.message,
                            NotificationType.Warning
                        );

                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();
                    }
                }
            }
            finally
            {
                _circularLoading.Visibility = Visibility.Collapsed;
            }
        }

        private async void RejectBus(Bus bus)
        {
            try
            {
                _circularLoading.Visibility = Visibility.Visible;
                if (bus != null)
                {
                    dynamic data = await CensorBusRegistration(bus.Id, false);

                    if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" || data.message == "Refresh token không hợp lệ")
                    {
                        _notificationService.ShowNotification(
                            "Lỗi",
                            data.message,
                            NotificationType.Error
                        );
                        return;
                    }

                    if (data.message == "Bạn không có quyền thực hiện hành động này")
                    {
                        _notificationService.ShowNotification(
                            "Lỗi",
                            data.message,
                            NotificationType.Error
                        );
                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();
                        return;
                    }

                    if (data.message == "Lỗi dữ liệu đầu vào")
                    {
                        foreach (dynamic items in data.errors)
                        {
                            _notificationService.ShowNotification(
                                "Lỗi",
                                (string)items.Value.msg,
                                 NotificationType.Warning
                            );
                        }

                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();
                        return;
                    }

                    if (data.message == "Kiếm duyệt đăng ký doanh nghiệp thành công!")
                    {
                        _notificationService.ShowNotification(
                            "Thành công",
                            (string)data.message,
                            NotificationType.Success
                        );

                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();

                        Buss.Remove(bus);

                        if (Buss.Count < 1)
                        {
                            txtkco.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        _notificationService.ShowNotification(
                            "Lỗi",
                            (string)data.message,
                            NotificationType.Warning
                        );

                        Properties.Settings.Default.access_token = data.authenticate.access_token;
                        Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                        Properties.Settings.Default.Save();
                    }
                }
            }
            finally
            {
                _circularLoading.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<dynamic> CensorBusRegistration(string id, bool decision)
        {
            try
            {
                Dictionary<string, string> businessRegistrationHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {Properties.Settings.Default.access_token}" }
                };
                var businessRegistrationBody = new
                {
                    refresh_token = Properties.Settings.Default.refresh_token,
                    business_registration_id = id,
                    decision = decision
                };

                dynamic data = await _service.PutWithHeaderAndBodyAsync("api/business-registration/censor", businessRegistrationHeader, businessRegistrationBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void btnReload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _circularLoading.Visibility = Visibility.Visible;

                current = 0;
                session_time = DateTime.Now.ToString("o");

                await LoadBusData();
            }
            finally
            {
                _circularLoading.Visibility = Visibility.Collapsed;
            }
        }
    }
}
