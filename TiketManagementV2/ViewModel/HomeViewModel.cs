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
        private string _revenue;
        private string _Deals;
        private ApiServices _service;
        public string Revenue
        {
            get => _revenue;
            set => SetProperty(ref _revenue, value);
        }
        public HomeViewModel()
        {
            _service = new ApiServices();
        }

    }
}
