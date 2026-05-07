using QuanLyLuong.Services;

namespace QuanLyLuong.Forms;

public partial class MainForm
{
    private Panel pnlBarChart = new(), pnlPieChart = new();
    private NumericUpDown numBDNam = new();
    private Label lblBDTitle = new();

    private void BuildTabBieuDo()
    {
        var toolbar = MakeToolbar();
        numBDNam = new NumericUpDown { Width = 80, Minimum = 2000, Maximum = 2100, Value = DateTime.Today.Year };
        var btnVe = MakeBtn("📊 Vẽ Biểu Đồ", AppTheme.Accent);
        var lN = new Label { Text = "Năm:", Width = 40, TextAlign = ContentAlignment.MiddleCenter, Height = 28 };
        toolbar.Controls.AddRange(new Control[] { lN, numBDNam, btnVe });

        lblBDTitle = new Label
        {
            Dock = DockStyle.Top, Height = 30,
            Text = "  Biểu Đồ Lương Theo Tháng & Phân Bổ Theo Phòng Ban",
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 90, 160),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 600,
            BorderStyle = BorderStyle.FixedSingle
        };

        pnlBarChart = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        pnlPieChart = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

        split.Panel1.Controls.Add(pnlBarChart);
        split.Panel2.Controls.Add(pnlPieChart);

        tpBD.Controls.Add(split);
        tpBD.Controls.Add(lblBDTitle);
        tpBD.Controls.Add(toolbar);

        pnlBarChart.Paint += PaintBarChart;
        pnlPieChart.Paint += PaintPieChart;
        btnVe.Click += (s, e) => DrawCharts();
        numBDNam.ValueChanged += (s, e) => DrawCharts();
    }

    internal void DrawCharts()
    {
        pnlBarChart.Invalidate();
        pnlPieChart.Invalidate();
    }

    private void PaintBarChart(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(AppTheme.Surface);
        int nam = (int)numBDNam.Value;

        var data = new List<(string label, decimal value)>();
        for (int m = 1; m <= 12; m++)
        {
            var recs = DataService.GetSalaryByMonth(m, nam);
            data.Add(($"T{m}", recs.Sum(r => r.LuongThucNhan)));
        }

        DrawBarChart(g, pnlBarChart.ClientRectangle, data,
            $"Tổng Lương Thực Nhận Theo Tháng – Năm {nam}",
            Color.FromArgb(0, 120, 215));
    }

    private void PaintPieChart(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(AppTheme.Surface);
        int t = (int)numBLThang.Value, n = (int)numBLNam.Value;

        var recs = DataService.GetSalaryByMonth(t, n);
        var data = recs
            .GroupBy(r => r.PhongBan)
            .Select(gr => (gr.Key.Length > 0 ? gr.Key : "Khác", gr.Sum(r => r.LuongThucNhan)))
            .OrderByDescending(x => x.Item2)
            .ToList();

        DrawPieChart(g, pnlPieChart.ClientRectangle, data,
            $"Phân Bổ Lương Theo Phòng Ban – T{t}/{n}");
    }

    private static readonly Color[] ChartColors = {
        Color.FromArgb(0,  120, 215), Color.FromArgb(0,  180, 100), Color.FromArgb(220, 80,  50),
        Color.FromArgb(160,80,  220), Color.FromArgb(220, 160, 0),  Color.FromArgb(0,  200, 200),
        Color.FromArgb(200,80,  160), Color.FromArgb(80,  140, 60),  Color.FromArgb(60,  80,  200)
    };

    private static void DrawBarChart(Graphics g, Rectangle bounds, List<(string label, decimal value)> data, string title, Color barColor)
    {
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        int pad = 50, topPad = 55, botPad = 50;
        int chartW = bounds.Width - pad * 2;
        int chartH = bounds.Height - topPad - botPad;
        if (chartW <= 0 || chartH <= 0 || data.Count == 0) return;

        decimal maxVal = data.Max(d => d.value);
        if (maxVal == 0) maxVal = 1;

        var titleFont = new Font("Segoe UI", 10f, FontStyle.Bold);
        var axisFont  = new Font("Segoe UI", 8f);
        var valFont   = new Font("Segoe UI", 7.5f);

        g.DrawString(title, titleFont, Brushes.Black, new RectangleF(pad, 10, chartW, 30), new StringFormat { Alignment = StringAlignment.Center });

        g.DrawLine(Pens.Gray, pad, topPad, pad, topPad + chartH);
        g.DrawLine(Pens.Gray, pad, topPad + chartH, bounds.Width - pad, topPad + chartH);

        for (int i = 0; i <= 4; i++)
        {
            int y = topPad + chartH - (int)(chartH * i / 4.0);
            g.DrawLine(new Pen(Color.LightGray, 0.5f), pad, y, bounds.Width - pad, y);
            decimal gridVal = maxVal * i / 4;
            g.DrawString(FormatMillion(gridVal), axisFont, Brushes.Gray, 0, y - 8, new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center });
        }

        int barW = Math.Max(4, chartW / data.Count - 8);
        int barGap = chartW / data.Count;

        for (int i = 0; i < data.Count; i++)
        {
            int barH = (int)(chartH * data[i].value / maxVal);
            int x = pad + i * barGap + (barGap - barW) / 2;
            int y = topPad + chartH - barH;

            using var grad = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Rectangle(x, y, barW, barH + 1), barColor, Color.FromArgb(180, barColor), System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            if (barH > 0) g.FillRectangle(grad, x, y, barW, barH);

            g.DrawString(data[i].label, axisFont, Brushes.Gray, x + barW / 2f, topPad + chartH + 6, new StringFormat { Alignment = StringAlignment.Center });
            if (data[i].value > 0)
                g.DrawString(FormatMillion(data[i].value), valFont, Brushes.DimGray, x + barW / 2f, y - 14, new StringFormat { Alignment = StringAlignment.Center });
        }

        titleFont.Dispose(); axisFont.Dispose(); valFont.Dispose();
    }

    private static void DrawPieChart(Graphics g, Rectangle bounds, List<(string label, decimal value)> data, string title)
    {
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        if (data.Count == 0 || data.Sum(d => d.value) == 0)
        {
            g.DrawString("Không có dữ liệu", new Font("Segoe UI", 11f), Brushes.Gray, bounds, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            return;
        }

        var titleFont = new Font("Segoe UI", 10f, FontStyle.Bold);
        var lblFont   = new Font("Segoe UI", 8.5f);
        g.DrawString(title, titleFont, Brushes.Black, new RectangleF(0, 10, bounds.Width, 30), new StringFormat { Alignment = StringAlignment.Center });

        int size = Math.Min(bounds.Width - 200, bounds.Height - 80);
        size = Math.Max(size, 100);
        int px = (bounds.Width - 180 - size) / 2, py = 50;
        var rect = new Rectangle(px, py, size, size);

        decimal total = data.Sum(d => d.value);
        float startAngle = -90;
        for (int i = 0; i < data.Count; i++)
        {
            float sweep = (float)(data[i].value / total * 360);
            using var brush = new SolidBrush(ChartColors[i % ChartColors.Length]);
            g.FillPie(brush, rect, startAngle, sweep);
            g.DrawPie(Pens.White, rect, startAngle, sweep);
            startAngle += sweep;
        }

        int legendY = py + 10;
        for (int i = 0; i < data.Count && i < 8; i++)
        {
            int lx = px + size + 16;
            using var brush = new SolidBrush(ChartColors[i % ChartColors.Length]);
            g.FillRectangle(brush, lx, legendY, 14, 14);
            float pct = (float)(data[i].value / total * 100);
            g.DrawString($"{data[i].label}\n{pct:F1}%  {FormatMillion(data[i].value)}", lblFont, Brushes.Black, lx + 18, legendY - 1);
            legendY += 36;
        }

        titleFont.Dispose(); lblFont.Dispose();
    }

    private static string FormatMillion(decimal val)
    {
        if (val >= 1_000_000_000) return $"{val / 1_000_000_000:F1}T";
        if (val >= 1_000_000) return $"{val / 1_000_000:F1}M";
        if (val >= 1_000) return $"{val / 1_000:F0}K";
        return val.ToString("N0");
    }
}
