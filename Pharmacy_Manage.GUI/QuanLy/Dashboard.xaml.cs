using LiveCharts;
using LiveCharts.Wpf;
using Pharmacy_Manage.BUS;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Manage.QuanLy
{
    public partial class Dashboard : UserControl
    {
        SanPhamBUS spBUS = new SanPhamBUS();

        public SeriesCollection RevenueSeries { get; set; }
        public string[] ChartLabels { get; set; }
        public Func<double, string> Formatter { get; set; }

        public Dashboard()
        {
            InitializeComponent();
            LoadUrgentData();
            LoadChartData();
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
        // HÀM MỚI: XỬ LÝ BIỂU ĐỒ VÀ DOANH THU
        private void LoadChartData()
        {
            try
            {
                // Gọi hàm BUS lấy dữ liệu từ Procedure GetRevenueLast7Days
                DataTable dt = spBUS.GetRevenue();

                if (dt != null && dt.Rows.Count > 0)
                {
                    ChartValues<double> values = new ChartValues<double>();
                    string[] labels = new string[dt.Rows.Count];
                    double todayRevenue = 0;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        labels[i] = dt.Rows[i]["NgayThang"].ToString();
                        double revenue = Convert.ToDouble(dt.Rows[i]["DoanhThu"]);
                        values.Add(revenue);

                        // Dòng cuối cùng của kết quả trả về là doanh thu hôm nay
                        if (i == dt.Rows.Count - 1)
                        {
                            todayRevenue = revenue;
                        }
                    }

                    // Cấu hình đường biểu đồ
                    RevenueSeries = new SeriesCollection
                    {
                        new LineSeries
                        {
                            Title = "Doanh thu",
                            Values = values,
                            PointGeometrySize = 10,
                            StrokeThickness = 3
                        }
                    };

                    ChartLabels = labels;
                    Formatter = value => value.ToString("N0") + " đ";

                    // Cập nhật con số "Doanh thu hôm nay"
                    txtRevenue.Text = todayRevenue.ToString("N0") + " đ";

                    // Gán DataContext để các Binding {Binding ...} trong XAML hoạt động
                    this.DataContext = this;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải biểu đồ: " + ex.Message);
            }
        }
    }
}