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
    public partial class Report : UserControl
    {
        private ReportBUS reportBus = new ReportBUS();

        public Report()
        {
            InitializeComponent();

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
                List<HoaDonDTO> dsHoaDon = reportBus.GetListHoaDon();
                int soDonHang = reportBus.LaySoLuongDon();
                decimal doanhThu = reportBus.LayTongDoanhThu();
                decimal loiNhuan = reportBus.LayLoiNhuan();

                txtSoLuongDon.Text = soDonHang.ToString();
                txtDoanhThu.Text = string.Format("{0:N0}đ", doanhThu);
                txtLoiNhuan.Text = string.Format("{0:N0}đ", loiNhuan);

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
                ReportDAL dal = new ReportDAL();

                string query = $@"
            SELECT h.*, k.MaKH, k.HoTen, k.SoDienThoai, k.Email, k.NgaySinh 
            FROM HoaDon h 
            LEFT JOIN KhachHang k ON h.MaKH = k.MaKH 
            WHERE h.MaHD = '{selectedInvoice.MaHD}'"; 

                DataTable dt = DataProvider.Instance.ExecuteQuery(query);

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow detail = dt.Rows[0];

                    InvoiceDetailWindow detailWindow = new InvoiceDetailWindow();

                    detailWindow.txtMaKH.Text = $"Mã KH: {detail["MaKH"]}";
                    detailWindow.txtHoTen.Text = $"Họ Tên: {detail["HoTen"] ?? "Khách vãng lai"}";
                    detailWindow.txtSDT.Text = $"Số Điện Thoại: {detail["SoDienThoai"]}";
                    detailWindow.txtEmail.Text = $"Email: {detail["Email"]}";

                    if (detail["NgaySinh"] != DBNull.Value)
                        detailWindow.txtNgaySinh.Text = $"Ngày Sinh: {Convert.ToDateTime(detail["NgaySinh"]):dd/MM/yyyy}";
                    else
                        detailWindow.txtNgaySinh.Text = "Ngày Sinh: N/A";

                    detailWindow.txtNgayLap.Text = $"Ngày Lập: {Convert.ToDateTime(detail["NgayLap"]):dd/MM/yyyy HH:mm}";
                    detailWindow.txtTienDV.Text = $"Tiền Dịch Vụ: {string.Format("{0:N0}đ", detail["TongTienDichVu"])}";
                    detailWindow.txtTienSP.Text = $"Tiền Sản Phẩm: {string.Format("{0:N0}đ", detail["TongTienSanPham"])}";
                    detailWindow.txtTongTien.Text = $"Tổng Thanh Toán: {string.Format("{0:N0}đ", detail["TongThanhToan"])}";

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