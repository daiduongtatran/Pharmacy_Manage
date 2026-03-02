using System;
using System.Windows;
using System.Windows.Input;

namespace Pharmacy_Manage.GUI
{
    public partial class HomeWindow : Window
    {
        public HomeWindow()
        {
            InitializeComponent();
        }

        // Logic Đặt lịch: Chuyển trạng thái khách hàng thành "Đang đợi"
        private void BtnBooking_Click(object sender, RoutedEventArgs e)
        {
            string customerName = txtCustomerName.Text;

            if (string.IsNullOrWhiteSpace(customerName))
            {
                MessageBox.Show("Vui lòng nhập tên quý khách để đặt lịch!", "Thông báo");
                return;
            }

            // Ở đây bạn gọi hàm từ BUS để lưu vào hàng đợi Database
            // Giả sử: spBUS.BookingExamination(customerName);
            
            MessageBox.Show($"Chúc mừng {customerName}! Bạn đã đăng ký thành công.\nTrạng thái: ĐANG ĐỢI KHÁM.", 
                            "MediTrack Booking", MessageBoxButton.OK, MessageBoxImage.Information);
            
            txtCustomerName.Clear();
        }

        private void BtnGoToStore_Click(object sender, RoutedEventArgs e)
        {
            // Chuyển Tab hoặc mở cửa sổ mua sắm
            MessageBox.Show("Đang chuyển hướng tới cửa hàng thuốc...");
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Trả về trang MainWindow (Login)
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            // Xử lý chuyển tab nếu bạn dùng TabControl
        }
    }
}