using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Pharmacy_Manage.DTO;

namespace Pharmacy_Manage.GUI
{
    public partial class StaffWindow : Window
    {
        private AccountDTO _currentStaff;

        public StaffWindow()
        {
            InitializeComponent();
            SetupWindow();
        }

        public StaffWindow(AccountDTO staff)
        {
            InitializeComponent();
            _currentStaff = staff;

            if (_currentStaff != null)
            {
                txtStaffName.Text = string.IsNullOrEmpty(_currentStaff.FullName) ? _currentStaff.Username : _currentStaff.FullName;
                txtStaffRole.Text = "Nhân viên bán hàng";
            }

            SetupWindow();
        }

        private void SetupWindow()
        {
            // Mở mặc định thẻ đầu tiên (Bán Thuốc)
            MainTabControl.SelectedIndex = 0;
            SetButtonActive(BtnBanthuoc);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            txtClock.Text = DateTime.Now.ToString("HH:mm:ss  -  dd/MM/yyyy");
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            ResetMenuHighlight();

            Button btn = sender as Button;
            if (btn == null || btn.Tag == null) return;

            SetButtonActive(btn);

            // Chuyển đổi thẻ (Card) dựa vào Tag của nút bấm
            int tag = int.Parse(btn.Tag.ToString());
            MainTabControl.SelectedIndex = tag;
        }

        private void ResetMenuHighlight()
        {
            Brush transparent = Brushes.Transparent;
            Brush grayText = (Brush)new BrushConverter().ConvertFrom("#A0AEC0");

            BtnBanthuoc.Background = transparent;
            BtnBanthuoc.Foreground = grayText;

            BtnQLthuoc.Background = transparent;
            BtnQLthuoc.Foreground = grayText;

            BtnQLkhachhang.Background = transparent;
            BtnQLkhachhang.Foreground = grayText;

          
        }

        private void SetButtonActive(Button btn)
        {
            btn.Background = (Brush)new BrushConverter().ConvertFrom("#2D3748");
            btn.Foreground = Brushes.White;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                MainWindow login = new MainWindow();
                login.Show();
                this.Close();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}