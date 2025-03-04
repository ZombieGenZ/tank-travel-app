using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using TiketManagementV2.Commands;
using TiketManagementV2.Controls;
using TiketManagementV2.Helpers;
using TiketManagementV2.Model;
using TiketManagementV2.Services;
using TiketManagementV2.View;

namespace TiketManagementV2.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public Frame MainFrame { get; }
        private UserAccount _currentUserAccount;
        private string _pathUser;
        private string _originalDisplayName;
        private string _originalEmail;
        private string _originalPhone;
        private string _originalPathUser;
        private dynamic _user;
        private CircularLoadingControl _circularLoadingControl;

        private string _revenue;
        public string Revenue
        {
            get => _revenue;
            set => SetProperty(ref _revenue, value);
        }

        private ApiServices _service;

        public UserAccount CurrentUserAccount
        {
            get => _currentUserAccount;
            set => SetProperty(ref _currentUserAccount, value);
        }

        public string PathUser
        {
            get => _pathUser;
            set => SetProperty(ref _pathUser, value);
        }

        public class UserAccount
        {
            public string DisplayName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
        }

        public object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        private bool _hasNewNotifications;
        public bool HasNewNotifications
        {
            get => _hasNewNotifications;
            set => SetProperty(ref _hasNewNotifications, value);
        }
        //ProfileView
        public ICommand SaveProfileCommand { get; }
        public ICommand CancelProfileCommand { get; }
        public ICommand ChangeProfileImageCommand { get; }

        //giao diện
        public ICommand ShowHomeViewCommand { get; }
        public ICommand ShowBankViewCommand { get; }
        public ICommand ShowProfileViewCommand { get; }
        public ICommand ShowAccountViewCommand { get; }
        public ICommand ShowCensorViewCommand { get; } 
        public ICommand ShowTicketViewCommand { get; }
        public ICommand ShowChartViewCommand { get; }
        public ICommand ShowLogViewCommand { get; }
        public ICommand ShowNotificationViewCommand { get; }
        public ICommand ShowBusCensorViewCommand { get; }
        public ICommand ShowVehicleCensorViewCommand { get; }
        public ICommand ShowVehicleManagementViewCommand { get; }
        public ICommand ShowBusRouteViewCommand {  get; }
        public ICommand ShowMailViewCommand { get; }
        public ICommand ShowResetPasswordViewCommand { get; }
        public ICommand ShowGlobalViewCommand { get; }

        //command bus
        public ICommand ShowHomeBusViewCommand { get; }
        // Thêm các command khác

        private void ExecuteShowHomeView(object obj)
        {
            if (CurrentView is HomeView)
            {
                return;
            }
            CurrentView = new HomeView(_user, this, _circularLoadingControl);
        }
        private void ExecuteShowBankView (object obj)
        {
            if (CurrentView is BankView)
            {
                return;
            }
            CurrentView = new BankView();
        }
        private void ExecuteShowProfileView(object obj)
        {
            if (CurrentView is ProfileView)
            {
                return;
            }
            CurrentView = new ProfileView(_user, _circularLoadingControl, this, _display_name, _adminView);
        }

        private void ExecuteShowAccountView(object obj)
        {
            if (CurrentView is AccountView)
            {
                return;
            }
            CurrentView = new AccountView(_circularLoadingControl);
        }
        private void ExecuteShowTicketView(object obj)
        {

            CurrentView = new TicketView();
        }
        private void ExecuteShowChartView(object obj)
        {
            if (CurrentView is ChartView)
            {
                return;
            }
            CurrentView = new ChartView(_circularLoadingControl);
        }
        private void ExecuteShowLogView(object obj)
        {
            if (CurrentView is LogView)
            {
                return;
            }
            CurrentView = new LogView();
        }
        private void ExecuteShowNotificationView(object obj)
        {
            CurrentView = new NotificationView();
        }
        private void ExecuteShowBusCensorView(object obj)
        {
            if (CurrentView is BusCensorView)
            {
                return;
            }
            CurrentView = new BusCensorView(_notificationService, _circularLoadingControl);
        }
        private void ExecuteShowVehicleCensorView(object obj)
        {
            if (CurrentView is VehicleCensorView)
            {
                return;
            }
            CurrentView = new VehicleCensorView(_notificationService, _circularLoadingControl);
        }
        private void ExecuteShowVehicleManagementView(object obj)
        {
            if (CurrentView is VehicleManagementView)
            {
                return;
            }
            CurrentView = new VehicleManagementView(_notificationService, _circularLoadingControl);
        }
        private void ExecuteShowBusRouteView(object obj)
        {
            if (CurrentView is BusRouteView)
            {
                return;
            }
            CurrentView = new BusRouteView(_circularLoadingControl);
        }
        private void ExecuteShowMailView(object obj)
        {

            var mailView = new MailView();
            HasNewNotifications = false;
            mailView.ShowDialog();
        }
        private void ExecuteShowResetPasswordView(object obj)
        {
            var resetPasswordView = new ResetPasswordView();
            resetPasswordView.ShowDialog();
        }
        private void ExecuteShowGlobalNotificationView(object obj)
        {
            var global = new GlobalNotificationView();
            global.ShowDialog();
        }
        //BUS VIEW
        private void ExecuteShowHomeBusView(object obj)
        {
            if (CurrentView is HomeBusView)
            {
                return;
            }
            CurrentView = new HomeBusView();
        }
        public readonly INotificationService _notificationService;

        public MainViewModel(INotificationService notificationService)
        {
            //_service = new ApiServices();

            //_notificationService = notificationService;

            //CurrentUserAccount = new UserAccount
            //{
            //    DisplayName = "Người dùng"
            //};
            //PathUser = "/Images/ducanh.jpg";

            ////Khởi tạo commands cho profile
            //SaveProfileCommand = new RelayCommand(ExecuteSaveProfile);
            //CancelProfileCommand = new RelayCommand(ExecuteCancelProfile);
            //ChangeProfileImageCommand = new RelayCommand(ExecuteChangeProfileImage);


            ////Khởi tạo commands
            //ShowHomeViewCommand = new RelayCommand(ExecuteShowHomeView);
            //ShowBankViewCommand = new RelayCommand(ExecuteShowBankView);
            //ShowProfileViewCommand = new RelayCommand(ExecuteShowProfileView);
            //ShowAccountViewCommand = new RelayCommand(ExecuteShowAccountView);
            //ShowCensorViewCommand = new RelayCommand(ExecuteShowCensorView);
            //ShowTicketViewCommand = new RelayCommand(ExecuteShowTicketView);
            //ShowChartViewCommand = new RelayCommand(ExecuteShowChartView);
            //ShowLogViewCommand = new RelayCommand(ExecuteShowLogView);
            //ShowNotificationViewCommand = new RelayCommand(ExecuteShowNotificationView);

            ////Set view mặc định
            //ExecuteShowHomeView(null);
        }

        private async Task<dynamic> GetRevenue()
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

                dynamic data = await _service.PostWithHeaderAndBodyAsync("api/statistical/get-revenue-statistics", statisticsHeader, statisticsBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public MainViewModel(INotificationService notificationService, dynamic user)
        {
            _service = new ApiServices();

            _user = user;
            _notificationService = notificationService;



            CurrentUserAccount = new UserAccount
            {
                DisplayName = user.display_name,
                Email = user.email,
                Phone = user.phone
            };
            PathUser = user.avatar.url;

            //Revenue = "0";
            //_Deals = "0";

            //LoadRevenue();

            // Khởi tạo commands cho profile
            SaveProfileCommand = new RelayCommand(ExecuteSaveProfile);
            CancelProfileCommand = new RelayCommand(ExecuteCancelProfile);
            ChangeProfileImageCommand = new RelayCommand(ExecuteChangeProfileImage);


            // Khởi tạo commands
            ShowHomeViewCommand = new RelayCommand(ExecuteShowHomeView);
            ShowBankViewCommand = new RelayCommand(ExecuteShowBankView);
            ShowProfileViewCommand = new RelayCommand(ExecuteShowProfileView);
            ShowAccountViewCommand = new RelayCommand(ExecuteShowAccountView);
            ShowTicketViewCommand = new RelayCommand(ExecuteShowTicketView);
            ShowChartViewCommand = new RelayCommand(ExecuteShowChartView);
            ShowLogViewCommand = new RelayCommand(ExecuteShowLogView);
            ShowNotificationViewCommand = new RelayCommand(ExecuteShowNotificationView);
            ShowBusCensorViewCommand = new RelayCommand(ExecuteShowBusCensorView);
            ShowVehicleCensorViewCommand = new RelayCommand(ExecuteShowVehicleCensorView);
            ShowVehicleManagementViewCommand = new RelayCommand(ExecuteShowVehicleManagementView);
            ShowBusRouteViewCommand = new RelayCommand(ExecuteShowBusRouteView);
            ShowMailViewCommand = new RelayCommand(ExecuteShowMailView);
            ShowResetPasswordViewCommand = new RelayCommand(ExecuteShowResetPasswordView);
            ShowGlobalViewCommand = new RelayCommand(ExecuteShowGlobalNotificationView);
            //command bus
            ShowHomeBusViewCommand = new RelayCommand(ExecuteShowHomeBusView);

            // Set view mặc định
            if (user.permission == 2)
            {
                ExecuteShowHomeView(null);
            }
            else if(user.permission == 1)
            {
                ExecuteShowChartView(null);
            }
        }

        public MainViewModel(INotificationService notificationService, dynamic user, CircularLoadingControl loading)
        {
            _service = new ApiServices();

            _user = user;
            _notificationService = notificationService;

            _circularLoadingControl = loading;

            CurrentUserAccount = new UserAccount
            {
                DisplayName = user.display_name,
                Email = user.email,
                Phone = user.phone
            };
            PathUser = user.avatar.url;

            //Revenue = "0";
            //_Deals = "0";

            //LoadRevenue();

            // Khởi tạo commands cho profile
            SaveProfileCommand = new RelayCommand(ExecuteSaveProfile);
            CancelProfileCommand = new RelayCommand(ExecuteCancelProfile);
            ChangeProfileImageCommand = new RelayCommand(ExecuteChangeProfileImage);


            // Khởi tạo commands
            ShowHomeViewCommand = new RelayCommand(ExecuteShowHomeView);
            ShowBankViewCommand = new RelayCommand(ExecuteShowBankView);
            ShowProfileViewCommand = new RelayCommand(ExecuteShowProfileView);
            ShowAccountViewCommand = new RelayCommand(ExecuteShowAccountView);
            ShowTicketViewCommand = new RelayCommand(ExecuteShowTicketView);
            ShowChartViewCommand = new RelayCommand(ExecuteShowChartView);
            ShowLogViewCommand = new RelayCommand(ExecuteShowLogView);
            ShowNotificationViewCommand = new RelayCommand(ExecuteShowNotificationView);
            ShowBusCensorViewCommand = new RelayCommand(ExecuteShowBusCensorView);
            ShowVehicleCensorViewCommand = new RelayCommand(ExecuteShowVehicleCensorView);
            ShowVehicleManagementViewCommand = new RelayCommand(ExecuteShowVehicleManagementView);
            ShowBusRouteViewCommand = new RelayCommand(ExecuteShowBusRouteView);
            ShowMailViewCommand = new RelayCommand(ExecuteShowMailView);
            ShowResetPasswordViewCommand = new RelayCommand(ExecuteShowResetPasswordView);
            ShowGlobalViewCommand = new RelayCommand(ExecuteShowGlobalNotificationView);
            //command bus
            ShowHomeBusViewCommand = new RelayCommand(ExecuteShowHomeBusView);

            // Set view mặc định
            if (user.permission == 2)
            {
                ExecuteShowHomeView(null);
            }
            else if (user.permission == 1)
            {
                ExecuteShowChartView(null);
            }
        }

        private AdminView _adminView;
        private TextBlock _display_name;
        public MainViewModel(INotificationService notificationService, dynamic user, CircularLoadingControl loading, TextBlock display_name, AdminView adminView)
        {
            _service = new ApiServices();
            _display_name = display_name;
            _adminView = adminView;
            _user = user;
            _notificationService = notificationService;

            _circularLoadingControl = loading;

            CurrentUserAccount = new UserAccount
            {
                DisplayName = user.display_name,
                Email = user.email,
                Phone = user.phone
            };
            PathUser = user.avatar.url;

            //Revenue = "0";
            //_Deals = "0";

            //LoadRevenue();

            // Khởi tạo commands cho profile
            SaveProfileCommand = new RelayCommand(ExecuteSaveProfile);
            CancelProfileCommand = new RelayCommand(ExecuteCancelProfile);
            ChangeProfileImageCommand = new RelayCommand(ExecuteChangeProfileImage);


            // Khởi tạo commands
            ShowHomeViewCommand = new RelayCommand(ExecuteShowHomeView);
            ShowBankViewCommand = new RelayCommand(ExecuteShowBankView);
            ShowProfileViewCommand = new RelayCommand(ExecuteShowProfileView);
            ShowAccountViewCommand = new RelayCommand(ExecuteShowAccountView);
            ShowTicketViewCommand = new RelayCommand(ExecuteShowTicketView);
            ShowChartViewCommand = new RelayCommand(ExecuteShowChartView);
            ShowLogViewCommand = new RelayCommand(ExecuteShowLogView);
            ShowNotificationViewCommand = new RelayCommand(ExecuteShowNotificationView);
            ShowBusCensorViewCommand = new RelayCommand(ExecuteShowBusCensorView);
            ShowVehicleCensorViewCommand = new RelayCommand(ExecuteShowVehicleCensorView);
            ShowVehicleManagementViewCommand = new RelayCommand(ExecuteShowVehicleManagementView);
            ShowBusRouteViewCommand = new RelayCommand(ExecuteShowBusRouteView);
            ShowMailViewCommand = new RelayCommand(ExecuteShowMailView);
            ShowResetPasswordViewCommand = new RelayCommand(ExecuteShowResetPasswordView);
            ShowGlobalViewCommand = new RelayCommand(ExecuteShowGlobalNotificationView);
            //command bus
            ShowHomeBusViewCommand = new RelayCommand(ExecuteShowHomeBusView);

            // Set view mặc định
            if (user.permission == 2)
            {
                ExecuteShowHomeView(null);
            }
            else if (user.permission == 1)
            {
                ExecuteShowHomeBusView(null);
            }
        }

        //private async void LoadRevenue()
        //{
        //    dynamic data = await GetRevenue();

        //    if (data == null)
        //    {
        //        _notificationService.ShowNotification(
        //            "Error",
        //            "Error connecting to server!",
        //            NotificationType.Error
        //        );
        //        return;
        //    }

        //    Properties.Settings.Default.access_token = data.authenticate.access_token;
        //    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
        //    Properties.Settings.Default.Save();

        //    Revenue = data.result;
        //}

        // Ví dụ sử dụng trong command hoặc method
        private ICommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new RelayCommand(param =>
                {
                    try
                    {
                        // Logic để save
                        _notificationService.ShowNotification(
                            "success",
                            "Data saved successfully!",
                            NotificationType.Success
                        );
                    }
                    catch (Exception ex)
                    {
                        _notificationService.ShowNotification(
                            ex.Message,
                            "error",
                            NotificationType.Error
                        );
                    }
                }));
            }
        }

        // Hoặc có thể gọi trực tiếp trong method
        private void HandleSomeAction()
        {
            try
            {
                // Xử lý logic
                _notificationService.ShowNotification(
                    "Thông tin",
                    "Đã xử lý thành công!",
                    NotificationType.Success
                );
            }
            catch
            {
                _notificationService.ShowNotification(
                    "Cảnh báo",
                    "Có lỗi xảy ra trong quá trình xử lý",
                    NotificationType.Warning
                );
            }
        }

        private void ExecuteSaveProfile(object obj)
        {
            // Lưu các giá trị hiện tại làm giá trị gốc
            _originalDisplayName = CurrentUserAccount.DisplayName;
            //_originalEmail = CurrentUserAccount.Email;
            //_originalPhone = CurrentUserAccount.Phone;
            _originalPathUser = PathUser;

            _notificationService.ShowNotification(
                "Thành công",
                "Đã lưu thông tin profile!",
                NotificationType.Success
            );
        }

        private void ExecuteCancelProfile(object obj)
        {
            // Khôi phục về giá trị gốc
            CurrentUserAccount.DisplayName = _originalDisplayName;
            //CurrentUserAccount.Email = _originalEmail;
            //CurrentUserAccount.Phone = _originalPhone;
            PathUser = _originalPathUser;
        }

        private void ExecuteChangeProfileImage(object obj)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                PathUser = openFileDialog.FileName;
            }
        }
    }
}
