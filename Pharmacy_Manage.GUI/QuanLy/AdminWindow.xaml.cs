using Pharmacy_Manage.DTO;
using Pharmacy_Manage.GUI.QuanLy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Pharmacy_Manage.GUI
{
    public partial class AdminWindow : Window
    {
        private AccountDTO _currentAdmin;

        public AdminWindow()
        {
            InitializeComponent();
            MainContentArea.Content = new Pharmacy_Manage.QuanLy.Dashboard();
            SetButtonActive(btnMenu0); // Mặc định sáng nút Trang chủ
        }

        // Constructor mới nhận dữ liệu từ form Đăng nhập
        public AdminWindow(AccountDTO admin) : this()
        {
            _currentAdmin = admin;
            if (_currentAdmin != null)
            {
                txtAdminName.Text = _currentAdmin.Username;
            }
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null || btn.Tag == null) return;

            ResetMenuHighlight();
            SetButtonActive(btn); // Làm sáng nút vừa bấm

            string tag = btn.Tag.ToString();
            switch (tag)
            {
                case "0":
                    MainContentArea.Content = new Pharmacy_Manage.QuanLy.Dashboard();
                    break;
                case "1":
                    MainContentArea.Content = new Pharmacy_Manage.QuanLy.Product();
                    break;
                case "2":
                    MainContentArea.Content = new Report();
                    break;
                     case "4":
                    MainContentArea.Content = new Pharmacy_Manage.QuanLy.AppointmentView();
                    break;
            }
        }

        // Logic làm mờ tất cả các nút
        private void ResetMenuHighlight()
        {
            Brush transparent = Brushes.Transparent;
            Brush grayText = (Brush)new BrushConverter().ConvertFrom("#A0AEC0");

            btnMenu0.Background = transparent; btnMenu0.Foreground = grayText;
            btnMenu1.Background = transparent; btnMenu1.Foreground = grayText;
            btnMenu2.Background = transparent; btnMenu2.Foreground = grayText;
            btnMenu4.Background = transparent; btnMenu4.Foreground = grayText;
        }

        // Logic làm sáng nút được chọn
        private void SetButtonActive(Button btn)
        {
            btn.Background = (Brush)new BrushConverter().ConvertFrom("#2A3B5C");
            btn.Foreground = Brushes.White;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                new MainWindow().Show();
                this.Close();
            }
        }
    }
}