using ClosedXML.Excel;
using QuanLyLuong.Models;

namespace QuanLyLuong.Services;

public static class ExcelService
{
    public static (List<Employee> imported, List<string> errors) ImportEmployees(string filePath)
    {
        var list = new List<Employee>();
        var errors = new List<string>();

        using var wb = new XLWorkbook(filePath);
        var ws = wb.Worksheet(1);

        foreach (var row in ws.RowsUsed().Skip(1))
        {
            try
            {
                var emp = new Employee
                {
                    MaNhanVien  = row.Cell(1).GetString().Trim(),
                    HoTen       = row.Cell(2).GetString().Trim(),
                    ChucVu      = row.Cell(3).GetString().Trim(),
                    PhongBan    = row.Cell(4).GetString().Trim(),
                    LuongCoBan  = decimal.TryParse(row.Cell(5).GetString(), out var lcb) ? lcb : 0,
                    PhuCap      = decimal.TryParse(row.Cell(6).GetString(), out var pc) ? pc : 0,
                    SoDienThoai = row.Cell(7).GetString().Trim(),
                    Email       = row.Cell(8).GetString().Trim(),
                    NgayVaoLam  = row.Cell(9).TryGetValue<DateTime>(out var dt) ? dt : DateTime.Today,
                    GioiTinh    = row.Cell(10).GetString().Trim() is { Length: > 0 } g ? g : "Nam",
                };
                if (string.IsNullOrWhiteSpace(emp.MaNhanVien) || string.IsNullOrWhiteSpace(emp.HoTen))
                { errors.Add($"Dòng {row.RowNumber()}: Thiếu Mã NV hoặc Họ Tên"); continue; }
                list.Add(emp);
            }
            catch (Exception ex)
            {
                errors.Add($"Dòng {row.RowNumber()}: {ex.Message}");
            }
        }
        return (list, errors);
    }

    public static void CreateImportTemplate(string filePath)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("NhanVien");
        string[] headers = { "Ma NV*", "Ho Ten*", "Chuc Vu", "Phong Ban", "Luong Co Ban", "Phu Cap", "So Dien Thoai", "Email", "Ngay Vao Lam", "Gioi Tinh" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 90, 160);
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }
        ws.Cell(2, 1).Value = "NV001";
        ws.Cell(2, 2).Value = "Nguyen Van A";
        ws.Cell(2, 3).Value = "Nhan vien";
        ws.Cell(2, 4).Value = "Ke toan";
        ws.Cell(2, 5).Value = 8000000;
        ws.Cell(2, 6).Value = 500000;
        ws.Cell(2, 7).Value = "0901234567";
        ws.Cell(2, 8).Value = "nva@email.com";
        ws.Cell(2, 9).Value = DateTime.Today.ToString("dd/MM/yyyy");
        ws.Cell(2, 10).Value = "Nam";
        ws.Columns().AdjustToContents();
        wb.SaveAs(filePath);
    }

    public static void ExportSalaryToExcel(List<SalaryRecord> records, int thang, int nam, string filePath)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet($"Luong T{thang:D2}.{nam}");
        string[] headers = { "Ma NV", "Ho Ten", "Phong Ban", "Luong CB", "Phu Cap", "Ngay Lam", "Gio OT", "Tien OT", "Thuong", "Phat", "BHXH", "Thue TNCN", "KT Khac", "Thuc Nhan" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 90, 160);
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }
        int row = 2;
        foreach (var r in records)
        {
            ws.Cell(row, 1).Value = r.MaNhanVien;
            ws.Cell(row, 2).Value = r.HoTen;
            ws.Cell(row, 3).Value = r.PhongBan;
            ws.Cell(row, 4).Value = (double)r.LuongCoBan;
            ws.Cell(row, 5).Value = (double)r.PhuCap;
            ws.Cell(row, 6).Value = (double)r.SoNgayLam;
            ws.Cell(row, 7).Value = (double)r.GioLamThem;
            ws.Cell(row, 8).Value = (double)r.TienLamThem;
            ws.Cell(row, 9).Value = (double)r.ThuongKhac;
            ws.Cell(row, 10).Value = (double)r.PhatKhac;
            ws.Cell(row, 11).Value = (double)r.BaoHiemXaHoi;
            ws.Cell(row, 12).Value = (double)r.ThueTNCN;
            ws.Cell(row, 13).Value = (double)r.KhauTruKhac;
            ws.Cell(row, 14).Value = (double)r.LuongThucNhan;
            for (int c = 4; c <= 14; c++) ws.Cell(row, c).Style.NumberFormat.Format = "#,##0";
            row++;
        }
        ws.Columns().AdjustToContents();
        wb.SaveAs(filePath);
    }
}
