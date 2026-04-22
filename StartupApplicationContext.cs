using System.Windows.Forms;
using DesktopAnimatedWallpaper.Models;
using DesktopAnimatedWallpaper.Presenters;
using DesktopAnimatedWallpaper.Services;
using DesktopAnimatedWallpaper.Views;

namespace DesktopAnimatedWallpaper;

internal sealed class StartupApplicationContext : ApplicationContext
{
    private static readonly TimeSpan MinimumSplashTime = TimeSpan.FromMilliseconds(900);

    private readonly DateTime _startedAt = DateTime.UtcNow;
    private readonly StartupSplashForm _splashForm;
    private WallpaperPlayerService? _wallpaperService;
    private MainPresenter? _presenter;
    private MainForm? _mainForm;
    private bool _startupCompleted;

    public StartupApplicationContext()
    {
        _splashForm = new StartupSplashForm();
        _splashForm.FormClosed += OnSplashFormClosed;
        _splashForm.Shown += OnSplashFormShown;
        _splashForm.Show();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _presenter?.Dispose();
            _wallpaperService?.Dispose();
            _splashForm.Dispose();
            _mainForm?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void OnSplashFormShown(object? sender, EventArgs e)
    {
        _splashForm.SetProgress(0.68, "Загрузка видеодвижка");
        Task.Run(InitializeVideoEngine).ContinueWith(OnVideoEngineInitialized);
    }

    private static void InitializeVideoEngine()
    {
        LibVlcBootstrapper.Initialize();
    }

    private void OnVideoEngineInitialized(Task initializationTask)
    {
        if (_splashForm.IsDisposed)
        {
            return;
        }

        try
        {
            _splashForm.BeginInvoke((MethodInvoker)(() =>
            {
                if (initializationTask.IsFaulted)
                {
                    ShowStartupError(initializationTask.Exception?.GetBaseException().Message ?? "Неизвестная ошибка.");
                    return;
                }

                try
                {
                    CreateMainForm();
                    CompleteStartupAfterMinimumDelay();
                }
                catch (Exception ex)
                {
                    ShowStartupError(ex.Message);
                }
            }));
        }
        catch (ObjectDisposedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void CreateMainForm()
    {
        _splashForm.SetProgress(0.82, "Подготовка интерфейса");

        _wallpaperService = new WallpaperPlayerService();
        var settings = new WallpaperSettings();
        _mainForm = new MainForm();
        _presenter = new MainPresenter(_mainForm, _wallpaperService, settings);
        _mainForm.FormClosed += OnMainFormClosed;

        _splashForm.SetProgress(1.0, "Готово");
    }

    private void CompleteStartupAfterMinimumDelay()
    {
        var elapsed = DateTime.UtcNow - _startedAt;
        var delay = Math.Max(150, (int)(MinimumSplashTime - elapsed).TotalMilliseconds);
        var timer = new System.Windows.Forms.Timer
        {
            Interval = delay,
        };

        timer.Tick += (_, _) =>
        {
            timer.Stop();
            timer.Dispose();
            ShowMainForm();
        };
        timer.Start();
    }

    private void ShowMainForm()
    {
        if (_mainForm == null)
        {
            ShowStartupError("Главное окно не было создано.");
            return;
        }

        _startupCompleted = true;
        MainForm = _mainForm;
        _mainForm.Show();
        _splashForm.Close();
    }

    private void ShowStartupError(string message)
    {
        _startupCompleted = true;
        _splashForm.SetProgress(1.0, "Ошибка запуска");
        MessageBox.Show(
            _splashForm,
            $"Не удалось запустить приложение: {message}",
            "Animated Wallpaper",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
        _splashForm.Close();
        ExitThread();
    }

    private void OnSplashFormClosed(object? sender, FormClosedEventArgs e)
    {
        if (!_startupCompleted)
        {
            ExitThread();
        }
    }

    private void OnMainFormClosed(object? sender, FormClosedEventArgs e)
    {
        ExitThread();
    }
}
