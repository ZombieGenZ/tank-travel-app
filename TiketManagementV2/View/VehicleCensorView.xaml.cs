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
    /// <summary>
    /// Interaction logic for VehicleCensorView.xaml
    /// </summary>
    public partial class VehicleCensorView : UserControl
    {
        public VehicleCensorView(INotificationService notificationService)
        {
            InitializeComponent();
            var viewModel = new VehicleCensorViewModel(notificationService);
            DataContext = viewModel;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(SearchTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }


        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is VehicleCensorViewModel viewModel)
            {
                viewModel.LoadMore();
            }
        }
    }
}
