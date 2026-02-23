using System;
using System.Data;
using Microsoft.Data.SqlClient; // Đảm bảo có thư viện này

namespace Pharmacy_Manage.DAL
{
    public class SanPhamDAL
    {
        public DataTable GetUrgentStats()
        {
            DataTable dt = new DataTable();
            DbConnection db = new DbConnection(); // Khởi tạo từ file DbConnection của bạn

            try
            {
                using (SqlConnection con = db.GetConnection()) // Gọi hàm GetConnection bạn đã viết
                {
                    string query = "EXEC GetProductUrgentStats";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần
            }
            return dt;
        }
    }
}