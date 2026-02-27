using System;
using System.Data;
using Pharmacy_Manage.DAL;

namespace Pharmacy_Manage.BUS
{
    public class SanPhamBUS
    {
        SanPhamDAL dal = new SanPhamDAL();

        public DataTable GetAll() => dal.GetAllProducts();

        // Nhận 12 tham số (trừ MaSP) để thêm
        public bool Add(string ten, string loai, string dv, string nsx, DateTime hsd, DateTime nn, decimal gn, decimal gb, string hx, int ton, string tt, string gc)
        {
            return dal.InsertProduct(ten, loai, dv, nsx, hsd, nn, gn, gb, hx, ton, tt, gc);
        }

        // Nhận đủ 13 tham số để sửa
        public bool Edit(int ma, string ten, string loai, string dv, string nsx, DateTime hsd, DateTime nn, decimal gn, decimal gb, string hx, int ton, string tt, string gc)
        {
            return dal.UpdateProduct(ma, ten, loai, dv, nsx, hsd, nn, gn, gb, hx, ton, tt, gc);
        }

        public bool Remove(int ma) => dal.DeleteProduct(ma);

        // Hàm gọi sang DAL để lấy dữ liệu Dashboard
        public DataTable GetUrgentStats() => dal.GetUrgentStats();
    }
}