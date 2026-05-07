using QuanLyLuong.Models;
using QuanLyLuong.Services;

namespace QuanLyLuong.Forms;

public class LeaveForm : Form
{
    private ComboBox cboEmployee = new(), cboLoai = new(), cboTrangThai = new();
    private DateTimePicker dtpBD = new(), dtpKT = new();
    private NumericUpDown numNgay = new();
    private TextBox txtLyDo = new();
    private List<Employee> _employees;

    public LeaveForm()
    {
        _employees = DataService.GetEmployees();
        Text = "Tạo Đơn Nghỉ Phép";
        Size = new Size(460, 400);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Font = new Font("Segoe UI", 9.5f);
        BackColor = Color.FromArgb(245, 247, 250);

        foreach (var e in _employees) cboEmployee.Items.Add($"[{e.MaNhanVien}] {e.HoTen}");
        if (cboEmployee.Items.Count > 0) cboEmployee.SelectedIndex = 0;
        cboEmployee.DropDownStyle = ComboBoxStyle.DropDownList;

        cboLoai.Items.AddRange(new object[] { "Nghỉ phép", "Nghỉ ốm", "Nghỉ thai sản", "Nghỉ không lương", "Nghỉ lễ bổ sung" });
        cboLoai.DropDownStyle = ComboBoxStyle.DropDownList; cboLoai.SelectedIndex = 0;

        cboTrangThai.Items.AddRange(new object[] { "Chờ duyệt", "Đã duyệt", "Từ chối" });
        cboTrangThai.DropDownStyle = ComboBoxStyle.DropDownList; cboTrangThai.SelectedIndex = 0;

        numNgay.Minimum = 0.5m; numNgay.Maximum = 30; numNgay.Value = 1; numNgay.DecimalPlaces = 1; numNgay.Increment = 0.5m;

        dtpKT.Value = dtpBD.Value.AddDays(0);
        dtpBD.ValueChanged += (s, e) => numNgay.Value = Math.Max(0.5m, (decimal)(dtpKT.Value - dtpBD.Value).TotalDays + 1);
        dtpKT.ValueChanged += (s, e) => numNgay.Value = Math.Max(0.5m, (decimal)(dtpKT.Value - dtpBD.Value).TotalDays + 1);

        var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(20) };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        int r = 0;
        void AR(string lbl, Control ctrl)
        {
            var l = new Label { Text = lbl, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            ctrl.Dock = DockStyle.Fill;
            t.Controls.Add(l, 0, r); t.Controls.Add(ctrl, 1, r); r++;
        }

        AR("Nhân Viên:", cboEmployee);
        AR("Loại Nghỉ:", cboLoai);
        AR("Từ Ngày:", dtpBD);
        AR("Đến Ngày:", dtpKT);
        AR("Số Ngày:", numNgay);
        AR("Trạng Thái:", cboTrangThai);
        txtLyDo.Multiline = true; txtLyDo.Height = 60;
        AR("Lý Do:", txtLyDo);

        var btnRow = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 46, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
        var btnHuy = new Button { Text = "Hủy", Size = new Size(90, 32), BackColor = Color.Gray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; btnHuy.FlatAppearance.BorderSize = 0;
        var btnLuu = new Button { Text = "Lưu", Size = new Size(100, 32), BackColor = Color.FromArgb(0, 150, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; btnLuu.FlatAppearance.BorderSize = 0;
        btnRow.Controls.Add(btnHuy); btnRow.Controls.Add(btnLuu);

        Controls.Add(t); Controls.Add(btnRow);
        AcceptButton = btnLuu; CancelButton = btnHuy;

        btnLuu.Click += BtnLuu_Click;
        btnHuy.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
    }

    private void BtnLuu_Click(object? sender, EventArgs e)
    {
        if (cboEmployee.SelectedIndex < 0) return;
        var emp = _employees[cboEmployee.SelectedIndex];
        var rec = new LeaveRecord
        {
            EmployeeId = emp.Id, MaNhanVien = emp.MaNhanVien, HoTen = emp.HoTen, PhongBan = emp.PhongBan,
            NgayBatDau = dtpBD.Value, NgayKetThuc = dtpKT.Value,
            SoNgay = numNgay.Value, LoaiNghi = cboLoai.Text,
            LyDo = txtLyDo.Text.Trim(), TrangThai = cboTrangThai.Text
        };
        DataService.AddLeaveRecord(rec);
        if (rec.LoaiNghi == "Nghỉ phép" && rec.TrangThai == "Đã duyệt")
        {
            emp.SoNgayPhepDaDung += (int)rec.SoNgay;
            DataService.UpdateEmployee(emp);
        }
        MessageBox.Show("Đã lưu đơn nghỉ!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.OK; Close();
    }
}
