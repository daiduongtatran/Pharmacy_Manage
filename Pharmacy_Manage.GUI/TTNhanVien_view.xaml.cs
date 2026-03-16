using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using Pharmacy_Manage.DAL; // Gọi DbConnection

namespace Pharmacy_Manage.GUI
{
    public partial class TTNhanVien_view : UserControl
    {
        private ObservableCollection<NhanVienViewModel> _danhSachGoc = new ObservableCollection<NhanVienViewModel>();
        private ObservableCollection<NhanVienViewModel> _danhSachHienThi = new ObservableCollection<NhanVienViewModel>();

        private DbConnection _db = new DbConnection();

        public TTNhanVien_view()
        {
            InitializeComponent();
            LoadDuLieuNhanVien();
        }

        private void LoadDuLieuNhanVien()
        {
            _danhSachGoc.Clear();

            try
            {
                using (SqlConnection con = _db.GetConnection())
                {
                    con.Open();

                    // Lấy các cột đúng như yêu cầu từ bảng Employees
                    string query = "SELECT EmployeeID, FullName, Email, Phone, Username FROM Employees";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _danhSachGoc.Add(new NhanVienViewModel
                                {
                                    EmployeeID = reader["EmployeeID"] != DBNull.Value ? reader["EmployeeID"].ToString() : "",
                                    FullName = reader["FullName"] != DBNull.Value ? reader["FullName"].ToString() : "",
                                    Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : "",
                                    Phone = reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : "",
                                    Username = reader["Username"] != DBNull.Value ? reader["Username"].ToString() : ""
                                });
                            }
                        }
                    }
                }
                LocDuLieu(); // Đổ dữ liệu ra DataGrid
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu Nhân viên:\n" + ex.Message, "Lỗi Database", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            LocDuLieu();
        }

        private void LocDuLieu()
        {
            if (_danhSachGoc == null || dgNhanVien == null) return;

            string tuKhoa = txtTimKiem.Text.Trim().ToLower();

            // Tìm kiếm theo Tên hoặc Số điện thoại
            var ketQua = _danhSachGoc.Where(nv =>
                string.IsNullOrEmpty(tuKhoa) ||
                nv.FullName.ToLower().Contains(tuKhoa) ||
                nv.Phone.Contains(tuKhoa)
            ).ToList();

            _danhSachHienThi.Clear();
            foreach (var item in ketQua)
            {
                _danhSachHienThi.Add(item);
            }

            dgNhanVien.ItemsSource = _danhSachHienThi;
        }

        private void btnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            txtTimKiem.Clear();
            LoadDuLieuNhanVien(); // Quét lại cơ sở dữ liệu
        }
    }

    // Lớp Model chứa đúng các trường bạn yêu cầu
    public class NhanVienViewModel
    {
        public string EmployeeID { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Username { get; set; } = "";
    }
} 