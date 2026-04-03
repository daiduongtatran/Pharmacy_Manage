public class HoaDonDTO
{
    public string? MaHD { get; set; }
    public string? HoTen { get; set; }
    public DateTime NgayLap { get; set; }
    public decimal TongThanhToan { get; set; }
    public decimal LoiNhuan { get; set; } // Cột này sẽ hiển thị lợi nhuận thực
}