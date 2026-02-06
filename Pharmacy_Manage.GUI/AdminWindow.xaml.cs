using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input; // Thêm thư viện này để dùng MouseButtonEventArgs
using LiveCharts;
using LiveCharts.Wpf;

namespace Pharmacy_Manage.GUI
{
    public partial class AdminWindow : Window
    {
        public SeriesCollection RevenueSeries { get; set; } = new SeriesCollection();
        public string[] ChartLabels { get; set; } = Array.Empty<string>();
        public Func<double, string> Formatter { get; set; } = value => value.ToString("N0");

        public AdminWindow()
        {
            InitializeComponent();

            // Khởi tạo dữ liệu
            LoadChartData();
            LoadAlertData();

            // QUAN TRỌNG: Dòng này giúp Binding hoạt động
            this.DataContext = this;
        }

        private void LoadChartData()
        {
            RevenueSeries.Add(new LineSeries
            {
                Title = "Doanh thu",
                Values = new ChartValues<double> { 8500000, 12000000, 9500000, 16000000, 11000000, 18500000, 14500000 },
                PointGeometrySize = 10,
                StrokeThickness = 3,
                Stroke = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#2C9BB3"),
                Fill = System.Windows.Media.Brushes.Transparent
            });

            ChartLabels = new[] { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
        }

        private void LoadAlertData()
        {
            // Dữ liệu mẫu (Khớp với Binding trong XAML: ProductName, StockQuantity, Status)
            var alertList = new List<object>
            {
                new { ProductName = "Panadol Extra", StockQuantity = 5, Status = "SẮP HẾT" },
                new { ProductName = "Augmentin 1g", StockQuantity = 40, Status = "HẾT HẠN" },
                new { ProductName = "Berberin", StockQuantity = 0, Status = "HẾT HÀNG" },
                new { ProductName = "Vitamin C", StockQuantity = 8, Status = "SẮP HẾT" }
            };

            if (dgAlert != null)
            {
                dgAlert.ItemsSource = alertList;
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

        // Hàm xử lý kéo cửa sổ
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                MainWindow loginWindow = new MainWindow();
                loginWindow.Show();
                this.Close();
            }
        }
    }
}