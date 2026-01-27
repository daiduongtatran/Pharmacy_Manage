using System.Windows;
using System.Windows.Input;
using Pharmacy_Manage.BUS;
using Pharmacy_Manage.DTO;

namespace Pharmacy_Manage.GUI
{
    public partial class MainWindow : Window
    {
        private AccountBUS _accountBUS = new AccountBUS();

        public MainWindow()
        {
            InitializeComponent();
        }

        // Kéo thả cửa sổ
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // === LOGIC CHUYỂN ĐỔI MÀN HÌNH ===
        private void SwitchToRegister(object sender, MouseButtonEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;
        }

        private void SwitchToLogin(object sender, MouseButtonEventArgs e)
        {
            RegisterPanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;
        }

        // === XỬ LÝ ĐĂNG NHẬP ===
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Logic đăng nhập cũ của bạn ở đây
            string role = "Admin";
            if (rbStaff.IsChecked == true) role = "Staff";
            if (rbCust.IsChecked == true) role = "Customer";

            MessageBox.Show($"Đang đăng nhập quyền: {role}");
            // _accountBUS.CheckLogin(...)
        }

        // === XỬ LÝ ĐĂNG KÝ ===
        private void btnDoRegister_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(regName.Text) || string.IsNullOrEmpty(regEmail.Text))
            {
                MessageBox.Show("Vui lòng nhập đủ thông tin!");
                return;
            }

            // Gọi BUS lưu vào DB
            // _accountBUS.Register(regName.Text, regEmail.Text, ...);

            MessageBox.Show("Đăng ký thành công! Vui lòng đăng nhập.");
            SwitchToLogin(null, null);
        }   
    }
}