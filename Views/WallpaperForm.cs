using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DesktopAnimatedWallpaper.Services;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using Microsoft.Win32;

namespace DesktopAnimatedWallpaper.Views;

internal sealed class WallpaperForm : Form
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(30);

    private readonly LibVLC _libVlc;
    private readonly System.Windows.Forms.Timer _startupTimer;
    private VideoView _videoView;
    private MediaPlayer _mediaPlayer;
    private Media? _media;
    private bool _desktopAttached;
    private bool _startupRecoveryAttempted;
    private string? _currentVideoPath;

    public WallpaperForm()
    {
        LibVlcBootstrapper.Initialize();

        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        TopMost = false;
        BackColor = Color.Black;

        _libVlc = new LibVLC(
            "--quiet",
            "--no-audio",
            "--aout=dummy",
            "--no-sub-autodetect-file",
            "--no-video-title-show",
            "--drop-late-frames",
            "--skip-frames",
            "--avcodec-hw=any");

        _videoView = CreateVideoView();
        _mediaPlayer = CreateMediaPlayer();
        _videoView.MediaPlayer = _mediaPlayer;
        Controls.Add(_videoView);

        _startupTimer = new System.Windows.Forms.Timer
        {
            Interval = (int)StartupTimeout.TotalMilliseconds,
        };
        _startupTimer.Tick += OnStartupTimerTick;

        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
    }

    public event EventHandler? PlaybackStarted;

    public event EventHandler<WallpaperPlaybackFailedEventArgs>? PlaybackFailed;

    public bool IsPlaying { get; private set; }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            const int toolWindowStyle = 0x00000080;
            var createParams = base.CreateParams;
            createParams.ExStyle |= toolWindowStyle;
            return createParams;
        }
    }

    public void ShowWallpaper(string videoPath)
    {
        if (!File.Exists(videoPath))
        {
            throw new FileNotFoundException("Видео не найдено.", videoPath);
        }

        _currentVideoPath = videoPath;
        _startupRecoveryAttempted = false;
        IsPlaying = true;

        PrepareWindowForLoading();
        StartPlayback(videoPath);
    }

    public void StopPlayback()
    {
        _startupTimer.Stop();
        IsPlaying = false;
        _currentVideoPath = null;
        StopAndResetMediaPlayer();
        Hide();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
            _startupTimer.Tick -= OnStartupTimerTick;
            _startupTimer.Dispose();
            DetachMediaPlayerEvents(_mediaPlayer);
            _media?.Dispose();
            _videoView.MediaPlayer = null;
            _mediaPlayer.Dispose();
            _videoView.Dispose();
            _libVlc.Dispose();
        }

        base.Dispose(disposing);
    }

    private void PrepareWindowForLoading()
    {
        ResizeToDesktop();

        if (_desktopAttached)
        {
            NativeDesktopHost.DetachFromDesktop(Handle, GetDesktopBounds());
            _desktopAttached = false;
        }

        StopAndResetMediaPlayer();

        if (!IsHandleCreated || !Visible)
        {
            Opacity = 0;
            Show();
        }
        else
        {
            Opacity = 0;
        }

        ResizeToDesktop();
    }

    private void StartPlayback(string videoPath)
    {
        _media?.Dispose();
        _media = new Media(_libVlc, new Uri(videoPath));
        _media.AddOption(":no-audio");
        _media.AddOption(":audio-track=-1");
        _media.AddOption(":sub-track=-1");
        _media.AddOption(":input-repeat=65535");
        _media.AddOption(":file-caching=5000");
        _media.AddOption(":network-caching=3000");
        _media.AddOption(":live-caching=3000");

        _mediaPlayer.Mute = true;
        _mediaPlayer.EnableHardwareDecoding = true;
        _mediaPlayer.Scale = 0f;
        _mediaPlayer.AspectRatio = null;

        _startupTimer.Stop();
        _startupTimer.Start();

        if (!_mediaPlayer.Play(_media))
        {
            RecoverOrFail("LibVLC не смог начать воспроизведение.");
        }
    }

    private void StopAndResetMediaPlayer()
    {
        _startupTimer.Stop();
        TryStopMediaPlayer(_mediaPlayer);
        DetachMediaPlayerEvents(_mediaPlayer);
        _videoView.MediaPlayer = null;
        _mediaPlayer.Dispose();

        _media?.Dispose();
        _media = null;

        _mediaPlayer = CreateMediaPlayer();
        _videoView.MediaPlayer = _mediaPlayer;
    }

    private MediaPlayer CreateMediaPlayer()
    {
        var mediaPlayer = new MediaPlayer(_libVlc)
        {
            EnableHardwareDecoding = true,
            Mute = true,
            Scale = 0f,
            AspectRatio = null,
        };

        mediaPlayer.Playing += OnMediaPlayerPlaying;
        mediaPlayer.EndReached += OnMediaPlayerEndReached;
        mediaPlayer.EncounteredError += OnMediaPlayerEncounteredError;

        return mediaPlayer;
    }

    private VideoView CreateVideoView()
    {
        return new VideoView
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Black,
        };
    }

    private void DetachMediaPlayerEvents(MediaPlayer mediaPlayer)
    {
        mediaPlayer.Playing -= OnMediaPlayerPlaying;
        mediaPlayer.EndReached -= OnMediaPlayerEndReached;
        mediaPlayer.EncounteredError -= OnMediaPlayerEncounteredError;
    }

    private void OnMediaPlayerPlaying(object? sender, EventArgs e)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke((MethodInvoker)(() => OnMediaPlayerPlaying(sender, e)));
            return;
        }

        _startupTimer.Stop();
        FinalizeStartup();
    }

    private void OnMediaPlayerEndReached(object? sender, EventArgs e)
    {
        if (IsDisposed || string.IsNullOrWhiteSpace(_currentVideoPath))
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke((MethodInvoker)(() => OnMediaPlayerEndReached(sender, e)));
            return;
        }

        _mediaPlayer.Stop();
        StartPlayback(_currentVideoPath!);
    }

    private void OnMediaPlayerEncounteredError(object? sender, EventArgs e)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke((MethodInvoker)(() => OnMediaPlayerEncounteredError(sender, e)));
            return;
        }

        RecoverOrFail("VLC столкнулся с ошибкой во время воспроизведения.");
    }

    private void FinalizeStartup()
    {
        if (!_desktopAttached)
        {
            NativeDesktopHost.AttachToDesktop(Handle, GetDesktopBounds());
            _desktopAttached = true;
        }

        ResizeToDesktop();
        Opacity = 1;
        PlaybackStarted?.Invoke(this, EventArgs.Empty);
    }

    private void OnStartupTimerTick(object? sender, EventArgs e)
    {
        RecoverOrFail("Видео слишком долго запускается.");
    }

    private void RecoverOrFail(string message)
    {
        _startupTimer.Stop();

        if (_startupRecoveryAttempted || string.IsNullOrWhiteSpace(_currentVideoPath))
        {
            FailPlayback(message);
            return;
        }

        _startupRecoveryAttempted = true;
        StartPlayback(_currentVideoPath!);
    }

    private void FailPlayback(string message)
    {
        IsPlaying = false;
        StopAndResetMediaPlayer();
        Hide();
        PlaybackFailed?.Invoke(this, new WallpaperPlaybackFailedEventArgs(message));
    }

    private void ResizeToDesktop()
    {
        var bounds = GetDesktopBounds();
        Bounds = bounds;

        if (_desktopAttached)
        {
            NativeDesktopHost.ResizeWindow(Handle, bounds);
        }
    }

    private static Rectangle GetDesktopBounds()
    {
        return SystemInformation.VirtualScreen;
    }

    private void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke((MethodInvoker)ResizeToDesktop);
            return;
        }

        ResizeToDesktop();
    }

    private static void TryStopMediaPlayer(MediaPlayer mediaPlayer)
    {
        try
        {
            mediaPlayer.Stop();
        }
        catch
        {
        }
    }
}
