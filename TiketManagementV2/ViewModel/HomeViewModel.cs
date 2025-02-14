using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiketManagementV2.Model;
using TiketManagementV2.Services;

namespace TiketManagementV2.ViewModel
{

    public class HomeViewModel : ViewModelBase
    {
        private dynamic _user;
        private ApiServices _service;
        private string _revenue;
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
        public HomeViewModel(dynamic user)
        {
            _service = new ApiServices();

            LoadRevenue();
            LoadDeals();
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

                dynamic data = await _service.PostWithHeaderAndBodyAsync("api/statistical/get-order-statistics", statisticsHeader, statisticsBody);

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
    }
}
