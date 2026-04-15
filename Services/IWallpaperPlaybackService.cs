namespace DesktopAnimatedWallpaper.Services;

internal interface IWallpaperPlaybackService : IDisposable
{
    event EventHandler? PlaybackStarted;

    event EventHandler<WallpaperPlaybackFailedEventArgs>? PlaybackFailed;

    bool IsRunning { get; }

    void Play(string videoPath);

    void Stop();
}
