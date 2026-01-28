using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Manage.GUI
{
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            // Tạo dữ liệu mẫu để bạn thấy bảng hiện lên đẹp
            var list = new List<object>
            {
                new { ID="MED01", Name="Panadol Extra", Category="Giảm đau", Stock=150, Price="1.500đ", Expiry="12/2026" },
                new { ID="MED02", Name="Augmentin 1g", Category="Kháng sinh", Stock=40, Price="15.000đ", Expiry="05/2026" },
                new { ID="MED03", Name="Berberin", Category="Tiêu hóa", Stock=12, Price="500đ", Expiry="02/2026" },
                new { ID="MED04", Name="Vitamin C 500mg", Category="Bổ sung", Stock=200, Price="2.000đ", Expiry="10/2027" }
            };
            dgInventory.ItemsSource = list;
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int index = int.Parse(btn.Tag.ToString());
                MainTabControl.SelectedIndex = index;
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có muốn đăng xuất không?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                MainWindow login = new MainWindow();
                login.Show();
                this.Close();
            }
        }
    }
}