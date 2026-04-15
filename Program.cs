using System.Windows.Forms;
using DesktopAnimatedWallpaper.Models;
using DesktopAnimatedWallpaper.Presenters;
using DesktopAnimatedWallpaper.Services;
using DesktopAnimatedWallpaper.Views;

namespace DesktopAnimatedWallpaper;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        try
        {
            LibVlcBootstrapper.Initialize();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось инициализировать видеодвижок VLC: {ex.Message}",
                "Animated Wallpaper",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        using var wallpaperService = new WallpaperPlayerService();
        var settings = new WallpaperSettings();
        using var mainForm = new MainForm();
        using var presenter = new MainPresenter(mainForm, wallpaperService, settings);

        Application.Run(mainForm);
    }
}
