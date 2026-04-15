using LibVLCSharp.Shared;

namespace DesktopAnimatedWallpaper.Services;

internal static class LibVlcBootstrapper
{
    private static bool _initialized;
    private static readonly object SyncRoot = new();

    public static void Initialize()
    {
        lock (SyncRoot)
        {
            if (_initialized)
            {
                return;
            }

            var architectureFolder = Environment.Is64BitProcess ? "win-x64" : "win-x86";
            var nativePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libvlc", architectureFolder);
            if (!Directory.Exists(nativePath))
            {
                throw new DirectoryNotFoundException(
                    $"Не найдены нативные библиотеки VLC в папке {nativePath}.");
            }

            Core.Initialize(nativePath);
            _initialized = true;
        }
    }
}
