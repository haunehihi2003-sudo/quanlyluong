namespace QuanLyLuong.Models;

public class PhongBanReport
{
    public string PhongBan { get; set; } = "";
    public int SoNhanVien { get; set; }
    public decimal TongLuongCoBan { get; set; }
    public decimal TongPhuCap { get; set; }
    public decimal TongLamThem { get; set; }
    public decimal TongBHXH { get; set; }
    public decimal TongThue { get; set; }
    public decimal TongThucNhan { get; set; }
    public decimal TrungBinhThucNhan { get; set; }
}

public class SoSanhRow
{
    public string MaNhanVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string PhongBan { get; set; } = "";
    public decimal ThangTruoc { get; set; }
    public decimal ThangNay { get; set; }
    public decimal ChenhLech { get; set; }
    public string PhanTram { get; set; } = "";
}
