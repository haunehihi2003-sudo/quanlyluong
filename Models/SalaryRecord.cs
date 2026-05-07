namespace QuanLyLuong.Models;

public class SalaryRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EmployeeId { get; set; } = "";
    public string MaNhanVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string PhongBan { get; set; } = "";
    public int Thang { get; set; }
    public int Nam { get; set; }
    public decimal LuongCoBan { get; set; }
    public decimal PhuCap { get; set; }
    public decimal SoNgayCong { get; set; } = 26;
    public decimal SoNgayLam { get; set; }
    public decimal SoNgayNghiPhep { get; set; }
    public decimal SoNgayNghiKhongPhep { get; set; }
    public decimal GioLamThem { get; set; }
    public decimal TienLamThem { get; set; }
    public decimal ThuongKhac { get; set; }
    public decimal PhatKhac { get; set; }
    public decimal BaoHiemXaHoi { get; set; }
    public decimal ThueTNCN { get; set; }
    public decimal KhauTruKhac { get; set; }
    public decimal LuongThucNhan { get; set; }
    public string GhiChu { get; set; } = "";
}
