using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

// Import các tầng dựa trên cấu trúc thư mục của bạn
using Pharmacy_Manage.BUS;
using Pharmacy_Manage.DTO;

namespace Pharmacy_Manage.GUI.QuanLy
{
    /// <summary>
    /// Interaction logic for Report.xaml
    /// </summary>
    public partial class Report : UserControl
    {
        // Khai báo đối tượng tầng Business để xử lý dữ liệu
        private ReportBUS reportBus = new ReportBUS();

        public Report()
        {
            InitializeComponent();

            // Đảm bảo dữ liệu được load khi UserControl sẵn sàng
            this.Loaded += Report_Loaded;
        }

        private void Report_Loaded(object sender, RoutedEventArgs e)
        {
            LoadReportData();
        }

        private void LoadReportData()
        {
            try
            {
                // 1. Lấy dữ liệu từ tầng BUS
                List<HoaDonDTO> dsHoaDon = reportBus.GetListHoaDon();
                int soDonHang = reportBus.LaySoLuongDon();
                decimal doanhThu = reportBus.LayTongDoanhThu();
                decimal loiNhuan = reportBus.LayLoiNhuan();

                // 2. Hiển thị lên các Card layout qua x:Name
                txtSoLuongDon.Text = soDonHang.ToString();
                txtDoanhThu.Text = string.Format("{0:N0}đ", doanhThu);
                txtLoiNhuan.Text = string.Format("{0:N0}đ", loiNhuan);

                // 3. Đổ dữ liệu vào DataGrid
                dgHoaDon.ItemsSource = dsHoaDon;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu báo cáo: {ex.Message}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgHoaDon_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Kiểm tra xem người dùng có nhấn vào một dòng dữ liệu hay không
            var selectedInvoice = dgHoaDon.SelectedItem as HoaDonDTO;

            if (selectedInvoice != null)
            {
                MessageBox.Show($"Đang xem chi tiết hóa đơn: {selectedInvoice.MaHD}", "Thông tin");
                // Bạn có thể mở một Window chi tiết tại đây
            }
        }
    }
}