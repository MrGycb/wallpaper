namespace DesktopAnimatedWallpaper.Models;

internal sealed class WallpaperSettings
{
    public string? VideoPath { get; set; }

    public AppTheme Theme { get; set; } = AppTheme.Dark;
}
