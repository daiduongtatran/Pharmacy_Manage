using System;
using Microsoft.Data.SqlClient;
using Pharmacy_Manage.DTO;

namespace Pharmacy_Manage.DAL
{
    public class AccountDAL : DbConnection
    {
        public bool Register(string name, string email, string phone, string pass, string role)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        string sqlAcc = "INSERT INTO Accounts(Username, Password, RoleName, CreatedAt) VALUES(@user, @pass, @role, GETDATE())";
                        SqlCommand cmd1 = new SqlCommand(sqlAcc, conn, trans);
                        cmd1.Parameters.AddWithValue("@user", email);
                        cmd1.Parameters.AddWithValue("@pass", pass);
                        cmd1.Parameters.AddWithValue("@role", role);
                        cmd1.ExecuteNonQuery();

                        string sqlProfile = "";

                        if (role == "Customer")
                        {
                            sqlProfile = "INSERT INTO Customers(FullName, Email, Phone, Username, Points) VALUES(@name, @email, @phone, @user, 0)";
                        }
                        else
                        {
                            sqlProfile = "INSERT INTO Employees(FullName, Email, Phone, Username) VALUES(@name, @email, @phone, @user)";
                        }

                        SqlCommand cmd2 = new SqlCommand(sqlProfile, conn, trans);
                        cmd2.Parameters.AddWithValue("@name", name);
                        cmd2.Parameters.AddWithValue("@email", email);
                        cmd2.Parameters.AddWithValue("@phone", phone);
                        cmd2.Parameters.AddWithValue("@user", email);
                        cmd2.ExecuteNonQuery();

                        trans.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw ex;
                    }
                }
            }
        }

        public AccountDTO? Login(string user, string pass, string role)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = "";
                if (role == "Customer")
                {
                    query = "SELECT a.Username, a.RoleName, c.FullName FROM Accounts a LEFT JOIN Customers c ON a.Username = c.Username WHERE a.Username=@user AND a.Password=@pass AND a.RoleName=@role";
                }
                else
                {
                    query = "SELECT a.Username, a.RoleName, e.FullName FROM Accounts a LEFT JOIN Employees e ON a.Username = e.Username WHERE a.Username=@user AND a.Password=@pass AND a.RoleName=@role";
                }

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
                            Role = dr["RoleName"].ToString(),
                            FullName = dr["FullName"] != DBNull.Value ? dr["FullName"].ToString() : ""
                        };
                    }
                }
            }
            return null;
        }
    }
}