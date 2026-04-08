using Microsoft.Data.SqlClient;
using System.Data;
using System;
using System.Text.RegularExpressions;


namespace Pharmacy_Manage.DAL
{
    public class DbConnection
    {
        protected string strCon = @"Data Source=localhost;Initial Catalog=PharmacyManage;Integrated Security=True;TrustServerCertificate=True";

        public SqlConnection GetConnection()
        {
            return new SqlConnection(strCon);
        }
    }

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

        private DataProvider() { }

        public DataTable ExecuteQuery(string query)
        {
            return ExecuteQuery(query, null);
        }

        public DataTable ExecuteQuery(string query, object[] parameters)
        {
            DataTable data = new DataTable();
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
        public object ExecuteScalar(string query)
        {
            return ExecuteScalar(query, null);
        }
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