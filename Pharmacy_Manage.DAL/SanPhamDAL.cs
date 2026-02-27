using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Pharmacy_Manage.DAL
{
    public class SanPhamDAL : DbConnection
    {
        // 1. Lấy đủ các cột
        public DataTable GetAllProducts()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = GetConnection())
            {
                string query = "SELECT MaSP, TenSP, LoaiSP, DonVi, NhaSanXuat, HanDung, NgayNhap, GiaNhap, GiaBan, HangXuat, TonKho, TrangThai, GhiChu FROM SanPham";
                new SqlDataAdapter(query, con).Fill(dt);
            }
            return dt;
        }
        public DataTable GetUrgentStats()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("GetProductUrgentStats", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi để bạn biết cụ thể lỗi gì (ví dụ: sai tên server, sai DB...)
                System.Diagnostics.Debug.WriteLine("Lỗi SQL GetUrgentStats: " + ex.Message);
            }
            return dt;
        }
        // 2. Thêm mới (Không truyền MaSP vì SQL tự tăng)
        public bool InsertProduct(string ten, string loai, string dv, string nsx, DateTime hsd, DateTime nn, decimal gn, decimal gb, string hx, int ton, string tt, string gc)
        {
            using (SqlConnection con = GetConnection())
            {
                string query = @"INSERT INTO SanPham (TenSP, LoaiSP, DonVi, NhaSanXuat, HanDung, NgayNhap, GiaNhap, GiaBan, HangXuat, TonKho, TrangThai, GhiChu) 
                                VALUES (@ten, @loai, @dv, @nsx, @hsd, @nn, @gn, @gb, @hx, @ton, @tt, @gc)";
                SqlCommand cmd = new SqlCommand(query, con);
                AddParameters(cmd, ten, loai, dv, nsx, hsd, nn, gn, gb, hx, ton, tt, gc);
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // 3. Sửa (Truyền đủ 13 tham số, dùng MaSP để định danh WHERE)
        public bool UpdateProduct(int ma, string ten, string loai, string dv, string nsx, DateTime hsd, DateTime nn, decimal gn, decimal gb, string hx, int ton, string tt, string gc)
        {
            using (SqlConnection con = GetConnection())
            {
                string query = @"UPDATE SanPham SET TenSP=@ten, LoaiSP=@loai, DonVi=@dv, NhaSanXuat=@nsx, HanDung=@hsd, NgayNhap=@nn, 
                                GiaNhap=@gn, GiaBan=@gb, HangXuat=@hx, TonKho=@ton, TrangThai=@tt, GhiChu=@gc WHERE MaSP=@ma";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ma", ma);
                AddParameters(cmd, ten, loai, dv, nsx, hsd, nn, gn, gb, hx, ton, tt, gc);
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Hàm phụ để tránh viết lặp code add parameter
        private void AddParameters(SqlCommand cmd, string ten, string loai, string dv, string nsx, DateTime hsd, DateTime nn, decimal gn, decimal gb, string hx, int ton, string tt, string gc)
        {
            cmd.Parameters.AddWithValue("@ten", ten);
            cmd.Parameters.AddWithValue("@loai", loai);
            cmd.Parameters.AddWithValue("@dv", dv);
            cmd.Parameters.AddWithValue("@nsx", nsx);
            cmd.Parameters.AddWithValue("@hsd", hsd);
            cmd.Parameters.AddWithValue("@nn", nn);
            cmd.Parameters.AddWithValue("@gn", gn);
            cmd.Parameters.AddWithValue("@gb", gb);
            cmd.Parameters.AddWithValue("@hx", hx);
            cmd.Parameters.AddWithValue("@ton", ton);
            cmd.Parameters.AddWithValue("@tt", tt);
            cmd.Parameters.AddWithValue("@gc", gc);
        }

        public bool DeleteProduct(int ma)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("DELETE FROM SanPham WHERE MaSP=@ma", con);
                cmd.Parameters.AddWithValue("@ma", ma);
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}