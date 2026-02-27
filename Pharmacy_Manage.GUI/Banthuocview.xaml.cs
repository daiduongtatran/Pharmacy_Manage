using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Data.SqlClient;

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
            tongTien = 0;

            foreach (DataRow row in gioHangTable.Rows)
            {
                tongTien += Convert.ToDecimal(row["ThanhTien"]);
            }

            txtTongTien.Text = "Tổng tiền: " + tongTien.ToString("N0") + " VNĐ";
        }

        // ================= THANH TOÁN =================
        private void ThanhToan()
        {
            if (gioHangTable.Rows.Count == 0)
            {
                MessageBox.Show("Giỏ hàng đang trống.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    foreach (DataRow row in gioHangTable.Rows)
                    {
                        string ma = row["MaSP"].ToString();
                        int soLuongMua = Convert.ToInt32(row["SoLuong"]);

                        string checkQuery = @"
                            SELECT TonKho, HanDung, TrangThai
                            FROM SanPham
                            WHERE MaSP = @MaSP";

                        SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                        checkCmd.Parameters.AddWithValue("@MaSP", ma);

                        using (SqlDataReader reader = checkCmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show("Sản phẩm không tồn tại.");
                                return;
                            }

                            int tonKho = Convert.ToInt32(reader["TonKho"]);
                            DateTime hanDung = Convert.ToDateTime(reader["HanDung"]);
                            string trangThai = reader["TrangThai"].ToString();

                            if (trangThai != "Đang bán")
                            {
                                MessageBox.Show("Sản phẩm đã ngưng bán.");
                                return;
                            }

                            if (!CheckHan(hanDung))
                            {
                                MessageBox.Show("Sản phẩm đã hết hạn.");
                                return;
                            }

                            if (tonKho < soLuongMua)
                            {
                                MessageBox.Show("Không đủ tồn kho.");
                                return;
                            }
                        }

                        string updateQuery = @"
                            UPDATE SanPham
                            SET TonKho = TonKho - @SoLuong
                            WHERE MaSP = @MaSP";

                        SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                        updateCmd.Parameters.AddWithValue("@SoLuong", soLuongMua);
                        updateCmd.Parameters.AddWithValue("@MaSP", ma);
                        updateCmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Thanh toán thành công!");

                gioHangTable.Clear();
                CapNhatTongTien();
                LoadDuLieuKho();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thanh toán: " + ex.Message);
            }
        }

        // ================= BUTTON ACTION =================
        private void btnAction_Click(object sender, RoutedEventArgs e)
        {
            if (btnAction.Content.ToString() == "Xóa giỏ")
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
        }
    }
}