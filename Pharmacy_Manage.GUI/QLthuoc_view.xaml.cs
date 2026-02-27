using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using Pharmacy_Manage.DAL; // Khai báo thư viện DAL để dùng DbConnection

namespace Pharmacy_Manage.GUI
{
    public partial class QLthuoc_view : UserControl
    {
        // Danh sách gốc chứa toàn bộ dữ liệu
        private ObservableCollection<ThuocViewModel> _danhSachGoc = new ObservableCollection<ThuocViewModel>();
        // Danh sách hiển thị lên DataGrid (có thể bị lọc)
        private ObservableCollection<ThuocViewModel> _danhSachHienThi = new ObservableCollection<ThuocViewModel>();

        // Khởi tạo đối tượng kết nối DB của bạn
        private DbConnection _db = new DbConnection();

        public QLthuoc_view()
        {
            InitializeComponent();
            LoadDuLieuKhoTuDatabase();
        }

        private void LoadDuLieuKhoTuDatabase()
        {
            _danhSachGoc.Clear();

            try
            {
                using (SqlConnection con = _db.GetConnection())
                {
                    con.Open();

                    // ⚠️ LƯU Ý: Bạn cần sửa tên bảng 'Thuoc' và tên các cột trong chuỗi query dưới đây
                    // sao cho khớp chuẩn xác với thiết kế trong SQL Server của bạn.
                    string query = "Select TenSP, LoaiSP, DonVi, NhaSanXuat, HanDung, GiaBan, TonKho, TrangThai, GhiChu FROM SanPham";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _danhSachGoc.Add(new ThuocViewModel
                                {
                                    TenSP = reader["TenSP"] != DBNull.Value ? reader["TenSP"].ToString() : "",
                                    LoaiSP = reader["LoaiSP"] != DBNull.Value ? reader["LoaiSP"].ToString() : "",
                                    DonVi = reader["DonVi"] != DBNull.Value ? reader["DonVi"].ToString() : "",
                                    NhaSanXuat = reader["NhaSanXuat"] != DBNull.Value ? reader["NhaSanXuat"].ToString() : "",
                                    TonKho = reader["TonKho"] != DBNull.Value ? Convert.ToInt32(reader["TonKho"]) : 0,
                                   GiaBan = reader["GiaBan"] != DBNull.Value ? Convert.ToDecimal(reader["GiaBan"]) : 0,
                                    HanDung = reader["HanDung"] != DBNull.Value ? Convert.ToDateTime(reader["HanDung"]) : DateTime.MinValue,
                                    TrangThai = reader["TrangThai"] != DBNull.Value ? reader["TrangThai"].ToString() : "Bình thường"
                                });
                            }
                        }
                    }
                }

                // Gọi hàm lọc để đổ dữ liệu từ _danhSachGoc sang DataGrid
                LocDuLieu();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu từ SQL Server:\n" + ex.Message, "Lỗi Database", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Xử lý sự kiện khi gõ vào ô tìm kiếm
        private void txtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            LocDuLieu();
        }

        // Xử lý sự kiện khi chọn Combo box lọc trạng thái
        private void cbBoLoc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LocDuLieu();
        }

        // Hàm gộp chung cả Tìm kiếm Text và Lọc ComboBox
        private void LocDuLieu()
        {
            if (_danhSachGoc == null || dgKhoThuoc == null) return;

            string tuKhoa = txtTimKiem?.Text.ToLower() ?? "";
            string trangThaiLoc = "Tất cả";

            if (cbBoLoc?.SelectedItem is ComboBoxItem selectedItem)
            {
                trangThaiLoc = selectedItem.Content.ToString();
            }

            // Lọc dữ liệu qua LINQ
            var ketQua = _danhSachGoc.Where(t =>
                (string.IsNullOrEmpty(tuKhoa) || t.TenSP.ToLower().Contains(tuKhoa)) &&
                (trangThaiLoc == "Tất cả" || t.TrangThai == trangThaiLoc)
            ).ToList();

            // Cập nhật lại giao diện
            _danhSachHienThi.Clear();
            foreach (var item in ketQua)
            {
                _danhSachHienThi.Add(item);
            }

            dgKhoThuoc.ItemsSource = _danhSachHienThi;
        }

        // Nút báo cáo thiếu hàng
        private void btnBaoThieuHang_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đã gửi yêu cầu nhập thêm hàng đến Quản lý (Admin).", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // Lớp Model cho Thuốc (Đảm bảo Property khớp với Binding trong XAML)
    // Lớp Model cho Thuốc (Phải khớp với cơ sở dữ liệu)
    public class ThuocViewModel
    {
        public string TenSP { get; set; } = "";
        public string LoaiSP { get; set; } = "";
        public string DonVi { get; set; } = "";
        public string NhaSanXuat { get; set; } = "";
        public int TonKho { get; set; }
        public decimal GiaBan { get; set; }
        public DateTime HanDung { get; set; }
        public string TrangThai { get; set; } = "";
        public string GhiChu { get; set; } = "";
    }
}