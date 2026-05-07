namespace QuanLyLuong.Models;

public class AttendanceRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EmployeeId { get; set; } = "";
    public string MaNhanVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string PhongBan { get; set; } = "";
    public int Thang { get; set; }
    public int Nam { get; set; }
    public decimal SoNgayCong { get; set; } = 26;
    public decimal SoNgayLam { get; set; }
    public decimal SoNgayNghiPhep { get; set; }
    public decimal SoNgayNghiKhongPhep { get; set; }
    public decimal SoLanDiTre { get; set; }
    public decimal GioLamThem { get; set; }
    public string GhiChu { get; set; } = "";
}
