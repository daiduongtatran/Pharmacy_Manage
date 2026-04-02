using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Pharmacy_Manage.GUI.KhachHang; 

namespace Pharmacy_Manage.GUI
{
    public partial class HomeWindow : Window
    {
        public HomeWindow()
        {
            InitializeComponent();
            SwitchView(0); // Gọi SwitchView để vừa bật Trang chủ, vừa làm sáng nút
        }

        private void SwitchView(int index, string name = "")
        {
            ResetMenuHighlight(); // Tắt hết màu các nút

            if (index == 0)
            {
                SetButtonActive(btnMenu0); // Bật màu nút 0
                pnlHomeContent.Visibility = Visibility.Visible;
                MainContent.Visibility = Visibility.Collapsed;
            }
            else if (index == 1)
            {
                SetButtonActive(btnMenu1); // Bật màu nút 1
                pnlHomeContent.Visibility = Visibility.Collapsed;
                MainContent.Visibility = Visibility.Visible;
                MainContent.Content = new BookingView(name);
            }
            else if (index == 2) 
            {
                SetButtonActive(btnMenu2); // Bật màu nút 2
                pnlHomeContent.Visibility = Visibility.Collapsed;
                MainContent.Visibility = Visibility.Visible;
                MainContent.Content = new StoreView(); 
            }
        }

        // Logic làm mờ nút
        private void ResetMenuHighlight()
        {
            Brush transparent = Brushes.Transparent;
            Brush grayText = (Brush)new BrushConverter().ConvertFrom("#CBD5E1");

            if(btnMenu0 != null) { btnMenu0.Background = transparent; btnMenu0.Foreground = grayText; }
            if(btnMenu1 != null) { btnMenu1.Background = transparent; btnMenu1.Foreground = grayText; }
            if(btnMenu2 != null) { btnMenu2.Background = transparent; btnMenu2.Foreground = grayText; }
        }

        // Logic làm sáng nút
        private void SetButtonActive(Button btn)
        {
            if (btn != null)
            {
                btn.Background = (Brush)new BrushConverter().ConvertFrom("#334155");
                btn.Foreground = Brushes.White;
            }
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag != null)
            {
                int index = int.Parse(btn.Tag.ToString());
                SwitchView(index); 
            }
        }
        
        private void BtnBooking_Click(object sender, RoutedEventArgs e)
        {
            string nameFromHome = txtCustomerName.Text;
            SwitchView(1, nameFromHome);
        }

        private void BtnGoToStore_Click(object sender, RoutedEventArgs e)
        {
            SwitchView(2);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }
    }
}