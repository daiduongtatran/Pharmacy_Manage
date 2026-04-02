// Import các tầng dựa trên cấu trúc thư mục của bạn
using Pharmacy_Manage.BUS;
using Pharmacy_Manage.DAL;
using Pharmacy_Manage.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            var selectedInvoice = dgHoaDon.SelectedItem as HoaDonDTO;
            if (selectedInvoice == null) return;

            try
            {
                // 1. Chỉnh sửa cách gọi DAL: Đừng viết SQL trực tiếp ở đây
                ReportDAL dal = new ReportDAL();

                // Sử dụng phương thức GetRecentInvoices hoặc tạo một hàm chuyên biệt trong DAL
                // Ở đây tôi dùng tạm ExecuteQuery với 1 tham số theo đúng khai báo của bạn
                string query = $@"
            SELECT h.*, k.MaKH, k.HoTen, k.SoDienThoai, k.Email, k.NgaySinh 
            FROM HoaDon h 
            LEFT JOIN KhachHang k ON h.MaKH = k.MaKH 
            WHERE h.MaHD = '{selectedInvoice.MaHD}'"; // Lưu ý: Cách này tiềm ẩn SQL Injection nếu MaHD không an toàn

                DataTable dt = DataProvider.Instance.ExecuteQuery(query);

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow detail = dt.Rows[0];

                    // 2. KIỂM TRA LẠI: InvoiceDetailWindow PHẢI là Window, không phải UserControl
                    InvoiceDetailWindow detailWindow = new InvoiceDetailWindow();

                    // Gán dữ liệu khách hàng
                    detailWindow.txtMaKH.Text = $"Mã KH: {detail["MaKH"]}";
                    detailWindow.txtHoTen.Text = $"Họ Tên: {detail["HoTen"] ?? "Khách vãng lai"}";
                    detailWindow.txtSDT.Text = $"Số Điện Thoại: {detail["SoDienThoai"]}";
                    detailWindow.txtEmail.Text = $"Email: {detail["Email"]}";

                    if (detail["NgaySinh"] != DBNull.Value)
                        detailWindow.txtNgaySinh.Text = $"Ngày Sinh: {Convert.ToDateTime(detail["NgaySinh"]):dd/MM/yyyy}";
                    else
                        detailWindow.txtNgaySinh.Text = "Ngày Sinh: N/A";

                    // Gán dữ liệu hóa đơn
                    detailWindow.txtNgayLap.Text = $"Ngày Lập: {Convert.ToDateTime(detail["NgayLap"]):dd/MM/yyyy HH:mm}";
                    detailWindow.txtTienDV.Text = $"Tiền Dịch Vụ: {string.Format("{0:N0}đ", detail["TongTienDichVu"])}";
                    detailWindow.txtTienSP.Text = $"Tiền Sản Phẩm: {string.Format("{0:N0}đ", detail["TongTienSanPham"])}";
                    detailWindow.txtTongTien.Text = $"Tổng Thanh Toán: {string.Format("{0:N0}đ", detail["TongThanhToan"])}";

                    // Hiển thị cửa sổ
                    detailWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi chi tiết: {ex.Message}");
            }
        }
    }
}