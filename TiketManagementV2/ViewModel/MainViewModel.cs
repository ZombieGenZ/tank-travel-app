using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TiketManagementV2.Commands;
using TiketManagementV2.Services;
using TiketManagementV2.View;

namespace TiketManagementV2.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public ICommand ShowHomeViewCommand { get; }
        public ICommand ShowBankViewCommand { get; }
        public ICommand ShowProfileViewCommand { get; }
        public ICommand ShowAccountViewCommand { get; }
        public ICommand ShowCensorViewCommand { get; }
        public ICommand ShowTicketViewCommand { get; }
        public ICommand ShowChartViewCommand { get; }
        public ICommand ShowLogViewCommand { get; }
        public ICommand ShowNotificationViewCommand { get; }
        // Thêm các command khác

        public MainViewModel()
        {
            // Khởi tạo commands
            ShowHomeViewCommand = new RelayCommand(ExecuteShowHomeView);
            ShowBankViewCommand = new RelayCommand(ExecuteShowBankView);
            ShowProfileViewCommand = new RelayCommand(ExecuteShowProfileView);
            ShowAccountViewCommand = new RelayCommand(ExecuteShowAccountView);
            ShowCensorViewCommand = new RelayCommand(ExecuteShowCensorView);
            ShowTicketViewCommand = new RelayCommand(ExecuteShowTicketView);
            ShowChartViewCommand = new RelayCommand(ExecuteShowChartView);
            ShowLogViewCommand = new RelayCommand(ExecuteShowLogView);
            ShowNotificationViewCommand = new RelayCommand(ExecuteShowNotificationView);
            // Set view mặc định
            ExecuteShowHomeView(null);
        }

        private void ExecuteShowHomeView(object obj)
        {
            CurrentView = new HomeView();
        }
        private void ExecuteShowBankView (object obj)
        {
            CurrentView = new BankView();
        }
        private void ExecuteShowProfileView(object obj)
        {
            CurrentView = new ProfileView();
        }

        private void ExecuteShowAccountView(object obj)
        {
            CurrentView = new AccountView();
        }
        private void ExecuteShowCensorView(object obj)
        {
            CurrentView = new CensorView();
        }
        private void ExecuteShowTicketView(object obj)
        {
            CurrentView = new TicketView();
        }
        private void ExecuteShowChartView(object obj)
        {
            CurrentView = new ChartView();
        }
        private void ExecuteShowLogView(object obj)
        {
            CurrentView = new LogView();
        }
        private void ExecuteShowNotificationView(object obj)
        {
            CurrentView = new NotificationView();
        }
        private readonly INotificationService _notificationService;

        // Constructor với DI
        public MainViewModel(INotificationService notificationService)
        {
            _notificationService = notificationService;

            // Khởi tạo commands
            ShowHomeViewCommand = new RelayCommand(ExecuteShowHomeView);
            ShowBankViewCommand = new RelayCommand(ExecuteShowBankView);
            ShowProfileViewCommand = new RelayCommand(ExecuteShowProfileView);
            ShowAccountViewCommand = new RelayCommand(ExecuteShowAccountView);
            ShowCensorViewCommand = new RelayCommand(ExecuteShowCensorView);
            ShowTicketViewCommand = new RelayCommand(ExecuteShowTicketView);
            ShowChartViewCommand = new RelayCommand(ExecuteShowChartView);
            ShowLogViewCommand = new RelayCommand(ExecuteShowLogView);
            ShowNotificationViewCommand = new RelayCommand(ExecuteShowNotificationView);
            // Set view mặc định
            ExecuteShowHomeView(null);
        }

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
                            "Thành công",
                            "Đã lưu dữ liệu thành công!",
                            NotificationType.Success
                        );
                    }
                    catch (Exception ex)
                    {
                        _notificationService.ShowNotification(
                            ex.Message,
                            "Lỗi",
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
                    NotificationType.Info
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


    }
}
