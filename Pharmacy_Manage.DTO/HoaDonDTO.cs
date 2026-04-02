namespace Pharmacy_Manage.DTO
{
    public class HoaDonDTO
{
    public string? MaHD { get; set; }
    public string? HoTen { get; set; }
    public DateTime NgayLap { get; set; }
    public decimal TongThanhToan { get; set; }
    public decimal LoiNhuan { get; set; } // Tính toán dựa trên giá nhập/bán nếu cần
}
}
