using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Pharmacy_Manage.GUI.KhachHang
{
    public partial class BookingView : UserControl
    {

        private string connectionString = @"Data Source=localhost;Initial Catalog=PharmacyManage;Integrated Security=True;TrustServerCertificate=True";

        // Tìm đến hàm BookingView() và sửa lại như sau:
        public BookingView(string customerName = "")
        {
            InitializeComponent();
            LoadCountries();
            LoadEthnicities();

            // Nếu có tên truyền từ trang Home sang thì tự điền vào ô Họ tên
            if (!string.IsNullOrWhiteSpace(customerName))
            {
                txtHoTen.Text = customerName;
            }
        }

        private void LoadCountries()
        {
            List<string> countries = new List<string> {
        "Việt Nam", "Afghanistan", "Ai Cập", "Albania", "Algeria", "Andorra", "Angola", "Anh", "Áo", "Argentina",
    "Armenia", "Azerbaijan", "Ấn Độ", "Bahamas", "Ba Lan", "Bahrain", "Bangladesh", "Barbados", "Belarus", "Bỉ",
    "Belize", "Benin", "Bhutan", "Bỉ", "Bolivia", "Bosnia và Herzegovina", "Botswana", "Bồ Đào Nha", "Brazil", "Brunei",
    "Bulgaria", "Burkina Faso", "Burundi", "Cabo Verde", "Các Tiểu vương quốc Ả Rập Thống nhất", "Cameroon", "Campuchia", "Canada", "Chile", "Colombia",
    "Comoros", "Congo", "Costa Rica", "Croatia", "Cuba", "Curaçao", "Cyprus", "Chad", "Czechia", "Đan Mạch",
    "Djibouti", "Dominica", "Dominican Republic", "Đông Timor", "Đức", "Ecuador", "El Salvador", "Eritrea", "Estonia", "Eswatini",
    "Ethiopia", "Fiji", "Gabon", "Gambia", "Georgia", "Ghana", "Grenada", "Guatemala", "Guinea", "Guinea-Bissau",
    "Guyana", "Haiti", "Hà Lan", "Hàn Quốc", "Hoa Kỳ", "Honduras", "Hungary", "Hy Lạp", "Iceland", "Indonesia",
    "Iran", "Iraq", "Ireland", "Israel", "Ý", "Jamaica", "Jordan", "Kazakhstan", "Kenya", "Kiribati",
    "Kuwait", "Kyrgyzstan", "Lào", "Latvia", "Lebanon", "Lesotho", "Liberia", "Libya", "Liechtenstein", "Lithuania",
    "Luxembourg", "Madagascar", "Malawi", "Malaysia", "Maldives", "Mali", "Malta", "Mauritania", "Mauritius", "Mexico",
    "Micronesia", "Moldova", "Monaco", "Mongolia", "Montenegro", "Morocco", "Mozambique", "Myanmar", "Namibia", "Nauru",
    "Nepal", "New Zealand", "Nga", "Nhật Bản", "Nicaragua", "Niger", "Nigeria", "Na Uy", "Oman", "Pakistan",
    "Palau", "Palestine", "Panama", "Papua New Guinea", "Paraguay", "Peru", "Philippines", "Pháp", "Phần Lan", "Qatar",
    "Romania", "Rwanda", "Saint Kitts và Nevis", "Saint Lucia", "Samoa", "San Marino", "Saudi Arabia", "Senegal", "Serbia", "Seychelles",
    "Sierra Leone", "Singapore", "Slovakia", "Slovenia", "Solomon Islands", "Somalia", "South Africa", "South Sudan", "Tây Ban Nha", "Sri Lanka",
    "Sudan", "Suriname", "Thụy Điển", "Thụy Sĩ", "Syria", "Tajikistan", "Tanzania", "Thái Lan", "Togo", "Tonga",
    "Trinidad và Tobago", "Tunisia", "Thổ Nhĩ Kỳ", "Turkmenistan", "Tuvalu", "Úc", "Uganda", "Ukraine", "Uruguay", "Uzbekistan",
    "Vanuatu", "Vatican", "Venezuela", "Yemen", "Zambia", "Zimbabwe"
    };
            cbQuocTich.ItemsSource = countries;
            cbQuocTich.SelectedIndex = 0; // Mặc định chọn Việt Nam
        }

        private void LoadEthnicities()
        {
            List<string> ethnicities = new List<string> {
        "Kinh", "Tày", "Thái", "Mường", "Khơ Me", "Hoa", "Nùng", "Hmong", "Dao", "Gia Rai",
    "Ê Đê", "Ba Na", "Sán Chay", "Chăm", "Kơ Ho", "Xơ Đăng", "Sán Dìu", "Hrê", "Ra Glai", "M'Nông",
    "Thổ", "Xtiêng", "Khơ mú", "Bru - Vân Kiều", "Cơ Tu", "Giáy", "Tà Ôi", "Mạ", "Giẻ-Triêng", "Co",
    "Chơ Ro", "Xinh Mun", "Hà Nhì", "Chu Ru", "Lào", "La Chí", "Kháng", "Phù Lá", "La Hủ", "La Ha",
    "Pà Thẻn", "Lự", "Ngái", "Chứt", "Lô Lô", "Mảng", "Cơ Lao", "Bố Y", "Cống", "Si La",
    "Pu Péo", "Rơ Măm", "Brâu", "Ơ Đu", "Khác"
    };
            cbDanToc.ItemsSource = ethnicities;
            cbDanToc.SelectedIndex = 0; // Mặc định chọn Kinh
        }
        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Chỉ cho phép nhập các kí tự từ 0 đến 9
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "[0-9]");
        }
        private void BtnXacNhan_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validate dữ liệu cơ bản
            if (string.IsNullOrWhiteSpace(txtHoTen.Text) || string.IsNullOrWhiteSpace(txtSdt.Text) ||
                dpNgayKham.SelectedDate == null || string.IsNullOrWhiteSpace(cbGioKham.Text) ||
                string.IsNullOrWhiteSpace(txtLyDo.Text))
            {
                MessageBox.Show("Vui lòng điền đầy đủ các trường có dấu *!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime thoiGianKham;
            DateTime ngayChon = dpNgayKham.SelectedDate.Value;

            if (TimeSpan.TryParse(cbGioKham.Text, out TimeSpan gioKham))
            {
                
                DateTime temp = ngayChon.Date.Add(gioKham);
                thoiGianKham = new DateTime(temp.Year, temp.Month, temp.Day, temp.Hour, temp.Minute, 0);
                
            }
            else
            {
                MessageBox.Show("Giờ khám không hợp lệ (VD: 08:30)", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Insert Khách hàng
                    string queryKH = @"INSERT INTO KhachHang (HoTen, SoDienThoai, Email, NgaySinh, DanToc, QuocTich) 
                               VALUES (@HoTen, @Sdt, @Email, @NgaySinh, @DanToc, @QuocTich);
                               SELECT SCOPE_IDENTITY();";

                    SqlCommand cmdKH = new SqlCommand(queryKH, conn);
                    cmdKH.Parameters.AddWithValue("@HoTen", txtHoTen.Text);
                    cmdKH.Parameters.AddWithValue("@Sdt", txtSdt.Text);
                    cmdKH.Parameters.AddWithValue("@Email", (object)txtEmail.Text ?? DBNull.Value);
                    cmdKH.Parameters.AddWithValue("@NgaySinh", (object)dpNgaySinh.SelectedDate ?? DBNull.Value);
                    cmdKH.Parameters.AddWithValue("@DanToc", cbDanToc.Text ?? "Kinh");
                    cmdKH.Parameters.AddWithValue("@QuocTich", cbQuocTich.Text ?? "Vietnam");

                    int maKH = Convert.ToInt32(cmdKH.ExecuteScalar());

                    // Insert Lịch Hẹn với thời gian đã chuẩn hóa
                    string queryLH = @"INSERT INTO LichHen (MaKH, ThoiGianKham, ChuyenKhoa, PhongKham, LyDoKham, TrangThai) 
                               VALUES (@MaKH, @ThoiGianKham, @ChuyenKhoa, @PhongKham, @LyDoKham, N'Đang chờ');";

                    SqlCommand cmdLH = new SqlCommand(queryLH, conn);
                    cmdLH.Parameters.AddWithValue("@MaKH", maKH);

                    // Truyền thoiGianKham đã xử lý (giây = 0)
                    cmdLH.Parameters.AddWithValue("@ThoiGianKham", thoiGianKham);

                    cmdLH.Parameters.AddWithValue("@ChuyenKhoa", cbChuyenKhoa.Text ?? (object)DBNull.Value);
                    cmdLH.Parameters.AddWithValue("@PhongKham", cbPhongKham.Text ?? (object)DBNull.Value);
                    cmdLH.Parameters.AddWithValue("@LyDoKham", txtLyDo.Text);

                    cmdLH.ExecuteNonQuery();

                    MessageBox.Show($"Đặt lịch thành công!\nThời gian: {thoiGianKham:dd/MM/yyyy HH:mm}", "Thông báo");

                    // Xóa dữ liệu sau khi nhập
                    txtHoTen.Clear(); txtSdt.Clear(); txtEmail.Clear(); txtLyDo.Clear(); cbGioKham.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}