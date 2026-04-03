using System;
using System.Collections.Generic;
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

        private DbConnection _db = new DbConnection();

        // 1. Thêm từ điển ánh xạ (COPY Y HỆT TỪ BÊN BOOKINGVIEW)
        private Dictionary<string, List<string>> chuyenKhoaPhongKhamMap;

        public DatLich_view()
        {
            InitializeComponent();
            LoadDuLieuTuDatabase();
            
            // 2. Gọi hàm load phòng khám khi mở màn hình
            LoadPhongKham();
        }

        // ================= THÊM HÀM NÀY =================
        private void LoadPhongKham()
        {
            // Định nghĩa danh sách phòng y hệt bên khách hàng để đồng bộ
            chuyenKhoaPhongKhamMap = new Dictionary<string, List<string>>
            {
                { "Khám nội tổng quát", new List<string> { "PK Nội 1 (Tầng 1)", "PK Nội 2 (Tầng 1)" } },
                { "Khám chuyên khoa Nhi", new List<string> { "PK Nhi 1 (Tầng 2)", "PK Nhi 2 (Tầng 2)" } },
                { "Khám Tai Mũi Họng", new List<string> { "PK TMH 1 (Tầng 3)", "PK TMH 2 (Tầng 3)" } },
                { "Khám Răng Hàm Mặt", new List<string> { "PK Răng Hàm Mặt (Tầng 3)" } },
                { "Khám Da Liễu", new List<string> { "PK Da Liễu (Tầng 4)" } },
                { "Khám Mắt", new List<string> { "PK Mắt (Tầng 4)" } }
            };

            // Gom tất cả các phòng khám thành 1 list phẳng để Lễ tân chọn
            List<string> tatCaPhongKham = new List<string>();
            foreach (var listPhong in chuyenKhoaPhongKhamMap.Values)
            {
                tatCaPhongKham.AddRange(listPhong);
            }

            // Gán vào ComboBox
            popPhongKham.ItemsSource = tatCaPhongKham;
            popPhongKham.SelectedIndex = 0;
        }
        // ================================================

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
                                    MaDV = reader["MaDV"] != DBNull.Value ? reader["MaDV"].ToString() : "",
                                    LoaiKham = reader["LoaiKham"] != DBNull.Value ? reader["LoaiKham"].ToString().Trim() : "Lấy số trực tiếp"
                                };

                                if (lich.LoaiKham.Equals("Đặt lịch trước", StringComparison.OrdinalIgnoreCase))
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
            popPhongKham.SelectedIndex = 0;
        }

        private void BtnLuuLich_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(popHoTen.Text) || string.IsNullOrWhiteSpace(popSDT.Text))
            {
                MessageBox.Show("Vui lòng nhập đủ Họ Tên và Số điện thoại!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection con = _db.GetConnection())
                {
                    con.Open();
                    int maKH = 0;

                    // 1. Tạo hoặc lấy Khách hàng
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

                    if (maKH == 0)
                    {
                        MessageBox.Show("Không thể tạo Khách hàng. Vui lòng kiểm tra lại bảng KhachHang trong SQL.", "Lỗi CSDL", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    

                    // 3. Lưu Lịch hẹn
                    DateTime thoiGianKham = DateTime.Now;
                    string loaiKham = "Lấy số trực tiếp";
                    // =============== SỬA LẠI ĐỂ LẤY TEXT TỪ COMBOBOX ===============
                    string phongKham = popPhongKham.SelectedItem?.ToString() ?? "";
                    // ===============================================================

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

                MessageBox.Show("Đã tiếp nhận bệnh nhân & Tạo Hóa đơn chờ thành công!", "Hoàn tất", MessageBoxButton.OK, MessageBoxImage.Information);
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

                        SqlCommand cmd = new SqlCommand(
                            "UPDATE LichHen SET TrangThai = N'Đang khám' WHERE MaLichHen = @ma",
                            con);
                        cmd.Parameters.AddWithValue("@ma", lh.MaLichHen);
                        cmd.ExecuteNonQuery();

                        int maKH = 0;
                        SqlCommand cmdGetKH = new SqlCommand(
                            "SELECT MaKH FROM LichHen WHERE MaLichHen = @ma",
                            con);
                        cmdGetKH.Parameters.AddWithValue("@ma", lh.MaLichHen);

                        object result = cmdGetKH.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            maKH = Convert.ToInt32(result);
                        }

                        if (maKH == 0)
                        {
                            MessageBox.Show("Không tìm thấy khách hàng!", "Lỗi");
                            return;
                        }
                        string insHoaDon = @"
                    INSERT INTO HoaDon(MaKH, NgayLap, TongTienDichVu, TongTienSanPham, TrangThai) 
                    VALUES(@maKH, GETDATE(), 0, 0, N'Chờ Thanh Toán')";

                        SqlCommand cmdHD = new SqlCommand(insHoaDon, con);
                        cmdHD.Parameters.AddWithValue("@maKH", maKH);
                        cmdHD.ExecuteNonQuery();
                    }

                    LoadDuLieuTuDatabase();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
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
                            SqlCommand cmd = new SqlCommand("UPDATE LichHen SET TrangThai = N'Đã hủy' WHERE MaLichHen = @ma", con);
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
}