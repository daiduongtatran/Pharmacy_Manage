using Pharmacy_Manage.DAL;
using Pharmacy_Manage.DTO;

namespace Pharmacy_Manage.BUS
{
    public class AccountBUS
    {
        // BUS gọi DAL chứ không tự viết SQL bên trong
        private AccountDAL _accountDAL = new AccountDAL();

        public AccountDTO? CheckLogin(string user, string pass, string role)
        {
            return _accountDAL.Login(user, pass, role);
        }

        public bool RegisterAccount(string name, string email, string phone, string pass)
        {
            return _accountDAL.Register(name, email, phone, pass);
        }
    }
}