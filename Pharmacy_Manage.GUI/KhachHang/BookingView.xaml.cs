using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Pharmacy_Manage.GUI.KhachHang
{
    public partial class BookingView : UserControl
    {
        private string connectionString = @"Data Source=localhost;Initial Catalog=PharmacyManage;Integrated Security=True;TrustServerCertificate=True";

        private Dictionary<string, List<string>> chuyenKhoaPhongKhamMap;

        public BookingView(string customerName = "")
        {
            InitializeComponent();
            LoadCountries();
            LoadEthnicities();
            
            LoadChuyenKhoaPhongKham();

            if (!string.IsNullOrWhiteSpace(customerName))
            {
                txtHoTen.Text = customerName;
            }
        }

        private void LoadChuyenKhoaPhongKham()
        {
            chuyenKhoaPhongKhamMap = new Dictionary<string, List<string>>
            {
                { "Khám nội tổng quát", new List<string> { "PK Nội 1 (Tầng 1)", "PK Nội 2 (Tầng 1)" } },
                { "Khám chuyên khoa Nhi", new List<string> { "PK Nhi 1 (Tầng 2)", "PK Nhi 2 (Tầng 2)" } },
                { "Khám Tai Mũi Họng", new List<string> { "PK TMH 1 (Tầng 3)", "PK TMH 2 (Tầng 3)" } },
                { "Khám Răng Hàm Mặt", new List<string> { "PK Răng Hàm Mặt (Tầng 3)" } },
                { "Khám Da Liễu", new List<string> { "PK Da Liễu (Tầng 4)" } },
                { "Khám Mắt", new List<string> { "PK Mắt (Tầng 4)" } }
            };

            cbChuyenKhoa.ItemsSource = chuyenKhoaPhongKhamMap.Keys;
            cbChuyenKhoa.SelectedIndex = 0; 
        }

        private void cbChuyenKhoa_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbChuyenKhoa.SelectedItem != null)
            {
                string selectedKhoa = cbChuyenKhoa.SelectedItem.ToString();
                if (chuyenKhoaPhongKhamMap.ContainsKey(selectedKhoa))
                {
                    cbPhongKham.ItemsSource = chuyenKhoaPhongKhamMap[selectedKhoa];
                    cbPhongKham.SelectedIndex = 0; 
                }
            }
        }

        private void LoadCountries()
        {
            List<string> countries = new List<string> {
                "Việt Nam", "Anh", "Hoa Kỳ", "Hàn Quốc", "Nhật Bản", "Pháp", "Đức"
            };
            cbQuocTich.ItemsSource = countries;
            cbQuocTich.SelectedIndex = 0;
        }

        private void LoadEthnicities()
        {
            List<string> ethnicities = new List<string> {
                "Kinh", "Tày", "Thái", "Mường", "Khơ Me", "Hoa", "Nùng", "Hmong", "Khác"
            };
            cbDanToc.ItemsSource = ethnicities;
            cbDanToc.SelectedIndex = 0;
        }

        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "[0-9]");
        }

        private void BtnXacNhan_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHoTen.Text) || string.IsNullOrWhiteSpace(txtSdt.Text) ||
                dpNgayKham.SelectedDate == null || cbGioKham.SelectedItem == null ||
                string.IsNullOrWhiteSpace(txtLyDo.Text) || cbPhongKham.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng điền đầy đủ các trường có dấu * và chọn phòng khám!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Giờ khám không hợp lệ", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string phongKhamChon = cbPhongKham.SelectedItem.ToString();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string checkQuery = @"SELECT COUNT(*) FROM LichHen 
                                          WHERE ThoiGianKham = @ThoiGianKham 
                                            AND PhongKham = @PhongKham 
                                            AND TrangThai != N'Đã hủy'"; 

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@ThoiGianKham", thoiGianKham);
                        checkCmd.Parameters.AddWithValue("@PhongKham", phongKhamChon);
                        
                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show($"Khung giờ {thoiGianKham:HH:mm} ngày {thoiGianKham:dd/MM/yyyy} tại {phongKhamChon} đã có khách hàng khác đặt.\n\nVui lòng chọn giờ hoặc phòng khám khác!", 
                                            "Trùng lịch hẹn", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return; 
                        }
                    }

                    string queryKH = @"INSERT INTO KhachHang (HoTen, SoDienThoai, Email, NgaySinh, DanToc, QuocTich) 
                                       VALUES (@HoTen, @Sdt, @Email, @NgaySinh, @DanToc, @QuocTich);
                                       SELECT SCOPE_IDENTITY();";

                    int maKH;
                    using (SqlCommand cmdKH = new SqlCommand(queryKH, conn))
                    {
                        cmdKH.Parameters.AddWithValue("@HoTen", txtHoTen.Text);
                        cmdKH.Parameters.AddWithValue("@Sdt", txtSdt.Text);
                        cmdKH.Parameters.AddWithValue("@Email", (object)txtEmail.Text ?? DBNull.Value);
                        cmdKH.Parameters.AddWithValue("@NgaySinh", (object)dpNgaySinh.SelectedDate ?? DBNull.Value);
                        cmdKH.Parameters.AddWithValue("@DanToc", cbDanToc.Text ?? "Kinh");
                        cmdKH.Parameters.AddWithValue("@QuocTich", cbQuocTich.Text ?? "Vietnam");
                        maKH = Convert.ToInt32(cmdKH.ExecuteScalar());
                    }

                    string queryLH = @"INSERT INTO LichHen (MaKH, ThoiGianKham, ChuyenKhoa, PhongKham, LyDoKham, TrangThai, LoaiKham) 
                                       VALUES (@MaKH, @ThoiGianKham, @ChuyenKhoa, @PhongKham, @LyDoKham, N'Đang chờ', N'Đặt lịch trước');";
                    
                    using (SqlCommand cmdLH = new SqlCommand(queryLH, conn))
                    {
                        cmdLH.Parameters.AddWithValue("@MaKH", maKH);
                        cmdLH.Parameters.AddWithValue("@ThoiGianKham", thoiGianKham);
                        cmdLH.Parameters.AddWithValue("@ChuyenKhoa", cbChuyenKhoa.Text);
                        cmdLH.Parameters.AddWithValue("@PhongKham", phongKhamChon);
                        cmdLH.Parameters.AddWithValue("@LyDoKham", txtLyDo.Text);
                        cmdLH.ExecuteNonQuery();
                    }

                    MessageBox.Show($"Đặt lịch thành công!\nThời gian: {thoiGianKham:dd/MM/yyyy HH:mm}\nTại: {phongKhamChon}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

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