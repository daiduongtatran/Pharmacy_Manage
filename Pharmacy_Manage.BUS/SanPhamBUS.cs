using System.Data;
using Pharmacy_Manage.DAL; // Reference tới DAL

namespace Pharmacy_Manage.BUS
{
    public class SanPhamBUS
    {
        SanPhamDAL dal = new SanPhamDAL();

        public DataTable GetUrgentStats()
        {
            return dal.GetUrgentStats();
        }
    }
}