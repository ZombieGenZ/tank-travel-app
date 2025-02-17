using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TiketManagementV2.Commands;
using TiketManagementV2.Model;
using TiketManagementV2.Services;
using TiketManagementV2.View;

namespace TiketManagementV2.ViewModel
{

    public class HomeViewModel : ViewModelBase
    {
        //private object _currentView;
        //public object CurrentView
        //{
        //    get => _currentView;
        //    set => SetProperty(ref _currentView, value);
        //}
        private string session_time;
        private dynamic _user;
        private ApiServices _service;
        private string _revenue;
        private ObservableCollection<BusinessItem> _businessItems;
        public ObservableCollection<BusinessItem> BusinessItems
        {
            get => _businessItems;
            set => SetProperty(ref _businessItems, value);
        }
        //public bool HasItems => BusinessItems?.Any() ?? false;
        private ObservableCollection<VehicleItem> _vehicleItems;
        public ObservableCollection<VehicleItem> VehicleItems
        {
            get => _vehicleItems;
            set => SetProperty(ref _vehicleItems, value);
        }
        private bool _hasItems;
        public bool HasItems
        {
            get => BusinessItems?.Any() ?? false;
            private set
            {
                _hasItems = value;
                OnPropertyChanged(nameof(HasItems));
            }
        }

        private bool _hasVehicleItems;
        public bool HasVehicleItems
        {
            get => VehicleItems?.Any() ?? false;
            private set
            {
                _hasVehicleItems = value;
                OnPropertyChanged(nameof(HasVehicleItems));
            }
        }
        public class BusinessItem : INotifyPropertyChanged
        {
            private string _id;
            private string _name;
            private string _email;
            private string _phoneNumber;
            private string _status = "Pending";

            public string Id
            {
                get => _id;
                set
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }

            public string Name
            {
                get => _name;
                set
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }

            public string Email
            {
                get => _email;
                set
                {
                    _email = value;
                    OnPropertyChanged(nameof(Email));
                }
            }

            public string PhoneNumber
            {
                get => _phoneNumber;
                set
                {
                    _phoneNumber = value;
                    OnPropertyChanged(nameof(PhoneNumber));
                }
            }

            public string Status
            {
                get => _status;
                set
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(StatusText));
                    OnPropertyChanged(nameof(IsActionRequired));
                }
            }

            public string StatusText
            {
                get
                {
                    if (Status == "Pending") return "Pending";
                    if (Status == "Approved") return "Approved";
                    if (Status == "Rejected") return "Rejected";
                    return Status;
                }
            }

            public bool IsActionRequired => Status == "Pending";


            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class VehicleItem : INotifyPropertyChanged
        {
            private string _id;
            private string _licensePlate;
            private string _vehicleType;
            private string _seatType;
            private int _seats;
            private string _rules;
            private string _amenities;
            private string _vehicleImage;
            private string _status = "Pending";

            public string Id
            {
                get => _id;
                set
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }

            public string LicensePlate
            {
                get => _licensePlate;
                set
                {
                    _licensePlate = value;
                    OnPropertyChanged(nameof(LicensePlate));
                }
            }

            public string VehicleType
            {
                get => _vehicleType;
                set
                {
                    _vehicleType = value;
                    OnPropertyChanged(nameof(VehicleType));
                }
            }

            public string SeatType
            {
                get => _seatType;
                set
                {
                    _seatType = value;
                    OnPropertyChanged(nameof(SeatType));
                }
            }

            public int Seats
            {
                get => _seats;
                set
                {
                    _seats = value;
                    OnPropertyChanged(nameof(Seats));
                }
            }

            public string Rules
            {
                get => _rules;
                set
                {
                    _rules = value;
                    OnPropertyChanged(nameof(Rules));
                }
            }

            public string Amenities
            {
                get => _amenities;
                set
                {
                    _amenities = value;
                    OnPropertyChanged(nameof(Amenities));
                }
            }

            public string VehicleImage
            {
                get => _vehicleImage;
                set
                {
                    _vehicleImage = value;
                    OnPropertyChanged(nameof(VehicleImage));
                }
            }

            public string Status
            {
                get => _status;
                set
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(StatusText));
                    OnPropertyChanged(nameof(IsActionRequired));
                }
            }

            public string StatusText
            {
                get
                {
                    if (Status == "Pending") return "Pending";
                    if (Status == "Approved") return "Approved";
                    if (Status == "Rejected") return "Rejected";
                    return Status;
                }
            }

            public bool IsActionRequired => Status == "Pending";

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public string Revenue
        {
            get => _revenue;
            set => SetProperty(ref _revenue, value);
        }
        public string _deals;
        public string Deals
        {
            get => _deals;
            set => SetProperty(ref _deals, value);
        }
        public string _ticket;
        public string Ticket
        {
            get => _ticket;
            set => SetProperty(ref _ticket, value);
        }
        //public ICommand ShowCensorViewCommand { get; }
        public ICommand AcceptCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand AcceptVehicleCommand { get; }
        public ICommand RejectVehicleCommand { get; }

        private MainViewModel _mainViewModel;
        public HomeViewModel(dynamic user, MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _service = new ApiServices();
            session_time = DateTime.Now.ToString("o");
            _businessItems = new ObservableCollection<BusinessItem>();
            _businessItems.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasItems));
            };

            LoadRevenue();
            LoadDeals();
            LoadTicket();
            LoadBusinessRegistration();
            //ShowCensorViewCommand = new RelayCommand(ExecuteShowCensorView);

            AcceptCommand = new RelayCommandGeneric<BusinessItem>(OnAccept);
            RejectCommand = new RelayCommandGeneric<BusinessItem>(OnReject);
        }

        private async void OnAccept(BusinessItem item)
        {
            if (item != null)
            {
                dynamic data = await CensorBusinessRegistration(item.Id, true);

                if (data.message == "You must log in to use this function" || data.message == "Invalid refresh token")
                {
                    _mainViewModel._notificationService.ShowNotification(
                        "Error",
                        data.message,
                        NotificationType.Error
                    );
                    return;
                }

                if (data.message == "You do not have permission to perform this action")
                {
                    _mainViewModel._notificationService.ShowNotification(
                        "Error",
                        data.message,
                        NotificationType.Error
                    );
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();
                    return;
                }

                if (data.message == "Input data error")
                {
                    foreach (dynamic items in data.errors)
                    {
                        _mainViewModel._notificationService.ShowNotification(
                            "Error",
                            (string)items.Value.msg,
                             NotificationType.Warning
                        );
                    }

                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();
                    return;
                }

                if (data.message == "Failed to approve business registration")
                {
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    _mainViewModel._notificationService.ShowNotification(
                        "Error",
                        (string)data.messsage,
                        NotificationType.Warning
                    );

                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();
                    return;
                }

                if (data.message == "Business registration approved successfully!")
                {
                    _mainViewModel._notificationService.ShowNotification(
                        "Successfully",
                        (string)data.messsage,
                        NotificationType.Success
                    );

                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    item.Status = "Approved";
                    return;
                }
            }
        }

        private async void OnReject(BusinessItem item)
        {
            if (item != null)
            {
                dynamic data = await CensorBusinessRegistration(item.Id, false);

                if (data.message == "You must log in to use this function" || data.message == "Invalid refresh token")
                {
                    _mainViewModel._notificationService.ShowNotification(
                        "Error",
                        data.message,
                        NotificationType.Error
                    );
                    return;
                }

                if (data.message == "You do not have permission to perform this action")
                {
                    _mainViewModel._notificationService.ShowNotification(
                        "Error",
                        data.message,
                        NotificationType.Error
                    );
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();
                    return;
                }

                if (data.message == "Input data error")
                {
                    foreach (dynamic items in data.errors)
                    {
                        _mainViewModel._notificationService.ShowNotification(
                            "Error",
                            (string)items.Value.msg,
                             NotificationType.Warning
                        );
                    }

                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();
                    return;
                }

                if (data.message == "Failed to approve business registration")
                {
                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    _mainViewModel._notificationService.ShowNotification(
                        "Error",
                        (string)data.messsage,
                        NotificationType.Warning
                    );

                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();
                    return;
                }

                if (data.message == "Business registration approved successfully!")
                {
                    _mainViewModel._notificationService.ShowNotification(
                        "Successfully",
                        (string)data.messsage,
                        NotificationType.Success
                    );

                    Properties.Settings.Default.access_token = data.authenticate.access_token;
                    Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                    Properties.Settings.Default.Save();

                    item.Status = "Rejected";
                    return;
                }
            }
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

        private async void LoadRevenue()
        {
            dynamic data = await GetRevenue();

            if (data == null)
            {
                //_notificationService.ShowNotification(
                //    "Error",
                //    "Error connecting to server!",
                //    NotificationType.Error
                //);
                return;
            }

            Properties.Settings.Default.access_token = data.authenticate.access_token;
            Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
            Properties.Settings.Default.Save();

            Revenue = data.result;
        }

        private async Task<dynamic> GetDeals()
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

                dynamic data = await _service.PostWithHeaderAndBodyAsync("api/statistical/get-deal-statistics", statisticsHeader, statisticsBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async void LoadDeals()
        {
            dynamic data = await GetDeals();

            if (data == null)
            {
                //_notificationService.ShowNotification(
                //    "Error",
                //    "Error connecting to server!",
                //    NotificationType.Error
                //);
                return;
            }

            Properties.Settings.Default.access_token = data.authenticate.access_token;
            Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
            Properties.Settings.Default.Save();

            Deals = data.result;
        }

        private async Task<dynamic> GetTicket()
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

                dynamic data = await _service.PostWithHeaderAndBodyAsync("api/statistical/get-order-statistics", statisticsHeader, statisticsBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async void LoadTicket()
        {
            dynamic data = await GetTicket();

            if (data == null)
            {
                //_notificationService.ShowNotification(
                //    "Error",
                //    "Error connecting to server!",
                //    NotificationType.Error
                //);
                return;
            }

            Properties.Settings.Default.access_token = data.authenticate.access_token;
            Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
            Properties.Settings.Default.Save();

            Ticket = data.result;
        }

        private async Task<dynamic> GetBusinessRegistration()
        {
            try
            {
                Dictionary<string, string> businessRegistrationHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {Properties.Settings.Default.access_token}" }
                };
                var businessRegistrationBody = new
                {
                    session_time = session_time,
                    refresh_token = Properties.Settings.Default.refresh_token,
                    current = 0
                };

                dynamic data = await _service.PostWithHeaderAndBodyAsync("api/business-registration/get-business-registration", businessRegistrationHeader, businessRegistrationBody);

                Properties.Settings.Default.access_token = data.authenticate.access_token;
                Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                Properties.Settings.Default.Save();

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async void LoadBusinessRegistration()
        {
            dynamic data = await GetBusinessRegistration();

            if (data == null)
            {
                //_notificationService.ShowNotification(
                //    "Error",
                //    "Error connecting to server!",
                //    NotificationType.Error
                //);
                return;
            }
            int count = 0;

            foreach (dynamic item in data.result.business_registration)
            {
                if (count > 2)
                {
                    return;
                }

                BusinessItems.Add(new BusinessItem
                {
                    Id = item._id,
                    Email = item.email,
                    Name = item.name,
                    PhoneNumber = item.phone,
                });

                count++;
            }

            //BusinessItems = new ObservableCollection<BusinessItem>
            //{
            //    new BusinessItem { Name = "nguyễn văn a", Email = "abc@gmail.com", PhoneNumber = "0123456789"},
            //    new BusinessItem { Name = "nguyễn văn a", Email = "abc@gmail.com", PhoneNumber = "0123456789"},
            //    new BusinessItem { Name = "nguyễn văn a", Email = "abc@gmail.com", PhoneNumber = "0123456789"},

            //};

            //Ticket = data.result;
        }

        private async Task<dynamic> CensorBusinessRegistration(string id, bool decision)
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
    }
}
