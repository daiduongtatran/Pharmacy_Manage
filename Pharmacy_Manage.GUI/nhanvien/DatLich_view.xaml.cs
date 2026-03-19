using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Manage.GUI
{
    public partial class DatLich_view : UserControl
    {
        // Danh sách lưu trữ hàng chờ hiện tại
        private ObservableCollection<LichKhamViewModel> _danhSachCho = new ObservableCollection<LichKhamViewModel>();
        private int _soThuTuHienTai = 1;

        public DatLich_view()
        {
            InitializeComponent();
            LoadDuLieuMau();
        }

        private void LoadDuLieuMau()
        {
            // Dữ liệu giả lập ban đầu
            _danhSachCho.Add(new LichKhamViewModel { STT = _soThuTuHienTai++, HoTen = "Nguyễn Văn A", GioiTinhTuoi = "Nam/45", PhongKham = "Phòng Khám Nội 1", TrangThai = "Đang chờ" });
            _danhSachCho.Add(new LichKhamViewModel { STT = _soThuTuHienTai++, HoTen = "Trần Thị B", GioiTinhTuoi = "Nữ/32", PhongKham = "Phòng Răng Hàm Mặt", TrangThai = "Đang chờ" });

            dgDanhSachCho.ItemsSource = _danhSachCho;
        }

        // Sự kiện: Nút Tìm Bệnh Nhân (giả lập)
        private void BtnTimBN_Click(object sender, RoutedEventArgs e)
        {
            if (txtSDT.Text == "0987654321")
            {
                txtHoTen.Text = "Lê Hoàng Khách Cũ";
                cbGioiTinh.SelectedIndex = 0;
                txtNamSinh.Text = "1990";
            }
            else
            {
                MessageBox.Show("Không tìm thấy bệnh nhân. Vui lòng nhập mới!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Sự kiện: Thêm bệnh nhân vào hàng chờ
        private void BtnDatLich_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHoTen.Text) || string.IsNullOrWhiteSpace(txtSDT.Text))
            {
                MessageBox.Show("Vui lòng nhập đủ Họ Tên và Số điện thoại!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string gioiTinh = (cbGioiTinh.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";
            string phongKham = (cbPhongKham.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";

            int tuoi = 0;
            if (int.TryParse(txtNamSinh.Text, out int namSinh))
            {
                tuoi = DateTime.Now.Year - namSinh;
            }

            // Thêm vào danh sách UI
            _danhSachCho.Add(new LichKhamViewModel
            {
                STT = _soThuTuHienTai++,
                HoTen = txtHoTen.Text,
                GioiTinhTuoi = $"{gioiTinh}/{(tuoi > 0 ? tuoi.ToString() : "?")}",
                PhongKham = phongKham,
                TrangThai = "Đang chờ"
            });

            MessageBox.Show($"Đã tiếp nhận bệnh nhân: {txtHoTen.Text}\nSố thứ tự: {_soThuTuHienTai - 1}\nPhòng: {phongKham}", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

            // TODO: Nơi đây bạn có thể gọi BUS để INSERT vào Database SQL

            // Reset Form
            txtHoTen.Clear();
            txtSDT.Clear();
            txtNamSinh.Clear();
            txtLyDo.Clear();
        }

        // Sự kiện: Đổi trạng thái thành Đang Khám
        private void BtnGoiKham_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is LichKhamViewModel bn)
            {
                bn.TrangThai = "Đang khám";
                // Lọc/Refresh lại DataGrid để hiển thị thay đổi (nếu cần thiết)
                dgDanhSachCho.Items.Refresh();
                MessageBox.Show($"Đang gọi bệnh nhân số {bn.STT} vào phòng khám.");
            }
        }

        // Sự kiện: Xóa khỏi hàng chờ
        private void BtnHuyKham_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is LichKhamViewModel bn)
            {
                if (MessageBox.Show("Bạn có chắc chắn muốn hủy lịch khám của bệnh nhân này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _danhSachCho.Remove(bn);
                }
            }
        }
    }

    // Lớp Model cho View này
    public class LichKhamViewModel
    {
        public int STT { get; set; }
        public string HoTen { get; set; }
        public string GioiTinhTuoi { get; set; }
        public string PhongKham { get; set; }
        public string TrangThai { get; set; }
    }
}