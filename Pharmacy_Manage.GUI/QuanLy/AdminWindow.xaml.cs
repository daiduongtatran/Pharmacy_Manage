using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Pharmacy_Manage.GUI
{
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();

            MainContentArea.Content = new Pharmacy_Manage.QuanLy.Dashboard();
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null || btn.Tag == null) return;

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
                    // MainContentArea.Content = new Invoice(); // Nếu bạn có file Invoice.xaml
                    break;

                case "3":
                    // MainContentArea.Content = new Employee(); 
                    break;
                case "4":
                    MainContentArea.Content = new Pharmacy_Manage.QuanLy.AppointmentView();
                    break;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                MainWindow login = new MainWindow();
                login.Show();
                this.Close();
            }
        }
    }
}