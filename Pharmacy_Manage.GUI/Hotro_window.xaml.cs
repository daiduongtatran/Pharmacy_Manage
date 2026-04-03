using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Manage.GUI
{
    public partial class Hotro_window : Window
    {
        public Hotro_window()
        {
            InitializeComponent();

            AdminList.ItemsSource = new List<Admin>
            {
                new Admin { Ten="Femboy ngon vc", SDT="0123456789", Email="abc@gmail.com" },
                new Admin { Ten="t k nói thế", SDT="0987654321", Email="xyz@gmail.com" }
            };
        }
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string sdt = btn.Tag.ToString();

            Clipboard.SetText(sdt);
            MessageBox.Show("Đã copy: " + sdt);
        }
    }

    public class Admin
    {
        public string Ten { get; set; }
        public string SDT { get; set; }
        public string Email { get; set; }
    }
}