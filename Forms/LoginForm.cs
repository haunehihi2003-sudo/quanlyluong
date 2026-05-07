using QuanLyLuong.Models;
using QuanLyLuong.Services;

namespace QuanLyLuong.Forms;

public class LoginForm : Form
{
    public UserAccount? LoggedInUser { get; private set; }
    private TextBox txtUser = new(), txtPass = new();
    private Label lblError = new();
    private Button btnLogin = new();

    public LoginForm()
    {
        Text = "Đăng Nhập - Quản Lý Lương";
        Size = new Size(400, 420);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Font = new Font("Segoe UI", 10f);
        BackColor = Color.FromArgb(245, 247, 250);

        var header = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(0, 90, 160) };
        var lblTitle = new Label
        {
            Text = "💼 QUẢN LÝ LƯƠNG",
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };
        var lblSub = new Label
        {
            Text = "Phần Mềm Quản Lý Lương Nhân Viên",
            Dock = DockStyle.Bottom,
            Height = 24,
            ForeColor = Color.FromArgb(180, 210, 255),
            Font = new Font("Segoe UI", 9f),
            TextAlign = ContentAlignment.MiddleCenter
        };
        header.Controls.Add(lblTitle);
        header.Controls.Add(lblSub);

        var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(40, 24, 40, 20) };

        txtPass.PasswordChar = '●';
        txtPass.UseSystemPasswordChar = false;

        lblError = new Label
        {
            ForeColor = Color.FromArgb(200, 50, 50),
            Height = 22,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9f)
        };

        btnLogin = new Button
        {
            Text = "ĐĂNG NHẬP",
            Height = 42,
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(0, 90, 160),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11f, FontStyle.Bold)
        };
        btnLogin.FlatAppearance.BorderSize = 0;

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7,
            AutoSize = true
        };

        Label MkLbl(string t) => new() { Text = t, Height = 22, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(80, 80, 100) };
        Control MkBox(Control c) { c.Dock = DockStyle.Fill; c.Height = 36; ((c as TextBox)!).BorderStyle = BorderStyle.FixedSingle; c.Font = new Font("Segoe UI", 11f); return c; }

        table.Controls.Add(MkLbl("Tên đăng nhập:"), 0, 0);
        table.Controls.Add(MkBox(txtUser), 0, 1);
        table.Controls.Add(MkLbl("Mật khẩu:"), 0, 2);
        table.Controls.Add(MkBox(txtPass), 0, 3);
        table.Controls.Add(lblError, 0, 4);
        table.Controls.Add(new Label { Height = 6 }, 0, 5);
        table.Controls.Add(btnLogin, 0, 6);
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 10));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

        var hint = new Label
        {
            Text = "Admin: admin / admin123    |    Kế toán: ketoan / ketoan123",
            Dock = DockStyle.Bottom,
            Height = 22,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 8f),
            TextAlign = ContentAlignment.MiddleCenter
        };

        body.Controls.Add(table);
        body.Controls.Add(hint);
        Controls.Add(body);
        Controls.Add(header);

        AcceptButton = btnLogin;
        btnLogin.Click += BtnLogin_Click;
        txtUser.Text = "admin";
    }

    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        lblError.Text = "";
        var user = DataService.Authenticate(txtUser.Text.Trim(), txtPass.Text);
        if (user == null) { lblError.Text = "Sai tên đăng nhập hoặc mật khẩu!"; txtPass.Clear(); txtPass.Focus(); return; }
        LoggedInUser = user;
        DialogResult = DialogResult.OK;
        Close();
    }
}
