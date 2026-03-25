using System;
using System.Data;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using Microsoft.Data.SqlClient;
using Pharmacy_Manage.GUI.nhanvien;
using static Pharmacy_Manage.GUI.Banthuocview;


namespace Pharmacy_Manage.GUI
{
    public partial class Banthuocview : UserControl
    {
        private DataTable gioHangTable = new DataTable();
        private decimal tongTien = 0;

        private string connectionString =
            "Server=.;Database=PharmacyManage;Trusted_Connection=True;TrustServerCertificate=True;";

        public Banthuocview()
        {
            InitializeComponent();

            // Tạo cấu trúc giỏ hàng
            gioHangTable.Columns.Add("MaSP");
            gioHangTable.Columns.Add("TenSP");
            gioHangTable.Columns.Add("SoLuong", typeof(int));
            gioHangTable.Columns.Add("DonGia", typeof(decimal));
            gioHangTable.Columns.Add("ThanhTien", typeof(decimal));

            icGioHang.ItemsSource = gioHangTable.DefaultView;

            LoadDuLieuKho();
        }

        // ================= LOAD KHO =================
        private void LoadDuLieuKho()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT * FROM SanPham
                        WHERE TrangThai = N'Đang bán'
                        AND TonKho > 0
                        AND HanDung >= CAST(GETDATE() AS DATE)";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    icSanPham.ItemsSource = table.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load dữ liệu: " + ex.Message);
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbKhachHang.ItemsSource = LoadKhachHang();
            cbKhachHang.DisplayMemberPath = "HoTen";
            cbKhachHang.SelectedValuePath = "HoTen";
        }
        // ================= CHECK HẠN =================
        private bool CheckHan(DateTime hanDung)
        {
            return hanDung >= DateTime.Now.Date;
        }

        // ================= TĂNG GIẢM SỐ LƯỢNG =================
        private void TangSoLuong_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn &&
                btn.Tag is TextBox txt &&
                int.TryParse(txt.Text, out int soLuong))
            {
                txt.Text = (soLuong + 1).ToString();
            }
        }

        private void SoLuong_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void SoLuong_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox txt)
            {
                if (!int.TryParse(txt.Text, out int value) || value < 1)
                {
                    txt.Text = "1";
                }
            }
        }

        private void GiamSoLuong_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn &&
                btn.Tag is TextBox txt &&
                int.TryParse(txt.Text, out int soLuong))
            {
                if (soLuong > 1)
                    txt.Text = (soLuong - 1).ToString();
                else
                    txt.Text = "1";
            }
        }

        // ================= THÊM VÀO GIỎ =================
        private void Mua_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is not Button btn) return;
                if (btn.DataContext is not DataRowView sanPham) return;
                if (btn.Tag is not TextBox txtSoLuong) return;

                if (!int.TryParse(txtSoLuong.Text, out int soLuong) || soLuong <= 0)
                {
                    MessageBox.Show("Số lượng không hợp lệ.");
                    return;
                }

                string ma = sanPham["MaSP"].ToString();
                string ten = sanPham["TenSP"].ToString();
                decimal gia = Convert.ToDecimal(sanPham["GiaBan"]);
                decimal thanhTien = gia * soLuong;

                bool daTonTai = false;

                foreach (DataRow row in gioHangTable.Rows)
                {
                    if (row["MaSP"].ToString() == ma)
                    {
                        int slCu = Convert.ToInt32(row["SoLuong"]);
                        int slMoi = slCu + soLuong;

                        row["SoLuong"] = slMoi;
                        row["ThanhTien"] = slMoi * gia;
                        daTonTai = true;
                        break;
                    }
                }

                if (!daTonTai)
                {
                    gioHangTable.Rows.Add(ma, ten, soLuong, gia, thanhTien);
                }

                CapNhatTongTien();

                txtSoLuong.Text = "1"; // reset sau khi mua
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm vào giỏ: " + ex.Message);
            }
        }



        // ================= CẬP NHẬT TỔNG TIỀN =================
        private void CapNhatTongTien()
        {
            decimal tongTienThuoc = 0;
            foreach (DataRow row in gioHangTable.Rows)
            {
                tongTienThuoc += Convert.ToDecimal(row["ThanhTien"]);
            }
            txtTongTienThuoc.Text = "Tổng thuốc: " + tongTienThuoc.ToString("N0") + " VNĐ";
            decimal tongTatCa = tongTienThuoc + tongTienDichVu;
            txtTongTien.Text = "Tổng tiền: " + tongTatCa.ToString("N0") + " VNĐ";
        }
        // ================= THANH TOÁN =================
        private void ThanhToan()
        {
            if (gioHangTable.Rows.Count == 0 && listDichVuDaChon.Count == 0)
            {
                MessageBox.Show("Không có gì để thanh toán.");
                return;
            }

            string maKH = txtMaKH.Text;
            if (string.IsNullOrEmpty(maKH))
            {
                MessageBox.Show("Vui lòng chọn khách hàng.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // TRANSACTION (RẤT QUAN TRỌNG)
                    SqlTransaction tran = conn.BeginTransaction();

                    try
                    {
                        // ===== 1. TÍNH TIỀN =====
                        decimal tongTienThuoc = 0;
                        foreach (DataRow row in gioHangTable.Rows)
                        {
                            tongTienThuoc += Convert.ToDecimal(row["ThanhTien"]);
                        }

                        decimal tongTienDV = 0;
                        foreach (var dv in listDichVuDaChon)
                        {
                            tongTienDV += dv.Gia;
                        }

                        // ===== 2. INSERT HÓA ĐƠN + LẤY MaHD =====
                        string insertHoaDon = @"
                INSERT INTO HoaDon (MaKH, NgayLap, TongTienDichVu, TongTienSanPham)
                OUTPUT INSERTED.MaHD
                VALUES (@MaKH, GETDATE(), @TongDV, @TongSP)";

                        SqlCommand cmdHD = new SqlCommand(insertHoaDon, conn, tran);
                        cmdHD.Parameters.AddWithValue("@MaKH", maKH);
                        cmdHD.Parameters.AddWithValue("@TongDV", tongTienDV);
                        cmdHD.Parameters.AddWithValue("@TongSP", tongTienThuoc);

                        int maHD = (int)cmdHD.ExecuteScalar();

                        // ===== 3. CHI TIẾT THUỐC =====
                        foreach (DataRow row in gioHangTable.Rows)
                        {
                            string maSP = row["MaSP"].ToString();
                            int soLuong = Convert.ToInt32(row["SoLuong"]);
                            decimal donGia = Convert.ToDecimal(row["DonGia"]);

                            // CHECK
                            string checkQuery = @"
                    SELECT TonKho, HanDung, TrangThai
                    FROM SanPham
                    WHERE MaSP = @MaSP";

                            SqlCommand checkCmd = new SqlCommand(checkQuery, conn, tran);
                            checkCmd.Parameters.AddWithValue("@MaSP", maSP);

                            using (SqlDataReader reader = checkCmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                    throw new Exception("Sản phẩm không tồn tại.");

                                int tonKho = Convert.ToInt32(reader["TonKho"]);
                                DateTime hanDung = Convert.ToDateTime(reader["HanDung"]);
                                string trangThai = reader["TrangThai"].ToString();

                                if (trangThai != "Đang bán")
                                    throw new Exception("Sản phẩm đã ngưng bán.");

                                if (!CheckHan(hanDung))
                                    throw new Exception("Sản phẩm đã hết hạn.");

                                if (tonKho < soLuong)
                                    throw new Exception("Không đủ tồn kho.");
                            }

                            // INSERT CHI TIẾT HÓA ĐƠN
                            string insertCT = @"
                    INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia)
                    VALUES (@MaHD, @MaSP, @SoLuong, @DonGia)";

                            SqlCommand cmdCT = new SqlCommand(insertCT, conn, tran);
                            cmdCT.Parameters.AddWithValue("@MaHD", maHD);
                            cmdCT.Parameters.AddWithValue("@MaSP", maSP);
                            cmdCT.Parameters.AddWithValue("@SoLuong", soLuong);
                            cmdCT.Parameters.AddWithValue("@DonGia", donGia);
                            cmdCT.ExecuteNonQuery();

                            // UPDATE KHO
                            string updateQuery = @"
                    UPDATE SanPham
                    SET TonKho = TonKho - @SoLuong,
                        HangXuat = HangXuat + @SoLuong
                    WHERE MaSP = @MaSP";

                            SqlCommand updateCmd = new SqlCommand(updateQuery, conn, tran);
                            updateCmd.Parameters.AddWithValue("@SoLuong", soLuong);
                            updateCmd.Parameters.AddWithValue("@MaSP", maSP);
                            updateCmd.ExecuteNonQuery();
                        }

                        // ===== 4. CHI TIẾT DỊCH VỤ =====
                        foreach (var dv in listDichVuDaChon)
                        {
                            string insertDV = @"
                    INSERT INTO ChiTietDichVu (MaHD, MaDV, ThanhTien)
                    VALUES (@MaHD, @MaDV, @ThanhTien)";

                            SqlCommand cmdDV = new SqlCommand(insertDV, conn, tran);
                            cmdDV.Parameters.AddWithValue("@MaHD", maHD);
                            cmdDV.Parameters.AddWithValue("@MaDV", dv.MaDV);
                            cmdDV.Parameters.AddWithValue("@ThanhTien", dv.Gia);
                            cmdDV.ExecuteNonQuery();
                        }

                        // ===== 5. COMMIT =====
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw new Exception("Lỗi transaction: " + ex.Message);
                    }
                }

                MessageBox.Show("Thanh toán thành công!");

                // ===== RESET =====
                gioHangTable.Clear();
                listDichVuDaChon.Clear();
                icDichvu.ItemsSource = null;

                txtTongTienDichVu.Text = "Tổng DV: 0 VNĐ";
                txtTongTienThuoc.Text = "Tổng thuốc: 0 VNĐ";
                txtTongTien.Text = "Tổng tiền: 0 VNĐ";

                CapNhatTrangThaiNut();
                LoadDuLieuKho();
                hamloadchung.Reload();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thanh toán: " + ex.Message);
            }
        }
        // ================= BUTTON ACTION =================
        private void btnAction_Click(object sender, RoutedEventArgs e)
        {
            if (btnAction.Content.ToString() == "Xóa giỏ 🗑")
            {
                gioHangTable.Clear();
                CapNhatTongTien();
            }
            else
            {
                ThanhToan();
            }
        }


        // ================= TÌM KIẾM TỰ ĐỘNG =================
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query;

                    if (string.IsNullOrEmpty(keyword))
                    {
                        query = @"
                            SELECT * FROM SanPham
                            WHERE TrangThai = N'Đang bán'
                            AND TonKho > 0
                            AND HanDung >= CAST(GETDATE() AS DATE)";
                    }
                    else
                    {
                        query = @"
                            SELECT * FROM SanPham
                            WHERE TrangThai = N'Đang bán'
                            AND TonKho > 0
                            AND HanDung >= CAST(GETDATE() AS DATE)
                            AND (
                                CAST(MaSP AS NVARCHAR) LIKE @kw
                                OR TenSP LIKE @kw
                            )";
                    }

                    SqlCommand cmd = new SqlCommand(query, conn);

                    if (!string.IsNullOrEmpty(keyword))
                        cmd.Parameters.AddWithValue("@kw", keyword + "%");

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    icSanPham.ItemsSource = table.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tìm kiếm: " + ex.Message);
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim();

            try
            {
                string connectionString =
                    "Server=.;Database=PharmacyManage;Trusted_Connection=True;TrustServerCertificate=True;";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query;

                    if (string.IsNullOrEmpty(keyword))
                    {
                        query = "SELECT * FROM SanPham";
                    }
                    else
                    {
                        query = @"SELECT * FROM SanPham
                          WHERE MaSP LIKE @kw
                          OR TenSP LIKE @kw";
                    }

                    SqlCommand cmd = new SqlCommand(query, conn);

                    if (!string.IsNullOrEmpty(keyword))
                    {
                        cmd.Parameters.AddWithValue("@kw", "%" + keyword + "%");
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    icSanPham.ItemsSource = table.DefaultView;

                    if (table.Rows.Count == 0)
                    {
                        MessageBox.Show("Không tìm thấy sản phẩm trong kho bán.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tìm kiếm: " + ex.Message);
            }
        }



        // ================= TAB CONTROL =================
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tab = sender as TabControl;

            if (tab.SelectedIndex == 0)
            {
                // ===== XÓA GIỎ =====
                btnAction.Content = "Xóa giỏ 🗑";

                btnAction.Background = (Brush)new BrushConverter().ConvertFromString("#FFEBEE"); // hồng nhạt
                btnAction.Foreground = (Brush)new BrushConverter().ConvertFromString("#E53935"); // đỏ
                btnAction.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#E53935");
            }
            else
            {
                // ===== THANH TOÁN =====
                btnAction.Content = "Thanh toán 💳";

                btnAction.Background = (Brush)new BrushConverter().ConvertFromString("#E3F2FD"); // xanh nhạt
                btnAction.Foreground = (Brush)new BrushConverter().ConvertFromString("#2196F3"); // xanh nước
                btnAction.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#2196F3");
            }
            CapNhatTrangThaiNut();
        }
        //================= COMBO BOX CHO BỆNH NHÂN (dịch vụ) =============
        public class KhachHang()
        {
            public string MaKH { get; set; }
            public string HoTen { get; set; }
        }

        public List<KhachHang> LoadKhachHang()
        {
            List<KhachHang> list = new List<KhachHang>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT MaKH, HoTen FROM KhachHang";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new KhachHang
                    {
                        MaKH = reader["MaKH"].ToString(),
                        HoTen = reader["HoTen"].ToString()
                    });
                }

                reader.Close();
            }

            return list;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            var kh = (KhachHang)cbKhachHang.SelectedItem;

            if (kh != null)
            {
               
                txtMaKH.Text = kh.MaKH;
            }
        }
        public class DichVu
        {
            public string MaDV { get; set; }
            public string TenDichVu { get; set; }
            public decimal Gia { get; set; }
            public bool IsSelected { get; set; }
        }

        private void XoaItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.Tag is DataRowView rowView)
                {
                    gioHangTable.Rows.Remove(rowView.Row);
                    CapNhatTongTien();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa: " + ex.Message);
            }
        }

        List<DichVu> gioDichVu = new List<DichVu>();

        decimal tongTienDichVu = 0;

        List<DichVu> listDichVuDaChon = new List<DichVu>();
        private void BtnChonDichVu_Click(object sender, RoutedEventArgs e)
        {
            ChonDichVuWindow win = new ChonDichVuWindow();

            if (win.ShowDialog() == true)
            {
                listDichVuDaChon = win.SelectedDichVu ?? new List<DichVu>();
                icDichvu.ItemsSource = listDichVuDaChon;
                tongTienDichVu = listDichVuDaChon.Sum(x => x.Gia);
                txtTongTienDichVu.Text = $"Tổng DV: {tongTienDichVu:N0} VNĐ";
                CapNhatTongTien();
            }
        }
        

        private void CapNhatTrangThaiNut()
        {
            bool isHoaDon = tabControlMain.SelectedIndex == 1;

            decimal tongThuoc = 0;
            foreach (DataRow row in gioHangTable.Rows)
            {
                tongThuoc += Convert.ToDecimal(row["ThanhTien"]);
            }

            bool coTien = (tongThuoc + tongTienDichVu) > 0;

            btnResetAll.Visibility = (isHoaDon && coTien)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        private void btnResetAll_Click(object sender, RoutedEventArgs e)
        {
            gioHangTable.Rows.Clear();

            listDichVuDaChon.Clear();
            icDichvu.ItemsSource = null;

            tongTienDichVu = 0;

            txtTongTienDichVu.Text = "Tổng DV: 0 VNĐ";
            txtTongTienThuoc.Text = "Tổng thuốc: 0 VNĐ";
            txtTongTien.Text = "Tổng tiền: 0 VNĐ";

            CapNhatTrangThaiNut();
        }

        private void XoaDV_Click(object sender, RoutedEventArgs e)
        {
            listDichVuDaChon.Clear();
            icDichvu.ItemsSource = null;
            tongTienDichVu = 0;
            txtTongTienDichVu.Text = "Tổng DV: 0 VNĐ";
            CapNhatTongTien();
            CapNhatTrangThaiNut();
        }
    }
}