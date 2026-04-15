using System.IO;
using DesktopAnimatedWallpaper.Views;

namespace DesktopAnimatedWallpaper.Services;

internal sealed class WallpaperPlayerService : IWallpaperPlaybackService
{
    private readonly WallpaperForm _wallpaperForm;

    public WallpaperPlayerService()
    {
        _wallpaperForm = new WallpaperForm();
        _wallpaperForm.PlaybackStarted += OnPlaybackStarted;
        _wallpaperForm.PlaybackFailed += OnPlaybackFailed;
    }

    public event EventHandler? PlaybackStarted;

    public event EventHandler<WallpaperPlaybackFailedEventArgs>? PlaybackFailed;

    public bool IsRunning => _wallpaperForm.IsPlaying;

    public void Play(string videoPath)
    {
        if (string.IsNullOrWhiteSpace(videoPath))
        {
            throw new ArgumentException("Путь к видео не задан.", nameof(videoPath));
        }

        if (!File.Exists(videoPath))
        {
            throw new FileNotFoundException("Видео не найдено.", videoPath);
        }

        _wallpaperForm.ShowWallpaper(videoPath);
    }

    public void Stop()
    {
        _wallpaperForm.StopPlayback();
    }

    public void Dispose()
    {
        _wallpaperForm.PlaybackStarted -= OnPlaybackStarted;
        _wallpaperForm.PlaybackFailed -= OnPlaybackFailed;
        _wallpaperForm.Dispose();
    }

    private void OnPlaybackStarted(object? sender, EventArgs e)
    {
        PlaybackStarted?.Invoke(this, EventArgs.Empty);
    }

    private void OnPlaybackFailed(object? sender, WallpaperPlaybackFailedEventArgs e)
    {
        PlaybackFailed?.Invoke(this, e);
    }
}
