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
                DataTable dt = spBUS.GetUrgentStats();

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    txtLowStock.Text = row["SoLuongTonThap"].ToString();

                    txtExpired.Text = row["SoLuongSapHetHan"].ToString();

                    txtRevenue.Text = "0 đ";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Loi tai thong ke Dashboard: " + ex.Message);
            }
        }
        private void LoadChartData()
        {
            try
            {
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

                        if (i == dt.Rows.Count - 1)
                        {
                            todayRevenue = revenue;
                        }
                    }

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

                    txtRevenue.Text = todayRevenue.ToString("N0") + " đ";

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