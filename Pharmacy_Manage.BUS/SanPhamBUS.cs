using System;
using System.Data;
using Pharmacy_Manage.DAL;

namespace Pharmacy_Manage.BUS
{
    public class SanPhamBUS
    {
        SanPhamDAL dal = new SanPhamDAL();

        public DataTable GetAll() => dal.GetAllProducts();

        public bool Add(string ten, int ton, DateTime hsd, DateTime ngayNhap)
            => dal.InsertProduct(ten, ton, hsd, ngayNhap);

        public bool Edit(int ma, string ten, int ton, DateTime hsd, DateTime ngayNhap)
            => dal.UpdateProduct(ma, ten, ton, hsd, ngayNhap);

        public bool Remove(int ma) => dal.DeleteProduct(ma);

        // Hàm gọi sang DAL để lấy dữ liệu Dashboard
        public DataTable GetUrgentStats() => dal.GetUrgentStats();
    }
}