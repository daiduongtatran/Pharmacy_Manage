using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;

namespace Pharmacy_Manage.GUI
{
    public partial class AdminWindow : Window
    {
        // Khởi tạo mặc định để tránh lỗi CS8618 (Warning)
        public SeriesCollection RevenueSeries { get; set; } = new SeriesCollection();
        public string[] ChartLabels { get; set; } = Array.Empty<string>();
        public Func<double, string> Formatter { get; set; } = x => x.ToString("N0") + " đ";

        public AdminWindow()
        {
            InitializeComponent();
            SetupChart();
            LoadData();
            this.DataContext = this;
        }

        private void SetupChart()
        {
            RevenueSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Doanh thu",
                    Values = new ChartValues<double> { 8500000, 12450000, 9200000, 15000000, 11000000, 18200000, 14500000 },
                    PointGeometrySize = 12,
                    StrokeThickness = 4
                }
            };
            ChartLabels = new[] { "24/01", "25/01", "26/01", "27/01", "28/01", "29/01", "30/01" };
        }

        private void LoadData()
        {
            // Kiểm tra dgInventory có tồn tại không trước khi gán
            if (dgInventory != null)
            {
                var list = new List<object>
                {
                    new { ID="MED01", Name="Panadol Extra", Category="Giảm đau", Stock=150, Price="1.500đ", Expiry="12/2026" },
                    new { ID="MED02", Name="Augmentin 1g", Category="Kháng sinh", Stock=40, Price="15.000đ", Expiry="05/2026" }
                };
                dgInventory.ItemsSource = list;
            }
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int index = int.Parse(btn.Tag.ToString());
                MainTabControl.SelectedIndex = index;
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có muốn đăng xuất không?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                new MainWindow().Show();
                this.Close();
            }
        }
    }
}