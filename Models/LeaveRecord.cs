namespace QuanLyLuong.Models;

public class LeaveRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EmployeeId { get; set; } = "";
    public string MaNhanVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string PhongBan { get; set; } = "";
    public DateTime NgayBatDau { get; set; } = DateTime.Today;
    public DateTime NgayKetThuc { get; set; } = DateTime.Today;
    public decimal SoNgay { get; set; } = 1;
    public string LoaiNghi { get; set; } = "Nghỉ phép";
    public string LyDo { get; set; } = "";
    public string TrangThai { get; set; } = "Chờ duyệt";
    public DateTime NgayTao { get; set; } = DateTime.Now;
}
