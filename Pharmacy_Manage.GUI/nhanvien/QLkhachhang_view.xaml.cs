using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using Pharmacy_Manage.DAL;

namespace Pharmacy_Manage.GUI
{
    public partial class QLkhachhang_view : UserControl
    {
        private ObservableCollection<HoaDonViewModel> _danhSachGoc = new ObservableCollection<HoaDonViewModel>();
        private ObservableCollection<HoaDonViewModel> _danhSachHienThi = new ObservableCollection<HoaDonViewModel>();
        private DbConnection _db = new DbConnection();

        public QLkhachhang_view()
        {
            InitializeComponent();
            dpLocNgay.SelectedDate = DateTime.Now.Date; // Mặc định hiển thị hóa đơn hôm nay
            LoadDuLieuTuDatabase();
        }

        private void LoadDuLieuTuDatabase()
        {
            _danhSachGoc.Clear();
            try
            {
                using (SqlConnection con = _db.GetConnection())
                {
                    con.Open();
                    // Truy vấn bảng HoaDon và JOIN lấy Tên/SĐT từ KhachHang
                    string query = @"
                        SELECT hd.MaHD, hd.NgayLap, 
                               kh.HoTen AS TenKhachHang, kh.SoDienThoai AS SDT,
                               hd.TongTienDichVu, hd.TongTienSanPham, hd.TongThanhToan, 
                               hd.TrangThai, hd.GhiChu
                        FROM HoaDon hd
                        LEFT JOIN KhachHang kh ON hd.MaKH = kh.MaKH
                        ORDER BY hd.NgayLap DESC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _danhSachGoc.Add(new HoaDonViewModel
                                {
                                    MaHD = reader["MaHD"] != DBNull.Value ? Convert.ToInt32(reader["MaHD"]) : 0,
                                    NgayLap = reader["NgayLap"] != DBNull.Value ? Convert.ToDateTime(reader["NgayLap"]) : DateTime.MinValue,
                                    TenKhachHang = reader["TenKhachHang"] != DBNull.Value ? reader["TenKhachHang"].ToString() : "Khách lẻ",
                                    SDT = reader["SDT"] != DBNull.Value ? reader["SDT"].ToString() : "",
                                    TongTienDichVu = reader["TongTienDichVu"] != DBNull.Value ? Convert.ToDecimal(reader["TongTienDichVu"]) : 0,
                                    TongTienSanPham = reader["TongTienSanPham"] != DBNull.Value ? Convert.ToDecimal(reader["TongTienSanPham"]) : 0,
                                    TongThanhToan = reader["TongThanhToan"] != DBNull.Value ? Convert.ToDecimal(reader["TongThanhToan"]) : 0,
                                    TrangThai = reader["TrangThai"] != DBNull.Value ? reader["TrangThai"].ToString() : "Chờ thanh toán",
                                    GhiChu = reader["GhiChu"] != DBNull.Value ? reader["GhiChu"].ToString() : ""
                                });
                            }
                        }
                    }
                }
                LocDuLieu();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu Hóa đơn:\n" + ex.Message, "Lỗi Database", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================= XỬ LÝ LỌC & TÌM KIẾM =================
        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            LocDuLieu();
        }

        private void LocDuLieu()
        {
            if (_danhSachGoc == null || dgHoaDon == null || cbTrangThai == null) return;

            string tuKhoa = txtTimKiem.Text.Trim().ToLower();
            string trangThai = (cbTrangThai.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Tất cả";
            DateTime? ngayLoc = dpLocNgay.SelectedDate;

            var ketQua = _danhSachGoc.Where(h =>
                (string.IsNullOrEmpty(tuKhoa) || h.TenKhachHang.ToLower().Contains(tuKhoa) || h.SDT.Contains(tuKhoa) || h.MaHD.ToString().Contains(tuKhoa)) &&
                (trangThai == "Tất cả" || h.TrangThai == trangThai) &&
                (ngayLoc == null || h.NgayLap.Date == ngayLoc.Value.Date)
            ).ToList();

            _danhSachHienThi.Clear();
            decimal tongDoanhThu = 0;
            decimal chuaThu = 0;

            foreach (var item in ketQua)
            {
                _danhSachHienThi.Add(item);

                // Tính toán số liệu thống kê
                if (item.TrangThai == "Đã thanh toán")
                    tongDoanhThu += item.TongThanhToan;
                else if (item.TrangThai == "Chờ thanh toán")
                    chuaThu += item.TongThanhToan;
            }

            dgHoaDon.ItemsSource = _danhSachHienThi;

            // Cập nhật lên UI
            txtTongHoaDon.Text = _danhSachHienThi.Count.ToString("N0");
            txtTongDoanhThu.Text = tongDoanhThu.ToString("N0");
            txtChuaThu.Text = chuaThu.ToString("N0");
        }

        // ================= XEM CHI TIẾT HÓA ĐƠN =================
        private void BtnChiTiet_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is HoaDonViewModel hd)
            {
                // Sau này bạn có thể tạo 1 Form (Window) mới, truyền hd.MaHD sang 
                // để truy vấn bảng ChiTietHoaDon và hiển thị lên nhé!
                MessageBox.Show($"Chức năng hiển thị Chi Tiết Hóa Đơn (Mã HĐ: {hd.MaHD})\nĐang được phát triển...", "Thông tin", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    // Lớp Model Khách Hàng (Khớp 100% với tên cột bạn yêu cầu)
    public class KhachHangViewModel
    {
        public string CustomerID { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public int Point { get; set; }
        public string UserName { get; set; } = "";

        // Cột phụ tự tính không lưu trong DB
        public string HangThanhVien { get; set; } = "";
    }
}