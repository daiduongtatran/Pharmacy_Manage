using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Pharmacy_Manage.GUI.KhachHang; // Đảm bảo có dòng này để gọi được BookingView

namespace Pharmacy_Manage.GUI
{
    public partial class HomeWindow : Window
    {
        public HomeWindow()
        {
            InitializeComponent();
        }

        private void SwitchView(int index, string name = "")
        {
            if (index == 0)
            {
                pnlHomeContent.Visibility = Visibility.Visible;
                MainContent.Visibility = Visibility.Collapsed;
            }
            else if (index == 1)
            {
                pnlHomeContent.Visibility = Visibility.Collapsed;
                MainContent.Visibility = Visibility.Visible;

                
                MainContent.Content = new BookingView(name);
            }
            else if (index == 2) // THÊM INDEX NÀY CHO STOREVIEW
            {
                pnlHomeContent.Visibility = Visibility.Collapsed;
                MainContent.Visibility = Visibility.Visible;
                MainContent.Content = new StoreView(); // Gọi màn hình cửa hàng
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

        private void BtnGoToStore_Click(object sender, RoutedEventArgs e)
        {
            SwitchView(2);
        }
    }
}