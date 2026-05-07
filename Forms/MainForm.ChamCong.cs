using QuanLyLuong.Models;
using QuanLyLuong.Services;

namespace QuanLyLuong.Forms;

public partial class MainForm
{
    private DataGridView dgvCC = new();
    private NumericUpDown numCCThang = new(), numCCNam = new();
    private Label lblCCFooter = new();

    private void BuildTabChamCong()
    {
        var toolbar = MakeToolbar();
        numCCThang = new NumericUpDown { Width = 55, Minimum = 1, Maximum = 12, Value = DateTime.Today.Month };
        numCCNam   = new NumericUpDown { Width = 75, Minimum = 2000, Maximum = 2100, Value = DateTime.Today.Year };

        var btnTaiCC  = MakeBtn("↺ Tải Dữ Liệu", AppTheme.Accent);
        var btnLuuCC  = MakeBtn("💾 Lưu Chấm Công", AppTheme.Success);
        var btnTaoTatCa = MakeBtn("⚡ Tạo Tất Cả NV", Color.FromArgb(100, 0, 180));

        var lT = new Label { Text = "Tháng:", Width = 52, TextAlign = ContentAlignment.MiddleCenter, Height = 28 };
        var lN = new Label { Text = "Năm:", Width = 40, TextAlign = ContentAlignment.MiddleCenter, Height = 28 };

        toolbar.Controls.AddRange(new Control[] { lT, numCCThang, lN, numCCNam, btnTaiCC, btnTaoTatCa, btnLuuCC });

        dgvCC = BuildGrid();
        dgvCC.ReadOnly = false;
        dgvCC.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
        dgvCC.Columns.AddRange(
            ColCenter("MaNhanVien", "Mã NV", 85),
            Col("HoTen", "Họ Tên", 160),
            Col("PhongBan", "Phòng Ban", 110),
            ColCenter("SoNgayCong", "Ngày C. Chuẩn", 95),
            ColCenter("SoNgayLam", "Ngày Thực Tế", 95),
            ColCenter("SoNgayNghiPhep", "Nghỉ Phép", 80),
            ColCenter("SoNgayNghiKhongPhep", "Nghỉ KP", 75),
            ColCenter("SoLanDiTre", "Đi Trễ", 65),
            ColCenter("GioLamThem", "Giờ OT", 70),
            Col("GhiChu", "Ghi Chú", 150)
        );

        // Make editable columns
        foreach (var editCol in new[] { "SoNgayCong", "SoNgayLam", "SoNgayNghiPhep", "SoNgayNghiKhongPhep", "SoLanDiTre", "GioLamThem", "GhiChu" })
            if (dgvCC.Columns.Contains(editCol)) dgvCC.Columns[editCol]!.ReadOnly = false;
        foreach (var readCol in new[] { "MaNhanVien", "HoTen", "PhongBan" })
            if (dgvCC.Columns.Contains(readCol)) dgvCC.Columns[readCol]!.ReadOnly = true;

        lblCCFooter = MakeFooter();

        tpCC.Controls.Add(dgvCC);
        tpCC.Controls.Add(lblCCFooter);
        tpCC.Controls.Add(toolbar);

        btnTaiCC.Click += (s, e) => LoadChamCong();
        btnTaoTatCa.Click += BtnTaoTatCaCC_Click;
        btnLuuCC.Click += BtnLuuChamCong_Click;
        numCCThang.ValueChanged += (s, e) => LoadChamCong();
        numCCNam.ValueChanged += (s, e) => LoadChamCong();
    }

    private void BtnTaoTatCaCC_Click(object? sender, EventArgs e)
    {
        int t = (int)numCCThang.Value, n = (int)numCCNam.Value;
        var employees = DataService.GetEmployees().Where(x => x.ConHieuLuc).ToList();
        foreach (var emp in employees)
        {
            if (DataService.GetAttendance(emp.Id, t, n) == null)
                DataService.UpsertAttendance(new AttendanceRecord
                {
                    EmployeeId = emp.Id, MaNhanVien = emp.MaNhanVien,
                    HoTen = emp.HoTen, PhongBan = emp.PhongBan,
                    Thang = t, Nam = n, SoNgayCong = 26, SoNgayLam = 26
                });
        }
        LoadChamCong();
        MessageBox.Show($"Đã tạo bảng chấm công cho {employees.Count} nhân viên tháng {t}/{n}!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnLuuChamCong_Click(object? sender, EventArgs e)
    {
        int t = (int)numCCThang.Value, n = (int)numCCNam.Value;
        int saved = 0;
        foreach (DataGridViewRow row in dgvCC.Rows)
        {
            if (row.DataBoundItem is not AttendanceRecord rec) continue;
            try
            {
                rec.SoNgayCong = Convert.ToDecimal(row.Cells["SoNgayCong"].Value ?? 26);
                rec.SoNgayLam  = Convert.ToDecimal(row.Cells["SoNgayLam"].Value ?? 26);
                rec.SoNgayNghiPhep = Convert.ToDecimal(row.Cells["SoNgayNghiPhep"].Value ?? 0);
                rec.SoNgayNghiKhongPhep = Convert.ToDecimal(row.Cells["SoNgayNghiKhongPhep"].Value ?? 0);
                rec.SoLanDiTre = Convert.ToDecimal(row.Cells["SoLanDiTre"].Value ?? 0);
                rec.GioLamThem = Convert.ToDecimal(row.Cells["GioLamThem"].Value ?? 0);
                rec.GhiChu = row.Cells["GhiChu"].Value?.ToString() ?? "";
                DataService.UpsertAttendance(rec);
                saved++;
            }
            catch { }
        }
        MessageBox.Show($"Đã lưu chấm công cho {saved} nhân viên!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    internal void LoadChamCong()
    {
        int t = (int)numCCThang.Value, n = (int)numCCNam.Value;
        var recs = DataService.GetAttendanceByMonth(t, n);
        dgvCC.DataSource = recs;
        lblCCFooter.Text = $"Tháng {t}/{n}  |  Tổng {recs.Count} nhân viên  |  TB ngày làm: {(recs.Count > 0 ? recs.Average(r => r.SoNgayLam):0):F1}  |  Tổng giờ OT: {recs.Sum(r => r.GioLamThem):F1}h  ";
    }
}
