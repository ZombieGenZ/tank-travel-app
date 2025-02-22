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
    /// Interaction logic for ConfirmationDialogView.xaml
    /// </summary>
    public partial class ConfirmationDialogView : Window
    {
        public event Action<bool> OnDialogClosed;
        public ConfirmationDialogView()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            OnDialogClosed?.Invoke(true);
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            OnDialogClosed?.Invoke(false);
            this.Close();
        }
    }
}
