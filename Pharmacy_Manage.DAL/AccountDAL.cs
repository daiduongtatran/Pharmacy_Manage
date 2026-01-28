using System;
using Microsoft.Data.SqlClient;
using Pharmacy_Manage.DTO;

namespace Pharmacy_Manage.DAL
{
    public class AccountDAL : DbConnection
    {
        // Hàm đăng ký xử lý đa vai trò
        public bool Register(string name, string email, string phone, string pass, string role)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. LUÔN LUÔN tạo tài khoản trong bảng Accounts trước
                        // Lưu ý: Tôi đã sửa lỗi thiếu dấu ngoặc ) và tham số @role
                        string sqlAcc = "INSERT INTO Accounts(Username, Password, RoleName, CreatedAt) VALUES(@user, @pass, @role, GETDATE())";
                        SqlCommand cmd1 = new SqlCommand(sqlAcc, conn, trans);
                        cmd1.Parameters.AddWithValue("@user", email); // Dùng Email làm Username để dễ nhớ
                        cmd1.Parameters.AddWithValue("@pass", pass);
                        cmd1.Parameters.AddWithValue("@role", role);
                        cmd1.ExecuteNonQuery();

                        // 2. Tùy vào Role mà lưu vào bảng phụ tương ứng
                        string sqlProfile = "";

                        if (role == "Customer")
                        {
                            // Lưu vào bảng Customers (Có cột Points)
                            sqlProfile = "INSERT INTO Customers(FullName, Email, Phone, Username, Points) VALUES(@name, @email, @phone, @user, 0)";
                        }
                        else
                        {
                            // Lưu vào bảng Employees (Cho Admin và Staff)
                            sqlProfile = "INSERT INTO Employees(FullName, Email, Phone, Username) VALUES(@name, @email, @phone, @user)";
                        }

                        SqlCommand cmd2 = new SqlCommand(sqlProfile, conn, trans);
                        cmd2.Parameters.AddWithValue("@name", name);
                        cmd2.Parameters.AddWithValue("@email", email);
                        cmd2.Parameters.AddWithValue("@phone", phone);
                        cmd2.Parameters.AddWithValue("@user", email); // Khóa ngoại trùng với Username ở trên
                        cmd2.ExecuteNonQuery();

                        trans.Commit(); // Chốt đơn! Lưu dữ liệu.
                        return true;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback(); // Có lỗi thì hủy hết
                        throw ex; // Ném lỗi ra để GUI hiện thông báo
                    }
                }
            }
        }

        public AccountDTO? Login(string user, string pass, string role)
        {
            using (SqlConnection conn = GetConnection())
            {
                // Truy vấn kiểm tra khớp cả User, Pass và Role
                string query = "SELECT Username, RoleName FROM Accounts WHERE Username=@user AND Password=@pass AND RoleName=@role";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@user", user);
                cmd.Parameters.AddWithValue("@pass", pass);
                cmd.Parameters.AddWithValue("@role", role);

                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        return new AccountDTO
                        {
                            Username = dr["Username"].ToString(),
                            Role = dr["RoleName"].ToString()
                        };
                    }
                }
            }
            return null;
        }
    }
}