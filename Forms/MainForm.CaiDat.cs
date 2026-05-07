using QuanLyLuong.Models;
using QuanLyLuong.Services;

namespace QuanLyLuong.Forms;

public partial class MainForm
{
    private DataGridView dgvUsers = new();
    private Label lblBackupInfo = new();

    private void BuildTabCaiDat()
    {
        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 320 };

        // ── Top: Settings ──────────────────────────────────────────────────────
        var grpGD = new GroupBox { Text = "  Giao Diện", Dock = DockStyle.Left, Width = 280, Padding = new Padding(12) };
        var chkDark = new CheckBox { Text = "Chế độ Tối (Dark Mode)", Checked = AppTheme.IsDark, Font = new Font("Segoe UI", 10f), AutoSize = true, Location = new Point(12, 28) };
        chkDark.CheckedChanged += (s, e) =>
        {
            AppTheme.Toggle();
            AppTheme.Apply(this);
            chkDark.Text = AppTheme.IsDark ? "Chế độ Tối ✔" : "Chế độ Sáng";
        };
        grpGD.Controls.Add(chkDark);

        var grpBK = new GroupBox { Text = "  Sao Lưu & Khôi Phục", Dock = DockStyle.Fill, Padding = new Padding(12) };
        var btnBackup  = MakeBtn("💾 Sao Lưu Ngay", AppTheme.Success);
        var btnRestore = MakeBtn("📂 Khôi Phục", AppTheme.Accent);
        var btnOpenDir = MakeBtn("📁 Mở Thư Mục Dữ Liệu", Color.Gray);
        lblBackupInfo = new Label { Dock = DockStyle.Bottom, Height = 36, ForeColor = Color.FromArgb(0, 90, 160), Font = new Font("Segoe UI", 9f) };

        var bkFlow = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 46, Padding = new Padding(0, 8, 0, 0) };
        bkFlow.Controls.AddRange(new Control[] { btnBackup, btnRestore, btnOpenDir });
        grpBK.Controls.Add(lblBackupInfo);
        grpBK.Controls.Add(bkFlow);

        var topFlow = new FlowLayoutPanel { Dock = DockStyle.Fill };
        topFlow.Controls.Add(grpGD);
        topFlow.Controls.Add(grpBK);
        split.Panel1.Controls.Add(topFlow);

        // ── Bottom: User Management ────────────────────────────────────────────
        var grpUsers = new GroupBox { Text = "  Quản Lý Tài Khoản", Dock = DockStyle.Fill, Padding = new Padding(8) };
        var btnAddUser = MakeBtn("＋ Thêm Tài Khoản", AppTheme.Success);
        var btnDelUser = MakeBtn("✖ Xóa", AppTheme.Danger);
        var btnPwdUser = MakeBtn("🔑 Đổi Mật Khẩu", AppTheme.Accent);
        var userToolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, Padding = new Padding(0, 6, 0, 0) };
        userToolbar.Controls.AddRange(new Control[] { btnAddUser, btnDelUser, btnPwdUser });

        dgvUsers = BuildGrid();
        dgvUsers.Columns.AddRange(
            Col("TenDangNhap", "Tên Đăng Nhập", 140),
            Col("HoTen", "Họ Tên", 160),
            Col("VaiTro", "Vai Trò", 120),
            ColCenter("ConHieuLuc", "Hiệu Lực", 85)
        );
        dgvUsers.CellFormatting += (s, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvUsers.Columns[e.ColumnIndex].Name == "ConHieuLuc")
            { e.Value = (bool)(e.Value ?? false) ? "✔ Hoạt động" : "✘ Khóa"; e.FormattingApplied = true; }
        };

        grpUsers.Controls.Add(dgvUsers);
        grpUsers.Controls.Add(userToolbar);
        split.Panel2.Controls.Add(grpUsers);

        tpCD.Controls.Add(split);

        LoadCaiDat();

        btnBackup.Click += (s, e) =>
        {
            using var dlg = new FolderBrowserDialog { Description = "Chọn thư mục lưu backup" };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try
            {
                var dest = BackupService.Backup(dlg.SelectedPath);
                lblBackupInfo.Text = $"✔ Đã sao lưu: {dest}";
                MessageBox.Show($"Sao lưu thành công!\n{dest}", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}"); }
        };

        btnRestore.Click += (s, e) =>
        {
            using var dlg = new FolderBrowserDialog { Description = "Chọn thư mục backup để khôi phục" };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            if (MessageBox.Show("Khôi phục sẽ ghi đè dữ liệu hiện tại. Tiếp tục?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try
            {
                var (count, errors) = BackupService.Restore(dlg.SelectedPath);
                lblBackupInfo.Text = $"✔ Đã khôi phục {count} file";
                MessageBox.Show($"Khôi phục {count} file thành công!" + (errors.Count > 0 ? $"\nLỗi: {string.Join("\n", errors)}" : ""), "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadNhanVien(); LoadBangLuong(); LoadNghiPhep(); LoadBaoCao();
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}"); }
        };

        btnOpenDir.Click += (s, e) =>
        {
            try { System.Diagnostics.Process.Start("explorer.exe", DataService.DataFolderPath); }
            catch { MessageBox.Show(DataService.DataFolderPath); }
        };

        btnAddUser.Click += (s, e) =>
        {
            if (_user.VaiTro != "Admin") { MessageBox.Show("Chỉ Admin mới có thể thêm tài khoản!", "Từ chối", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            ShowAddUserDialog();
            LoadCaiDat();
        };

        btnDelUser.Click += (s, e) =>
        {
            if (_user.VaiTro != "Admin") { MessageBox.Show("Chỉ Admin mới có thể xóa tài khoản!"); return; }
            if (dgvUsers.CurrentRow?.DataBoundItem is not UserAccount u) return;
            if (u.TenDangNhap == _user.TenDangNhap) { MessageBox.Show("Không thể xóa tài khoản đang đăng nhập!"); return; }
            if (MessageBox.Show($"Xóa tài khoản '{u.TenDangNhap}'?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            DataService.DeleteUser(u.Id);
            LoadCaiDat();
        };

        btnPwdUser.Click += (s, e) =>
        {
            if (dgvUsers.CurrentRow?.DataBoundItem is not UserAccount u) return;
            if (_user.VaiTro != "Admin" && u.Id != _user.Id) { MessageBox.Show("Bạn chỉ có thể đổi mật khẩu của chính mình!"); return; }
            ShowChangePasswordDialog(u);
        };
    }

    private void ShowAddUserDialog()
    {
        using var f = new Form { Text = "Thêm Tài Khoản", Size = new Size(360, 280), StartPosition = FormStartPosition.CenterParent, Font = new Font("Segoe UI", 9.5f) };
        var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(16) };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var txtUN = new TextBox { Dock = DockStyle.Fill };
        var txtHT = new TextBox { Dock = DockStyle.Fill };
        var txtPW = new TextBox { Dock = DockStyle.Fill, PasswordChar = '●' };
        var cboVT = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        cboVT.Items.AddRange(new object[] { "Admin", "Kế toán", "Nhân sự" }); cboVT.SelectedIndex = 1;
        int r = 0;
        void AR(string lbl, Control ctrl) { t.Controls.Add(new Label { Text = lbl, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, r); t.Controls.Add(ctrl, 1, r); r++; }
        AR("Tên đăng nhập:", txtUN); AR("Họ tên:", txtHT); AR("Mật khẩu:", txtPW); AR("Vai trò:", cboVT);
        var btnRow = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
        var btnOK = new Button { Text = "Thêm", Size = new Size(90, 30), BackColor = AppTheme.Success, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnOK.FlatAppearance.BorderSize = 0;
        btnRow.Controls.Add(btnOK);
        f.Controls.Add(t); f.Controls.Add(btnRow);
        btnOK.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtUN.Text) || string.IsNullOrWhiteSpace(txtPW.Text)) { MessageBox.Show("Vui lòng điền đầy đủ!"); return; }
            DataService.AddUser(new UserAccount { TenDangNhap = txtUN.Text.Trim(), MatKhauHash = DataService.Hash(txtPW.Text), HoTen = txtHT.Text.Trim(), VaiTro = cboVT.Text });
            f.Close();
        };
        f.ShowDialog(this);
    }

    private void ShowChangePasswordDialog(UserAccount u)
    {
        using var f = new Form { Text = "Đổi Mật Khẩu", Size = new Size(320, 200), StartPosition = FormStartPosition.CenterParent, Font = new Font("Segoe UI", 9.5f) };
        var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(16) };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130)); t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var txtNew = new TextBox { Dock = DockStyle.Fill, PasswordChar = '●' };
        var txtCon = new TextBox { Dock = DockStyle.Fill, PasswordChar = '●' };
        t.Controls.Add(new Label { Text = "Mật khẩu mới:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        t.Controls.Add(txtNew, 1, 0);
        t.Controls.Add(new Label { Text = "Xác nhận:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        t.Controls.Add(txtCon, 1, 1);
        var btnOK = new Button { Text = "Lưu", Dock = DockStyle.Bottom, Height = 36, BackColor = AppTheme.Success, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnOK.FlatAppearance.BorderSize = 0;
        f.Controls.Add(t); f.Controls.Add(btnOK);
        btnOK.Click += (s, e) =>
        {
            if (txtNew.Text != txtCon.Text) { MessageBox.Show("Mật khẩu không khớp!"); return; }
            if (txtNew.Text.Length < 6) { MessageBox.Show("Mật khẩu phải ít nhất 6 ký tự!"); return; }
            u.MatKhauHash = DataService.Hash(txtNew.Text);
            DataService.UpdateUser(u);
            MessageBox.Show("Đổi mật khẩu thành công!"); f.Close();
        };
        f.ShowDialog(this);
    }

    internal void LoadCaiDat()
    {
        dgvUsers.DataSource = DataService.GetUsers();
        lblBackupInfo.Text = $"Dữ liệu lưu tại: {DataService.DataFolderPath}";
    }
}
