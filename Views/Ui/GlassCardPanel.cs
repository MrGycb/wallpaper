using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DesktopAnimatedWallpaper.Views.Ui;

internal sealed class GlassCardPanel : Panel
{
    public GlassCardPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        Padding = new Padding(28);
    }

    public int CornerRadius { get; set; } = 24;

    public Color FillColor { get; set; } = Color.FromArgb(28, 30, 40);

    public Color BorderColor { get; set; } = Color.FromArgb(68, 116, 129, 158);

    public Color ShineColor { get; set; } = Color.Transparent;

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        if (Width <= 1 || Height <= 1)
        {
            return;
        }

        using var path = RoundRectangleHelper.Create(new Rectangle(0, 0, Width - 1, Height - 1), CornerRadius);
        Region = new Region(path);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var bounds = ClientRectangle;
        if (bounds.Width <= 1 || bounds.Height <= 1)
        {
            return;
        }

        var drawingBounds = Rectangle.Inflate(bounds, -1, -1);
        using var path = RoundRectangleHelper.Create(drawingBounds, CornerRadius);
        using var fillBrush = new SolidBrush(FillColor);
        using var borderPen = new Pen(BorderColor, 1f);

        e.Graphics.FillPath(fillBrush, path);
        e.Graphics.DrawPath(borderPen, path);
    }
}
