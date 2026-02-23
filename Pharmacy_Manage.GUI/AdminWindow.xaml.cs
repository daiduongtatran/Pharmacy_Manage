using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Wpf;
using Pharmacy_Manage.BUS; // Nhớ check namespace này

namespace Pharmacy_Manage.GUI
{
    public partial class AdminWindow : Window
    {
        // 1. Khai báo thuộc tính cho biểu đồ
        public SeriesCollection RevenueSeries { get; set; } = new SeriesCollection();
        public string[] ChartLabels { get; set; } = Array.Empty<string>();
        public Func<double, string> Formatter { get; set; } = value => value.ToString("N0") + " đ";

        // 2. Khai báo biến lưu trữ số liệu bóc tách
        private int _countExpiring = 0; // Số thuốc sắp hết hạn
        private int _countLowStock = 0; // Số thuốc tồn kho thấp

        // Khai báo lớp BUS
        SanPhamBUS spBUS = new SanPhamBUS();

        public AdminWindow()
        {
            InitializeComponent();

            // Chạy các hàm khởi tạo
            LoadChartData();
            LoadAlertData();
            LoadUrgentData(); // Lấy số liệu thật từ SQL

            this.DataContext = this;
        }

        // HÀM LẤY DỮ LIỆU ĐỘNG TỪ DATABASE
        private void LoadUrgentData()
        {
            try
            {
                DataTable dt = spBUS.GetUrgentStats();
                if (dt != null && dt.Rows.Count > 0)
                {
                    // 1. Dùng đúng tên cột "TongCanXuLy" từ SQL của bạn
                    txtUrgentTotal.Text = dt.Rows[0]["TongCanXuLy"].ToString() + " Thuốc";

                    // 2. Dùng đúng tên cột "SoLuongSapHetHan" và "SoLuongTonThap"
                    _countExpiring = Convert.ToInt32(dt.Rows[0]["SoLuongSapHetHan"]);
                    _countLowStock = Convert.ToInt32(dt.Rows[0]["SoLuongTonThap"]);
                }
            }
            catch (Exception ex)
            {
                txtUrgentTotal.Text = "0 Thuốc";
                // MessageBox.Show("Lỗi: " + ex.Message); // Mở dòng này nếu muốn xem lỗi chi tiết
            }
        }

        // SỰ KIỆN KHI CLICK VÀO CARD "CẦN XỬ LÝ GẤP"
        private void CardUrgent_Click(object sender, MouseButtonEventArgs e)
        {
            string detail = "📊 CHI TIẾT CÁC MẶT HÀNG CẦN XỬ LÝ:\n\n" +
                            $"• Thuốc sắp hết hạn (< 6 tháng): {_countExpiring} loại\n" +
                            $"• Thuốc có tồn kho thấp (< 50): {_countLowStock} loại\n\n" +
                            "Hệ thống khuyến nghị bạn nên nhập thêm hàng hoặc kiểm tra hạn dùng.";

            MessageBox.Show(detail, "Thông tin kho hàng", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        // --- CÁC HÀM CŨ CỦA BẠN (GIỮ NGUYÊN) ---
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
            var alertList = new List<object>
            {
                new { ProductName = "Panadol Extra", StockQuantity = 5, Status = "SẮP HẾT" },
                new { ProductName = "Augmentin 1g", StockQuantity = 40, Status = "HẾT HẠN" },
                new { ProductName = "Berberin", StockQuantity = 0, Status = "HẾT HÀNG" }
            };
            if (dgAlert != null) dgAlert.ItemsSource = alertList;
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
                MainTabControl.SelectedIndex = int.Parse(btn.Tag.ToString());
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Xác nhận đăng xuất?", "Thông báo", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                this.Close(); // Hoặc mở lại màn hình Login
            }
        }
    }
}