using QuanLyLuong.Models;
using QuanLyLuong.Services;

namespace QuanLyLuong.Forms;

public partial class MainForm
{
    private DataGridView dgvNP = new();
    private ComboBox cboNPTrangThai = new(), cboNPNhanVien = new();
    private Label lblNPFooter = new();
    private bool _loadingNP = false;

    private void BuildTabNghiPhep()
    {
        var toolbar = MakeToolbar();
        cboNPTrangThai = new ComboBox { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
        cboNPTrangThai.Items.AddRange(new object[] { "Tất cả", "Chờ duyệt", "Đã duyệt", "Từ chối" });
        cboNPTrangThai.SelectedIndex = 0;

        cboNPNhanVien = new ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        cboNPNhanVien.Items.Add("Tất cả nhân viên");
        cboNPNhanVien.SelectedIndex = 0;

        var btnThem   = MakeBtn("＋ Tạo Đơn Nghỉ", AppTheme.Success);
        var btnDuyet  = MakeBtn("✔ Duyệt", Color.FromArgb(0, 130, 100));
        var btnTuChoi = MakeBtn("✘ Từ Chối", AppTheme.Danger);
        var btnXoa    = MakeBtn("✖ Xóa", Color.Gray);
        var btnLamMoi = MakeBtn("↺ Làm Mới", Color.Gray);

        var lNV = new Label { Text = "NV:", Width = 30, TextAlign = ContentAlignment.MiddleCenter, Height = 28 };
        var lTS = new Label { Text = "Trạng thái:", Width = 78, TextAlign = ContentAlignment.MiddleCenter, Height = 28 };

        toolbar.Controls.AddRange(new Control[] { btnThem, lNV, cboNPNhanVien, lTS, cboNPTrangThai, btnDuyet, btnTuChoi, btnXoa, btnLamMoi });

        dgvNP = BuildGrid();
        dgvNP.Columns.AddRange(
            ColCenter("MaNhanVien", "Mã NV", 80),
            Col("HoTen", "Họ Tên", 150),
            Col("PhongBan", "Phòng Ban", 110),
            Col("LoaiNghi", "Loại Nghỉ", 120),
            Col("NgayBatDau", "Từ Ngày", 100),
            Col("NgayKetThuc", "Đến Ngày", 100),
            ColCenter("SoNgay", "Số Ngày", 75),
            Col("LyDo", "Lý Do", 160),
            Col("TrangThai", "Trạng Thái", 100),
            Col("NgayTao", "Ngày Tạo", 120)
        );
        foreach (var dc in new[] { "NgayBatDau", "NgayKetThuc" })
            if (dgvNP.Columns.Contains(dc)) dgvNP.Columns[dc]!.DefaultCellStyle.Format = "dd/MM/yyyy";
        if (dgvNP.Columns.Contains("NgayTao")) dgvNP.Columns["NgayTao"]!.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";

        dgvNP.CellFormatting += (s, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgvNP.Columns[e.ColumnIndex].Name == "TrangThai")
            {
                e.CellStyle!.ForeColor = e.Value?.ToString() switch
                {
                    "Đã duyệt" => Color.FromArgb(0, 150, 80),
                    "Từ chối"  => Color.Red,
                    _          => Color.FromArgb(180, 120, 0)
                };
                e.FormattingApplied = true;
            }
        };

        lblNPFooter = MakeFooter();

        tpNP.Controls.Add(dgvNP);
        tpNP.Controls.Add(lblNPFooter);
        tpNP.Controls.Add(toolbar);

        btnThem.Click   += (s, e) => { new LeaveForm().ShowDialog(this); LoadNghiPhep(); };
        btnDuyet.Click  += (s, e) => SetTrangThaiNP("Đã duyệt");
        btnTuChoi.Click += (s, e) => SetTrangThaiNP("Từ chối");
        btnXoa.Click    += BtnXoaNP_Click;
        btnLamMoi.Click += (s, e) => { cboNPTrangThai.SelectedIndex = 0; cboNPNhanVien.SelectedIndex = 0; LoadNghiPhep(); };
        // Attach events AFTER controls are ready
        cboNPTrangThai.SelectedIndexChanged += (s, e) => { if (!_loadingNP) LoadNghiPhep(); };
        cboNPNhanVien.SelectedIndexChanged  += (s, e) => { if (!_loadingNP) LoadNghiPhep(); };
    }

    private void SetTrangThaiNP(string status)
    {
        if (dgvNP.CurrentRow?.DataBoundItem is not LeaveRecord rec) return;
        rec.TrangThai = status;
        DataService.UpdateLeaveRecord(rec);
        if (status == "Đã duyệt" && rec.LoaiNghi == "Nghỉ phép")
        {
            var emp = DataService.GetEmployee(rec.EmployeeId);
            if (emp != null) { emp.SoNgayPhepDaDung += (int)rec.SoNgay; DataService.UpdateEmployee(emp); }
        }
        LoadNghiPhep();
    }

    private void BtnXoaNP_Click(object? sender, EventArgs e)
    {
        if (dgvNP.CurrentRow?.DataBoundItem is not LeaveRecord rec) return;
        if (MessageBox.Show("Xóa đơn nghỉ này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        DataService.DeleteLeaveRecord(rec.Id);
        LoadNghiPhep();
    }

    internal void LoadNghiPhep()
    {
        if (_loadingNP) return;
        _loadingNP = true;
        try
        {
            var employees = DataService.GetEmployees();

            // Refresh employee dropdown safely
            var selNV = cboNPNhanVien.SelectedItem?.ToString() ?? "Tất cả nhân viên";
            cboNPNhanVien.Items.Clear();
            cboNPNhanVien.Items.Add("Tất cả nhân viên");
            foreach (var e in employees)
                cboNPNhanVien.Items.Add($"[{e.MaNhanVien}] {e.HoTen}");
            int nvIdx = cboNPNhanVien.Items.IndexOf(selNV);
            cboNPNhanVien.SelectedIndex = nvIdx >= 0 ? nvIdx : 0;

            var list = DataService.GetLeaveRecords();

            if (cboNPTrangThai.SelectedIndex > 0)
                list = list.Where(r => r.TrangThai == cboNPTrangThai.SelectedItem?.ToString()).ToList();

            if (cboNPNhanVien.SelectedIndex > 0 && cboNPNhanVien.SelectedIndex - 1 < employees.Count)
            {
                var empId = employees[cboNPNhanVien.SelectedIndex - 1].Id;
                list = list.Where(r => r.EmployeeId == empId).ToList();
            }

            list = list.OrderByDescending(r => r.NgayTao).ToList();
            dgvNP.DataSource = list;

            lblNPFooter.Text = $"Tổng {list.Count} đơn  |  Chờ duyệt: {list.Count(r => r.TrangThai == "Chờ duyệt")}  |  Đã duyệt: {list.Count(r => r.TrangThai == "Đã duyệt")}  |  Tổng ngày: {list.Where(r => r.TrangThai == "Đã duyệt").Sum(r => r.SoNgay):F1}  ";
        }
        finally { _loadingNP = false; }
    }
}
