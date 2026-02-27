using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

        // Kéo thả cửa sổ (Drag Move)
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

        // === XỬ LÝ HIỆU ỨNG MÀU KHI CHỌN ROLE (ĐĂNG KÝ) ===
        private void RegRole_Checked(object sender, RoutedEventArgs e)
        {
            // Reset tất cả về màu xám
            if (rbRegAdmin == null || rbRegStaff == null || rbRegCust == null) return;

            rbRegAdmin.Foreground = Brushes.Gray;
            rbRegAdmin.FontWeight = FontWeights.Normal;

            rbRegStaff.Foreground = Brushes.Gray;
            rbRegStaff.FontWeight = FontWeights.Normal;

            rbRegCust.Foreground = Brushes.Gray;
            rbRegCust.FontWeight = FontWeights.Normal;

            // Tô màu xanh cho cái đang chọn
            RadioButton selected = sender as RadioButton;
            if (selected != null)
            {
                selected.Foreground = (Brush)new BrushConverter().ConvertFrom("#2C9BB3");
                selected.FontWeight = FontWeights.Bold;
            }
        }
        // === XỬ LÝ HIỆU ỨNG MÀU KHI CHỌN ROLE (ĐĂNG NHẬP) ===
        private void LoginRole_Checked(object sender, RoutedEventArgs e)
        {
            // 1. Reset tất cả về màu xám nhạt
            if (rbAdmin == null || rbStaff == null || rbCust == null) return;

            rbAdmin.Foreground = Brushes.Gray;
            rbAdmin.FontWeight = FontWeights.Normal;

            rbStaff.Foreground = Brushes.Gray;
            rbStaff.FontWeight = FontWeights.Normal;

            rbCust.Foreground = Brushes.Gray;
            rbCust.FontWeight = FontWeights.Normal;

            // 2. Tô màu xanh nổi bật cho cái đang được chọn
            RadioButton selected = sender as RadioButton;
            if (selected != null)
            {
                selected.Foreground = (Brush)new BrushConverter().ConvertFrom("#2C9BB3");
                selected.FontWeight = FontWeights.Bold;
            }
        }
        // === XỬ LÝ ĐĂNG NHẬP ===
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUsername.Text;
            string pass = txtPassword.Password;

            // Lấy role từ RadioButton ở màn hình Đăng nhập
            string role = "Customer";
            if (rbAdmin.IsChecked == true) role = "Admin";
            if (rbStaff.IsChecked == true) role = "Staff";

            AccountDTO? account = _accountBUS.CheckLogin(user, pass, role);

            if (account != null)
            {
                MessageBox.Show($"Xin chào {account.Username}!", "Đăng nhập thành công");
                this.Hide(); // Ẩn màn hình Login đi

                // Phân luồng mở giao diện tương ứng
                if (account.Role == "Admin")
                {
                    new AdminWindow().Show();
                }
                if (account.Role == "Staff")
                {
                    new StaffWindow(account).Show();
                }
                else
                {
                    new HomeWindow().Show();
                }

                this.Close(); // Đóng hẳn form login
            }
            else
            {
                MessageBox.Show("Sai tài khoản, mật khẩu hoặc vai trò đã chọn!", "Lỗi đăng nhập");
            }
        }

        // === XỬ LÝ ĐĂNG KÝ ===
        private void btnDoRegister_Click(object sender, RoutedEventArgs e)
        {
            string name = regName.Text;
            string email = regEmail.Text;
            string phone = regPhone.Text;
            string pass = regPass.Password;

            // Lấy role từ RadioButton ở màn hình Đăng ký
            string role = "Customer";
            if (rbRegAdmin.IsChecked == true) role = "Admin";
            if (rbRegStaff.IsChecked == true) role = "Staff";

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            try
            {
                // Gọi BUS (Bạn nhớ cập nhật BUS theo code tôi gửi ở tin nhắn trước nhé)
                bool isSuccess = _accountBUS.RegisterAccount(name, email, phone, pass, role);

                if (isSuccess)
                {
                    MessageBox.Show("Đăng ký thành công! Hãy đăng nhập ngay.", "Thông báo");
                    SwitchToLogin(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đăng ký thất bại. Email có thể đã tồn tại.\n" + ex.Message, "Lỗi");
            }
        }

        private void rbAdmin_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}