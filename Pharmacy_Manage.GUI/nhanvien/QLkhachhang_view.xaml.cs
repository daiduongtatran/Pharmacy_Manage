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
        private ObservableCollection<HoaDonViewModel> _danhSachGoc = new();
        private ObservableCollection<HoaDonViewModel> _danhSachHienThi = new();
        private DbConnection _db = new DbConnection();

        public QLkhachhang_view()
        {
            InitializeComponent();

            // Đăng ký reload
            hamloadchung.ReloadAll += LoadDuLieuTuDatabase;

            dpLocNgay.SelectedDate = DateTime.Now;
            dgHoaDon.ItemsSource = _danhSachHienThi;

            LoadDuLieuTuDatabase();
        }

        // ================= LOAD DATABASE =================
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
                               kh.HoTen, kh.SoDienThoai,
                               hd.TongTienDichVu, hd.TongTienSanPham, hd.TongThanhToan,
                               hd.TrangThai, hd.GhiChu
                        FROM HoaDon hd
                        LEFT JOIN KhachHang kh ON hd.MaKH = kh.MaKH
                        ORDER BY hd.NgayLap DESC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _danhSachGoc.Add(new HoaDonViewModel
                            {
                                MaHD = Convert.ToInt32(reader["MaHD"]),
                                NgayLap = Convert.ToDateTime(reader["NgayLap"]),
                                TenKhachHang = reader["HoTen"]?.ToString() ?? "Khách lẻ",
                                SDT = reader["SoDienThoai"]?.ToString() ?? "",
                                TongTienDichVu = reader["TongTienDichVu"] != DBNull.Value ? Convert.ToDecimal(reader["TongTienDichVu"]) : 0,
                                TongTienSanPham = reader["TongTienSanPham"] != DBNull.Value ? Convert.ToDecimal(reader["TongTienSanPham"]) : 0,
                                TongThanhToan = reader["TongThanhToan"] != DBNull.Value ? Convert.ToDecimal(reader["TongThanhToan"]) : 0,
                                TrangThai = reader["TrangThai"]?.ToString() ?? "",
                                GhiChu = reader["GhiChu"]?.ToString() ?? ""
                            });
                        }
                    }
                }

                LocDuLieu();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load dữ liệu:\n" + ex.Message);
            }
        }

        // ================= FILTER =================
        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            LocDuLieu();
        }

        private void LocDuLieu()
        {
            if (_danhSachGoc == null) return;

            string tuKhoa = txtTimKiem.Text?.ToLower() ?? "";
            string trangThai = (cbTrangThai.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Tất cả";
            DateTime? ngay = dpLocNgay.SelectedDate;

            var list = _danhSachGoc.Where(h =>
                (string.IsNullOrEmpty(tuKhoa)
                    || h.TenKhachHang.ToLower().Contains(tuKhoa)
                    || h.SDT.Contains(tuKhoa)
                    || h.MaHD.ToString().Contains(tuKhoa))
                &&
                (trangThai == "Tất cả" || h.TrangThai == trangThai)
                &&
                (ngay == null || h.NgayLap.Date == ngay.Value.Date)
            ).ToList();

            _danhSachHienThi.Clear();

            decimal doanhThu = 0;
            decimal chuaThu = 0;

            foreach (var item in list)
            {
                _danhSachHienThi.Add(item);

                if (item.TrangThai == "Đã thanh toán")
                    doanhThu += item.TongThanhToan;
                else
                    chuaThu += item.TongThanhToan;
            }

            txtTongHoaDon.Text = _danhSachHienThi.Count.ToString();
            txtTongDoanhThu.Text = doanhThu.ToString("N0");
            txtChuaThu.Text = chuaThu.ToString("N0");
        }

        // ================= CHI TIẾT =================
        private void BtnChiTiet_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;

            var hd = btn.DataContext as HoaDonViewModel;
            if (hd == null)
            {
                MessageBox.Show("Không lấy được dữ liệu hóa đơn!");
                return;
            }

            txtTieuDePopup.Text = $"HÓA ĐƠN #{hd.MaHD}";
            txtKhachHangPopup.Text = $"{hd.TenKhachHang} - {hd.SDT}";

            try
            {
                LoadChiTietHoaDon(hd.MaHD);
                DialogChiTiet.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi mở chi tiết:\n" + ex.Message);
            }
        }
        private void LoadChiTietHoaDon(int maHD)
        {
            var dsThuoc = new ObservableCollection<ChiTietThuocViewModel>();
            var dsDichVu = new ObservableCollection<ChiTietDichVuViewModel>();

            using (SqlConnection con = _db.GetConnection())
            {
                con.Open();

                // ===== THUỐC =====
                string sqlThuoc = @"
                    SELECT sp.TenSP, ct.SoLuong, ct.DonGia
                    FROM ChiTietHoaDon ct
                    JOIN SanPham sp ON ct.MaSP = sp.MaSP
                    WHERE ct.MaHD = @MaHD";

                using (SqlCommand cmd = new SqlCommand(sqlThuoc, con))
                {
                    cmd.Parameters.AddWithValue("@MaHD", maHD);

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            int sl = Convert.ToInt32(rd["SoLuong"]);
                            decimal dg = Convert.ToDecimal(rd["DonGia"]);

                            dsThuoc.Add(new ChiTietThuocViewModel
                            {
                                TenSP = rd["TenSP"].ToString(),
                                SoLuong = sl,
                                DonGia = dg,
                                ThanhTien = sl * dg
                            });
                        }
                    }
                }

                // ===== DỊCH VỤ =====
                string sqlDV = @"
                    SELECT dv.TenDV, cd.ThanhTien
                    FROM ChiTietDichVu cd
                    JOIN DichVu dv ON cd.MaDV = dv.MaDV
                    WHERE cd.MaHD = @MaHD";

                using (SqlCommand cmd = new SqlCommand(sqlDV, con))
                {
                    cmd.Parameters.AddWithValue("@MaHD", maHD);

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            dsDichVu.Add(new ChiTietDichVuViewModel
                            {
                                TenDV = rd["TenDV"].ToString(),
                                ThanhTien = Convert.ToDecimal(rd["ThanhTien"])
                            });
                        }
                    }
                }
            }

            dgChiTietThuoc.ItemsSource = dsThuoc;
            dgChiTietDichVu.ItemsSource = dsDichVu;

            dgChiTietDichVu.Visibility =
                dsDichVu.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnDongPopup_Click(object sender, RoutedEventArgs e)
        {
            DialogChiTiet.Visibility = Visibility.Collapsed;
        }
    }

    // ================= MODEL =================
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