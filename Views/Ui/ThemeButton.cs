using System.Drawing;
using System.Windows.Forms;

namespace DesktopAnimatedWallpaper.Views.Ui;

internal sealed class ThemeButton : Button
{
    private bool _hovered;
    private bool _pressed;
    private ButtonVisualStyle _style = new ButtonVisualStyle();

    public ThemeButton()
    {
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
        Region = new Region(path);
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
