using QuanLyLuong.Models;
using QuanLyLuong.Services;

namespace QuanLyLuong.Forms;

public class SalaryForm : Form
{
    private readonly Employee _emp;
    private readonly int _t, _n;
    private readonly SalaryRecord? _existing;

    private NumericUpDown numLCB = new(), numPC = new(), numNgayCong = new(),
        numNgayLam = new(), numNgayPhep = new(), numNgayKP = new(),
        numGioOT = new(), numDonGia = new(), numThuong = new(), numPhat = new(), numKhauTru = new();
    private Label lblOT = new(), lblBHXH = new(), lblThue = new(), lblThucNhan = new();
    private TextBox txtGhiChu = new();

    public SalaryForm(Employee emp, int t, int n, SalaryRecord? existing = null)
    {
        _emp = emp; _t = t; _n = n; _existing = existing;
        Text = $"Tính Lương – {emp.HoTen} – Tháng {t}/{n}";
        Size = new Size(540, 620);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Font = new Font("Segoe UI", 9.5f);
        BackColor = Color.FromArgb(245, 247, 250);
        BuildUI();
        LoadDefaults();
        TinhLuong();
    }

    private void BuildUI()
    {
        var header = new Label
        {
            Text = $"  {_emp.HoTen}  |  {_emp.PhongBan}  |  Tháng {_t}/{_n}",
            Dock = DockStyle.Top, Height = 36,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            ForeColor = Color.White, BackColor = Color.FromArgb(0, 90, 160),
            TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0)
        };

        numLCB.Maximum = numPC.Maximum = numThuong.Maximum = numPhat.Maximum = numKhauTru.Maximum = numDonGia.Maximum = 999_999_999;
        foreach (var n in new[] { numLCB, numPC, numThuong, numPhat, numKhauTru, numDonGia })
        { n.ThousandsSeparator = true; n.DecimalPlaces = 0; }
        numNgayCong.Maximum = numNgayLam.Maximum = numNgayPhep.Maximum = numNgayKP.Maximum = 31;
        numGioOT.Maximum = 200;
        foreach (var n in new[] { numNgayCong, numNgayLam, numNgayPhep, numNgayKP, numGioOT })
        { n.DecimalPlaces = 1; n.Increment = 0.5m; }

        lblOT = MkResult(); lblBHXH = MkResult(); lblThue = MkResult();
        lblThucNhan = MkResult(true);

        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        var table = new TableLayoutPanel { ColumnCount = 2, AutoSize = true, Width = 480, Padding = new Padding(12, 10, 12, 10) };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        scroll.Controls.Add(table);

        int r = 0;
        void AR(string lbl, Control ctrl) { var l = new Label { Text = lbl, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }; ctrl.Dock = DockStyle.Fill; table.Controls.Add(l, 0, r); table.Controls.Add(ctrl, 1, r); r++; }
        void ALbl(string lbl, Label ctrl) { var l = new Label { Text = lbl, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }; ctrl.Dock = DockStyle.Fill; table.Controls.Add(l, 0, r); table.Controls.Add(ctrl, 1, r); r++; }
        void Sep() { var sep = new Label { Height = 1, Dock = DockStyle.Fill, BackColor = Color.LightGray, Margin = new Padding(0, 4, 0, 4) }; table.SetColumnSpan(sep, 2); table.Controls.Add(sep, 0, r); r++; }

        AR("Lương Cơ Bản (VNĐ):", numLCB);
        AR("Phụ Cấp (VNĐ):", numPC);
        Sep();
        AR("Số Ngày Công Chuẩn:", numNgayCong);
        AR("Số Ngày Làm Thực Tế:", numNgayLam);
        AR("Số Ngày Nghỉ Phép:", numNgayPhep);
        AR("Số Ngày Nghỉ Không Phép:", numNgayKP);
        Sep();
        AR("Giờ Làm Thêm:", numGioOT);
        AR("Đơn Giá Làm Thêm/Giờ:", numDonGia);
        ALbl("→ Tiền Làm Thêm:", lblOT);
        AR("Thưởng (VNĐ):", numThuong);
        AR("Phạt (VNĐ):", numPhat);
        AR("Khấu Trừ Khác:", numKhauTru);
        Sep();
        ALbl("→ BHXH (10.5% lương CB):", lblBHXH);
        ALbl("→ Thuế TNCN (10% > 11tr):", lblThue);
        Sep();
        ALbl("LƯƠNG THỰC NHẬN:", lblThucNhan);
        Sep();
        AR("Ghi Chú:", txtGhiChu);

        var btnRow = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Bottom, Height = 46, Padding = new Padding(8) };
        var btnHuy = MakeBtn("Hủy", Color.Gray);
        var btnLuu = MakeBtn("Lưu Lương", Color.FromArgb(0, 150, 80));
        var btnTinh = MakeBtn("Tính Lại", Color.FromArgb(0, 120, 215));
        btnRow.Controls.AddRange(new Control[] { btnHuy, btnLuu, btnTinh });

        Controls.Add(scroll); Controls.Add(header); Controls.Add(btnRow);

        foreach (var ctl in new Control[] { numLCB, numPC, numNgayCong, numNgayLam, numNgayPhep, numNgayKP, numGioOT, numDonGia, numThuong, numPhat, numKhauTru })
            ctl.TextChanged += (s, e) => TinhLuong();
        btnTinh.Click += (s, e) => TinhLuong();
        btnLuu.Click += BtnLuu_Click;
        btnHuy.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
        AcceptButton = btnLuu; CancelButton = btnHuy;
    }

    private void LoadDefaults()
    {
        if (_existing != null)
        {
            numLCB.Value = _existing.LuongCoBan; numPC.Value = _existing.PhuCap;
            numNgayCong.Value = _existing.SoNgayCong; numNgayLam.Value = _existing.SoNgayLam;
            numNgayPhep.Value = _existing.SoNgayNghiPhep; numNgayKP.Value = _existing.SoNgayNghiKhongPhep;
            numGioOT.Value = _existing.GioLamThem; numThuong.Value = _existing.ThuongKhac;
            numPhat.Value = _existing.PhatKhac; numKhauTru.Value = _existing.KhauTruKhac;
            txtGhiChu.Text = _existing.GhiChu;
            if (_existing.GioLamThem > 0 && _existing.TienLamThem > 0)
                numDonGia.Value = Math.Round(_existing.TienLamThem / _existing.GioLamThem, 0);
        }
        else
        {
            var att = DataService.GetAttendance(_emp.Id, _t, _n);
            numLCB.Value = _emp.LuongCoBan; numPC.Value = _emp.PhuCap;
            numNgayCong.Value = 26;
            numNgayLam.Value = att?.SoNgayLam ?? 26;
            numNgayPhep.Value = att?.SoNgayNghiPhep ?? 0;
            numNgayKP.Value = att?.SoNgayNghiKhongPhep ?? 0;
            numGioOT.Value = att?.GioLamThem ?? 0;
            if (_emp.LuongCoBan > 0)
                numDonGia.Value = Math.Round(_emp.LuongCoBan / 26 / 8 * 1.5m, 0);
            var bp = DataService.GetBonusPenaltiesByMonth(_t, _n).Where(b => b.EmployeeId == _emp.Id).ToList();
            numThuong.Value = bp.Where(b => b.LoaiTP == "Thưởng").Sum(b => b.SoTien);
            numPhat.Value = bp.Where(b => b.LoaiTP == "Phạt").Sum(b => b.SoTien);
        }
    }

    private (decimal thucNhan, decimal ot, decimal bhxh, decimal thue) Calculate()
    {
        decimal lcb = numLCB.Value, pc = numPC.Value;
        decimal ngayCong = numNgayCong.Value <= 0 ? 26 : numNgayCong.Value;
        decimal ngayLam = numNgayLam.Value;
        decimal ot = numGioOT.Value * numDonGia.Value;
        decimal luongNgay = ngayCong > 0 ? lcb / ngayCong * ngayLam : 0;
        decimal tong = luongNgay + pc + ot + numThuong.Value - numPhat.Value;
        decimal bhxh = Math.Round(lcb * 0.105m, 0);
        decimal thue = tong > 11_000_000 ? Math.Round((tong - 11_000_000) * 0.10m, 0) : 0;
        decimal thucNhan = tong - bhxh - thue - numKhauTru.Value;
        return (thucNhan, ot, bhxh, thue);
    }

    private void TinhLuong()
    {
        var (thucNhan, ot, bhxh, thue) = Calculate();
        lblOT.Text = $"{ot:N0} VNĐ"; lblBHXH.Text = $"{bhxh:N0} VNĐ";
        lblThue.Text = $"{thue:N0} VNĐ"; lblThucNhan.Text = $"{thucNhan:N0} VNĐ";
    }

    private void BtnLuu_Click(object? sender, EventArgs e)
    {
        var (thucNhan, ot, bhxh, thue) = Calculate();
        var r = _existing ?? new SalaryRecord { EmployeeId = _emp.Id, MaNhanVien = _emp.MaNhanVien, HoTen = _emp.HoTen, PhongBan = _emp.PhongBan, Thang = _t, Nam = _n };
        r.LuongCoBan = numLCB.Value; r.PhuCap = numPC.Value;
        r.SoNgayCong = numNgayCong.Value; r.SoNgayLam = numNgayLam.Value;
        r.SoNgayNghiPhep = numNgayPhep.Value; r.SoNgayNghiKhongPhep = numNgayKP.Value;
        r.GioLamThem = numGioOT.Value; r.TienLamThem = ot;
        r.ThuongKhac = numThuong.Value; r.PhatKhac = numPhat.Value;
        r.BaoHiemXaHoi = bhxh; r.ThueTNCN = thue;
        r.KhauTruKhac = numKhauTru.Value; r.LuongThucNhan = thucNhan;
        r.GhiChu = txtGhiChu.Text.Trim();
        DataService.UpsertSalaryRecord(r);
        MessageBox.Show($"Đã lưu lương cho {_emp.HoTen}!\nThực nhận: {thucNhan:N0} VNĐ", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.OK; Close();
    }

    private static Label MkResult(bool hi = false) => new()
    {
        TextAlign = ContentAlignment.MiddleRight,
        Font = new Font("Segoe UI", hi ? 11.5f : 9.5f, hi ? FontStyle.Bold : FontStyle.Regular),
        ForeColor = hi ? Color.FromArgb(180, 0, 0) : Color.FromArgb(0, 120, 60),
        BackColor = hi ? Color.FromArgb(255, 245, 220) : Color.Transparent
    };

    private static Button MakeBtn(string t, Color c)
    {
        var b = new Button { Text = t, Size = new Size(110, 32), BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Margin = new Padding(4, 0, 0, 0) };
        b.FlatAppearance.BorderSize = 0; return b;
    }
}
