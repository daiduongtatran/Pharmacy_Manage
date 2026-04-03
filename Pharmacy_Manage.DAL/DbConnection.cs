using Microsoft.Data.SqlClient;
using System.Data;
using System;
using System.Text.RegularExpressions;


namespace Pharmacy_Manage.DAL
{
    // Lớp cơ sở quản lý kết nối
    public class DbConnection
    {
        // Chuỗi kết nối của bạn
        protected string strCon = @"Data Source=localhost;Initial Catalog=PharmacyManage;Integrated Security=True;TrustServerCertificate=True";

        public SqlConnection GetConnection()
        {
            return new SqlConnection(strCon);
        }
    }

    // Lớp thực thi truy vấn (DataProvider) kế thừa từ DbConnection
    public class DataProvider : DbConnection
    {
        private static DataProvider instance;

        public static DataProvider Instance
        {
            get
            {
                if (instance == null) instance = new DataProvider();
                return instance;
            }
        }

        // Constructor riêng tư cho Pattern Singleton
        private DataProvider() { }

        // Hàm thực thi truy vấn trả về bảng dữ liệu (dùng cho DataGrid)
        public DataTable ExecuteQuery(string query)
        {
            return ExecuteQuery(query, null);
        }

        // Overload hỗ trợ tham số truyền vào (ví dụ: @MaHD)
        public DataTable ExecuteQuery(string query, object[] parameters)
        {
            DataTable data = new DataTable();
            using (SqlConnection connection = GetConnection()) // Gọi hàm từ lớp cha
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Nếu có tham số, parse tên tham số theo dạng @name và map theo thứ tự
                        if (parameters != null && parameters.Length > 0)
                        {
                            // Tìm tất cả các tên tham số xuất hiện trong query theo thứ tự
                            MatchCollection matches = Regex.Matches(query, @"@\w+");
                            int paramCount = Math.Min(matches.Count, parameters.Length);
                            for (int i = 0; i < paramCount; i++)
                            {
                                string paramName = matches[i].Value;
                                object value = parameters[i] ?? DBNull.Value;
                                command.Parameters.AddWithValue(paramName, value);
                            }
                        }

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(data);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi ExecuteQuery: " + ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
            return data;
        }

        // Hàm thực thi trả về 1 giá trị đơn lẻ (dùng cho SUM, COUNT)
        public object ExecuteScalar(string query)
        {
            return ExecuteScalar(query, null);
        }

        // Overload ExecuteScalar hỗ trợ tham số
        public object ExecuteScalar(string query, object[] parameters)
        {
            object data = 0;
            using (SqlConnection connection = GetConnection())
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (parameters != null && parameters.Length > 0)
                        {
                            MatchCollection matches = Regex.Matches(query, @"@\w+");
                            int paramCount = Math.Min(matches.Count, parameters.Length);
                            for (int i = 0; i < paramCount; i++)
                            {
                                string paramName = matches[i].Value;
                                object value = parameters[i] ?? DBNull.Value;
                                command.Parameters.AddWithValue(paramName, value);
                            }
                        }

                        data = command.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi ExecuteScalar: " + ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
            return data;
        }
    }
}