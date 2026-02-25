using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Pharmacy_Manage.DAL
{
    // Kế thừa DbConnection để sử dụng hàm GetConnection()
    public class SanPhamDAL : DbConnection
    {
        // Lấy danh sách sản phẩm
        public DataTable GetAllProducts()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = GetConnection())
            {
                string query = "SELECT MaSP, TenSP, HanDung, NgayNhap, TonKho, DonVi FROM SanPham";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.Fill(dt);
            }
            return dt;
        }

        // Thêm sản phẩm
        public bool InsertProduct(string ten, int tonKho, DateTime hanDung, DateTime ngayNhap)
        {
            using (SqlConnection con = GetConnection())
            {
                string query = "INSERT INTO SanPham (TenSP, TonKho, HanDung, NgayNhap) VALUES (@ten, @ton, @hsd, @ngay)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ten", ten);
                cmd.Parameters.AddWithValue("@ton", tonKho);
                cmd.Parameters.AddWithValue("@hsd", hanDung);
                cmd.Parameters.AddWithValue("@ngay", ngayNhap);
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Sửa sản phẩm
        public bool UpdateProduct(int ma, string ten, int tonKho, DateTime hanDung, DateTime ngayNhap)
        {
            using (SqlConnection con = GetConnection())
            {
                string query = "UPDATE SanPham SET TenSP = @ten, TonKho = @ton, HanDung = @hsd, NgayNhap = @ngay WHERE MaSP = @ma";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ma", ma);
                cmd.Parameters.AddWithValue("@ten", ten);
                cmd.Parameters.AddWithValue("@ton", tonKho);
                cmd.Parameters.AddWithValue("@hsd", hanDung);
                cmd.Parameters.AddWithValue("@ngay", ngayNhap);
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Xóa sản phẩm
        public bool DeleteProduct(int ma)
        {
            using (SqlConnection con = GetConnection())
            {
                string query = "DELETE FROM SanPham WHERE MaSP = @ma";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ma", ma);
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Hàm thống kê Dashboard (Sửa lỗi CS0117/CS1061)
        public DataTable GetUrgentStats()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("GetProductUrgentStats", con);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }
    }
}