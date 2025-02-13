using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiketManagementV2.ViewModel
{
    public class HomeViewModel : ViewModelBase
    {
        private string _welcomeMessage;
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        public HomeViewModel()
        {
            WelcomeMessage = "Welcome to Dashboard!";
        }
    }
}
