using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Pharmacy_Manage.GUI
{
    /// <summary>
    /// Interaction logic for StaffWindow.xaml
    /// </summary>
    public partial class StaffWindow : Window
    {
        public StaffWindow()
        {
            InitializeComponent();
            MainContent.Content = banThuocView;
        }
        private Banthuocview banThuocView = new();
        private QLthuoc_view qLthuoc_View = new(); 
        private QLkhachhang_view quanLyKhachView = new();

        private void ResetMenuHighlight()
        {
            BtnBanthuoc.Background = Brushes.Transparent;
            BtnQLthuoc.Background = Brushes.Transparent;
            BtnQLkhachhang.Background = Brushes.Transparent;
        }


        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            ResetMenuHighlight();

            Button btn = sender as Button;
            btn.Background = new SolidColorBrush(Color.FromRgb(52, 152, 219));

            int tag = int.Parse(btn.Tag.ToString());

            switch (tag)
            {
                case 0:
                    MainContent.Content = banThuocView;
                    break;
                case 1:
                    MainContent.Content = qLthuoc_View;
                    break;
                case 2:
                    MainContent.Content = quanLyKhachView;
                    break;
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
