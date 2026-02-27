using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Manage.GUI
{
    public partial class ProductDialog : Window
    {
        public DataRow NewRow { get; set; }
        public int SelectedID { get; set; }

        public ProductDialog(DataRowView row = null)
        {
            InitializeComponent();
            if (row != null)
            {
                SelectedID = (int)row["MaSP"];
                txtTen.Text = row["TenSP"].ToString(); txtLoai.Text = row["LoaiSP"].ToString();
                txtDonVi.Text = row["DonVi"].ToString(); txtNSX.Text = row["NhaSanXuat"].ToString();
                dpHSD.SelectedDate = (DateTime)row["HanDung"]; dpNN.SelectedDate = (DateTime)row["NgayNhap"];
                txtGiaNhap.Text = row["GiaNhap"].ToString(); txtGiaBan.Text = row["GiaBan"].ToString();
                txtHangXuat.Text = row["HangXuat"].ToString(); txtTon.Text = row["TonKho"].ToString();
                string status = row["TrangThai"].ToString();
                foreach (ComboBoxItem item in cbTrangThai.Items)
                {
                    if (item.Content.ToString() == status)
                    {
                        cbTrangThai.SelectedItem = item;
                        break;
                    }
                }
                txtGhiChu.Text = row["GhiChu"].ToString();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // KIỂM TRA TRỐNG TOÀN BỘ
            if (IsAnyEmpty(txtTen, txtLoai, txtDonVi, txtNSX, txtGiaNhap, txtGiaBan, txtHangXuat, txtTon, txtGhiChu)
                || dpHSD.SelectedDate == null || dpNN.SelectedDate == null || cbTrangThai.SelectedItem == null)
            {
                MessageBox.Show("LỖI: Bạn phải nhập ĐỦ thuộc tính, không được để trống ô nào!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            this.DialogResult = true;
        }

        private bool IsAnyEmpty(params System.Windows.Controls.TextBox[] boxes)
        {
            foreach (var b in boxes) if (string.IsNullOrWhiteSpace(b.Text)) return true;
            return false;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}