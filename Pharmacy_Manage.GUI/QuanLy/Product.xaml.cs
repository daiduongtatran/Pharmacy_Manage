using Pharmacy_Manage.BUS;
using Pharmacy_Manage.GUI;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Manage.QuanLy
{
    public partial class Product : UserControl
    {
        SanPhamBUS spBUS = new SanPhamBUS();

        public Product()
        {
            InitializeComponent();
            LoadData();
        }

        public void LoadData()
        {
            try { dgSanPham.ItemsSource = spBUS.GetAll().DefaultView; }
            catch (Exception ex) { MessageBox.Show("Loi tai du lieu: " + ex.Message); }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            ProductDialog dialog = new ProductDialog();
            dialog.Owner = Window.GetWindow(this);

            // CHỖ NÀY QUAN TRỌNG: Nếu bấm "Lưu" trên Dialog (DialogResult = true)
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Gọi BUS để thêm vào SQL (Đủ 12 tham số)
                    bool result = spBUS.Add(
                        dialog.txtTen.Text,
                        dialog.txtLoai.Text,
                        dialog.txtDonVi.Text,
                        dialog.txtNSX.Text,
                        dialog.dpHSD.SelectedDate ?? DateTime.Now,
                        dialog.dpNN.SelectedDate ?? DateTime.Now,
                        decimal.Parse(dialog.txtGiaNhap.Text),
                        decimal.Parse(dialog.txtGiaBan.Text),
                        dialog.txtHangXuat.Text,
                        int.Parse(dialog.txtTon.Text), // Khớp với x:Name="txtTon" trong XAML của bạn
                        ((ComboBoxItem)dialog.cbTrangThai.SelectedItem).Content.ToString(),
                        dialog.txtGhiChu.Text
                    );

                    if (result) { LoadData(); MessageBox.Show("Thêm thuốc mới thành công!"); }
                }
                catch (Exception ex) { MessageBox.Show("Lỗi nhập liệu: " + ex.Message); }
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgSanPham.SelectedItem is DataRowView row)
            {
                // Truyền dòng đang chọn vào Dialog để hiện dữ liệu cũ lên các ô
                ProductDialog dialog = new ProductDialog(row);
                dialog.Owner = Window.GetWindow(this);

                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        // Lấy MaSP từ dòng đang chọn
                        int maSP = Convert.ToInt32(row["MaSP"]);

                        // Gọi BUS để sửa trong SQL (Đủ 13 tham số)
                        bool result = spBUS.Edit(
                            maSP,
                            dialog.txtTen.Text,
                            dialog.txtLoai.Text,
                            dialog.txtDonVi.Text,
                            dialog.txtNSX.Text,
                            dialog.dpHSD.SelectedDate ?? DateTime.Now,
                            dialog.dpNN.SelectedDate ?? DateTime.Now,
                            decimal.Parse(dialog.txtGiaNhap.Text),
                            decimal.Parse(dialog.txtGiaBan.Text),
                            dialog.txtHangXuat.Text,
                            int.Parse(dialog.txtTon.Text),
                            ((ComboBoxItem)dialog.cbTrangThai.SelectedItem).Content.ToString(),
                            dialog.txtGhiChu.Text
                        );

                        if (result) { LoadData(); MessageBox.Show("Cập nhật thành công!"); }
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi khi sửa: " + ex.Message); }
                }
            }
            else { MessageBox.Show("Vui lòng chọn thuốc cần sửa!"); }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgSanPham.SelectedItem is DataRowView row)
            {
                if (MessageBox.Show("Ban co chac muon xoa san pham nay?", "Xac nhan", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    int maSP = Convert.ToInt32(row["MaSP"]);
                    if (spBUS.Remove(maSP)) // Thay bằng hàm xóa thực tế trong BUS của bạn
                    {
                        LoadData();
                        MessageBox.Show("Da xoa thanh cong!");
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui long chon mot san pham de xoa!");
            }
        }
    }
}