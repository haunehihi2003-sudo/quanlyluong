using QuanLyLuong.Models;
using QuanLyLuong.Services;

namespace QuanLyLuong.Forms;

public partial class MainForm
{
    private DataGridView dgvBL = new();
    private NumericUpDown numBLThang = new(), numBLNam = new();
    private Label lblBLFooter = new();

    private void BuildTabBangLuong()
    {
        var toolbar = MakeToolbar();
        numBLThang = new NumericUpDown { Width = 55, Minimum = 1, Maximum = 12, Value = DateTime.Today.Month };
        numBLNam   = new NumericUpDown { Width = 75, Minimum = 2000, Maximum = 2100, Value = DateTime.Today.Year };

        var btnTinh    = MakeBtn("＋ Tính Lương NV", AppTheme.Success);
        var btnTinhAll = MakeBtn("⚡ Tính Tất Cả", Color.FromArgb(0, 130, 160));
        var btnXoa     = MakeBtn("✖ Xóa Dòng", AppTheme.Danger);
        var btnPDF     = MakeBtn("📄 Xuất PDF", Color.FromArgb(180, 0, 0));
        var btnExcel   = MakeBtn("📊 Xuất Excel", Color.FromArgb(0, 130, 70));
        var btnCSV     = MakeBtn("📥 Xuất CSV", Color.Gray);
        var btnTai     = MakeBtn("↺ Tải Lại", AppTheme.Accent);

        var lT = new Label { Text = "Tháng:", Width = 52, TextAlign = ContentAlignment.MiddleCenter, Height = 28 };
        var lN = new Label { Text = "Năm:", Width = 40, TextAlign = ContentAlignment.MiddleCenter, Height = 28 };
        toolbar.Controls.AddRange(new Control[] { lT, numBLThang, lN, numBLNam, btnTinh, btnTinhAll, btnXoa, btnPDF, btnExcel, btnCSV, btnTai });

        dgvBL = BuildGrid();
        dgvBL.Columns.AddRange(
            ColCenter("MaNhanVien", "Mã NV", 80),
            Col("HoTen", "Họ Tên", 145),
            Col("PhongBan", "Phòng Ban", 105),
            ColMoney("LuongCoBan", "Lương CB", 110),
            ColMoney("PhuCap", "Phụ Cấp", 90),
            ColCenter("SoNgayLam", "Ngày", 60),
            ColMoney("TienLamThem", "OT", 90),
            ColMoney("ThuongKhac", "Thưởng", 90),
            ColMoney("PhatKhac", "Phạt", 80),
            ColMoney("BaoHiemXaHoi", "BHXH", 90),
            ColMoney("ThueTNCN", "Thuế", 90),
            ColMoney("LuongThucNhan", "Thực Nhận", 115),
            Col("GhiChu", "Ghi Chú", 100)
        );
        if (dgvBL.Columns.Contains("LuongThucNhan"))
        {
            dgvBL.Columns["LuongThucNhan"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgvBL.Columns["LuongThucNhan"]!.DefaultCellStyle.ForeColor = Color.FromArgb(150, 0, 0);
        }

        lblBLFooter = MakeFooter();

        tpBL.Controls.Add(dgvBL);
        tpBL.Controls.Add(lblBLFooter);
        tpBL.Controls.Add(toolbar);

        btnTinh.Click += BtnTinhLuong_Click;
        btnTinhAll.Click += BtnTinhTatCa_Click;
        btnXoa.Click += BtnXoaBL_Click;
        btnPDF.Click += BtnXuatPDFBL_Click;
        btnExcel.Click += BtnXuatExcelBL_Click;
        btnCSV.Click += (s, e) => {
            var recs = DataService.GetSalaryByMonth((int)numBLThang.Value, (int)numBLNam.Value);
            if (recs.Count == 0) { MessageBox.Show("Không có dữ liệu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            var path = DataService.ExportCsv((int)numBLThang.Value, (int)numBLNam.Value);
            MessageBox.Show($"Xuất CSV thành công!\n{path}", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };
        btnTai.Click += (s, e) => LoadBangLuong();
        numBLThang.ValueChanged += (s, e) => LoadBangLuong();
        numBLNam.ValueChanged += (s, e) => LoadBangLuong();
        dgvBL.CellDoubleClick += (s, e) =>
        {
            if (dgvBL.CurrentRow?.DataBoundItem is not SalaryRecord rec) return;
            var emp = DataService.GetEmployee(rec.EmployeeId);
            if (emp == null) return;
            new SalaryForm(emp, (int)numBLThang.Value, (int)numBLNam.Value, rec).ShowDialog(this);
            LoadBangLuong();
        };
    }

    private void BtnTinhLuong_Click(object? sender, EventArgs e)
    {
        var employees = DataService.GetEmployees().Where(x => x.ConHieuLuc).ToList();
        if (employees.Count == 0) { MessageBox.Show("Chưa có nhân viên!"); return; }
        using var picker = new Form { Text = "Chọn Nhân Viên", Size = new Size(340, 400), StartPosition = FormStartPosition.CenterParent, Font = new Font("Segoe UI", 9.5f) };
        var lb = new ListBox { Dock = DockStyle.Fill, SelectionMode = SelectionMode.One };
        foreach (var emp in employees) lb.Items.Add($"[{emp.MaNhanVien}] {emp.HoTen} – {emp.PhongBan}");
        var btnOK = MakeBtn("Chọn & Tính Lương", AppTheme.Success);
        btnOK.Dock = DockStyle.Bottom; btnOK.Height = 38;
        picker.Controls.Add(lb); picker.Controls.Add(btnOK);
        btnOK.Click += (s2, e2) =>
        {
            if (lb.SelectedIndex < 0) return;
            var emp = employees[lb.SelectedIndex];
            picker.Close();
            int t = (int)numBLThang.Value, n = (int)numBLNam.Value;
            var existing = DataService.GetSalaryRecords().FirstOrDefault(r => r.EmployeeId == emp.Id && r.Thang == t && r.Nam == n);
            new SalaryForm(emp, t, n, existing).ShowDialog(this);
            LoadBangLuong();
        };
        picker.ShowDialog(this);
    }

    private void BtnTinhTatCa_Click(object? sender, EventArgs e)
    {
        int t = (int)numBLThang.Value, n = (int)numBLNam.Value;
        var employees = DataService.GetEmployees().Where(x => x.ConHieuLuc).ToList();
        if (employees.Count == 0) { MessageBox.Show("Chưa có nhân viên!"); return; }
        if (MessageBox.Show($"Tự động tính lương cho {employees.Count} nhân viên tháng {t}/{n}?\n(Sẽ dùng dữ liệu chấm công nếu có)", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        int done = 0;
        foreach (var emp in employees)
        {
            var att = DataService.GetAttendance(emp.Id, t, n);
            var existing = DataService.GetSalaryRecords().FirstOrDefault(r => r.EmployeeId == emp.Id && r.Thang == t && r.Nam == n);
            var bp = DataService.GetBonusPenaltiesByMonth(t, n).Where(b => b.EmployeeId == emp.Id).ToList();
            decimal ngayLam = att?.SoNgayLam ?? 26, ngayCong = att?.SoNgayCong ?? 26;
            decimal gioOT = att?.GioLamThem ?? 0;
            decimal donGia = emp.LuongCoBan > 0 ? Math.Round(emp.LuongCoBan / 26 / 8 * 1.5m, 0) : 0;
            decimal ot = gioOT * donGia;
            decimal thuong = bp.Where(b => b.LoaiTP == "Thưởng").Sum(b => b.SoTien);
            decimal phat = bp.Where(b => b.LoaiTP == "Phạt").Sum(b => b.SoTien);
            decimal luongNgay = ngayCong > 0 ? emp.LuongCoBan / ngayCong * ngayLam : 0;
            decimal tong = luongNgay + emp.PhuCap + ot + thuong - phat;
            decimal bhxh = Math.Round(emp.LuongCoBan * 0.105m, 0);
            decimal thue = tong > 11_000_000 ? Math.Round((tong - 11_000_000) * 0.10m, 0) : 0;
            var r = existing ?? new SalaryRecord { EmployeeId = emp.Id, MaNhanVien = emp.MaNhanVien, HoTen = emp.HoTen, PhongBan = emp.PhongBan, Thang = t, Nam = n };
            r.LuongCoBan = emp.LuongCoBan; r.PhuCap = emp.PhuCap;
            r.SoNgayCong = ngayCong; r.SoNgayLam = ngayLam;
            r.SoNgayNghiPhep = att?.SoNgayNghiPhep ?? 0; r.SoNgayNghiKhongPhep = att?.SoNgayNghiKhongPhep ?? 0;
            r.GioLamThem = gioOT; r.TienLamThem = ot; r.ThuongKhac = thuong; r.PhatKhac = phat;
            r.BaoHiemXaHoi = bhxh; r.ThueTNCN = thue;
            r.LuongThucNhan = tong - bhxh - thue;
            DataService.UpsertSalaryRecord(r);
            done++;
        }
        MessageBox.Show($"Đã tính lương cho {done} nhân viên tháng {t}/{n}!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        LoadBangLuong();
    }

    private void BtnXoaBL_Click(object? sender, EventArgs e)
    {
        if (dgvBL.CurrentRow?.DataBoundItem is not SalaryRecord rec) return;
        if (MessageBox.Show($"Xóa bảng lương của '{rec.HoTen}' tháng {rec.Thang}/{rec.Nam}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        DataService.DeleteSalaryRecord(rec.Id);
        LoadBangLuong();
    }

    private void BtnXuatPDFBL_Click(object? sender, EventArgs e)
    {
        int t = (int)numBLThang.Value, n = (int)numBLNam.Value;
        var recs = DataService.GetSalaryByMonth(t, n);
        if (recs.Count == 0) { MessageBox.Show("Không có dữ liệu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        if (dgvBL.CurrentRow?.DataBoundItem is SalaryRecord single)
        {
            var res = MessageBox.Show("Xuất phiếu lương cho nhân viên đang chọn?\n(Nhấn No để xuất toàn bộ bảng lương)", "Chọn kiểu xuất", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (res == DialogResult.Cancel) return;
            if (res == DialogResult.Yes)
            {
                try { var p = PdfService.ExportPhieuLuong(single); MessageBox.Show($"Đã xuất phiếu lương!\n{p}"); return; }
                catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}"); return; }
            }
        }
        try { var p = PdfService.ExportBangLuong(recs, t, n); MessageBox.Show($"Đã xuất bảng lương PDF!\n{p}"); }
        catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}"); }
    }

    private void BtnXuatExcelBL_Click(object? sender, EventArgs e)
    {
        int t = (int)numBLThang.Value, n = (int)numBLNam.Value;
        var recs = DataService.GetSalaryByMonth(t, n);
        if (recs.Count == 0) { MessageBox.Show("Không có dữ liệu!"); return; }
        using var dlg = new SaveFileDialog { Filter = "Excel|*.xlsx", FileName = $"BangLuong_{t:D2}_{n}.xlsx" };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        ExcelService.ExportSalaryToExcel(recs, t, n, dlg.FileName);
        MessageBox.Show($"Xuất Excel thành công!\n{dlg.FileName}");
    }

    internal void LoadBangLuong()
    {
        int t = (int)numBLThang.Value, n = (int)numBLNam.Value;
        var recs = DataService.GetSalaryByMonth(t, n);
        dgvBL.DataSource = recs;
        decimal tong = recs.Sum(r => r.LuongThucNhan);
        decimal tongBHXH = recs.Sum(r => r.BaoHiemXaHoi);
        decimal tongThue = recs.Sum(r => r.ThueTNCN);
        lblBLFooter.Text = $"Tháng {t}/{n}  |  {recs.Count} NV  |  Tổng thực nhận: {tong:N0} VNĐ  |  BHXH: {tongBHXH:N0}  |  Thuế: {tongThue:N0}  ";
    }
}
