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
using System.Windows.Shapes;

namespace TiketManagementV2.View
{
    /// <summary>
    /// Interaction logic for BanAccountView.xaml
    /// </summary>
    public partial class BanAccountView : Window
    {
        public BanAccountView()
        {
            InitializeComponent();
            var selectedDate = DateTime.Now;
            txtSelectedDateTime.Text = selectedDate.ToString("HH:mm dd/MM/yyyy");
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnSelectDateTime_Click(object sender, RoutedEventArgs e)
        {
            var selectedDate = Calendar.SelectedDate ?? DateTime.Now;

            var time = Clock.Time;

            var combinedDateTime = new DateTime(
                selectedDate.Year,
                selectedDate.Month,
                selectedDate.Day,
                time.Hour,
                time.Minute,
                0
            );

            txtSelectedDateTime.Text = combinedDateTime.ToString("HH:mm dd/MM/yyyy");
        }
    }
}
