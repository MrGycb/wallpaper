using System.Drawing;
using System.Drawing.Drawing2D;

namespace DesktopAnimatedWallpaper.Views.Ui;

internal static class RoundRectangleHelper
{
    public static GraphicsPath Create(Rectangle bounds, int radius)
    {
        var safeRadius = Math.Max(1, Math.Min(radius, Math.Min(bounds.Width, bounds.Height) / 2));
        var diameter = safeRadius * 2;
        var path = new GraphicsPath();

        path.StartFigure();
        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }
}
