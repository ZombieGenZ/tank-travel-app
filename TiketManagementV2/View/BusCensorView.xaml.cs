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
using TiketManagementV2.Commands;
using TiketManagementV2.Services;
using TiketManagementV2.ViewModel;

namespace TiketManagementV2.View
{
    /// <summary>
    /// Interaction logic for BusCensorView.xaml
    /// </summary>
    public partial class BusCensorView : UserControl, INotifyPropertyChanged
    {
        private int _itemsToLoad = 2;
        private ObservableCollection<User> _filteredUsers;
        private bool _canLoadMore;

        public ObservableCollection<User> Users { get; set; }
        public ObservableCollection<User> FilteredUsers
        {
            get { return _filteredUsers; }
            set { _filteredUsers = value; OnPropertyChanged(nameof(FilteredUsers)); }
        }

        public bool CanLoadMore
        {
            get { return _canLoadMore; }
            set { _canLoadMore = value; OnPropertyChanged(nameof(CanLoadMore)); }
        }

        public class User : INotifyPropertyChanged
        {
            private string Name { get; set; }
            private string Email { get; set; }
            private string Phone { get; set; }

            public string name
            {
                get => Name;
                set
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }

            public string email
            {
                get => Email;
                set
                {
                    email = value;
                    OnPropertyChanged(nameof(Email));
                }
            }
            public string phone
            {
                get => Phone;
                set
                {
                    phone = value;
                    OnPropertyChanged(nameof(Phone));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ICommand AcceptCommand { get; }
        public ICommand RejectCommand { get; }

        public BusCensorView(INotificationService notificationService)
        {
            InitializeComponent();
            DataContext = this;

            AcceptCommand = new RelayCommandGeneric<User>(AcceptBus);
            RejectCommand = new RelayCommandGeneric<User>(RejectBus);

        }
        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is BusCensorViewModel viewModel)
            {
                viewModel.LoadMore();
            }
        }

        public void LoadMore()
        {
            int currentCount = FilteredUsers.Count;

            if (currentCount < Users.Count)
            {
                var moreUsers = Users.Skip(currentCount).Take(_itemsToLoad).ToList();
                foreach (var user in moreUsers)
                {
                    FilteredUsers.Add(user);
                }
            }

            CanLoadMore = FilteredUsers.Count < Users.Count;
        }

        private async void AcceptBus(object obj)
        {

        }

        private async void RejectBus(object obj)
        {

        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
