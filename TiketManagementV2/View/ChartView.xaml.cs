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

        // These properties match what your XAML is looking for
        private ChartValues<double> _marketingCosts;
        public ChartValues<double> MarketingCosts
        {
            get => _marketingCosts;
            set
            {
                _marketingCosts = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MarketingCosts)));
            }
        }

        private ChartValues<double> _salesCosts;
        public ChartValues<double> SalesCosts
        {
            get => _salesCosts;
            set
            {
                _salesCosts = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SalesCosts)));
            }
        }

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

        // Your existing properties for labels
        private List<string> _months;
        public List<string> Months
        {
            get => _months;
            set
            {
                _months = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Months)));
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
            // Set data for both charts
            MarketingCosts = new ChartValues<double> { 10, 20, 15, 25, 30, 18, 22, 28 };
            SalesCosts = new ChartValues<double> { 8, 15, 12, 20, 25, 15, 18, 22 };
            HiringSources = new ChartValues<double> { 50, 60, 55, 70, 65, 80, 75, 85 };

            Months = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug" };
            SourceLabels = new List<string> { "Direct", "LinkedIn", "Hired", "Workin", "Instagram", "Referral" };
        }
    }
}