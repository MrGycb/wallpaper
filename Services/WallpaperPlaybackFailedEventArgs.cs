namespace DesktopAnimatedWallpaper.Services;

internal sealed class WallpaperPlaybackFailedEventArgs : EventArgs
{
    public WallpaperPlaybackFailedEventArgs(string message)
    {
        Message = message;
    }

    public string Message { get; }
}
