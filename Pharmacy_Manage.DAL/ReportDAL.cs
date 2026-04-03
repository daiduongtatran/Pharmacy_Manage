// File: Pharmacy_Manage.DAL/ReportDAL.cs
using System;
using System.Data;

namespace Pharmacy_Manage.DAL
{
    public class ReportDAL
    {
        // Lấy danh sách hóa đơn kèm lợi nhuận tính theo công thức yêu cầu
        public DataTable GetRecentInvoices()
        {
            string query = @"
                SELECT 
                    h.MaHD, 
                    k.HoTen, 
                    h.NgayLap, 
                    h.TongThanhToan,
                    -- Công thức: SUM((GiaBan - GiaNhap) * SoLuong) + TongTienDichVu
                    (ISNULL(ct_sum.LoiNhuanBanHang, 0) + ISNULL(h.TongTienDichVu, 0)) AS LoiNhuanThuc
                FROM HoaDon h
                LEFT JOIN KhachHang k ON h.MaKH = k.MaKH
                LEFT JOIN (
                    SELECT 
                        ct.MaHD, 
                        SUM((sp.GiaBan - sp.GiaNhap) * ct.SoLuong) AS LoiNhuanBanHang
                    FROM ChiTietHoaDon ct
                    JOIN SanPham sp ON ct.MaSP = sp.MaSP
                    GROUP BY ct.MaHD
                ) ct_sum ON h.MaHD = ct_sum.MaHD
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

        // Tính tổng lợi nhuận của toàn bộ hệ thống
        public decimal GetTotalProfit()
        {
            string query = @"
                SELECT SUM(LoiNhuan) FROM (
                    SELECT 
                        (ISNULL(SUM((sp.GiaBan - sp.GiaNhap) * ct.SoLuong), 0) + ISNULL(h.TongTienDichVu, 0)) AS LoiNhuan
                    FROM HoaDon h
                    LEFT JOIN ChiTietHoaDon ct ON h.MaHD = ct.MaHD
                    LEFT JOIN SanPham sp ON ct.MaSP = sp.MaSP
                    GROUP BY h.MaHD, h.TongTienDichVu
                ) AS ResultTable";
            object result = DataProvider.Instance.ExecuteScalar(query);
            return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }
    }
}