using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using TiketManagementV2.ViewModel;

namespace TiketManagementV2.View
{
    /// <summary>
    /// Interaction logic for LogView.xaml
    /// </summary>
    public partial class LogView : UserControl, INotifyPropertyChanged
    {
        public class LogItem
        {
            public LogType LogType { get; set; }
            public string Message { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public enum LogType
        {
            Success,
            Warning,
            Error
        }

        private ObservableCollection<LogItem> _logItems;
        public ObservableCollection<LogItem> LogItems
        {
            get => _logItems;
            set
            {
                _logItems = value;
                //OnPropertyChanged();
                OnPropertyChanged(nameof(HasLogItems));
            }
        }

        public bool HasLogItems => LogItems?.Count > 0;
        public LogView()
        {
            InitializeComponent();
            DataContext = this;
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            LogItems = new ObservableCollection<LogItem>
            {
                new LogItem
                {
                    LogType = LogType.Success,
                    Message = "Business registration approved for 'Saigon Express'",
                    Timestamp = DateTime.Now.AddHours(-1),
                },
                new LogItem
                {
                    LogType = LogType.Warning,
                    Message = "Vehicle inspection pending for XE-12345",
                    Timestamp = DateTime.Now.AddHours(-2),
                },
                new LogItem
                {
                    LogType = LogType.Error,
                    Message = "Failed to process payment for Booking #123456",
                    Timestamp = DateTime.Now.AddHours(-3),
                },
                new LogItem
                {
                    LogType = 0,
                    Message = "New route added: Saigon - Dalat",
                    Timestamp = DateTime.Now.AddHours(-4),
                },
                new LogItem
                {
                    LogType = LogType.Success,
                    Message = "Vehicle VIN#789012 successfully registered",
                    Timestamp = DateTime.Now.AddHours(-5),
                },

                new LogItem
                {
                    LogType = LogType.Warning,
                    Message = "Low seat availability for Route #456 tomorrow",
                    Timestamp = DateTime.Now.AddDays(-1),
                }


            };

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
