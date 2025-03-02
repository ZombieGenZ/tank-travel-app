using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;

namespace TiketManagementV2.View
{
    public partial class ChartView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ChartValues<double> _hiringSources;
        public ChartValues<double> HiringSources
        {
            get => _hiringSources;
            set
            {
                _hiringSources = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HiringSources)));
            }
        }


        private List<string> _sourceLabels;
        public List<string> SourceLabels
        {
            get => _sourceLabels;
            set
            {
                _sourceLabels = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceLabels)));
            }
        }

        public ChartView()
        {
            InitializeComponent();
            this.DataContext = this;
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            HiringSources = new ChartValues<double> { 50, 60, 55, 70, 65, 80, 75, 85, 90, 100, 75, 40 };

            SourceLabels = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        }
    }
}