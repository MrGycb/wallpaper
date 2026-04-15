using DesktopAnimatedWallpaper.Models;

namespace DesktopAnimatedWallpaper.Views;

internal interface IMainView
{
    event EventHandler? BrowseRequested;

    event EventHandler? ApplyRequested;

    event EventHandler? StopRequested;

    event EventHandler? ThemeToggleRequested;

    event EventHandler? VideoPathChanged;

    string SelectedVideoPath { get; set; }

    AppTheme CurrentTheme { set; }

    string StatusText { set; }

    string? SelectVideoFile(string? currentPath);

    void ShowError(string message);

    void SetApplyEnabled(bool enabled);

    void SetStopEnabled(bool enabled);
}
