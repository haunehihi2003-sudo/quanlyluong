using QuanLyLuong.Models;
using QuanLyLuong.Services;

namespace QuanLyLuong.Forms;

public partial class MainForm
{
    private DataGridView dgvNV = new();
    private TextBox txtSearch = new();
    private ComboBox cboPhongBan = new();
    private Label lblNVFooter = new();
    private bool _loadingNV = false;

    private void BuildTabNhanVien()
    {
        var toolbar = MakeToolbar();
        txtSearch = new TextBox { Width = 200, Height = 28, PlaceholderText = "Tìm tên hoặc mã NV…" };
        cboPhongBan = new ComboBox { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
        cboPhongBan.Items.Add("Tất cả phòng ban");
        cboPhongBan.SelectedIndex = 0;

        var btnTim    = MakeBtn("🔍 Tìm", AppTheme.Accent);
        var btnLamMoi = MakeBtn("↺ Làm Mới", Color.Gray);
        var btnThem   = MakeBtn("＋ Thêm NV", AppTheme.Success);
        var btnSua    = MakeBtn("✏ Sửa", AppTheme.Warning);
        var btnXoa    = MakeBtn("✖ Xóa", AppTheme.Danger);
        var btnLS     = MakeBtn("📋 Lịch Sử", Color.FromArgb(80, 0, 160));
        var btnNhap   = MakeBtn("📥 Nhập Excel", Color.FromArgb(0, 130, 80));
        var btnMau    = MakeBtn("📄 Tải Mẫu", Color.FromArgb(100, 100, 100));

        toolbar.Controls.AddRange(new Control[] { txtSearch, cboPhongBan, btnTim, btnLamMoi, btnThem, btnSua, btnXoa, btnLS, btnNhap, btnMau });

        dgvNV = BuildGrid();
        dgvNV.Columns.AddRange(
            ColCenter("MaNhanVien", "Mã NV", 85),
            Col("HoTen", "Họ Tên", 160),
            ColCenter("GioiTinh", "G.Tính", 60),
            Col("ChucVu", "Chức Vụ", 120),
            Col("PhongBan", "Phòng Ban", 120),
            ColMoney("LuongCoBan", "Lương Cơ Bản", 120),
            ColMoney("PhuCap", "Phụ Cấp", 95),
            Col("SoDienThoai", "SĐT", 110),
            Col("LoaiHopDong", "Loại HĐ", 110),
            Col("NgayHetHopDong", "Hết HĐ", 100),
            ColCenter("ConHieuLuc", "Hiệu Lực", 80)
        );
        if (dgvNV.Columns.Contains("NgayHetHopDong"))
            dgvNV.Columns["NgayHetHopDong"]!.DefaultCellStyle.Format = "dd/MM/yyyy";

        lblNVFooter = MakeFooter();

        tpNV.Controls.Add(dgvNV);
        tpNV.Controls.Add(lblNVFooter);
        tpNV.Controls.Add(toolbar);

        btnThem.Click   += (s, e) => { new EmployeeForm().ShowDialog(this); LoadNhanVien(); };
        btnSua.Click    += BtnSuaNV_Click;
        btnXoa.Click    += BtnXoaNV_Click;
        btnLS.Click     += BtnLichSu_Click;
        btnTim.Click    += (s, e) => LoadNhanVien();
        btnLamMoi.Click += (s, e) => { txtSearch.Clear(); cboPhongBan.SelectedIndex = 0; LoadNhanVien(); };
        btnNhap.Click   += BtnNhapExcel_Click;
        btnMau.Click    += BtnTaiMau_Click;
        txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadNhanVien(); };
        // Attach AFTER items loaded so index change during init doesn't fire
        cboPhongBan.SelectedIndexChanged += (s, e) => { if (!_loadingNV) LoadNhanVien(); };
        dgvNV.CellDoubleClick += BtnSuaNV_Click;

        dgvNV.CellFormatting += (s, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgvNV.Columns[e.ColumnIndex].Name == "ConHieuLuc")
            {
                e.Value = (bool)(e.Value ?? false) ? "✔" : "✘";
                e.FormattingApplied = true;
            }
            if (dgvNV.Columns[e.ColumnIndex].Name == "NgayHetHopDong")
            {
                if (e.Value is DateTime dt && dt < DateTime.Today.AddDays(30))
                    e.CellStyle!.ForeColor = Color.Red;
            }
        };
    }

    private void BtnSuaNV_Click(object? sender, EventArgs e)
    {
        if (dgvNV.CurrentRow?.DataBoundItem is not Employee emp) return;
        new EmployeeForm(emp).ShowDialog(this);
        LoadNhanVien();
    }

    private void BtnXoaNV_Click(object? sender, EventArgs e)
    {
        if (dgvNV.CurrentRow?.DataBoundItem is not Employee emp) return;
        if (MessageBox.Show($"Xóa nhân viên '{emp.HoTen}'?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        DataService.DeleteEmployee(emp.Id);
        LoadNhanVien();
    }

    private void BtnLichSu_Click(object? sender, EventArgs e)
    {
        if (dgvNV.CurrentRow?.DataBoundItem is not Employee emp) return;
        new HistoryForm(emp).ShowDialog(this);
    }

    private void BtnNhapExcel_Click(object? sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog { Filter = "Excel Files|*.xlsx", Title = "Chọn file Excel nhân viên" };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        try
        {
            var (imported, errors) = ExcelService.ImportEmployees(dlg.FileName);
            var existing = DataService.GetEmployees();
            int added = 0, skipped = 0;
            foreach (var imp in imported)
            {
                if (existing.Any(ex => ex.MaNhanVien == imp.MaNhanVien)) { skipped++; continue; }
                DataService.AddEmployee(imp); added++;
            }
            var msg = $"Nhập xong!\n✔ Thêm mới: {added} nhân viên\n⚠ Bỏ qua (trùng mã): {skipped}";
            if (errors.Count > 0) msg += $"\n✖ Lỗi: {errors.Count} dòng";
            MessageBox.Show(msg, "Kết quả nhập", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadNhanVien();
        }
        catch (Exception ex) { MessageBox.Show($"Lỗi đọc file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void BtnTaiMau_Click(object? sender, EventArgs e)
    {
        using var dlg = new SaveFileDialog { Filter = "Excel|*.xlsx", FileName = "MauNhapNhanVien.xlsx", Title = "Lưu file mẫu" };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        ExcelService.CreateImportTemplate(dlg.FileName);
        MessageBox.Show($"Đã tạo file mẫu tại:\n{dlg.FileName}", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    internal void LoadNhanVien()
    {
        if (_loadingNV) return;
        _loadingNV = true;
        try
        {
            var all    = DataService.GetEmployees();
            var q      = txtSearch.Text.Trim().ToLower();
            var list   = string.IsNullOrEmpty(q)
                ? all
                : all.Where(e => e.HoTen.ToLower().Contains(q) || e.MaNhanVien.ToLower().Contains(q)).ToList();

            var selPB = cboPhongBan.SelectedItem?.ToString() ?? "Tất cả phòng ban";
            if (selPB != "Tất cả phòng ban")
                list = list.Where(e => e.PhongBan == selPB).ToList();

            dgvNV.DataSource = list;

            // Refresh department dropdown without triggering events
            cboPhongBan.Items.Clear();
            cboPhongBan.Items.Add("Tất cả phòng ban");
            foreach (var pb in all.Select(e => e.PhongBan).Where(p => p.Length > 0).Distinct().OrderBy(x => x))
                cboPhongBan.Items.Add(pb);

            // Restore selection
            int idx = cboPhongBan.Items.IndexOf(selPB);
            cboPhongBan.SelectedIndex = idx >= 0 ? idx : 0;

            lblNVFooter.Text = $"Tổng: {all.Count} NV  |  Đang làm: {all.Count(e => e.ConHieuLuc)}  |  Sắp hết HĐ: {all.Count(e => e.NgayHetHopDong < DateTime.Today.AddDays(30) && e.ConHieuLuc)}  ";
        }
        finally { _loadingNV = false; }
    }
}
