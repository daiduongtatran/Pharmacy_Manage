using Pharmacy_Manage.DTO;

namespace Pharmacy_Manage.BUS
{
    public class AccountBUS
    {
        public AccountDTO CheckLogin(string user, string pass)
        {
            // Tạm thời giả lập dữ liệu thay vì gọi Database
            if (user == "admin" && pass == "123")
            {
                return new AccountDTO { Username = "admin", FullName = "Chủ nhà thuốc", Role = "Admin" };
            }
            else if (user == "staff" && pass == "123")
            {
                return new AccountDTO { Username = "staff", FullName = "Dược sĩ A", Role = "Staff" };
            }

            return null; // Trả về null nếu sai thông tin
        }
    }
}