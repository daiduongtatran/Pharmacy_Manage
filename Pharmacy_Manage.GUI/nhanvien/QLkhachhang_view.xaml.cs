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

                if (item.TrangThai == "Đã thanh toán")
                    tongDoanhThu += item.TongThanhToan;
                else if (item.TrangThai == "Chờ thanh toán")
                    chuaThu += item.TongThanhToan;
            }

            dgHoaDon.ItemsSource = _danhSachHienThi;
            txtTongHoaDon.Text = _danhSachHienThi.Count.ToString("N0");
            txtTongDoanhThu.Text = tongDoanhThu.ToString("N0");
            txtChuaThu.Text = chuaThu.ToString("N0");
        }

        // ================= XEM CHI TIẾT HÓA ĐƠN =================
        private void BtnChiTiet_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is HoaDonViewModel hd)
            {
                txtTieuDePopup.Text = $"CHI TIẾT HÓA ĐƠN #{hd.MaHD}";
                txtKhachHangPopup.Text = $"Khách hàng: {hd.TenKhachHang} - SĐT: {hd.SDT} ({hd.LoaiGiaoDich})";

                LoadChiTietHoaDon(hd.MaHD);
                DialogChiTiet.Visibility = Visibility.Visible;
            }
        }

        private void LoadChiTietHoaDon(int maHD)
        {
            var dsThuoc = new ObservableCollection<ChiTietThuocViewModel>();
            var dsDichVu = new ObservableCollection<ChiTietDichVuViewModel>();

            try
            {
                using (SqlConnection con = _db.GetConnection())
                {
                    con.Open();

                    // 1. LẤY CHI TIẾT THUỐC
                    string queryThuoc = @"
                        SELECT sp.TenSP, ct.SoLuong, ct.DonGia, (ct.SoLuong * ct.DonGia) AS ThanhTien
                        FROM ChiTietHoaDon ct
                        JOIN SanPham sp ON ct.MaSP = sp.MaSP
                        WHERE ct.MaHD = @MaHD";

                    using (SqlCommand cmd = new SqlCommand(queryThuoc, con))
                    {
                        cmd.Parameters.AddWithValue("@MaHD", maHD);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dsThuoc.Add(new ChiTietThuocViewModel
                                {
                                    TenSP = reader["TenSP"].ToString(),
                                    SoLuong = Convert.ToInt32(reader["SoLuong"]),
                                    DonGia = Convert.ToDecimal(reader["DonGia"]),
                                    ThanhTien = Convert.ToDecimal(reader["ThanhTien"])
                                });
                            }
                        }
                    }

                    // 2. LẤY CHI TIẾT DỊCH VỤ
                    string queryDichVu = @"
                        SELECT dv.TenDV, cd.ThanhTien
                        FROM ChiTietDichVu cd
                        JOIN DichVu dv ON cd.MaDV = dv.MaDV
                        WHERE cd.MaHD = @MaHD";

                    using (SqlCommand cmd = new SqlCommand(queryDichVu, con))
                    {
                        cmd.Parameters.AddWithValue("@MaHD", maHD);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dsDichVu.Add(new ChiTietDichVuViewModel
                                {
                                    TenDV = reader["TenDV"].ToString(),
                                    ThanhTien = Convert.ToDecimal(reader["ThanhTien"])
                                });
                            }
                        }
                    }
                }

                dgChiTietThuoc.ItemsSource = dsThuoc;
                dgChiTietDichVu.ItemsSource = dsDichVu;

                // Ẩn bảng Dịch Vụ đi nếu list trống (Khách chỉ mua thuốc)
                dgChiTietDichVu.Visibility = dsDichVu.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                // Tạm thời catch lỗi êm ái nếu bảng chưa tồn tại
                Console.WriteLine(ex.Message);
            }
        }

        private void BtnDongPopup_Click(object sender, RoutedEventArgs e)
        {
            DialogChiTiet.Visibility = Visibility.Collapsed;
        }
    }

    // ================= MODELS BINDING =================
    public class HoaDonViewModel
    {
        public int MaHD { get; set; }
        public DateTime NgayLap { get; set; }
        public string TenKhachHang { get; set; }
        public string SDT { get; set; }
        public decimal TongTienDichVu { get; set; }
        public decimal TongTienSanPham { get; set; }
        public decimal TongThanhToan { get; set; }
        public string TrangThai { get; set; }
        public string GhiChu { get; set; }

        public string LoaiGiaoDich
        {
            get
            {
                if (TongTienDichVu > 0) return "Khám bệnh & Kê đơn";
                else return "Chỉ mua thuốc";
            }
        }
    }

    public class ChiTietThuocViewModel
    {
        public string TenSP { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class ChiTietDichVuViewModel
    {
        public string TenDV { get; set; }
        public decimal ThanhTien { get; set; }
    }
}