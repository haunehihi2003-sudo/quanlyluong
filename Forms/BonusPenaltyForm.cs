using QuanLyLuong.Models;
using QuanLyLuong.Services;

namespace QuanLyLuong.Forms;

public class BonusPenaltyForm : Form
{
    private ComboBox cboEmployee = new(), cboLoai = new();
    private NumericUpDown numThang = new(), numNam = new(), numSoTien = new();
    private TextBox txtLyDo = new();
    private List<Employee> _employees;

    public BonusPenaltyForm(int thang, int nam)
    {
        _employees = DataService.GetEmployees().Where(e => e.ConHieuLuc).ToList();
        Text = "Thêm Thưởng / Phạt";
        Size = new Size(440, 360);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Font = new Font("Segoe UI", 9.5f);
        BackColor = Color.FromArgb(245, 247, 250);

        foreach (var e in _employees) cboEmployee.Items.Add($"[{e.MaNhanVien}] {e.HoTen}");
        if (cboEmployee.Items.Count > 0) cboEmployee.SelectedIndex = 0;
        cboEmployee.DropDownStyle = ComboBoxStyle.DropDownList;

        cboLoai.Items.AddRange(new object[] { "Thưởng", "Phạt" });
        cboLoai.DropDownStyle = ComboBoxStyle.DropDownList; cboLoai.SelectedIndex = 0;

        numThang.Minimum = 1; numThang.Maximum = 12; numThang.Value = thang;
        numNam.Minimum = 2000; numNam.Maximum = 2100; numNam.Value = nam;
        numSoTien.Maximum = 999_999_999; numSoTien.ThousandsSeparator = true; numSoTien.DecimalPlaces = 0;

        var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(20) };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        int r = 0;
        void AR(string lbl, Control ctrl)
        {
            var l = new Label { Text = lbl, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            ctrl.Dock = DockStyle.Fill;
            t.Controls.Add(l, 0, r); t.Controls.Add(ctrl, 1, r); r++;
        }

        AR("Nhân Viên:", cboEmployee);
        AR("Loại:", cboLoai);
        AR("Tháng:", numThang);
        AR("Năm:", numNam);
        AR("Số Tiền (VNĐ):", numSoTien);
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
        if (cboEmployee.SelectedIndex < 0 || numSoTien.Value <= 0)
        { MessageBox.Show("Vui lòng chọn nhân viên và nhập số tiền!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        var emp = _employees[cboEmployee.SelectedIndex];
        DataService.AddBonusPenalty(new BonusPenalty
        {
            EmployeeId = emp.Id, MaNhanVien = emp.MaNhanVien, HoTen = emp.HoTen, PhongBan = emp.PhongBan,
            Thang = (int)numThang.Value, Nam = (int)numNam.Value,
            LoaiTP = cboLoai.Text, SoTien = numSoTien.Value, LyDo = txtLyDo.Text.Trim()
        });
        DialogResult = DialogResult.OK; Close();
    }
}
