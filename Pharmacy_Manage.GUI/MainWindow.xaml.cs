using System.Windows;
using System.Windows.Input; // Cần thêm cái này để dùng MouseButtonEventArgs
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

        // Hàm này giúp di chuyển cửa sổ khi giữ chuột trái
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUser.Text.Trim();
            string password = txtPass.Password;
            string selectedRole = rbAdmin.IsChecked == true ? "Admin" : "Staff";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Gọi BUS xử lý
            AccountDTO user = _accountBUS.CheckLogin(username, password);

            if (user != null)
            {
                // Kiểm tra quyền (Logic demo, bạn có thể sửa lại theo DB của bạn)
                bool isRoleMatch = (selectedRole == "Admin" && user.Role.Contains("Admin")) ||
                                   (selectedRole == "Staff" && user.Role.Contains("Staff"));

                if (isRoleMatch)
                {
                    MessageBox.Show($"Đăng nhập thành công!\nXin chào {user.FullName}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    // this.Hide();
                    // new DashboardWindow().Show();
                }
                else
                {
                    MessageBox.Show($"Tài khoản đúng nhưng sai quyền truy cập!", "Lỗi phân quyền", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Sai tài khoản hoặc mật khẩu!", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}