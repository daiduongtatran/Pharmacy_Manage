using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using Pharmacy_Manage.DAL; // Để gọi DbConnection

namespace Pharmacy_Manage.GUI
{
    public partial class QLkhachhang_view : UserControl
    {
        // Danh sách gốc chứa dữ liệu từ SQL
        private ObservableCollection<KhachHangViewModel> _danhSachGoc = new ObservableCollection<KhachHangViewModel>();
        // Danh sách hiển thị trên DataGrid (thay đổi khi tìm kiếm)
        private ObservableCollection<KhachHangViewModel> _danhSachHienThi = new ObservableCollection<KhachHangViewModel>();

        // Khởi tạo đối tượng kết nối Database từ DAL
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

                    // Câu lệnh SQL truy vấn trực tiếp bảng Khách hàng
                    // ⚠️ Đảm bảo tên bảng 'Customers' là tên đúng trong SQL của bạn
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
                                    HangThanhVien = TinhHangThanhVien(diem) // Tính hạng thành viên tự động
                                });
                            }
                        }
                    }
                }

                // Cập nhật lên màn hình
                LocDuLieu();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu Khách hàng từ Database:\n" + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Logic chia hạng thành viên dựa trên điểm
        private string TinhHangThanhVien(int point)
        {
            if (point >= 1000) return "Kim Cương";
            if (point >= 500) return "Vàng";
            if (point >= 100) return "Bạc";
            return "Đồng";
        }

        // Sự kiện khi gõ chữ vào ô tìm kiếm (Tìm realtime)
        private void txtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            LocDuLieu();
        }

        private void LocDuLieu()
        {
            if (_danhSachGoc == null || dgKhachHang == null) return;

            string tuKhoa = txtTimKiem.Text.Trim().ToLower();

            // Lọc danh sách (Tìm theo Tên hoặc SĐT)
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

        // Xử lý nút Thêm Khách Hàng (Tạm thời hiển thị lên màn hình, bạn có thể gọi câu lệnh INSERT SQL tại đây)
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

            // TODO: Viết câu lệnh INSERT INTO Customers(...) VALUES(...) vào CSDL ở đây

            // Thêm giả lập vào giao diện (Cho đến khi bạn viết lệnh INSERT)
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

        // Nút Xem lịch sử mua hàng
        private void btnXemLichSu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is KhachHangViewModel kh)
            {
                MessageBox.Show($"Lịch sử mua hàng của khách hàng: {kh.FullName} ({kh.Phone})\n\nTính năng tra cứu hóa đơn cũ đang được bảo trì.", "Lịch sử mua hàng", MessageBoxButton.OK, MessageBoxImage.Information);
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