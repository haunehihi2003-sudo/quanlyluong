using QuanLyLuong.Models;
using QuanLyLuong.Services;

namespace QuanLyLuong.Forms;

public class EmployeeForm : Form
{
    private readonly Employee? _emp;
    private readonly bool _isEdit;

    // Tab 1 – Basic info
    private TextBox txtMaNV = new(), txtHoTen = new(), txtChucVu = new(), txtPhongBan = new();
    private NumericUpDown numLCB = new(), numPC = new();
    private ComboBox cboGioiTinh = new();
    private DateTimePicker dtpNgayVaoLam = new();

    // Tab 2 – Contract
    private ComboBox cboLoaiHD = new();
    private DateTimePicker dtpHetHopDong = new();
    private NumericUpDown numNgayPhep = new(), numPhepDaDung = new();
    private CheckBox chkActive = new();

    // Tab 3 – Other
    private TextBox txtCCCD = new(), txtSDT = new(), txtEmail = new(),
                    txtDiaChi = new(), txtGhiChu = new();

    public EmployeeForm(Employee? emp = null)
    {
        _emp = emp; _isEdit = emp != null;
        Text = _isEdit ? "Sửa Thông Tin Nhân Viên" : "Thêm Nhân Viên Mới";
        Size = new Size(560, 560);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Font = new Font("Segoe UI", 9.5f);
        BackColor = Color.FromArgb(245, 247, 250);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildTabCoban());
        tabs.TabPages.Add(BuildTabHopDong());
        tabs.TabPages.Add(BuildTabKhac());

        var btnPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom, Height = 46,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(8)
        };
        var btnHuy = MakeBtn("Hủy", Color.Gray);
        var btnLuu = MakeBtn(_isEdit ? "Cập Nhật" : "Thêm Mới", Color.FromArgb(0, 150, 80));
        btnPanel.Controls.Add(btnHuy);
        btnPanel.Controls.Add(btnLuu);

        Controls.Add(tabs);
        Controls.Add(btnPanel);

        AcceptButton = btnLuu; CancelButton = btnHuy;
        btnLuu.Click += BtnLuu_Click;
        btnHuy.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

        if (_isEdit) LoadData();
    }

    private TabPage BuildTabCoban()
    {
        var tp = new TabPage("  Thông Tin Cơ Bản  ");
        var t = MakeTable(tp);

        cboGioiTinh.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
        cboGioiTinh.DropDownStyle = ComboBoxStyle.DropDownList;
        cboGioiTinh.SelectedIndex = 0;

        numLCB.Maximum = 999_999_999; numLCB.ThousandsSeparator = true; numLCB.DecimalPlaces = 0;
        numPC.Maximum  = 999_999_999; numPC.ThousandsSeparator  = true; numPC.DecimalPlaces  = 0;

        AddRow(t, "Mã Nhân Viên *:", txtMaNV,      0);
        AddRow(t, "Họ Tên *:",        txtHoTen,     1);
        AddRow(t, "Giới Tính:",        cboGioiTinh,  2);
        AddRow(t, "Chức Vụ:",          txtChucVu,    3);
        AddRow(t, "Phòng Ban:",         txtPhongBan,  4);
        AddRow(t, "Lương Cơ Bản (VNĐ):", numLCB,    5);
        AddRow(t, "Phụ Cấp (VNĐ):",    numPC,        6);
        AddRow(t, "Ngày Vào Làm:",      dtpNgayVaoLam, 7);
        return tp;
    }

    private TabPage BuildTabHopDong()
    {
        var tp = new TabPage("  Hợp Đồng  ");
        var t = MakeTable(tp);

        cboLoaiHD.Items.AddRange(new object[] { "Chính thức", "Thử việc", "Hợp đồng 1 năm", "Hợp đồng 2 năm", "Thời vụ" });
        cboLoaiHD.DropDownStyle = ComboBoxStyle.DropDownList;
        cboLoaiHD.SelectedIndex = 0;

        chkActive.Text = "Đang làm việc"; chkActive.Checked = true;

        numNgayPhep.Maximum   = 50; numNgayPhep.Value   = 12;
        numPhepDaDung.Maximum = 50; numPhepDaDung.Value = 0;

        // Use a SEPARATE DateTimePicker for "Ngày Hết HĐ" to avoid sharing controls across tabs
        var dtpHetHD = dtpHetHopDong;

        AddRow(t, "Loại Hợp Đồng:", cboLoaiHD,    0);
        AddRow(t, "Ngày Hết HĐ:",   dtpHetHD,     1);
        AddRow(t, "Số Ngày Phép/Năm:", numNgayPhep, 2);
        AddRow(t, "Đã Dùng Phép:",  numPhepDaDung, 3);
        AddRow(t, "Trạng Thái:",    chkActive,     4);
        return tp;
    }

    private TabPage BuildTabKhac()
    {
        var tp = new TabPage("  Thông Tin Khác  ");
        var t = MakeTable(tp);

        txtGhiChu.Multiline = true; txtGhiChu.Height = 70;

        AddRow(t, "Số CCCD:",        txtCCCD,   0);
        AddRow(t, "Số Điện Thoại:", txtSDT,    1);
        AddRow(t, "Email:",          txtEmail,  2);
        AddRow(t, "Địa Chỉ:",        txtDiaChi, 3);
        AddRow(t, "Ghi Chú:",        txtGhiChu, 4);
        return tp;
    }

    private static TableLayoutPanel MakeTable(TabPage tp)
    {
        var t = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2,
            Padding = new Padding(16, 14, 16, 8)
        };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tp.Controls.Add(t);
        return t;
    }

    private static void AddRow(TableLayoutPanel t, string lbl, Control ctrl, int row)
    {
        var l = new Label
        {
            Text = lbl, TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5f)
        };
        ctrl.Dock = DockStyle.Fill;
        t.Controls.Add(l, 0, row);
        t.Controls.Add(ctrl, 1, row);
    }

    private static Button MakeBtn(string text, Color color)
    {
        var b = new Button
        {
            Text = text, Size = new Size(110, 32),
            BackColor = color, ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat, Margin = new Padding(4, 0, 0, 0)
        };
        b.FlatAppearance.BorderSize = 0;
        return b;
    }

    private void LoadData()
    {
        var e = _emp!;
        txtMaNV.Text = e.MaNhanVien;     txtHoTen.Text    = e.HoTen;
        txtChucVu.Text = e.ChucVu;       txtPhongBan.Text = e.PhongBan;
        numLCB.Value = e.LuongCoBan;     numPC.Value      = e.PhuCap;
        txtSDT.Text  = e.SoDienThoai;    txtEmail.Text    = e.Email;
        txtCCCD.Text = e.CCCD;           txtDiaChi.Text   = e.DiaChi;
        txtGhiChu.Text = e.GhiChu;

        dtpNgayVaoLam.Value  = e.NgayVaoLam;
        dtpHetHopDong.Value  = e.NgayHetHopDong;

        if (cboGioiTinh.Items.Contains(e.GioiTinh))
            cboGioiTinh.SelectedItem = e.GioiTinh;

        if (cboLoaiHD.Items.Contains(e.LoaiHopDong))
            cboLoaiHD.SelectedItem = e.LoaiHopDong;

        numNgayPhep.Value    = e.SoNgayPhepNam;
        numPhepDaDung.Value  = e.SoNgayPhepDaDung;
        chkActive.Checked    = e.ConHieuLuc;
    }

    private void BtnLuu_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtMaNV.Text) || string.IsNullOrWhiteSpace(txtHoTen.Text))
        {
            MessageBox.Show("Vui lòng nhập Mã NV và Họ Tên!", "Thiếu thông tin",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var emp = _isEdit ? _emp! : new Employee();
        emp.MaNhanVien    = txtMaNV.Text.Trim();
        emp.HoTen         = txtHoTen.Text.Trim();
        emp.ChucVu        = txtChucVu.Text.Trim();
        emp.PhongBan      = txtPhongBan.Text.Trim();
        emp.LuongCoBan    = numLCB.Value;
        emp.PhuCap        = numPC.Value;
        emp.SoDienThoai   = txtSDT.Text.Trim();
        emp.Email         = txtEmail.Text.Trim();
        emp.CCCD          = txtCCCD.Text.Trim();
        emp.DiaChi        = txtDiaChi.Text.Trim();
        emp.GhiChu        = txtGhiChu.Text.Trim();
        emp.GioiTinh      = cboGioiTinh.SelectedItem?.ToString() ?? "Nam";
        emp.LoaiHopDong   = cboLoaiHD.SelectedItem?.ToString() ?? "Chính thức";
        emp.NgayVaoLam    = dtpNgayVaoLam.Value;
        emp.NgayHetHopDong = dtpHetHopDong.Value;
        emp.SoNgayPhepNam  = (int)numNgayPhep.Value;
        emp.SoNgayPhepDaDung = (int)numPhepDaDung.Value;
        emp.ConHieuLuc    = chkActive.Checked;

        if (_isEdit) DataService.UpdateEmployee(emp);
        else         DataService.AddEmployee(emp);

        DialogResult = DialogResult.OK;
        Close();
    }
}
