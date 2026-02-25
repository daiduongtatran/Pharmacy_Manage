using System;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Pharmacy_Manage.GUI
{
    public partial class ProductDialog : Window
    {
        // Các thuộc tính public để AdminWindow có thể lấy dữ liệu ra sau khi nhập xong
        public int MaSP { get; set; } = 0;
        public string TenSP { get; set; } = "";
        public int TonKho { get; set; } = 0;
        public decimal GiaBan { get; set; } = 0;
        public string DonVi { get; set; } = "";
        public DateTime HanDung { get; set; } = DateTime.Now;
        public DateTime NgayNhap { get; set; } = DateTime.Now;

        // Constructor 1: Dùng cho nút THÊM MỚI
        public ProductDialog()
        {
            InitializeComponent();
            txtTitle.Text = "THÊM THUỐC MỚI";
            dpHanDung.SelectedDate = DateTime.Now.AddYears(1); // Mặc định HSD 1 năm
            dpNgayNhap.SelectedDate = DateTime.Now;
        }

        // Constructor 2: Dùng cho nút SỬA (Đổ dữ liệu cũ lên)
        public ProductDialog(DataRowView row)
        {
            InitializeComponent();
            txtTitle.Text = "SỬA THÔNG TIN THUỐC";

            // Lấy dữ liệu từ dòng được chọn và gán lên các ô nhập
            MaSP = Convert.ToInt32(row["MaSP"]);
            txtTenSP.Text = row["TenSP"].ToString();
            txtTonKho.Text = row["TonKho"].ToString();

            // Xử lý các cột có thể null hoặc không có sẵn trong DB để tránh lỗi
            if (row.DataView.Table.Columns.Contains("DonVi")) txtDonVi.Text = row["DonVi"].ToString();
            if (row.DataView.Table.Columns.Contains("GiaBan")) txtGiaBan.Text = row["GiaBan"].ToString();

            dpHanDung.SelectedDate = Convert.ToDateTime(row["HanDung"]);
            dpNgayNhap.SelectedDate = Convert.ToDateTime(row["NgayNhap"]);
        }

        // Xử lý nút LƯU
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra dữ liệu trống
            if (string.IsNullOrWhiteSpace(txtTenSP.Text) || string.IsNullOrWhiteSpace(txtTonKho.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên thuốc và Số lượng tồn!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Kiểm tra số lượng phải là số
            if (!int.TryParse(txtTonKho.Text, out int ton))
            {
                MessageBox.Show("Số lượng tồn phải là số nguyên hợp lệ!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Gán dữ liệu vào các Properties để AdminWindow lấy
            TenSP = txtTenSP.Text.Trim();
            TonKho = ton;
            DonVi = txtDonVi.Text.Trim();
            decimal.TryParse(txtGiaBan.Text, out decimal gia);
            GiaBan = gia;
            HanDung = dpHanDung.SelectedDate ?? DateTime.Now;
            NgayNhap = dpNgayNhap.SelectedDate ?? DateTime.Now;

            // Đóng Dialog và trả về kết quả Thành công (True)
            this.DialogResult = true;
            this.Close();
        }

        // Xử lý nút HỦY (hoặc X)
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // Cho phép kéo thả bảng khi nắm vào phần Header
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}