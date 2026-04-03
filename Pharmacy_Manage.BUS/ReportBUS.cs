// File: Pharmacy_Manage.BUS/ReportBUS.cs
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

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new HoaDonDTO
                {
                    MaHD = row["MaHD"].ToString(),
                    // Nếu khách hàng xóa hoặc không có tên, hiển thị "Khách vãng lai"
                    HoTen = (row["HoTen"] != DBNull.Value && !string.IsNullOrEmpty(row["HoTen"].ToString()))
                            ? row["HoTen"].ToString()
                            : "Khách vãng lai",
                    NgayLap = Convert.ToDateTime(row["NgayLap"]),
                    TongThanhToan = Convert.ToDecimal(row["TongThanhToan"]),
                    LoiNhuan = Convert.ToDecimal(row["LoiNhuanThuc"])
                });
            }
            return list;
        }

        public decimal LayTongDoanhThu() => dal.GetTotalRevenue();

        public int LaySoLuongDon() => dal.GetOrderCount();

        public decimal LayLoiNhuan() => dal.GetTotalProfit();
    }
}