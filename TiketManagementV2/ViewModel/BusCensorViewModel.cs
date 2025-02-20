using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using TiketManagementV2.Commands;

namespace TiketManagementV2.ViewModel
{
    public class BusCensorViewModel : INotifyPropertyChanged
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

        public class User
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
        }

        public ICommand AcceptCommand { get; }
        public ICommand RejectCommand { get; }

        public BusCensorViewModel()
        {
            Users = new ObservableCollection<User>
            {
                new User {Name = "John", Email = "Doe", Phone = "123456789"},
                new User {Name = "Alice", Email = "alice@example.com", Phone = "987654321"},
                new User {Name = "Bob", Email = "bob@example.com", Phone = "555666777"},
            };

            FilteredUsers = new ObservableCollection<User>(Users.Take(_itemsToLoad));
            CanLoadMore = Users.Count > _itemsToLoad;

            AcceptCommand = new RelayCommandGeneric<User>(AcceptBus);
            RejectCommand = new RelayCommandGeneric<User>(RejectBus);
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

        private void RemoveUser(User user)
        {
            if (user != null)
            {
                FilteredUsers.Remove(user);
                Users.Remove(user);
                CanLoadMore = FilteredUsers.Count < Users.Count;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
