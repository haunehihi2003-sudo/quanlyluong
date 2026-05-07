using PdfSharp.Drawing;
using PdfSharp.Pdf;
using QuanLyLuong.Models;

namespace QuanLyLuong.Services;

public static class PdfService
{
    public static string ExportPhieuLuong(SalaryRecord r, string companyName = "CÔNG TY TNHH ABC")
    {
        var doc = new PdfDocument();
        doc.Info.Title = $"PhieuLuong_{r.MaNhanVien}_{r.Thang}_{r.Nam}";

        var page = doc.AddPage();
        page.Width  = XUnit.FromMillimeter(210);
        page.Height = XUnit.FromMillimeter(148);

        var gfx = XGraphics.FromPdfPage(page);

        XFont fTitle  = new("Arial", 14, XFontStyleEx.Bold);
        XFont fBold   = new("Arial", 10, XFontStyleEx.Bold);
        XFont fNormal = new("Arial", 10);
        XFont fSmall  = new("Arial", 9);

        double W = page.Width.Point;
        double lx = 30, rx = W / 2 + 10;
        double y = 20;

        void CenterText(string text, XFont font, double yy)
        {
            var sz = gfx.MeasureString(text, font);
            gfx.DrawString(text, font, XBrushes.Black, (W - sz.Width) / 2, yy);
        }

        void Row(string label, string value, double yy, bool bold = false)
        {
            gfx.DrawString(label, bold ? fBold : fNormal, XBrushes.Black, lx, yy);
            gfx.DrawString(value, bold ? fBold : XBrushes.Black is XBrush ? fNormal : fNormal,
                bold ? XBrushes.DarkRed : XBrushes.Black, rx, yy);
        }

        void Line(double yy) =>
            gfx.DrawLine(new XPen(XColors.LightGray, 0.5), lx, yy, W - lx, yy);

        CenterText(companyName, fBold, y); y += 18;
        CenterText($"PHIEU LUONG THANG {r.Thang:D2}/{r.Nam}", fTitle, y); y += 6;
        Line(y); y += 12;

        gfx.DrawString($"Ma NV : {r.MaNhanVien}", fNormal, XBrushes.Black, lx, y);
        gfx.DrawString($"Ho Ten: {r.HoTen}", fNormal, XBrushes.Black, rx, y); y += 14;
        gfx.DrawString($"Phong Ban: {r.PhongBan}", fNormal, XBrushes.Black, lx, y); y += 8;
        Line(y); y += 10;

        Row("Luong co ban:", $"{r.LuongCoBan:N0} VND", y); y += 14;
        Row("Phu cap:", $"{r.PhuCap:N0} VND", y); y += 14;
        Row($"Ngay lam ({r.SoNgayLam}/{r.SoNgayCong}):", $"{(r.SoNgayCong > 0 ? r.LuongCoBan / r.SoNgayCong * r.SoNgayLam : 0):N0} VND", y); y += 14;
        Row($"Gio lam them ({r.GioLamThem}h):", $"{r.TienLamThem:N0} VND", y); y += 14;
        Row("Thuong:", $"{r.ThuongKhac:N0} VND", y); y += 8;
        Line(y); y += 10;
        Row("BHXH (10.5%):", $"- {r.BaoHiemXaHoi:N0} VND", y); y += 14;
        Row("Thue TNCN (10%):", $"- {r.ThueTNCN:N0} VND", y); y += 14;
        Row("Phat / Khau tru:", $"- {r.PhatKhac + r.KhauTruKhac:N0} VND", y); y += 8;
        Line(y); y += 10;
        Row("LUONG THUC NHAN:", $"{r.LuongThucNhan:N0} VND", y, true); y += 16;
        Line(y); y += 12;

        gfx.DrawString("Ke toan truong", fSmall, XBrushes.Gray, lx, y);
        gfx.DrawString("Nguoi nhan luong", fSmall, XBrushes.Gray, rx, y); y += 50;
        gfx.DrawString("(Ky, ghi ro ho ten)", fSmall, XBrushes.LightGray, lx, y);
        gfx.DrawString("(Ky, ghi ro ho ten)", fSmall, XBrushes.LightGray, rx, y);

        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var path = Path.Combine(desktop, $"PhieuLuong_{r.MaNhanVien}_{r.Thang:D2}_{r.Nam}.pdf");
        doc.Save(path);
        return path;
    }

    public static string ExportBangLuong(List<SalaryRecord> records, int thang, int nam, string companyName = "CONG TY TNHH ABC")
    {
        var doc = new PdfDocument();
        doc.Info.Title = $"BangLuong_{thang}_{nam}";

        var page = doc.AddPage();
        page.Orientation = PdfSharp.PageOrientation.Landscape;
        page.Width  = XUnit.FromMillimeter(297);
        page.Height = XUnit.FromMillimeter(210);

        var gfx = XGraphics.FromPdfPage(page);
        XFont fTitle  = new("Arial", 13, XFontStyleEx.Bold);
        XFont fHdr    = new("Arial", 8, XFontStyleEx.Bold);
        XFont fCell   = new("Arial", 8);

        double W = page.Width.Point;
        double lx = 20, y = 18;

        var sz = gfx.MeasureString($"BANG LUONG THANG {thang:D2}/{nam}", fTitle);
        gfx.DrawString($"BANG LUONG THANG {thang:D2}/{nam}", fTitle, XBrushes.Black, (W - sz.Width) / 2, y);
        y += 18;

        double[] cols = { 30, 100, 80, 70, 55, 55, 55, 55, 55, 55, 75 };
        string[] hdrs = { "Ma NV", "Ho Ten", "Phong Ban", "Luong CB", "Phu Cap", "OT", "Thuong", "BHXH", "Thue", "Phat", "Thuc Nhan" };

        double cx = lx;
        for (int i = 0; i < hdrs.Length; i++)
        {
            gfx.DrawRectangle(new XPen(XColors.Black, 0.3), new XSolidBrush(XColor.FromArgb(0, 90, 160)), cx, y, cols[i], 14);
            gfx.DrawString(hdrs[i], fHdr, XBrushes.White, cx + 2, y + 10);
            cx += cols[i];
        }
        y += 14;

        foreach (var r in records)
        {
            string[] vals = { r.MaNhanVien, r.HoTen, r.PhongBan, $"{r.LuongCoBan:N0}", $"{r.PhuCap:N0}", $"{r.TienLamThem:N0}", $"{r.ThuongKhac:N0}", $"{r.BaoHiemXaHoi:N0}", $"{r.ThueTNCN:N0}", $"{r.PhatKhac + r.KhauTruKhac:N0}", $"{r.LuongThucNhan:N0}" };
            cx = lx;
            for (int i = 0; i < vals.Length; i++)
            {
                gfx.DrawRectangle(new XPen(XColors.LightGray, 0.3), cx, y, cols[i], 13);
                gfx.DrawString(vals[i], fCell, XBrushes.Black, cx + 2, y + 9);
                cx += cols[i];
            }
            y += 13;
            if (y > page.Height.Point - 30) break;
        }

        decimal tong = records.Sum(r => r.LuongThucNhan);
        y += 8;
        gfx.DrawString($"Tong luong thuc nhan: {tong:N0} VND  |  So nhan vien: {records.Count}", fHdr, XBrushes.Black, lx, y);

        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var path = Path.Combine(desktop, $"BangLuong_{thang:D2}_{nam}.pdf");
        doc.Save(path);
        return path;
    }
}
