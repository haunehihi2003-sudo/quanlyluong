using QuanLyLuong.Models;
using QuanLyLuong.Services;

namespace QuanLyLuong.Forms;

public partial class MainForm
{
    private DataGridView dgvTP = new();
    private NumericUpDown numTPThang = new(), numTPNam = new();
    private Label lblTPFooter = new();

    private void BuildTabThuongPhat()
    {
        var toolbar = MakeToolbar();
        numTPThang = new NumericUpDown { Width = 55, Minimum = 1, Maximum = 12, Value = DateTime.Today.Month };
        numTPNam   = new NumericUpDown { Width = 75, Minimum = 2000, Maximum = 2100, Value = DateTime.Today.Year };

        var btnThem = MakeBtn("＋ Thêm", AppTheme.Success);
        var btnXoa  = MakeBtn("✖ Xóa", AppTheme.Danger);
        var btnTai  = MakeBtn("↺ Tải Lại", AppTheme.Accent);

        var lT = new Label { Text = "Tháng:", Width = 52, TextAlign = ContentAlignment.MiddleCenter, Height = 28 };
        var lN = new Label { Text = "Năm:", Width = 40, TextAlign = ContentAlignment.MiddleCenter, Height = 28 };
        toolbar.Controls.AddRange(new Control[] { lT, numTPThang, lN, numTPNam, btnThem, btnXoa, btnTai });

        dgvTP = BuildGrid();
        dgvTP.Columns.AddRange(
            ColCenter("MaNhanVien", "Mã NV", 85),
            Col("HoTen", "Họ Tên", 160),
            Col("PhongBan", "Phòng Ban", 120),
            ColCenter("Thang", "Tháng", 65),
            ColCenter("Nam", "Năm", 65),
            Col("LoaiTP", "Loại", 80),
            ColMoney("SoTien", "Số Tiền", 120),
            Col("LyDo", "Lý Do", 250),
            Col("NgayTao", "Ngày Tạo", 130)
        );
        if (dgvTP.Columns.Contains("NgayTao"))
            dgvTP.Columns["NgayTao"]!.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";

        dgvTP.CellFormatting += (s, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgvTP.Columns[e.ColumnIndex].Name == "LoaiTP")
            {
                e.CellStyle!.ForeColor = e.Value?.ToString() == "Thưởng" ? Color.FromArgb(0, 140, 60) : Color.FromArgb(180, 0, 0);
                e.CellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                e.FormattingApplied = true;
            }
        };

        lblTPFooter = MakeFooter();
        tpTP.Controls.Add(dgvTP);
        tpTP.Controls.Add(lblTPFooter);
        tpTP.Controls.Add(toolbar);

        btnThem.Click += (s, e) => { new BonusPenaltyForm((int)numTPThang.Value, (int)numTPNam.Value).ShowDialog(this); LoadThuongPhat(); };
        btnXoa.Click += (s, e) =>
        {
            if (dgvTP.CurrentRow?.DataBoundItem is not BonusPenalty bp) return;
            if (MessageBox.Show("Xóa khoản này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            DataService.DeleteBonusPenalty(bp.Id);
            LoadThuongPhat();
        };
        btnTai.Click += (s, e) => LoadThuongPhat();
        numTPThang.ValueChanged += (s, e) => LoadThuongPhat();
        numTPNam.ValueChanged += (s, e) => LoadThuongPhat();
    }

    internal void LoadThuongPhat()
    {
        int t = (int)numTPThang.Value, n = (int)numTPNam.Value;
        var list = DataService.GetBonusPenaltiesByMonth(t, n);
        dgvTP.DataSource = list;
        decimal tongThuong = list.Where(b => b.LoaiTP == "Thưởng").Sum(b => b.SoTien);
        decimal tongPhat   = list.Where(b => b.LoaiTP == "Phạt").Sum(b => b.SoTien);
        lblTPFooter.Text = $"Tháng {t}/{n}  |  Tổng thưởng: {tongThuong:N0} VNĐ  |  Tổng phạt: {tongPhat:N0} VNĐ  |  Số khoản: {list.Count}  ";
    }
}
