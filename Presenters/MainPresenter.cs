using System.IO;
using DesktopAnimatedWallpaper.Models;
using DesktopAnimatedWallpaper.Services;
using DesktopAnimatedWallpaper.Views;

namespace DesktopAnimatedWallpaper.Presenters;

internal sealed class MainPresenter : IDisposable
{
    private readonly IMainView _view;
    private readonly IWallpaperPlaybackService _wallpaperService;
    private readonly WallpaperSettings _settings;

    public MainPresenter(
        IMainView view,
        IWallpaperPlaybackService wallpaperService,
        WallpaperSettings settings)
    {
        _view = view;
        _wallpaperService = wallpaperService;
        _settings = settings;

        _wallpaperService.PlaybackStarted += OnPlaybackStarted;
        _wallpaperService.PlaybackFailed += OnPlaybackFailed;
        _view.BrowseRequested += OnBrowseRequested;
        _view.ApplyRequested += OnApplyRequested;
        _view.StopRequested += OnStopRequested;
        _view.ThemeToggleRequested += OnThemeToggleRequested;
        _view.VideoPathChanged += OnVideoPathChanged;

        _view.CurrentTheme = _settings.Theme;
        _view.SelectedVideoPath = _settings.VideoPath ?? string.Empty;
        _view.StatusText = "Выберите видео и нажмите «Запустить обои».";
        UpdateButtons();
    }

    public void Dispose()
    {
        _wallpaperService.PlaybackStarted -= OnPlaybackStarted;
        _wallpaperService.PlaybackFailed -= OnPlaybackFailed;
        _view.BrowseRequested -= OnBrowseRequested;
        _view.ApplyRequested -= OnApplyRequested;
        _view.StopRequested -= OnStopRequested;
        _view.ThemeToggleRequested -= OnThemeToggleRequested;
        _view.VideoPathChanged -= OnVideoPathChanged;
    }

    private void OnBrowseRequested(object? sender, EventArgs e)
    {
        var selectedFile = _view.SelectVideoFile(_view.SelectedVideoPath);
        if (string.IsNullOrWhiteSpace(selectedFile))
        {
            return;
        }

        _view.SelectedVideoPath = selectedFile!;
        _view.StatusText = "Видео добавлено. Можно запускать обои.";
        UpdateButtons();
    }

    private void OnApplyRequested(object? sender, EventArgs e)
    {
        var videoPath = _view.SelectedVideoPath?.Trim();
        if (string.IsNullOrWhiteSpace(videoPath))
        {
            _view.ShowError("Сначала добавьте видеофайл.");
            return;
        }

        var resolvedPath = videoPath!;
        if (!File.Exists(resolvedPath))
        {
            _view.ShowError("Выбранный файл не найден.");
            return;
        }

        try
        {
            _wallpaperService.Play(resolvedPath);
            _settings.VideoPath = resolvedPath;
            _view.StatusText = "Видео загружается. Для большого файла это может занять немного времени.";
            UpdateButtons();
        }
        catch (Exception ex)
        {
            _view.ShowError($"Не удалось запустить видео: {ex.Message}");
        }
    }

    private void OnStopRequested(object? sender, EventArgs e)
    {
        _wallpaperService.Stop();
        _view.StatusText = "Воспроизведение остановлено.";
        UpdateButtons();
    }

    private void OnThemeToggleRequested(object? sender, EventArgs e)
    {
        _settings.Theme = _settings.Theme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
        _view.CurrentTheme = _settings.Theme;
    }

    private void OnVideoPathChanged(object? sender, EventArgs e)
    {
        UpdateButtons();
    }

    private void OnPlaybackStarted(object? sender, EventArgs e)
    {
        _view.StatusText = "Обои запущены.";
        UpdateButtons();
    }

    private void OnPlaybackFailed(object? sender, WallpaperPlaybackFailedEventArgs e)
    {
        _view.StatusText = "Ошибка воспроизведения.";
        _view.ShowError(e.Message);
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        var hasFile = !string.IsNullOrWhiteSpace(_view.SelectedVideoPath);
        _view.SetApplyEnabled(hasFile);
        _view.SetStopEnabled(_wallpaperService.IsRunning);
    }
}
