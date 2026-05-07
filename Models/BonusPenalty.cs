namespace QuanLyLuong.Models;

public class BonusPenalty
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EmployeeId { get; set; } = "";
    public string MaNhanVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string PhongBan { get; set; } = "";
    public int Thang { get; set; }
    public int Nam { get; set; }
    public string LoaiTP { get; set; } = "Thưởng";
    public decimal SoTien { get; set; }
    public string LyDo { get; set; } = "";
    public DateTime NgayTao { get; set; } = DateTime.Now;
}
