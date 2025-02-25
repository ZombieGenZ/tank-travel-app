using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Quobject.SocketIoClientDotNet.Client;

namespace TiketManagementV2.View
{
    /// <summary>
    /// Interaction logic for MailView.xaml
    /// </summary>
    public partial class MailView : Window
    {
        private Socket socket;
        public ObservableCollection<Notification> Notifications { get; set; }

        public MailView()
        {
            InitializeComponent();
            LoadSampleData();
            DataContext = this;
        }

        private void LoadSampleData()
        {
            Notifications = new ObservableCollection<Notification>
            {
                new Notification { Icon = "Envelope", Title = "New Message", Message = "You have received a new message from Admin.", Time = "10:45 AM" },
                new Notification { Icon = "Bell", Title = "System Alert", Message = "Scheduled maintenance at 2:00 PM today.", Time = "9:30 AM" },
                new Notification { Icon = "CheckCircle", Title = "Task Completed", Message = "Your ticket #12345 has been resolved.", Time = "Yesterday" },
                new Notification { Icon = "Envelope", Title = "New Message", Message = "You have received a new message from Admin.", Time = "10:45 AM" },
                new Notification { Icon = "Bell", Title = "System Alert", Message = "Scheduled maintenance at 2:00 PM today.", Time = "9:30 AM" },
                new Notification { Icon = "CheckCircle", Title = "Task Completed", Message = "Your ticket #12345 has been resolved.", Time = "Yesterday" },
                new Notification { Icon = "Envelope", Title = "New Message", Message = "You have received a new message from Admin.", Time = "10:45 AM" },
                new Notification { Icon = "Bell", Title = "System Alert", Message = "Scheduled maintenance at 2:00 PM today.", Time = "9:30 AM" },
                new Notification { Icon = "CheckCircle", Title = "Task Completed", Message = "Your ticket #12345 has been resolved.", Time = "Yesterday" },
                new Notification { Icon = "Envelope", Title = "New Message", Message = "You have received a new message from Admin.", Time = "10:45 AM" },
                new Notification { Icon = "Bell", Title = "System Alert", Message = "Scheduled maintenance at 2:00 PM today.", Time = "9:30 AM" },
                new Notification { Icon = "CheckCircle", Title = "Task Completed", Message = "Your ticket #12345 has been resolved.", Time = "Yesterday" },
                new Notification { Icon = "ExclamationTriangle", Title = "Warning", Message = "Unusual login attempt detected.", Time = "2 days ago" }
            };

            NotificationsList.ItemsSource = Notifications;
        }

        public class Notification
        {
            public string Icon { get; set; }
            public string Title { get; set; }
            public string Message { get; set; }
            public string Time { get; set; }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
