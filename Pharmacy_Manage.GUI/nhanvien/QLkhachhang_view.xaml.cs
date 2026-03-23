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
        private ObservableCollection<KhachHangViewModel> _danhSachGoc = new ObservableCollection<KhachHangViewModel>();
        private ObservableCollection<KhachHangViewModel> _danhSachHienThi = new ObservableCollection<KhachHangViewModel>();
        private DbConnection _db = new DbConnection();

        public QLkhachhang_view()
        {
            InitializeComponent();
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
                    string query = "SELECT customerID, FullName, Email, Phone, Points, UserName FROM Customers";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int diem = reader["Points"] != DBNull.Value ? Convert.ToInt32(reader["Points"]) : 0;

                                _danhSachGoc.Add(new KhachHangViewModel
                                {
                                    CustomerID = reader["customerID"]?.ToString() ?? "",
                                    FullName = reader["FullName"]?.ToString() ?? "",
                                    Email = reader["Email"]?.ToString() ?? "",
                                    Phone = reader["Phone"]?.ToString() ?? "",
                                    Point = diem,
                                    UserName = reader["UserName"]?.ToString() ?? "",
                                    HangThanhVien = TinhHangThanhVien(diem) 
                                });
                            }
                        }
                    }
                }
                LocDuLieu();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu Khách hàng:\n" + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string TinhHangThanhVien(int point)
        {
            if (point >= 1000) return "Kim Cương";
            if (point >= 500) return "Vàng";
            if (point >= 100) return "Bạc";
            return "Đồng";
        }

        private void txtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            LocDuLieu();
        }

        private void LocDuLieu()
        {
            if (_danhSachGoc == null || dgKhachHang == null) return;
            string tuKhoa = txtTimKiem.Text.Trim().ToLower();

            var ketQua = _danhSachGoc.Where(kh =>
                string.IsNullOrEmpty(tuKhoa) ||
                kh.FullName.ToLower().Contains(tuKhoa) ||
                kh.Phone.Contains(tuKhoa)
            ).ToList();

            _danhSachHienThi.Clear();
            foreach (var item in ketQua)
            {
                _danhSachHienThi.Add(item);
            }
            dgKhachHang.ItemsSource = _danhSachHienThi;
        }

        private void btnThemKhach_Click(object sender, RoutedEventArgs e)
        {
            string tenKH = txtTenKH.Text.Trim();
            string sdt = txtSDT.Text.Trim();

            if (string.IsNullOrEmpty(tenKH) || string.IsNullOrEmpty(sdt))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Họ tên và Số điện thoại!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_danhSachGoc.Any(kh => kh.Phone == sdt))
            {
                MessageBox.Show("Số điện thoại này đã tồn tại trong hệ thống!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string maKHMoi = "CUS" + (_danhSachGoc.Count + 1).ToString("D3");
            _danhSachGoc.Add(new KhachHangViewModel
            {
                CustomerID = maKHMoi,
                FullName = tenKH,
                Phone = sdt,
                Email = "Chưa có",
                Point = 0,
                HangThanhVien = "Đồng"
            });

            MessageBox.Show("Thêm khách hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            txtTenKH.Clear();
            txtSDT.Clear();
            LocDuLieu();
        }

        // ==================== XỬ LÝ NÚT XEM LỊCH SỬ ====================
        private void btnXemLichSu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is KhachHangViewModel kh)
            {
                // Gắn tên khách hàng lên tiêu đề bảng
                txtTenKhachHangLichSu.Text = $"Lịch sử mua hàng: {kh.FullName} ({kh.Phone})";
                
                // Load dữ liệu hóa đơn của khách này
                LoadLichSuHoaDon(kh.CustomerID);
                
                // Hiện bảng Overlay lên
                LichSuOverlay.Visibility = Visibility.Visible;
            }
        }

        private void btnCloseLichSu_Click(object sender, RoutedEventArgs e)
        {
            LichSuOverlay.Visibility = Visibility.Collapsed;
        }

        private void LoadLichSuHoaDon(string customerIdStr)
        {
            var lichSuList = new ObservableCollection<HoaDonViewModel>();
            try
            {
                using (SqlConnection con = _db.GetConnection())
                {
                    con.Open();
                    // Lấy các hóa đơn thuộc về MaKH tương ứng. Sắp xếp hóa đơn mới nhất lên đầu.
                    string query = "SELECT MaHD, NgayLap, TongTienSanPham, TrangThai FROM HoaDon WHERE MaKH = @MaKH ORDER BY NgayLap DESC";
                    
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Kiểm tra parse ID an toàn (Nếu là ID giả lập như "CUS001" thì sẽ lọc = -1 không ra kết quả lỗi)
                        int maKhach;
                        if (int.TryParse(customerIdStr, out maKhach))
                        {
                            cmd.Parameters.AddWithValue("@MaKH", maKhach);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@MaKH", -1); 
                        }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lichSuList.Add(new HoaDonViewModel
                                {
                                    MaHD = reader.GetInt32(0),
                                    NgayLap = reader.IsDBNull(1) ? DateTime.Now : reader.GetDateTime(1),
                                    TongTienSanPham = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                                    TrangThai = reader.IsDBNull(3) ? "" : reader.GetString(3)
                                });
                            }
                        }
                    }
                }
                
                // Đổ dữ liệu vào bảng lịch sử
                dgLichSuHoaDon.ItemsSource = lichSuList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch sử hóa đơn: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // ==================== CÁC LỚP MODEL ====================
    public class KhachHangViewModel
    {
        public string CustomerID { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public int Point { get; set; }
        public string UserName { get; set; } = "";
        public string HangThanhVien { get; set; } = "";
    }

    // Lớp Model mới chứa dữ liệu Lịch sử Hóa đơn
    public class HoaDonViewModel
    {
        public int MaHD { get; set; }
        public DateTime NgayLap { get; set; }
        public decimal TongTienSanPham { get; set; }
        public string TrangThai { get; set; }
    }
}