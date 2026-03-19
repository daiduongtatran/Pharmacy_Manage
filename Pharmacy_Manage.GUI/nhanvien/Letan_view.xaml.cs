using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Manage.GUI
{
    public partial class Letan_view : UserControl
    {
        private ObservableCollection<BenhNhanLeTan> _danhSachGoc = new ObservableCollection<BenhNhanLeTan>();
        private ObservableCollection<BenhNhanLeTan> _danhSachHienThi = new ObservableCollection<BenhNhanLeTan>();

        public Letan_view()
        {
            InitializeComponent();
            LoadDuLieuMau();
        }

        private void LoadDuLieuMau()
        {
            _danhSachGoc.Add(new BenhNhanLeTan { STT = "01", HoTen = "Nguyễn Hữu A", SDT = "0901234567", LyDoKham = "Đau dạ dày", PhongKham = "Phòng Khám Nội 1", TrangThai = "Đang chờ" });
            _danhSachGoc.Add(new BenhNhanLeTan { STT = "02", HoTen = "Trần Thị B", SDT = "0987654321", LyDoKham = "Nhổ răng khôn", PhongKham = "Phòng Răng Hàm Mặt", TrangThai = "Đang khám" });
            _danhSachGoc.Add(new BenhNhanLeTan { STT = "03", HoTen = "Lê Hoàng C", SDT = "0911223344", LyDoKham = "Khám tổng quát", PhongKham = "Phòng Khám Nội 2", TrangThai = "Chờ thanh toán" });
            _danhSachGoc.Add(new BenhNhanLeTan { STT = "04", HoTen = "Phạm D", SDT = "0933445566", LyDoKham = "Tái khám viêm họng", PhongKham = "Phòng Tai Mũi Họng", TrangThai = "Hoàn thành" });
            _danhSachGoc.Add(new BenhNhanLeTan { STT = "05", HoTen = "Hoàng Tuấn E", SDT = "0977889900", LyDoKham = "Đau bụng dữ dội", PhongKham = "Phòng Khám Nội 1", TrangThai = "Đang chờ" });

            CapNhatThongKe();
            LocDuLieu();
        }

        // Cập nhật các con số trên Dashboard
        private void CapNhatThongKe()
        {
            txtTongBN.Text = _danhSachGoc.Count.ToString();
            txtDangCho.Text = _danhSachGoc.Count(x => x.TrangThai == "Đang chờ").ToString();
            txtChoThanhToan.Text = _danhSachGoc.Count(x => x.TrangThai == "Chờ thanh toán").ToString();
            txtHoanThanh.Text = _danhSachGoc.Count(x => x.TrangThai == "Hoàn thành").ToString();
        }

        // Lọc dữ liệu theo thanh tìm kiếm
        private void txtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            LocDuLieu();
        }

        private void LocDuLieu()
        {
            if (_danhSachGoc == null) return;

            string keyword = txtTimKiem.Text.Trim().ToLower();
            var result = _danhSachGoc.Where(bn =>
                string.IsNullOrEmpty(keyword) ||
                bn.HoTen.ToLower().Contains(keyword) ||
                bn.SDT.Contains(keyword)
            ).ToList();

            _danhSachHienThi.Clear();
            foreach (var item in result) _danhSachHienThi.Add(item);

            dgLeTan.ItemsSource = _danhSachHienThi;
        }

        // Nút Tiếp nhận bệnh nhân chuyển sang màn hình Đặt Lịch (Tab 6)
        private void BtnTiepDon_Click(object sender, RoutedEventArgs e)
        {
            // Tìm TabControl cha (MainTabControl trong StaffWindow) để chuyển tab
            var mainWindow = Window.GetWindow(this) as StaffWindow;
            if (mainWindow != null)
            {
                mainWindow.MainTabControl.SelectedIndex = 6; // Chuyển sang Tab "Tạo lịch mới"
            }
        }

        // Nút Thu tiền (Chỉ hiện khi trạng thái là "Chờ thanh toán")
        private void BtnThuTien_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is BenhNhanLeTan bn)
            {
                MessageBoxResult result = MessageBox.Show($"Xác nhận thanh toán viện phí cho bệnh nhân: {bn.HoTen}?", "Thanh toán", MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    bn.TrangThai = "Hoàn thành";
                    dgLeTan.Items.Refresh(); // Cập nhật lại UI DataGrid
                    CapNhatThongKe(); // Cập nhật lại số lượng trên Dashboard
                }
            }
        }
    }

    // Model Dữ liệu
    public class BenhNhanLeTan
    {
        public string STT { get; set; }
        public string HoTen { get; set; }
        public string SDT { get; set; }
        public string LyDoKham { get; set; }
        public string PhongKham { get; set; }
        public string TrangThai { get; set; }
    }
}