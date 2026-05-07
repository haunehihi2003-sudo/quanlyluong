using QuanLyLuong.Models;
using QuanLyLuong.Services;

namespace QuanLyLuong.Forms;

public partial class MainForm
{
    private TabControl tabBC = new();
    private DataGridView dgvBCPhongBan = new(), dgvBCSoSanh = new();
    private NumericUpDown numBCThang = new(), numBCNam = new();
    private Label lblBCFooter = new();
    private List<PhongBanReport> _pbReports = new();
    private List<SoSanhRow> _ssRows = new();

    private void BuildTabBaoCao()
    {
        var toolbar = MakeToolbar();
        numBCThang = new NumericUpDown { Width = 55, Minimum = 1, Maximum = 12, Value = DateTime.Today.Month };
        numBCNam   = new NumericUpDown { Width = 75, Minimum = 2000, Maximum = 2100, Value = DateTime.Today.Year };

        var btnXem = MakeBtn("🔍 Xem Báo Cáo", AppTheme.Accent);
        var lT = new Label { Text = "Tháng:", Width = 52, TextAlign = ContentAlignment.MiddleCenter, Height = 28 };
        var lN = new Label { Text = "Năm:", Width = 40, TextAlign = ContentAlignment.MiddleCenter, Height = 28 };
        toolbar.Controls.AddRange(new Control[] { lT, numBCThang, lN, numBCNam, btnXem });

        tabBC = new TabControl { Dock = DockStyle.Fill };
        var tpPB = new TabPage("  Theo Phòng Ban  ");
        var tpSS = new TabPage("  So Sánh Tháng Trước  ");
        tabBC.TabPages.Add(tpPB);
        tabBC.TabPages.Add(tpSS);

        // ── Phòng Ban tab ──
        dgvBCPhongBan = BuildGrid();
        dgvBCPhongBan.Columns.AddRange(
            Col("PhongBan", "Phòng Ban", 160),
            ColCenter("SoNhanVien", "Số NV", 70),
            ColMoney("TongLuongCoBan", "Tổng Lương CB", 140),
            ColMoney("TongPhuCap", "Tổng Phụ Cấp", 120),
            ColMoney("TongLamThem", "Tổng OT", 110),
            ColMoney("TongBHXH", "Tổng BHXH", 110),
            ColMoney("TongThue", "Tổng Thuế", 110),
            ColMoney("TongThucNhan", "Tổng Thực Nhận", 140),
            ColMoney("TrungBinhThucNhan", "TB Thực Nhận/NV", 145)
        );
        if (dgvBCPhongBan.Columns.Contains("TongThucNhan"))
        {
            dgvBCPhongBan.Columns["TongThucNhan"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgvBCPhongBan.Columns["TongThucNhan"]!.DefaultCellStyle.ForeColor = Color.FromArgb(150, 0, 0);
        }
        tpPB.Controls.Add(dgvBCPhongBan);

        // ── So Sánh tab ──
        dgvBCSoSanh = BuildGrid();
        dgvBCSoSanh.Columns.AddRange(
            ColCenter("MaNhanVien", "Mã NV", 80),
            Col("HoTen", "Họ Tên", 160),
            Col("PhongBan", "Phòng Ban", 115),
            ColMoney("ThangTruoc", "Tháng Trước", 125),
            ColMoney("ThangNay", "Tháng Này", 125),
            ColMoney("ChenhLech", "Chênh Lệch", 115),
            ColCenter("PhanTram", "% Thay Đổi", 100)
        );
        dgvBCSoSanh.CellFormatting += (s, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgvBCSoSanh.Columns[e.ColumnIndex].Name == "ChenhLech")
            {
                if (e.Value is decimal d)
                    e.CellStyle!.ForeColor = d >= 0 ? Color.FromArgb(0, 140, 60) : Color.Red;
                e.FormattingApplied = true;
            }
        };
        tpSS.Controls.Add(dgvBCSoSanh);

        lblBCFooter = MakeFooter();
        tpBC.Controls.Add(tabBC);
        tpBC.Controls.Add(lblBCFooter);
        tpBC.Controls.Add(toolbar);

        btnXem.Click += (s, e) => LoadBaoCao();
        numBCThang.ValueChanged += (s, e) => LoadBaoCao();
        numBCNam.ValueChanged += (s, e) => LoadBaoCao();
    }

    internal void LoadBaoCao()
    {
        int t = (int)numBCThang.Value, n = (int)numBCNam.Value;
        var recs = DataService.GetSalaryByMonth(t, n);

        // Phòng ban report
        _pbReports = recs
            .GroupBy(r => r.PhongBan.Length > 0 ? r.PhongBan : "Chưa phân loại")
            .Select(g => new PhongBanReport
            {
                PhongBan            = g.Key,
                SoNhanVien          = g.Count(),
                TongLuongCoBan      = g.Sum(r => r.LuongCoBan),
                TongPhuCap          = g.Sum(r => r.PhuCap),
                TongLamThem         = g.Sum(r => r.TienLamThem),
                TongBHXH            = g.Sum(r => r.BaoHiemXaHoi),
                TongThue            = g.Sum(r => r.ThueTNCN),
                TongThucNhan        = g.Sum(r => r.LuongThucNhan),
                TrungBinhThucNhan   = g.Average(r => r.LuongThucNhan)
            })
            .OrderByDescending(x => x.TongThucNhan)
            .ToList();
        dgvBCPhongBan.DataSource = _pbReports;

        // So sánh tháng trước
        int prevT = t == 1 ? 12 : t - 1;
        int prevN = t == 1 ? n - 1 : n;
        var prevRecs = DataService.GetSalaryByMonth(prevT, prevN);

        _ssRows = recs
            .Select(r =>
            {
                var prev = prevRecs.FirstOrDefault(p => p.EmployeeId == r.EmployeeId);
                decimal prevSal = prev?.LuongThucNhan ?? 0;
                decimal diff = r.LuongThucNhan - prevSal;
                return new SoSanhRow
                {
                    MaNhanVien = r.MaNhanVien, HoTen = r.HoTen, PhongBan = r.PhongBan,
                    ThangTruoc = prevSal,
                    ThangNay   = r.LuongThucNhan,
                    ChenhLech  = diff,
                    PhanTram   = prevSal > 0 ? $"{diff / prevSal * 100:+0.0;-0.0}%" : "N/A"
                };
            })
            .OrderByDescending(x => Math.Abs(x.ChenhLech))
            .ToList();
        dgvBCSoSanh.DataSource = _ssRows;

        decimal tong = recs.Sum(r => r.LuongThucNhan);
        decimal avg  = recs.Count > 0 ? recs.Average(r => r.LuongThucNhan) : 0;
        lblBCFooter.Text = $"Tháng {t}/{n}  |  {recs.Count} NV  |  Tổng: {tong:N0} VNĐ  |  TB/NV: {avg:N0} VNĐ  |  Phòng ban: {_pbReports.Count}  ";
    }
}
