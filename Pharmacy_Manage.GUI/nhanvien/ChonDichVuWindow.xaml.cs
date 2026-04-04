using System;
using System.Collections.Generic;
using System.Data.Common;
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
using Microsoft.Data.SqlClient;
using static Pharmacy_Manage.GUI.Banthuocview;
using Pharmacy_Manage.DAL;

namespace Pharmacy_Manage.GUI.nhanvien
{
    public partial class ChonDichVuWindow : Window
    {
        public ChonDichVuWindow()
        {
            InitializeComponent();
            LoadDichVu();
        }
        public List<DichVu> SelectedDichVu { get; set; }
        List<DichVu> listDV = new List<DichVu>();
        void LoadDichVu()
        {
            listDV = new List<DichVu>();
            var db = new Pharmacy_Manage.DAL.DbConnection();

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                string query = "SELECT MaDV, TenDV, Gia FROM DichVu";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    
                    listDV.Add(new DichVu
                    {
                        MaDV = reader["MaDV"].ToString(),
                        TenDichVu = reader["TenDV"].ToString(),
                        Gia = Convert.ToDecimal(reader["Gia"]),
                        IsSelected = false
                    });
                }
            }

            icDichVu.ItemsSource = listDV;
        }
       

        private void BtnHoanTat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SelectedDichVu = listDV.Where(x => x.IsSelected).ToList();

                if (SelectedDichVu.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn dịch vụ!");
                    return;
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }
        private void BtnBo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }


    }
}
