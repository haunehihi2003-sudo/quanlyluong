namespace QuanLyLuong.Models;

public class Employee
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string MaNhanVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string ChucVu { get; set; } = "";
    public string PhongBan { get; set; } = "";
    public decimal LuongCoBan { get; set; }
    public decimal PhuCap { get; set; }
    public string SoDienThoai { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime NgayVaoLam { get; set; } = DateTime.Today;
    public string GioiTinh { get; set; } = "Nam";
    public string CCCD { get; set; } = "";
    public string DiaChi { get; set; } = "";
    public int SoNgayPhepNam { get; set; } = 12;
    public int SoNgayPhepDaDung { get; set; } = 0;
    public string LoaiHopDong { get; set; } = "Chính thức";
    public DateTime NgayHetHopDong { get; set; } = DateTime.Today.AddYears(1);
    public bool ConHieuLuc { get; set; } = true;
    public string GhiChu { get; set; } = "";
}
