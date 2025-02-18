using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using TiketManagementV2.Commands;

namespace TiketManagementV2.ViewModel
{
    public class LogViewModel : ViewModelBase
    {
        private ObservableCollection<LogItem> _logItems;
        public ObservableCollection<LogItem> LogItems
        {
            get => _logItems;
            set
            {
                _logItems = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasLogItems));
            }
        }

        public bool HasLogItems => LogItems?.Count > 0;

        public ICommand ClearAllCommand { get; }
        public ICommand DeleteLogCommand { get; }

        public LogViewModel()
        {
            ClearAllCommand = new RelayCommand(obj => ExecuteClearAll());
            DeleteLogCommand = new RelayCommandGeneric<LogItem>(ExecuteDeleteLog);

            // Initialize with sample data
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
                    Source = "Business Management"
                },
                new LogItem
                {
                    LogType = LogType.Warning,
                    Message = "Vehicle inspection pending for XE-12345",
                    Timestamp = DateTime.Now.AddHours(-2),
                    Source = "Vehicle Management"
                },
                new LogItem
                {
                    LogType = LogType.Error,
                    Message = "Failed to process payment for Booking #123456",
                    Timestamp = DateTime.Now.AddHours(-3),
                    Source = "Payment System"
                },
                new LogItem
                {
                    LogType = LogType.Info,
                    Message = "New route added: Saigon - Dalat",
                    Timestamp = DateTime.Now.AddHours(-4),
                    Source = "Route Management"
                },
                new LogItem
                {
                    LogType = LogType.Success,
                    Message = "Vehicle VIN#789012 successfully registered",
                    Timestamp = DateTime.Now.AddHours(-5),
                    Source = "Vehicle Management"
                },

                new LogItem
                {
                    LogType = LogType.Warning,
                    Message = "Low seat availability for Route #456 tomorrow",
                    Timestamp = DateTime.Now.AddDays(-1),
                    Source = "Booking System"
                }
            };
        }

        private void ExecuteClearAll()
        {
            LogItems.Clear();
            OnPropertyChanged(nameof(HasLogItems));
        }

        private void ExecuteDeleteLog(LogItem logItem)
        {
            if (logItem != null)
            {
                LogItems.Remove(logItem);
                OnPropertyChanged(nameof(HasLogItems));
            }
        }
    }

    public class LogItem
    {
        public LogType LogType { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string Source { get; set; }
    }

    public enum LogType
    {
        Info,
        Success,
        Warning,
        Error
    }
}
