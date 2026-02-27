using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Wpf;
using Pharmacy_Manage.BUS;

namespace Pharmacy_Manage.GUI
{
    public partial class AdminWindow : Window
    {
        public SeriesCollection RevenueSeries { get; set; } = new SeriesCollection();
        public string[] ChartLabels { get; set; } = Array.Empty<string>();

        // Khai báo biến thống kê (Sửa lỗi CS0103)
        private int _countExpiring = 0;
        private int _countLowStock = 0;

        SanPhamBUS spBUS = new SanPhamBUS();

        public AdminWindow()
        {
            InitializeComponent();
            LoadDashboard();
            LoadAllProducts();
            this.DataContext = this;
        }

        private void LoadAllProducts()
        {
            try { dgSanPham.ItemsSource = spBUS.GetAll().DefaultView; }
            catch (Exception ex) { MessageBox.Show("Lỗi tải kho: " + ex.Message); }
        }

        private void LoadUrgentData()
        {
            try
            {
                DataTable dt = spBUS.GetUrgentStats();
                if (dt != null && dt.Rows.Count > 0)
                {
                    txtUrgentTotal.Text = dt.Rows[0]["TongCanXuLy"].ToString() + " Thuốc";
                    _countExpiring = Convert.ToInt32(dt.Rows[0]["SoLuongSapHetHan"]);
                    _countLowStock = Convert.ToInt32(dt.Rows[0]["SoLuongTonThap"]);
                }
            }
            catch { }
        }
        // 1. XỬ LÝ HÀM THÊM (ADD)
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            ProductDialog dialog = new ProductDialog();
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // --- BƯỚC 4: THAY ĐOẠN NÀY ---
                    // Lấy text từ item đang được chọn trong ComboBox
                    string selectedStatus = "Đang bán"; // Giá trị mặc định
                    if (dialog.cbTrangThai.SelectedItem is ComboBoxItem item)
                    {
                        selectedStatus = item.Content.ToString();
                    }

                    bool result = spBUS.Add(
                        dialog.txtTen.Text,
                        dialog.txtLoai.Text,
                        dialog.txtDonVi.Text,
                        dialog.txtNSX.Text,
                        dialog.dpHSD.SelectedDate.Value,
                        dialog.dpNN.SelectedDate.Value,
                        decimal.Parse(dialog.txtGiaNhap.Text),
                        decimal.Parse(dialog.txtGiaBan.Text),
                        dialog.txtHangXuat.Text,
                        int.Parse(dialog.txtTon.Text),
                        selectedStatus, // TRUYỀN BIẾN ĐÃ LẤY TỪ COMBOBOX VÀO ĐÂY
                        dialog.txtGhiChu.Text
                    );
                    // -----------------------------

                    if (result)
                    {
                        MessageBox.Show("Thêm thành công!");
                        RefreshSystem();
                    }
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            }
        }

        // 2. XỬ LÝ HÀM SỬA (EDIT/UPDATE)
        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgSanPham.SelectedItem is DataRowView row)
            {
                ProductDialog dialog = new ProductDialog(row);
                dialog.Owner = this;

                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        // --- BƯỚC 4: THAY ĐOẠN NÀY ---
                        string selectedStatus = "Đang bán";
                        if (dialog.cbTrangThai.SelectedItem is ComboBoxItem item)
                        {
                            selectedStatus = item.Content.ToString();
                        }

                        bool result = spBUS.Edit(
                            Convert.ToInt32(row["MaSP"]),
                            dialog.txtTen.Text,
                            dialog.txtLoai.Text,
                            dialog.txtDonVi.Text,
                            dialog.txtNSX.Text,
                            dialog.dpHSD.SelectedDate.Value,
                            dialog.dpNN.SelectedDate.Value,
                            decimal.Parse(dialog.txtGiaNhap.Text),
                            decimal.Parse(dialog.txtGiaBan.Text),
                            dialog.txtHangXuat.Text,
                            int.Parse(dialog.txtTon.Text),
                            selectedStatus, // TRUYỀN BIẾN ĐÃ LẤY TỪ COMBOBOX VÀO ĐÂY
                            dialog.txtGhiChu.Text
                        );
                        // -----------------------------

                        if (result)
                        {
                            MessageBox.Show("Cập nhật thành công!");
                            RefreshSystem();
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
                }
            }
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgSanPham.SelectedItem is DataRowView row)
            {
                if (MessageBox.Show("Bạn có chắc chắn muốn xóa thuốc này khỏi Database?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    int ma = Convert.ToInt32(row["MaSP"]);
                    if (spBUS.Remove(ma))
                    {
                        RefreshSystem();
                        MessageBox.Show("Đã xóa dữ liệu thành công!");
                    }
                }
            }
            else { MessageBox.Show("Hãy chọn một dòng để xóa!"); }
        }
        private void RefreshSystem() { LoadAllProducts(); LoadUrgentData(); }
        // Hàm bổ trợ để làm mới dữ liệu sau khi Thêm/Sửa
        private void RefreshData()
        {
            LoadAllProducts(); // Load lại DataGrid
            if (typeof(AdminWindow).GetMethod("LoadUrgentData") != null)
            {
                LoadUrgentData(); // Cập nhật lại các con số thống kê ở trang chủ
            }
        }
        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
                MainTabControl.SelectedIndex = int.Parse(btn.Tag.ToString());
        }

        private void LoadDashboard()
        {
            LoadUrgentData();
            RevenueSeries.Add(new LineSeries
            {
                Title = "Doanh thu",
                Values = new ChartValues<double> { 850, 1200, 950, 1600, 1100, 1850, 1450 },
                Stroke = System.Windows.Media.Brushes.Teal,
                Fill = System.Windows.Media.Brushes.Transparent
            });
            ChartLabels = new[] { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void Logout_Click(object sender, RoutedEventArgs e) => this.Close();

        private void CardUrgent_Click(object sender, MouseButtonEventArgs e) => MainTabControl.SelectedIndex = 1;

        // Xóa nội dung hàm này vì không dùng TextBox ở màn hình chính nữa
        private void dgSanPham_SelectionChanged(object sender, SelectionChangedEventArgs e) {}
    }
}