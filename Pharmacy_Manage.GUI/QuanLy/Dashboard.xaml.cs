using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Pharmacy_Manage.BUS;

namespace Pharmacy_Manage.QuanLy
{
    public partial class Dashboard : UserControl
    {
        SanPhamBUS spBUS = new SanPhamBUS();

        public Dashboard()
        {
            InitializeComponent();
            LoadUrgentData();
        }

        private void LoadUrgentData()
        {
            try
            {
                // Gọi hàm lấy thống kê từ BUS (hàm này gọi Procedure GetProductUrgentStats)[cite: 1, 2]
                DataTable dt = spBUS.GetUrgentStats();

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    // Cập nhật các con số lên giao diện dựa trên tên cột trong Procedure
                    // SoLuongTonThap: Tồn < 50
                    txtLowStock.Text = row["SoLuongTonThap"].ToString();

                    // SoLuongSapHetHan: Hạn < 6 tháng
                    txtExpired.Text = row["SoLuongSapHetHan"].ToString();

                    // Doanh thu (Hiện tại bạn có thể để mặc định hoặc lấy từ một hàm BUS khác)
                    txtRevenue.Text = "0 đ";
                }
            }
            catch (Exception ex)
            {
                // Thông báo lỗi nếu không gọi được SQL hoặc sai tên cột
                MessageBox.Show("Loi tai thong ke Dashboard: " + ex.Message);
            }
        }
    }
}