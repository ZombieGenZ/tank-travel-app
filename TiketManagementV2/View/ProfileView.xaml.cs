using System;
using System.Collections.Generic;
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
using TiketManagementV2.Services;
using TiketManagementV2.ViewModel;

namespace TiketManagementV2.View
{
    public partial class ProfileView : UserControl
    {
        public ProfileView(dynamic user)
        {
            InitializeComponent();
            var notificationService = new NotificationService();
            var viewModel = new MainViewModel(notificationService, user);
            DataContext = viewModel;
        }
    }
}
