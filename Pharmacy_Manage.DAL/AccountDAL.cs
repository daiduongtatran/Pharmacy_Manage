using System;
using Microsoft.Data.SqlClient;
using Pharmacy_Manage.DTO;

namespace Pharmacy_Manage.DAL
{
    public class AccountDAL : DbConnection
    {
        // 1. HÀM LOGIN (Thêm vào đây để BUS gọi được)
        public AccountDTO? Login(string user, string pass, string role)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = "SELECT * FROM Accounts WHERE Username=@user AND Password=@pass AND RoleName=@role";
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
                            Username = dr["Username"]?.ToString() ?? "",
                            Role = dr["RoleName"]?.ToString() ?? ""
                        };
                    }
                }
            }
            return null;
        }

        // 2. HÀM REGISTER (Giữ nguyên logic của bạn)
        public bool Register(string name, string email, string phone, string pass)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Chèn vào bảng Accounts
                        string sqlAcc = "INSERT INTO Accounts(Username, Password, RoleName) VALUES(@user, @pass, @role)";
                        SqlCommand cmd1 = new SqlCommand(sqlAcc, conn, trans);
                        cmd1.Parameters.AddWithValue("@user", email);
                        cmd1.Parameters.AddWithValue("@pass", pass);
                        cmd1.Parameters.AddWithValue("@role", "Customer");
                        cmd1.ExecuteNonQuery();

                        // Chèn vào bảng Customers
                        string sqlCust = "INSERT INTO Customers(FullName, Email, Phone, Username) VALUES(@name, @email, @phone, @user)";
                        SqlCommand cmd2 = new SqlCommand(sqlCust, conn, trans);
                        cmd2.Parameters.AddWithValue("@name", name);
                        cmd2.Parameters.AddWithValue("@email", email);
                        cmd2.Parameters.AddWithValue("@phone", phone);
                        cmd2.Parameters.AddWithValue("@user", email);
                        cmd2.ExecuteNonQuery();

                        trans.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        return false;
                    }
                }
            }
        }
    }
}