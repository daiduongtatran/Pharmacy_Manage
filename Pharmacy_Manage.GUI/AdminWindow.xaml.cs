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

        #region CRUD QUA DIALOG
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            ProductDialog dialog = new ProductDialog();
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                RefreshSystem();
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgSanPham.SelectedItem is DataRowView row)
            {
                ProductDialog dialog = new ProductDialog(row);
                dialog.Owner = this;
                if (dialog.ShowDialog() == true)
                {
                    RefreshSystem();
                }
            }
            else { MessageBox.Show("Vui lòng chọn thuốc cần sửa!"); }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgSanPham.SelectedItem is DataRowView row)
            {
                if (MessageBox.Show("Xóa sản phẩm này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (spBUS.Remove(Convert.ToInt32(row["MaSP"]))) RefreshSystem();
                }
            }
        }

        private void RefreshSystem()
        {
            LoadAllProducts();
            LoadUrgentData();
        }
        #endregion

        #region UI LOGIC
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
        private void dgSanPham_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        #endregion
    }
}