using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace DesktopAnimatedWallpaper.Services;

internal static class NativeDesktopHost
{
    private const uint ProgmanSpawnWorkerMessage = 0x052C;
    private const uint SendMessageTimeoutFlags = 0x0000;
    private const int WindowStyleIndex = -16;
    private const long ChildWindowStyle = 0x40000000L;
    private const long PopupWindowStyle = 0x80000000L;
    private const long VisibleWindowStyle = 0x10000000L;
    private const uint SetWindowPosFlags = 0x0040 | 0x0010 | 0x0200;
    private const uint RedrawWindowFlags = 0x0001 | 0x0004 | 0x0080 | 0x0100;
    private const int ShowWindowHide = 0;
    private const int ShowWindowShow = 5;
    private static readonly IntPtr HwndBottom = new(1);

    public static DesktopHostAttachment AttachToDesktop(IntPtr windowHandle, Rectangle bounds)
    {
        var hostWindow = GetWallpaperHostWindow();
        if (hostWindow == IntPtr.Zero)
        {
            throw new InvalidOperationException("Не удалось найти окно рабочего стола.");
        }

        var style = GetWindowStyle(windowHandle);
        style &= ~PopupWindowStyle;
        style |= ChildWindowStyle | VisibleWindowStyle;
        SetWindowStyle(windowHandle, style);

        ShowWindowIfValid(hostWindow, ShowWindowShow);
        SetParent(windowHandle, hostWindow);
        ResizeWindow(windowHandle, bounds);

        return new DesktopHostAttachment(hostWindow, ShouldHideHostOnDetach(hostWindow));
    }

    public static void DetachFromDesktop(IntPtr windowHandle, Rectangle bounds, DesktopHostAttachment attachment)
    {
        var style = GetWindowStyle(windowHandle);
        style &= ~ChildWindowStyle;
        style |= PopupWindowStyle | VisibleWindowStyle;
        SetWindowStyle(windowHandle, style);

        SetParent(windowHandle, IntPtr.Zero);
        ResizeWindow(windowHandle, bounds);

        if (attachment.HideHostOnDetach)
        {
            ShowWindowIfValid(attachment.HostWindow, ShowWindowHide);
        }
    }

    public static void ResizeWindow(IntPtr windowHandle, Rectangle bounds)
    {
        SetWindowPos(
            windowHandle,
            HwndBottom,
            bounds.Left,
            bounds.Top,
            bounds.Width,
            bounds.Height,
            SetWindowPosFlags);
    }

    public static void RefreshDesktop()
    {
        RedrawWindowIfValid(GetDesktopWindow());
        RedrawWindowIfValid(FindWindow("Progman", null));

        EnumWindows((topHandle, _) =>
        {
            if (string.Equals(GetWindowClassName(topHandle), "WorkerW", StringComparison.Ordinal))
            {
                RedrawWindowIfValid(topHandle);
            }

            return true;
        }, IntPtr.Zero);
    }

    public readonly struct DesktopHostAttachment
    {
        public DesktopHostAttachment(IntPtr hostWindow, bool hideHostOnDetach)
        {
            HostWindow = hostWindow;
            HideHostOnDetach = hideHostOnDetach;
        }

        public IntPtr HostWindow { get; }

        public bool HideHostOnDetach { get; }
    }

    private static IntPtr GetWallpaperHostWindow()
    {
        var progman = FindWindow("Progman", null);
        if (progman != IntPtr.Zero)
        {
            _ = SendMessageTimeout(
                progman,
                ProgmanSpawnWorkerMessage,
                IntPtr.Zero,
                IntPtr.Zero,
                SendMessageTimeoutFlags,
                1000,
                out _);
        }

        IntPtr workerWindow = IntPtr.Zero;

        EnumWindows((topHandle, _) =>
        {
            var shellView = FindWindowEx(topHandle, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (shellView == IntPtr.Zero)
            {
                return true;
            }

            workerWindow = FindWindowEx(IntPtr.Zero, topHandle, "WorkerW", null);
            return workerWindow == IntPtr.Zero;
        }, IntPtr.Zero);

        return workerWindow != IntPtr.Zero ? workerWindow : progman;
    }

    private static long GetWindowStyle(IntPtr windowHandle)
    {
        return IntPtr.Size == 8
            ? GetWindowLongPtr(windowHandle, WindowStyleIndex).ToInt64()
            : GetWindowLong(windowHandle, WindowStyleIndex);
    }

    private static void SetWindowStyle(IntPtr windowHandle, long style)
    {
        if (IntPtr.Size == 8)
        {
            SetWindowLongPtr(windowHandle, WindowStyleIndex, new IntPtr(style));
            return;
        }

        SetWindowLong(windowHandle, WindowStyleIndex, (int)style);
    }

    private static void RedrawWindowIfValid(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return;
        }

        RedrawWindow(windowHandle, IntPtr.Zero, IntPtr.Zero, RedrawWindowFlags);
    }

    private static string GetWindowClassName(IntPtr windowHandle)
    {
        var className = new StringBuilder(256);
        return GetClassName(windowHandle, className, className.Capacity) > 0
            ? className.ToString()
            : string.Empty;
    }

    private static bool ShouldHideHostOnDetach(IntPtr hostWindow)
    {
        if (hostWindow == IntPtr.Zero)
        {
            return false;
        }

        if (!string.Equals(GetWindowClassName(hostWindow), "WorkerW", StringComparison.Ordinal))
        {
            return false;
        }

        return FindWindowEx(hostWindow, IntPtr.Zero, "SHELLDLL_DefView", null) == IntPtr.Zero;
    }

    private static void ShowWindowIfValid(IntPtr windowHandle, int command)
    {
        if (windowHandle != IntPtr.Zero)
        {
            ShowWindow(windowHandle, command);
        }
    }

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindowEx(
        IntPtr parentHandle,
        IntPtr childAfter,
        string? className,
        string? windowTitle);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SendMessageTimeout(
        IntPtr hWnd,
        uint msg,
        IntPtr wParam,
        IntPtr lParam,
        uint fuFlags,
        uint uTimeout,
        out IntPtr lpdwResult);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RedrawWindow(
        IntPtr hWnd,
        IntPtr lprcUpdate,
        IntPtr hrgnUpdate,
        uint flags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint uFlags);
}
