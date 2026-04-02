using System;
using System.Data;

namespace Pharmacy_Manage.DAL // THÊM DÒNG NÀY ĐỂ LIÊN KẾT VỚI DATA PROVIDER
{
    public class ReportDAL
    {
        public DataTable GetRecentInvoices()
        {
            // Sử dụng LEFT JOIN để lấy HoTen từ bảng KhachHang
            string query = @"SELECT h.MaHD, k.HoTen, h.NgayLap, h.TongThanhToan 
                     FROM HoaDon h 
                     LEFT JOIN KhachHang k ON k.MaKH = h.MaKH 
                     ORDER BY h.NgayLap DESC";

            return DataProvider.Instance.ExecuteQuery(query);
        }

        public decimal GetTotalRevenue()
        {
            string query = "SELECT SUM(TongThanhToan) FROM HoaDon";
            object result = DataProvider.Instance.ExecuteScalar(query);
            return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        public int GetOrderCount()
        {
            string query = "SELECT COUNT(*) FROM HoaDon";
            object result = DataProvider.Instance.ExecuteScalar(query);
            return result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }
    }
}