using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DesktopAnimatedWallpaper.Views.Ui;

internal sealed class ThemeButton : Button
{
    private bool _hovered;
    private bool _pressed;
    private ButtonVisualStyle _style = new ButtonVisualStyle();

    public ThemeButton()
    {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);

        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        FlatAppearance.MouseDownBackColor = Color.Empty;
        FlatAppearance.MouseOverBackColor = Color.Empty;
        UseVisualStyleBackColor = false;
        Cursor = Cursors.Hand;
        MinimumSize = new Size(132, 44);
        TabStop = false;
        TextAlign = ContentAlignment.MiddleCenter;
    }

    public int CornerRadius { get; set; } = 16;

    public ButtonVisualStyle Style
    {
        get => _style;
        set
        {
            _style = value ?? new ButtonVisualStyle();
            ApplyState();
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (Width <= 1 || Height <= 1)
        {
            return;
        }

        using var path = RoundRectangleHelper.Create(new Rectangle(0, 0, Width - 1, Height - 1), CornerRadius);
        var previousRegion = Region;
        Region = new Region(path);
        previousRegion?.Dispose();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _hovered = true;
        ApplyState();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hovered = false;
        _pressed = false;
        ApplyState();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        _pressed = true;
        ApplyState();
        base.OnMouseDown(mevent);
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        _pressed = false;
        ApplyState();
        base.OnMouseUp(mevent);
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        ApplyState();
        base.OnEnabledChanged(e);
    }

    protected override bool ShowFocusCues => false;

    protected override void OnPaint(PaintEventArgs pevent)
    {
        if (Width <= 1 || Height <= 1)
        {
            return;
        }

        pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = RoundRectangleHelper.Create(bounds, CornerRadius);
        using var fillBrush = new SolidBrush(BackColor);

        pevent.Graphics.FillPath(fillBrush, path);

        if (_style.BorderColor.A > 0)
        {
            using var borderPen = new Pen(BackColor, 1f);
            pevent.Graphics.DrawPath(borderPen, path);
        }

        const TextFormatFlags textFlags =
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis |
            TextFormatFlags.SingleLine;

        TextRenderer.DrawText(pevent.Graphics, Text, Font, ClientRectangle, ForeColor, textFlags);
    }

    private void ApplyState()
    {
        if (!Enabled)
        {
            BackColor = _style.DisabledBackgroundColor;
            ForeColor = _style.DisabledForegroundColor;
        }
        else if (_pressed)
        {
            BackColor = _style.PressedBackgroundColor;
            ForeColor = _style.ForegroundColor;
        }
        else if (_hovered)
        {
            BackColor = _style.HoverBackgroundColor;
            ForeColor = _style.ForegroundColor;
        }
        else
        {
            BackColor = _style.BackgroundColor;
            ForeColor = _style.ForegroundColor;
        }

        Invalidate();
    }
}
