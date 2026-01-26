namespace Pharmacy_Manage.DTO
{
    public class UserDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        
        // Phân quyền: "Admin", "Staff", "Customer"
        public string Role { get; set; } 
    }
}