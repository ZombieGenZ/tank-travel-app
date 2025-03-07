﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TiketManagementV2.Commands;
using TiketManagementV2.Controls;
using TiketManagementV2.Model;
using TiketManagementV2.Services;
using TiketManagementV2.View;
using static TiketManagementV2.ViewModel.HomeViewModel;

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
                get
                {
                    if (_vehicleType == "0")
                    {
                        return "Bus";
                    }
                    if (_vehicleType == "1")
                    {
                        return "Train";
                    }
                    if (_vehicleType == "2")
                    {
                        return "Plane";
                    }

                    return "Unknown";
                }
                set
                {
                    _vehicleType = value;
                    OnPropertyChanged(nameof(VehicleType));
                }
            }

            public string SeatType
            {
                get
                {
                    if (_seatType == "0")
                    {
                        return "Seating seat";
                    }
                    if (_seatType == "1")
                    {
                        return "Sleeper seat";
                    }
                    if (_seatType == "2")
                    {
                        return "Hybrid seat";
                    }
                    return "Unknown";
                }
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
        private CircularLoadingControl _circularLoadingControl;
        public HomeViewModel(dynamic user, MainViewModel mainViewModel, CircularLoadingControl loading)
        {
            _mainViewModel = mainViewModel;
            _circularLoadingControl = loading;
            _service = new ApiServices();
            session_time = DateTime.Now.ToString("o");
            _businessItems = new ObservableCollection<BusinessItem>();
            VehicleItems = new ObservableCollection<VehicleItem>();
            _businessItems.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasItems));
            };
            VehicleItems.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasVehicleItems));
            };

            //_circularLoadingControl.Visibility = Visibility.Visible;

            //LoadRevenue();
            //LoadDeals();
            //LoadTicket();
            //LoadBusinessRegistration();
            //LoadVehicleRegistration();

            //_circularLoadingControl.Visibility = Visibility.Collapsed;

            //ShowCensorViewCommand = new RelayCommand(ExecuteShowCensorView);

            AcceptCommand = new RelayCommandGeneric<BusinessItem>(OnAccept);
            RejectCommand = new RelayCommandGeneric<BusinessItem>(OnReject);
            AcceptVehicleCommand = new RelayCommandGeneric<VehicleItem>(OnAcceptVehicle);
            RejectVehicleCommand = new RelayCommandGeneric<VehicleItem>(OnRejectVehicle);

            LoadDataWithProgress();
        }

        #region Load data

        private async Task LoadAllDataAsync()
        {
            try
            {
                var tasks = new[]
                {
                    LoadRevenueAsync(),
                    LoadDealsAsync(),
                    LoadTicketAsync(),
                    LoadBusinessRegistrationAsync(),
                    LoadVehicleRegistrationAsync()
                };

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _mainViewModel._notificationService.ShowNotification(
                    "Error",
                    "Lỗi khi tải dử liệu: " + ex.Message,
                    NotificationType.Error
                );
            }
        }

        private async Task ShowLoadingWhileExecutingAsync(Func<Task> action)
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _circularLoadingControl.Visibility = Visibility.Visible;
                });

                await Task.Run(async () =>
                {
                    await action();
                });
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _circularLoadingControl.Visibility = Visibility.Collapsed;
                });
            }
        }

        private async void OnAcceptVehicle(VehicleItem item)
        {
            if (item != null)
            {
                await ShowLoadingWhileExecutingAsync(async () =>
                {
                    dynamic data = await CensorVehicleRegistration(item.Id, true);

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" || data.message == "Refresh token không hợp lệ")
                        {
                            _mainViewModel._notificationService.ShowNotification(
                                "Lỗi",
                                data.message,
                                NotificationType.Error
                            );
                            return;
                        }

                        if (data.message == "Bạn không có quyền thực hiện hành động này")
                        {
                            _mainViewModel._notificationService.ShowNotification(
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
                                _mainViewModel._notificationService.ShowNotification(
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

                        if (data.message == "Kiểm duyệt phương tiện thành công!")
                        {
                            _mainViewModel._notificationService.ShowNotification(
                                "Thành công!",
                                (string)data.message,
                                NotificationType.Success
                            );

                            Properties.Settings.Default.access_token = data.authenticate.access_token;
                            Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                            Properties.Settings.Default.Save();

                            item.Status = "Approved";
                        }
                        else
                        {
                            _mainViewModel._notificationService.ShowNotification(
                                "Lỗi",
                                (string)data.message,
                                NotificationType.Error
                            );

                            Properties.Settings.Default.access_token = data.authenticate.access_token;
                            Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                            Properties.Settings.Default.Save();
                        }
                    });
                });
            }
        }

        private async void OnRejectVehicle(VehicleItem item)
        {
            if (item != null)
            {
                await ShowLoadingWhileExecutingAsync(async () =>
                {
                    dynamic data = await CensorVehicleRegistration(item.Id, false);

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" || data.message == "Refresh token không hợp lệ")
                        {
                            _mainViewModel._notificationService.ShowNotification(
                                "Lỗi",
                                data.message,
                                NotificationType.Error
                            );
                            return;
                        }

                        if (data.message == "Bạn không có quyền thực hiện hành động này")
                        {
                            _mainViewModel._notificationService.ShowNotification(
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
                                _mainViewModel._notificationService.ShowNotification(
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

                        if (data.message == "Kiểm duyệt phương tiện thành công!")
                        {
                            _mainViewModel._notificationService.ShowNotification(
                                "Thành công",
                                (string)data.message,
                                NotificationType.Success
                            );

                            Properties.Settings.Default.access_token = data.authenticate.access_token;
                            Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                            Properties.Settings.Default.Save();

                            item.Status = "Rejected";
                        }
                        else
                        {
                            _mainViewModel._notificationService.ShowNotification(
                                "Lỗi",
                                (string)data.message,
                                NotificationType.Error
                            );

                            Properties.Settings.Default.access_token = data.authenticate.access_token;
                            Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                            Properties.Settings.Default.Save();
                        }
                    });
                });
            }
        }

        private async void OnAccept(BusinessItem item)
        {
            if (item != null)
            {
                await ShowLoadingWhileExecutingAsync(async () =>
                {
                    dynamic data = await CensorBusinessRegistration(item.Id, true);

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" || data.message == "Refresh token không hợp lệ")
                        {
                            _mainViewModel._notificationService.ShowNotification(
                                "Error",
                                data.message,
                                NotificationType.Error
                            );
                            return;
                        }

                        if (data.message == "Bạn không có quyền thực hiện hành động này")
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

                        if (data.message == "Lỗi dữ liệu đầu vào")
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

                        if (data.message == "Kiếm duyệt đăng ký doanh nghiệp thành công!")
                        {
                            _mainViewModel._notificationService.ShowNotification(
                                "Thành công",
                                (string)data.message,
                                NotificationType.Success
                            );

                            Properties.Settings.Default.access_token = data.authenticate.access_token;
                            Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                            Properties.Settings.Default.Save();

                            item.Status = "Approved";
                        }
                        else
                        {
                            _mainViewModel._notificationService.ShowNotification(
                                "Lỗi",
                                (string)data.message,
                                NotificationType.Error
                            );

                            Properties.Settings.Default.access_token = data.authenticate.access_token;
                            Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                            Properties.Settings.Default.Save();
                        }
                    });
                });
            }
        }

        private async void OnReject(BusinessItem item)
        {
            if (item != null)
            {
                await ShowLoadingWhileExecutingAsync(async () =>
                {
                    dynamic data = await CensorBusinessRegistration(item.Id, false);

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (data.message == "Bạn phải đăng nhập bỏ sử dụng chức năng này" || data.message == "Refresh token không hợp lệ")
                        {
                            _mainViewModel._notificationService.ShowNotification(
                                "Error",
                                data.message,
                                NotificationType.Error
                            );
                            return;
                        }

                        if (data.message == "Bạn không có quyền thực hiện hành động này")
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

                        if (data.message == "Lỗi dữ liệu đầu vào")
                        {
                            foreach (dynamic items in data.errors)
                            {
                                _mainViewModel._notificationService.ShowNotification(
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
                            _mainViewModel._notificationService.ShowNotification(
                                "Thành công",
                                (string)data.message,
                                NotificationType.Success
                            );

                            Properties.Settings.Default.access_token = data.authenticate.access_token;
                            Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                            Properties.Settings.Default.Save();

                            item.Status = "Rejected";
                        }
                        else
                        {
                            _mainViewModel._notificationService.ShowNotification(
                                "Lỗi",
                                (string)data.message,
                                NotificationType.Warning
                            );

                            Properties.Settings.Default.access_token = data.authenticate.access_token;
                            Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                            Properties.Settings.Default.Save();
                        }
                    });
                });
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

        private async Task<dynamic> GetVehicleRegistration()
        {
            try
            {
                Dictionary<string, string> vehicleRegistrationHeader = new Dictionary<string, string>()
                {
                    { "Authorization", $"Bearer {Properties.Settings.Default.access_token}" }
                };
                var vehicleRegistrationBody = new
                {
                    session_time = session_time,
                    refresh_token = Properties.Settings.Default.refresh_token,
                    current = 0
                };

                dynamic data = await _service.PostWithHeaderAndBodyAsync("api/vehicle/get-vehicle-registration", vehicleRegistrationHeader, vehicleRegistrationBody);

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

        private async Task<dynamic> CensorVehicleRegistration(string id, bool decision)
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
                    vehicle_id = id,
                    decision = decision
                };

                dynamic data = await _service.PutWithHeaderAndBodyAsync("api/vehicle/censor-vehicle", businessRegistrationHeader, businessRegistrationBody);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }


        private async Task LoadRevenueAsync()
        {
            dynamic data = await GetRevenue();
            if (data != null)
            {
                Properties.Settings.Default.access_token = data.authenticate.access_token;
                Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                Properties.Settings.Default.Save();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Revenue = data.result;
                });
            }
        }

        private async Task LoadDealsAsync()
        {
            dynamic data = await GetDeals();
            if (data != null)
            {
                Properties.Settings.Default.access_token = data.authenticate.access_token;
                Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                Properties.Settings.Default.Save();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Deals = data.result;
                });
            }
        }

        private async Task LoadTicketAsync()
        {
            dynamic data = await GetTicket();
            if (data != null)
            {
                Properties.Settings.Default.access_token = data.authenticate.access_token;
                Properties.Settings.Default.refresh_token = data.authenticate.refresh_token;
                Properties.Settings.Default.Save();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Ticket = data.result;
                });
            }
        }

        private async Task LoadBusinessRegistrationAsync()
        {
            dynamic data = await GetBusinessRegistration();
            if (data != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    int count = 0;
                    foreach (dynamic item in data.result.business_registration)
                    {
                        if (count > 2) return;

                        BusinessItems.Add(new BusinessItem
                        {
                            Id = item._id,
                            Email = item.email,
                            Name = item.name,
                            PhoneNumber = item.phone,
                        });

                        count++;
                    }
                });
            }
        }

        private async Task LoadVehicleRegistrationAsync()
        {
            dynamic data = await GetVehicleRegistration();
            if (data != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    int count = 0;
                    foreach (dynamic item in data.result.vehicle)
                    {
                        if (count > 2) return;

                        VehicleItems.Add(new VehicleItem()
                        {
                            Id = item._id,
                            VehicleType = (string)item.vehicle_type,
                            SeatType = (string)item.seat_type,
                            Seats = (int)item.seats,
                            Rules = item.rules,
                            Amenities = item.amenities,
                            LicensePlate = item.license_plate,
                            VehicleImage = item.preview[0].url
                        });

                        count++;
                    }
                });
            }
        }

        private async void LoadDataWithProgress()
        {
            try
            {
                _circularLoadingControl.Visibility = Visibility.Visible;
                await LoadAllDataAsync();
            }
            finally
            {
                _circularLoadingControl.Visibility = Visibility.Collapsed;
            }
        }
        #endregion
    }
}
