namespace QuanLyLuong.Models;

public class UserAccount
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TenDangNhap { get; set; } = "";
    public string MatKhauHash { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string VaiTro { get; set; } = "Kế toán";
    public bool ConHieuLuc { get; set; } = true;
}
