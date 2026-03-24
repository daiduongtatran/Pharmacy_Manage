using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using Pharmacy_Manage.DAL;

namespace Pharmacy_Manage.GUI
{
    public partial class DatLich_view : UserControl
    {
        private ObservableCollection<LichHenViewModel> _danhSachDatTruoc = new ObservableCollection<LichHenViewModel>();
        private ObservableCollection<LichHenViewModel> _danhSachLaySo = new ObservableCollection<LichHenViewModel>();

        private ObservableCollection<DichVuModel> _danhSachDichVu = new ObservableCollection<DichVuModel>();

        private DbConnection _db = new DbConnection();

        public DatLich_view()
        {
            InitializeComponent();
            popNgayHen.SelectedDate = DateTime.Now.Date;
            LoadDuLieuTuDatabase();
            LoadDanhSachDichVu();
        }

        // ================= TẢI DANH SÁCH DỊCH VỤ (ĐÃ SỬA CỘT 'Gia' VÀ KIỂU STRING) =================
        private void LoadDanhSachDichVu()
        {
            _danhSachDichVu.Clear();
            try
            {
                using (SqlConnection con = _db.GetConnection())
                {
                    con.Open();
                    // Sửa lại cho đúng tên cột Gia trong Database của bạn
                    string query = "SELECT MaDV, TenDV, Gia FROM DichVu";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _danhSachDichVu.Add(new DichVuModel
                            {
                                // Sửa lại thành ToString() thay vì Convert.ToInt32()
                                MaDV = reader["MaDV"].ToString(),
                                TenDV = reader["TenDV"].ToString(),
                                GiaTien = Convert.ToDecimal(reader["Gia"]),
                                IsSelected = false
                            });
                        }
                    }
                }
                if (lbDichVu != null)
                    lbDichVu.ItemsSource = _danhSachDichVu;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải Dịch vụ: " + ex.Message); }
        }

        private void LoadDuLieuTuDatabase()
        {
            _danhSachDatTruoc.Clear();
            _danhSachLaySo.Clear();

            try
            {
                using (SqlConnection con = _db.GetConnection())
                {
                    con.Open();
                    string query = @"
                        SELECT lh.MaLichHen, lh.MaKH, c.HoTen AS HoTen, c.SoDienThoai AS SDT, 
                               lh.ThoiGianKham, lh.ChuyenKhoa, lh.PhongKham, lh.LyDoKham, lh.TrangThai, lh.MaDV,
                               lh.LoaiKham 
                        FROM LichHen lh
                        LEFT JOIN KhachHang c ON lh.MaKH = c.MaKH
                        WHERE lh.TrangThai = N'Đang chờ'
                        ORDER BY lh.ThoiGianKham ASC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var lich = new LichHenViewModel
                                {
                                    MaLichHen = reader["MaLichHen"] != DBNull.Value ? Convert.ToInt32(reader["MaLichHen"]) : 0,
                                    MaKH = reader["MaKH"] != DBNull.Value ? Convert.ToInt32(reader["MaKH"]) : 0,
                                    HoTen = reader["HoTen"] != DBNull.Value ? reader["HoTen"].ToString() : "Khách vãng lai",
                                    SDT = reader["SDT"] != DBNull.Value ? reader["SDT"].ToString() : "",
                                    ThoiGianKham = reader["ThoiGianKham"] != DBNull.Value ? Convert.ToDateTime(reader["ThoiGianKham"]) : DateTime.MinValue,
                                    ChuyenKhoa = reader["ChuyenKhoa"] != DBNull.Value ? reader["ChuyenKhoa"].ToString() : "",
                                    PhongKham = reader["PhongKham"] != DBNull.Value ? reader["PhongKham"].ToString() : "",
                                    LyDo = reader["LyDoKham"] != DBNull.Value ? reader["LyDoKham"].ToString() : "",
                                    TrangThai = reader["TrangThai"] != DBNull.Value ? reader["TrangThai"].ToString() : "Đang chờ",

                                    // Sửa lại ép kiểu thành chuỗi (String) cho an toàn
                                    MaDV = reader["MaDV"] != DBNull.Value ? reader["MaDV"].ToString() : "",

                                    LoaiKham = reader["LoaiKham"] != DBNull.Value ? reader["LoaiKham"].ToString() : "Lấy số trực tiếp"
                                };

                                if (lich.LoaiKham == "Đặt lịch trước")
                                    _danhSachDatTruoc.Add(lich);
                                else
                                    _danhSachLaySo.Add(lich);
                            }
                        }
                    }
                }

                dgDatTruoc.ItemsSource = _danhSachDatTruoc;
                dgLaySo.ItemsSource = _danhSachLaySo;
                CapNhatThongKe();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu:\n" + ex.Message, "Lỗi Database", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CapNhatThongKe()
        {
            txtTongLich.Text = (_danhSachDatTruoc.Count + _danhSachLaySo.Count).ToString();
            int dangCho = _danhSachDatTruoc.Count(x => x.TrangThai == "Đang chờ") + _danhSachLaySo.Count(x => x.TrangThai == "Đang chờ");
            txtChoKham.Text = dangCho.ToString();
        }

        private void popLoaiKham_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (panelThoiGianHen == null) return;
            if (popLoaiKham.SelectedIndex == 1)
                panelThoiGianHen.Visibility = Visibility.Visible;
            else
                panelThoiGianHen.Visibility = Visibility.Collapsed;
        }

        private void BtnMoFormDatLich_Click(object sender, RoutedEventArgs e)
        {
            DialogOverlay.Visibility = Visibility.Visible;
            MainContent.IsEnabled = false;
        }

        private void BtnDongForm_Click(object sender, RoutedEventArgs e)
        {
            DialogOverlay.Visibility = Visibility.Collapsed;
            MainContent.IsEnabled = true;
            ClearForm();
        }

        private void ClearForm()
        {
            popHoTen.Clear();
            popSDT.Clear();
            popLyDo.Clear();
            popLoaiKham.SelectedIndex = 0;
            popPhongKham.SelectedIndex = 0;
            popNgayHen.SelectedDate = DateTime.Now.Date;
            popGioHen.SelectedIndex = 1;

            foreach (var item in _danhSachDichVu)
                item.IsSelected = false;

            if (lbDichVu != null)
                lbDichVu.Items.Refresh();
        }

        private void BtnLuuLich_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(popHoTen.Text) || string.IsNullOrWhiteSpace(popSDT.Text))
            {
                MessageBox.Show("Vui lòng nhập đủ Họ Tên và Số điện thoại!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dichVuDaChon = _danhSachDichVu.Where(x => x.IsSelected).ToList();
            if (dichVuDaChon.Count == 0)
            {
                MessageBox.Show("Vui lòng tick chọn ít nhất 1 Dịch vụ khám bệnh!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection con = _db.GetConnection())
                {
                    con.Open();
                    int maKH = 0;
                    decimal tongTienDichVu = dichVuDaChon.Sum(x => x.GiaTien);

                    // A. TẠO HOẶC LẤY MÃ KHÁCH HÀNG
                    string checkKH = "SELECT MaKH FROM KhachHang WHERE SoDienThoai = @phone";
                    using (SqlCommand cmdCheck = new SqlCommand(checkKH, con))
                    {
                        cmdCheck.Parameters.AddWithValue("@phone", popSDT.Text.Trim());
                        object result = cmdCheck.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            maKH = Convert.ToInt32(result);
                        }
                        else
                        {
                            string insKH = "INSERT INTO KhachHang(HoTen, SoDienThoai) VALUES(@ten, @sdt); SELECT SCOPE_IDENTITY();";
                            using (SqlCommand cmdIns = new SqlCommand(insKH, con))
                            {
                                cmdIns.Parameters.AddWithValue("@ten", popHoTen.Text.Trim());
                                cmdIns.Parameters.AddWithValue("@sdt", popSDT.Text.Trim());
                                object newKhId = cmdIns.ExecuteScalar();
                                if (newKhId != null && newKhId != DBNull.Value)
                                    maKH = Convert.ToInt32(newKhId);
                            }
                        }
                    }

                    // CHỐT CHẶN: Nếu không lấy được mã khách hàng thì báo lỗi ngay, tránh lỗi FOREIGN KEY
                    if (maKH == 0)
                    {
                        MessageBox.Show("Không thể tạo Khách hàng. Vui lòng kiểm tra lại bảng KhachHang trong SQL (Cột MaKH phải bật Identity = Yes).", "Lỗi CSDL", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // B. TẠO HÓA ĐƠN TREO (Đã xóa cột TongThanhToan vì SQL sẽ tự tính)
                    int maHD = 0;
                    string insHoaDon = @"INSERT INTO HoaDon(MaKH, NgayLap, TongTienDichVu, TongTienSanPham, TrangThai) 
                                         VALUES(@maKH, GETDATE(), @tienDV, 0, N'Chờ thanh toán'); 
                                         SELECT SCOPE_IDENTITY();";
                    using (SqlCommand cmdHD = new SqlCommand(insHoaDon, con))
                    {
                        cmdHD.Parameters.AddWithValue("@maKH", maKH);
                        cmdHD.Parameters.AddWithValue("@tienDV", tongTienDichVu);
                        object hdResult = cmdHD.ExecuteScalar();
                        if (hdResult != null && hdResult != DBNull.Value)
                            maHD = Convert.ToInt32(hdResult);
                    }

                    // C. LƯU CHI TIẾT DỊCH VỤ 
                    foreach (var dv in dichVuDaChon)
                    {
                        string insChiTiet = "INSERT INTO ChiTietDichVu(MaHD, MaDV, ThanhTien) VALUES(@maHD, @maDV, @tien)";
                        using (SqlCommand cmdCT = new SqlCommand(insChiTiet, con))
                        {
                            cmdCT.Parameters.AddWithValue("@maHD", maHD);
                            cmdCT.Parameters.AddWithValue("@maDV", dv.MaDV);
                            cmdCT.Parameters.AddWithValue("@tien", dv.GiaTien);
                            cmdCT.ExecuteNonQuery();
                        }
                    }

                    // D. LƯU LỊCH HẸN
                    DateTime thoiGianKham;
                    string loaiKham = (popLoaiKham.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Lấy số trực tiếp";

                    if (loaiKham == "Đặt lịch trước")
                    {
                        thoiGianKham = popNgayHen.SelectedDate ?? DateTime.Now.Date;
                        string timeStr = (popGioHen.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "08:00 AM";
                        if (DateTime.TryParse(timeStr, out DateTime parsedTime))
                        {
                            thoiGianKham = thoiGianKham.Date.Add(parsedTime.TimeOfDay);
                        }
                    }
                    else
                    {
                        thoiGianKham = DateTime.Now;
                    }

                    string phongKham = (popPhongKham.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";

                    string insLich = @"INSERT INTO LichHen(MaKH, ThoiGianKham, PhongKham, LyDoKham, TrangThai, LoaiKham) 
                                       VALUES(@maKH, @tg, @phong, @lydo, N'Đang chờ', @loai)";
                    using (SqlCommand cmdLich = new SqlCommand(insLich, con))
                    {
                        cmdLich.Parameters.AddWithValue("@maKH", maKH);
                        cmdLich.Parameters.AddWithValue("@tg", thoiGianKham);
                        cmdLich.Parameters.AddWithValue("@phong", phongKham);
                        cmdLich.Parameters.AddWithValue("@lydo", popLyDo.Text.Trim());
                        cmdLich.Parameters.AddWithValue("@loai", loaiKham);
                        cmdLich.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Đã chỉ định Dịch vụ & Tạo Hóa đơn chờ thành công!", "Hoàn tất", MessageBoxButton.OK, MessageBoxImage.Information);
                BtnDongForm_Click(null, null);
                LoadDuLieuTuDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi SQL: " + ex.Message, "Lỗi Server", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGoiKham_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is LichHenViewModel lh)
            {
                try
                {
                    using (SqlConnection con = _db.GetConnection())
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand("UPDATE LichHen SET TrangThai = N'Đang khám' WHERE MaLichHen = @ma", con);
                        cmd.Parameters.AddWithValue("@ma", lh.MaLichHen);
                        cmd.ExecuteNonQuery();
                    }
                    LoadDuLieuTuDatabase();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            }
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is LichHenViewModel lh)
            {
                if (MessageBox.Show($"Bạn có chắc muốn hủy lịch của {lh.HoTen}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (SqlConnection con = _db.GetConnection())
                        {
                            con.Open();
                            SqlCommand cmd = new SqlCommand("DELETE FROM LichHen WHERE MaLichHen = @ma", con);
                            cmd.Parameters.AddWithValue("@ma", lh.MaLichHen);
                            cmd.ExecuteNonQuery();
                        }
                        LoadDuLieuTuDatabase();
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
                }
            }
        }
    }

    // ================= MODEL DỮ LIỆU BINDING =================
    public class LichHenViewModel
    {
        public int MaLichHen { get; set; }
        public int MaKH { get; set; }
        public DateTime ThoiGianKham { get; set; }

        public string ThoiGianHienThi
        {
            get
            {
                if (ThoiGianKham == DateTime.MinValue) return "";
                if (LoaiKham == "Đặt lịch trước")
                    return ThoiGianKham.ToString("dd/MM/yyyy HH:mm");
                else
                    return ThoiGianKham.ToString("HH:mm");
            }
        }

        public string HoTen { get; set; }
        public string SDT { get; set; }
        public string ChuyenKhoa { get; set; }
        public string PhongKham { get; set; }
        public string LyDo { get; set; }
        public string TrangThai { get; set; }
        public string MaDV { get; set; }
        public string LoaiKham { get; set; }
    }

    public class DichVuModel
    {
        // Đổi MaDV thành kiểu string
        public string MaDV { get; set; }
        public string TenDV { get; set; }
        public decimal GiaTien { get; set; }
        public bool IsSelected { get; set; }

        public string TenHienThi => $"{TenDV} ({GiaTien:N0} VNĐ)";
    }
}