using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using Pharmacy_Manage.DAL;

namespace Pharmacy_Manage.GUI
{
    public partial class Hotro_window : Window
    {
        public Hotro_window()
        {
            InitializeComponent();

            AdminList.ItemsSource = GetAdmins();
        }
        public List<Admin> GetAdmins()
        {
            List<Admin> list = new List<Admin>();
            DbConnection db = new DbConnection();
            using (SqlConnection conn = db.GetConnection() )
            {
                conn.Open();

                string query = "SELECT FullName, Phone, Email FROM Employees";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new Admin
                    {
                        Ten = reader["FullName"].ToString(),
                        SDT = reader["Phone"].ToString(),
                        Email = reader["Email"].ToString()
                    });
                }
            }

            return list;
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