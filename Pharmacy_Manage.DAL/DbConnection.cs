using Microsoft.Data.SqlClient;

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
}