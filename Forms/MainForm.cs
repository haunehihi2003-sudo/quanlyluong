using QuanLyLuong.Models;
using QuanLyLuong.Services;

namespace QuanLyLuong.Forms;

public partial class MainForm : Form
{
    private readonly UserAccount _user;
    private TabControl tabMain = new();

    // Shared tab pages
    private TabPage tpNV = new(), tpCC = new(), tpNP = new(), tpBL = new(),
                    tpTP = new(), tpBD = new(), tpBC = new(), tpCD = new();

    public MainForm(UserAccount user)
    {
        _user = user;
        Text = $"Quản Lý Lương  –  {user.HoTen}  [{user.VaiTro}]";
        Size = new Size(1200, 720);
        MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9.5f);
        BackColor = AppTheme.Background;
        Icon = SystemIcons.Application;

        BuildLayout();
        AppTheme.Apply(this);

        LoadNhanVien();
        LoadChamCong();
        LoadNghiPhep();
        LoadBangLuong();
        LoadThuongPhat();
        LoadBaoCao();
        LoadCaiDat();

        tabMain.SelectedIndexChanged += (s, e) =>
        {
            if (tabMain.SelectedTab == tpBD) DrawCharts();
        };
    }

    private void BuildLayout()
    {
        // ── Header ────────────────────────────────────────────────────────────
        var header = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = AppTheme.Header };
        var lblTitle = new Label
        {
            Text = "💼  PHẦN MỀM QUẢN LÝ LƯƠNG",
            Dock = DockStyle.Left, Width = 380,
            ForeColor = Color.White, Font = new Font("Segoe UI", 13f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(14, 0, 0, 0)
        };
        var lblUser = new Label
        {
            Text = $"👤 {_user.HoTen}  |  {_user.VaiTro}",
            Dock = DockStyle.Right, Width = 250,
            ForeColor = Color.FromArgb(180, 215, 255), Font = new Font("Segoe UI", 9.5f),
            TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0, 0, 14, 0)
        };
        header.Controls.Add(lblTitle);
        header.Controls.Add(lblUser);

        // ── Tabs ──────────────────────────────────────────────────────────────
        tabMain.Dock = DockStyle.Fill;
        tabMain.Font = new Font("Segoe UI", 9.5f);

        tpNV.Text  = "  Nhân Viên  ";  tpNV.BackColor  = AppTheme.Background;
        tpCC.Text  = "  Chấm Công  ";  tpCC.BackColor  = AppTheme.Background;
        tpNP.Text  = "  Nghỉ Phép  ";  tpNP.BackColor  = AppTheme.Background;
        tpBL.Text  = "  Bảng Lương  "; tpBL.BackColor  = AppTheme.Background;
        tpTP.Text  = "  Thưởng/Phạt  ";tpTP.BackColor  = AppTheme.Background;
        tpBD.Text  = "  Biểu Đồ  ";    tpBD.BackColor  = AppTheme.Background;
        tpBC.Text  = "  Báo Cáo  ";    tpBC.BackColor  = AppTheme.Background;
        tpCD.Text  = "  Cài Đặt  ";    tpCD.BackColor  = AppTheme.Background;

        tabMain.TabPages.AddRange(new[] { tpNV, tpCC, tpNP, tpBL, tpTP, tpBD, tpBC, tpCD });

        BuildTabNhanVien();
        BuildTabChamCong();
        BuildTabNghiPhep();
        BuildTabBangLuong();
        BuildTabThuongPhat();
        BuildTabBieuDo();
        BuildTabBaoCao();
        BuildTabCaiDat();

        Controls.Add(tabMain);
        Controls.Add(header);
    }

    // ── Shared helpers ────────────────────────────────────────────────────────
    internal static DataGridView BuildGrid()
    {
        var dgv = new DataGridView
        {
            Dock = DockStyle.Fill, AutoGenerateColumns = false, ReadOnly = true,
            AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false, BackgroundColor = AppTheme.Surface,
            BorderStyle = BorderStyle.None, CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = Color.FromArgb(220, 225, 235), RowHeadersVisible = false,
            Font = new Font("Segoe UI", 9.5f), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            RowTemplate = { Height = 29 }
        };
        dgv.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.Header;
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = AppTheme.Header;
        dgv.DefaultCellStyle.SelectionBackColor = AppTheme.Selection;
        dgv.DefaultCellStyle.SelectionForeColor = AppTheme.SelectText;
        dgv.AlternatingRowsDefaultCellStyle.BackColor = AppTheme.GridAlt;
        dgv.EnableHeadersVisualStyles = false;
        return dgv;
    }

    internal static DataGridViewTextBoxColumn Col(string name, string header, int width,
        string? fmt = null, DataGridViewContentAlignment align = DataGridViewContentAlignment.MiddleLeft)
    {
        var c = new DataGridViewTextBoxColumn { DataPropertyName = name, HeaderText = header, Width = width, Name = name };
        if (fmt != null) c.DefaultCellStyle.Format = fmt;
        c.DefaultCellStyle.Alignment = align;
        return c;
    }

    internal static DataGridViewTextBoxColumn ColMoney(string name, string header, int width) =>
        Col(name, header, width, "N0", DataGridViewContentAlignment.MiddleRight);

    internal static DataGridViewTextBoxColumn ColCenter(string name, string header, int width) =>
        Col(name, header, width, null, DataGridViewContentAlignment.MiddleCenter);

    internal static Button MakeBtn(string text, Color color, int w = 0)
    {
        var b = new Button
        {
            Text = text, Height = 30, BackColor = color, ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Margin = new Padding(4, 0, 0, 0), Padding = new Padding(8, 0, 8, 0)
        };
        if (w > 0) b.Width = w; else b.AutoSize = true;
        b.FlatAppearance.BorderSize = 0;
        return b;
    }

    internal static FlowLayoutPanel MakeToolbar() => new()
    {
        Dock = DockStyle.Top, Height = 48,
        Padding = new Padding(8, 9, 8, 0),
        BackColor = AppTheme.Toolbar
    };

    internal static Label MakeFooter(string text = "") => new()
    {
        Dock = DockStyle.Bottom, Height = 34,
        BackColor = Color.FromArgb(220, 235, 255), ForeColor = Color.FromArgb(0, 90, 160),
        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
        TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0, 0, 14, 0),
        Text = text
    };
}
