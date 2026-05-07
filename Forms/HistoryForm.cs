using QuanLyLuong.Models;
using QuanLyLuong.Services;

namespace QuanLyLuong.Forms;

public class HistoryForm : Form
{
    public HistoryForm(Employee emp)
    {
        Text = $"Lịch Sử Lương – {emp.HoTen} ({emp.MaNhanVien})";
        Size = new Size(860, 500);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 9.5f);
        BackColor = Color.FromArgb(245, 247, 250);

        var header = new Label
        {
            Text = $"  {emp.HoTen}  |  {emp.PhongBan}  |  {emp.ChucVu}  |  Lương CB: {emp.LuongCoBan:N0} VNĐ",
            Dock = DockStyle.Top, Height = 36,
            BackColor = Color.FromArgb(0, 90, 160), ForeColor = Color.White,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0)
        };

        var records = DataService.GetSalaryByEmployee(emp.Id);

        var dgv = new DataGridView
        {
            Dock = DockStyle.Fill, AutoGenerateColumns = false, ReadOnly = true,
            AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            RowHeadersVisible = false, Font = new Font("Segoe UI", 9.5f),
            RowTemplate = { Height = 28 }
        };
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 90, 160);
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        dgv.EnableHeadersVisualStyles = false;
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(180, 210, 255);
        dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 255);

        DataGridViewTextBoxColumn Col(string prop, string hdr, int w, string? fmt = null, DataGridViewContentAlignment align = DataGridViewContentAlignment.MiddleLeft)
        {
            var c = new DataGridViewTextBoxColumn { DataPropertyName = prop, HeaderText = hdr, Width = w, Name = prop };
            if (fmt != null) c.DefaultCellStyle.Format = fmt;
            c.DefaultCellStyle.Alignment = align;
            return c;
        }

        dgv.Columns.AddRange(
            Col("Thang", "Tháng", 60, null, DataGridViewContentAlignment.MiddleCenter),
            Col("Nam", "Năm", 60, null, DataGridViewContentAlignment.MiddleCenter),
            Col("LuongCoBan", "Lương CB", 110, "N0", DataGridViewContentAlignment.MiddleRight),
            Col("PhuCap", "Phụ Cấp", 90, "N0", DataGridViewContentAlignment.MiddleRight),
            Col("SoNgayLam", "Ngày Làm", 75, null, DataGridViewContentAlignment.MiddleCenter),
            Col("TienLamThem", "Làm Thêm", 95, "N0", DataGridViewContentAlignment.MiddleRight),
            Col("ThuongKhac", "Thưởng", 90, "N0", DataGridViewContentAlignment.MiddleRight),
            Col("PhatKhac", "Phạt", 80, "N0", DataGridViewContentAlignment.MiddleRight),
            Col("BaoHiemXaHoi", "BHXH", 90, "N0", DataGridViewContentAlignment.MiddleRight),
            Col("ThueTNCN", "Thuế TNCN", 95, "N0", DataGridViewContentAlignment.MiddleRight),
            Col("LuongThucNhan", "Thực Nhận", 110, "N0", DataGridViewContentAlignment.MiddleRight)
        );
        if (dgv.Columns.Contains("LuongThucNhan"))
        {
            dgv.Columns["LuongThucNhan"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.Columns["LuongThucNhan"]!.DefaultCellStyle.ForeColor = Color.FromArgb(150, 0, 0);
        }

        dgv.DataSource = records;

        decimal avg = records.Count > 0 ? records.Average(r => r.LuongThucNhan) : 0;
        decimal max = records.Count > 0 ? records.Max(r => r.LuongThucNhan) : 0;

        var footer = new Label
        {
            Text = $"  Tổng {records.Count} tháng  |  TB: {avg:N0} VNĐ  |  Cao nhất: {max:N0} VNĐ",
            Dock = DockStyle.Bottom, Height = 32,
            BackColor = Color.FromArgb(220, 235, 255), ForeColor = Color.FromArgb(0, 90, 160),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0)
        };

        Controls.Add(dgv); Controls.Add(footer); Controls.Add(header);
    }
}
