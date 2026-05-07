namespace QuanLyLuong;

public static class AppTheme
{
    public static bool IsDark { get; private set; } = false;

    public static Color Background => IsDark ? Color.FromArgb(28, 28, 30) : Color.FromArgb(245, 247, 250);
    public static Color Surface    => IsDark ? Color.FromArgb(44, 44, 46) : Color.White;
    public static Color Toolbar    => IsDark ? Color.FromArgb(36, 36, 38) : Color.FromArgb(235, 240, 248);
    public static Color Header     => IsDark ? Color.FromArgb(0, 70, 130) : Color.FromArgb(0, 90, 160);
    public static Color TextColor  => IsDark ? Color.FromArgb(230, 230, 230) : Color.FromArgb(30, 30, 30);
    public static Color SubText    => IsDark ? Color.FromArgb(160, 160, 170) : Color.FromArgb(100, 100, 110);
    public static Color GridAlt    => IsDark ? Color.FromArgb(52, 52, 54) : Color.FromArgb(245, 248, 255);
    public static Color Accent     => Color.FromArgb(0, 120, 215);
    public static Color Success    => IsDark ? Color.FromArgb(0, 180, 100) : Color.FromArgb(0, 150, 80);
    public static Color Danger     => Color.FromArgb(200, 50, 50);
    public static Color Warning    => Color.FromArgb(200, 130, 0);
    public static Color Selection  => IsDark ? Color.FromArgb(0, 80, 160) : Color.FromArgb(180, 210, 255);
    public static Color SelectText => IsDark ? Color.White : Color.Black;

    public static void Toggle() => IsDark = !IsDark;

    public static void Apply(Form form)
    {
        form.BackColor = Background;
        ApplyControls(form.Controls);
    }

    private static void ApplyControls(Control.ControlCollection controls)
    {
        foreach (Control c in controls)
        {
            switch (c)
            {
                case DataGridView dgv:
                    dgv.BackgroundColor = Surface;
                    dgv.DefaultCellStyle.BackColor = Surface;
                    dgv.DefaultCellStyle.ForeColor = TextColor;
                    dgv.DefaultCellStyle.SelectionBackColor = Selection;
                    dgv.DefaultCellStyle.SelectionForeColor = SelectText;
                    dgv.AlternatingRowsDefaultCellStyle.BackColor = GridAlt;
                    dgv.AlternatingRowsDefaultCellStyle.ForeColor = TextColor;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = Header;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    dgv.GridColor = IsDark ? Color.FromArgb(60, 60, 65) : Color.FromArgb(220, 225, 235);
                    break;
                case TabControl tc:
                    tc.BackColor = Background;
                    ApplyControls(tc.Controls);
                    break;
                case TabPage tp:
                    tp.BackColor = Background;
                    ApplyControls(tp.Controls);
                    break;
                case Panel p:
                    if (p.BackColor != Header && p.BackColor != Toolbar)
                        p.BackColor = c is FlowLayoutPanel || c is TableLayoutPanel ? Toolbar : Background;
                    ApplyControls(p.Controls);
                    break;
                case Label lbl:
                    if (lbl.BackColor != Header && lbl.BackColor != Success && lbl.BackColor != Danger)
                        lbl.BackColor = Color.Transparent;
                    if (lbl.ForeColor != Color.White)
                        lbl.ForeColor = TextColor;
                    break;
                case TextBox tb:
                    tb.BackColor = Surface; tb.ForeColor = TextColor;
                    break;
                case NumericUpDown nud:
                    nud.BackColor = Surface; nud.ForeColor = TextColor;
                    break;
                case ComboBox cb:
                    cb.BackColor = Surface; cb.ForeColor = TextColor;
                    break;
                case ListBox lb:
                    lb.BackColor = Surface; lb.ForeColor = TextColor;
                    break;
                case GroupBox gb:
                    gb.BackColor = Background; gb.ForeColor = TextColor;
                    ApplyControls(gb.Controls);
                    break;
                case SplitContainer sc:
                    sc.BackColor = Background;
                    ApplyControls(sc.Panel1.Controls);
                    ApplyControls(sc.Panel2.Controls);
                    break;
            }
        }
    }
}
