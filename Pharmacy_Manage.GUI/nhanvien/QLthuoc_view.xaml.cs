using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using Pharmacy_Manage.DAL;

namespace Pharmacy_Manage.GUI
{
    public partial class QLthuoc_view : UserControl
    {
        private ObservableCollection<ThuocViewModel> _danhSachGoc = new ObservableCollection<ThuocViewModel>();
        private ObservableCollection<ThuocViewModel> _danhSachHienThi = new ObservableCollection<ThuocViewModel>();

        private DbConnection _db = new DbConnection();
        private string _trangThaiLoc = "Tất cả"; // Biến lưu trạng thái của Filter Chips

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
                    // Đã thêm MaSP vào truy vấn SQL
                    string query = "SELECT MaSP, TenSP, LoaiSP, DonVi, NhaSanXuat, HanDung, GiaBan, TonKho, TrangThai, GhiChu FROM SanPham";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _danhSachGoc.Add(new ThuocViewModel
                                {
                                    MaSP = reader["MaSP"] != DBNull.Value ? Convert.ToInt32(reader["MaSP"]) : 0,
                                    TenSP = reader["TenSP"] != DBNull.Value ? reader["TenSP"].ToString() : "",
                                    LoaiSP = reader["LoaiSP"] != DBNull.Value ? reader["LoaiSP"].ToString() : "",
                                    DonVi = reader["DonVi"] != DBNull.Value ? reader["DonVi"].ToString() : "",
                                    NhaSanXuat = reader["NhaSanXuat"] != DBNull.Value ? reader["NhaSanXuat"].ToString() : "",
                                    TonKho = reader["TonKho"] != DBNull.Value ? Convert.ToInt32(reader["TonKho"]) : 0,
                                    GiaBan = reader["GiaBan"] != DBNull.Value ? Convert.ToDecimal(reader["GiaBan"]) : 0,
                                    HanDung = reader["HanDung"] != DBNull.Value ? Convert.ToDateTime(reader["HanDung"]) : DateTime.MinValue,
                                    TrangThai = reader["TrangThai"] != DBNull.Value ? reader["TrangThai"].ToString() : "Đang bán"
                                });
                            }
                        }
                    }
                }
                LocDuLieu();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu từ SQL Server:\n" + ex.Message, "Lỗi Database", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Sự kiện khi gõ TextBox hoặc đổi ComboBox
        // Sự kiện khi gõ TextBox hoặc đổi ComboBox
        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            LocDuLieu();
        }

        // Sự kiện khi bấm vào Filter Chips (Radio Button)
        private void Filter_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag != null)
            {
                _trangThaiLoc = rb.Tag.ToString();
                LocDuLieu();
            }
        }

        private void LocDuLieu()
        {
            if (_danhSachGoc == null || dgKhoThuoc == null || cbLoaiSP == null || cbDVT == null || cbSapXep == null) return;

            // 1. Đọc dữ liệu từ Form Lọc
            string tuKhoa = txtTimKiem.Text.Trim().ToLower();
            string loaiSP = (cbLoaiSP.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Tất cả";
            string dvt = (cbDVT.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Tất cả";
            string sapXep = (cbSapXep.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Mặc định";

            // Danh sách các loại sản phẩm mặc định trong ComboBox
            string[] cacLoaiMacDinh = { "Giảm đau – hạ sốt", "Kháng sinh", "Tim mạch", "Thực phẩm chức năng" };

            // 2. Lọc Từ khóa, Trạng thái, và ĐVT trước
            var ketQua = _danhSachGoc.Where(t =>
                (string.IsNullOrEmpty(tuKhoa) || t.TenSP.ToLower().Contains(tuKhoa) || t.MaSP.ToString().Contains(tuKhoa) || t.NhaSanXuat.ToLower().Contains(tuKhoa)) &&
                (_trangThaiLoc == "Tất cả" || t.TrangThai == _trangThaiLoc) &&
                (dvt == "Tất cả" || t.DonVi == dvt)
            ).ToList();

            // 3. Xử lý bộ lọc Loại SP (Thêm logic cho nút "Khác")
            if (loaiSP == "Khác")
            {
                // Nếu chọn "Khác": Lấy những thuốc mà Loại SP KHÔNG chứa bất kỳ từ khóa nào trong danh sách mặc định
                ketQua = ketQua.Where(t => !cacLoaiMacDinh.Any(loai => t.LoaiSP.Contains(loai))).ToList();
            }
            else if (loaiSP != "Tất cả")
            {
                // Lọc bình thường cho các loại còn lại
                ketQua = ketQua.Where(t => t.LoaiSP.Contains(loaiSP)).ToList();
            }

            // 4. Sắp xếp hiển thị
            switch (sapXep)
            {
                case "Giá bán: Cao -> Thấp":
                    ketQua = ketQua.OrderByDescending(t => t.GiaBan).ToList();
                    break;
                case "Giá bán: Thấp -> Cao":
                    ketQua = ketQua.OrderBy(t => t.GiaBan).ToList();
                    break;
                case "Tồn kho: Nhiều -> Ít":
                    ketQua = ketQua.OrderByDescending(t => t.TonKho).ToList();
                    break;
                case "Tồn kho: Ít -> Nhiều":
                    ketQua = ketQua.OrderBy(t => t.TonKho).ToList();
                    break;
                case "Hạn dùng: Gần nhất":
                    ketQua = ketQua.OrderBy(t => t.HanDung).ToList();
                    break;
                default:
                    ketQua = ketQua.OrderBy(t => t.MaSP).ToList(); // Sắp xếp theo Mã SP mặc định
                    break;
            }

            // 5. Cập nhật Bảng (DataGrid)
            _danhSachHienThi.Clear();
            foreach (var item in ketQua)
            {
                _danhSachHienThi.Add(item);
            }

            dgKhoThuoc.ItemsSource = _danhSachHienThi;
        }


        private void btnBaoThieuHang_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đã gửi yêu cầu nhập thêm hàng đến Quản lý (Admin).", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

    // Model Dữ Liệu (Đã thêm MaSP)
    public class ThuocViewModel
    {
        public int MaSP { get; set; }
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
