using Microsoft.Data.SqlClient;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Manage.QuanLy
{
    public partial class AppointmentView : UserControl
    {
        private string connectionString = @"Data Source=localhost;Initial Catalog=PharmacyManage;Integrated Security=True;TrustServerCertificate=True";
        
        public ObservableCollection<LichHenViewModel> DanhSachGoc { get; set; }
        public ObservableCollection<LichHenViewModel> DanhSachHienThi { get; set; }

        public AppointmentView()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            DanhSachGoc = new ObservableCollection<LichHenViewModel>();
            DanhSachHienThi = new ObservableCollection<LichHenViewModel>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // SÁNG TẠO: Dùng lệnh JOIN để nối bảng LichHen và KhachHang để lấy Tên và SĐT
                    string query = @"
                        SELECT lh.MaLichHen, kh.HoTen, kh.SoDienThoai, 
                               lh.ThoiGianKham, lh.ChuyenKhoa, lh.LyDoKham, lh.TrangThai 
                        FROM LichHen lh
                        INNER JOIN KhachHang kh ON lh.MaKH = kh.MaKH
                        ORDER BY lh.ThoiGianKham DESC"; // Sắp xếp lịch mới nhất lên đầu

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DanhSachGoc.Add(new LichHenViewModel
                                {
                                    MaLichHen = reader.GetInt32(0),
                                    HoTen = reader.GetString(1),
                                    SoDienThoai = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                    ThoiGianKham = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3),
                                    ChuyenKhoa = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                    LyDoKham = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                    TrangThai = reader.IsDBNull(6) ? "Đang chờ" : reader.GetString(6)
                                });
                            }
                        }
                    }
                }
                LocDuLieu(); // Nạp vào danh sách hiển thị
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách lịch hẹn: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Tính năng tìm kiếm theo Tên hoặc Số điện thoại
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LocDuLieu();
        }

        private void LocDuLieu()
        {
            if (DanhSachGoc == null || dgLichHen == null) return;

            string keyword = txtSearch.Text.Trim().ToLower();

            var filtered = DanhSachGoc.Where(lh =>
                string.IsNullOrEmpty(keyword) ||
                lh.HoTen.ToLower().Contains(keyword) ||
                lh.SoDienThoai.Contains(keyword)
            ).ToList();

            DanhSachHienThi.Clear();
            foreach (var item in filtered)
            {
                DanhSachHienThi.Add(item);
            }

            dgLichHen.ItemsSource = DanhSachHienThi;
        }

        // Xử lý khi Admin bấm nút "Xử lý"
        // Xử lý khi Admin bấm nút "Xử lý"
        private void BtnXuLy_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var lich = btn.DataContext as LichHenViewModel;

            if (lich != null)
            {
                if (lich.TrangThai == "Đang chờ")
                {
                    // Hỏi xác nhận trước khi chuyển
                    var result = MessageBox.Show($"Xác nhận tiếp nhận bệnh nhân {lich.HoTen} vào khám?", 
                                                 "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            using (SqlConnection conn = new SqlConnection(connectionString))
                            {
                                conn.Open();
                                // Lệnh cập nhật trạng thái thành Đang khám
                                string query = "UPDATE LichHen SET TrangThai = N'Đang khám' WHERE MaLichHen = @MaLH";
                                using (SqlCommand cmd = new SqlCommand(query, conn))
                                {
                                    cmd.Parameters.AddWithValue("@MaLH", lich.MaLichHen);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            
                            // Load lại danh sách để màn hình cập nhật ngay lập tức
                            LoadData();
                            MessageBox.Show("Đã chuyển trạng thái thành: Đang khám", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Lỗi cập nhật trạng thái: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else if (lich.TrangThai == "Đang khám")
                {
                    // Tương tự, nếu đang khám mà bấm Xử lý thì chuyển thành Hoàn thành
                    var result = MessageBox.Show($"Bệnh nhân {lich.HoTen} đã khám xong?", 
                                                 "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            using (SqlConnection conn = new SqlConnection(connectionString))
                            {
                                conn.Open();
                                string query = "UPDATE LichHen SET TrangThai = N'Hoàn thành' WHERE MaLichHen = @MaLH";
                                using (SqlCommand cmd = new SqlCommand(query, conn))
                                {
                                    cmd.Parameters.AddWithValue("@MaLH", lich.MaLichHen);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            LoadData();
                            MessageBox.Show("Lịch hẹn đã hoàn thành!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Lỗi cập nhật trạng thái: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Lịch hẹn này đã hoàn thành, không thể xử lý thêm.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }

    // Lớp chứa dữ liệu gộp từ 2 bảng (DTO)
    public class LichHenViewModel
    {
        public int MaLichHen { get; set; }
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public DateTime ThoiGianKham { get; set; }
        public string ChuyenKhoa { get; set; }
        public string LyDoKham { get; set; }
        public string TrangThai { get; set; }
    }
}