using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace TiketManagementV2.View
{
    public partial class AddVehicleView : Window, INotifyPropertyChanged
    {
        public ObservableCollection<string> ImageFiles { get; set; } = new ObservableCollection<string>();

        private string _selectedImagePath;
        public string SelectedImagePath
        {
            get { return _selectedImagePath; }
            set
            {
                _selectedImagePath = value;
                OnPropertyChanged(nameof(SelectedImagePath));
            }
        }

        public AddVehicleView()
        {
            InitializeComponent();
            DataContext = this; // Set DataContext để binding hoạt động
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

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem comboBoxItem = (ComboBoxItem)cmbEditVehicle.SelectedItem;
            string vehicleType = comboBoxItem?.Content.ToString();

        }

        private void ChangeProfileImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Select Images",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var file in openFileDialog.FileNames)
                {
                    string fileName = System.IO.Path.GetFileName(file); // Lấy tên file ảnh

                    if (!ImageFiles.Contains(fileName))
                    {
                        ImageFiles.Add(fileName);
                    }
                }

                if (ImageFiles.Count > 0 && string.IsNullOrEmpty(SelectedImagePath))
                {
                    SelectedImagePath = ImageFiles[0];
                }
            }

            Console.WriteLine("Selected Images: " + string.Join(", ", ImageFiles));
        }


        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is string fileName)
            {
                ImageFiles.Remove(fileName);
                if (ImageFiles.Count > 0)
                {
                    SelectedImagePath = ImageFiles[0]; // Cập nhật tên ảnh đầu tiên còn lại
                }
                else
                {
                    SelectedImagePath = string.Empty;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
