using System;
using System.Data;
using System.Collections.Generic;
using Pharmacy_Manage.DAL;
using Pharmacy_Manage.DTO;

namespace Pharmacy_Manage.BUS
{
    public class ReportBUS
    {
        private ReportDAL dal = new ReportDAL();

        public List<HoaDonDTO> GetListHoaDon()
        {
            DataTable dt = dal.GetRecentInvoices();
            var list = new List<HoaDonDTO>();

            // Trong file ReportBUS.cs, phần vòng lặp foreach:
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new HoaDonDTO
                {
                    MaHD = row["MaHD"].ToString(),

                    // Kiểm tra xem cột HoTen có tồn tại trong DataTable trả về không
                    HoTen = row.Table.Columns.Contains("HoTen") && row["HoTen"] != DBNull.Value
                            ? row["HoTen"].ToString()
                            : "Khách vãng lai",

                    NgayLap = Convert.ToDateTime(row["NgayLap"]),
                    TongThanhToan = Convert.ToDecimal(row["TongThanhToan"])
                });
            }
            return list;
        }

        public decimal LayTongDoanhThu() => dal.GetTotalRevenue();

        public int LaySoLuongDon() => dal.GetOrderCount();

        // Lợi nhuận có thể tính toán chính xác hơn nếu bạn có cột giá vốn, 
        // tạm thời để 20% doanh thu như cũ.
        public decimal LayLoiNhuan() => LayTongDoanhThu() * 0.2m;
    }
}