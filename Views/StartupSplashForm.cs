using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DesktopAnimatedWallpaper.Views.Ui;

namespace DesktopAnimatedWallpaper.Views;

internal sealed class StartupSplashForm : Form
{
    private const float MiddleGearRadius = 72f;
    private readonly System.Windows.Forms.Timer _animationTimer;
    private readonly GearSpec[] _gears;
    private float _angle;
    private double _progress;
    private double _targetProgress;
    private string _statusText = "Загрузка приложения";

    public StartupSplashForm()
    {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);

        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.Black;
        ClientSize = new Size(900, 620);
        DoubleBuffered = true;
        FormBorderStyle = FormBorderStyle.None;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = true;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Animated Wallpaper";
        Icon = LoadApplicationIcon();

        _gears = new[]
        {
            new GearSpec(-300f, 0f, 46f, 11, Color.FromArgb(146, 66, 255), 1),
            new GearSpec(-170f, -22f, 62f, 14, Color.FromArgb(22, 169, 236), -1),
            new GearSpec(0f, -8f, 72f, 16, Color.FromArgb(113, 220, 70), 1),
            new GearSpec(165f, -18f, 58f, 12, Color.FromArgb(255, 154, 0), -1),
            new GearSpec(295f, 2f, 47f, 11, Color.FromArgb(239, 43, 141), 1),
        };

        _animationTimer = new System.Windows.Forms.Timer
        {
            Interval = 16,
        };
        _animationTimer.Tick += OnAnimationTick;
        _animationTimer.Start();
    }

    public void SetProgress(double progress, string statusText)
    {
        _targetProgress = Math.Max(0d, Math.Min(1d, progress));
        _statusText = string.IsNullOrWhiteSpace(statusText) ? "Загрузка приложения" : statusText;
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _animationTimer.Tick -= OnAnimationTick;
            _animationTimer.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var graphics = e.Graphics;
        graphics.Clear(Color.Black);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        DrawGears(graphics);
        DrawLoadingText(graphics);
        DrawProgressBar(graphics);
    }

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        _angle = (_angle + 2.2f) % 360f;

        if (_progress < _targetProgress)
        {
            var step = _targetProgress >= 1d ? 0.025d : 0.012d;
            _progress = Math.Min(_targetProgress, _progress + step);
        }
        else if (_progress > _targetProgress)
        {
            _progress = _targetProgress;
        }

        Invalidate();
    }

    private void DrawGears(Graphics graphics)
    {
        var center = new PointF(ClientSize.Width / 2f, (ClientSize.Height / 2f) - 22f);

        foreach (var gear in _gears)
        {
            var position = new PointF(center.X + gear.OffsetX, center.Y + gear.OffsetY);
            var physicalAngle = _angle * gear.Direction * (MiddleGearRadius / gear.Radius);
            DrawGear(graphics, position, gear, physicalAngle);
        }
    }

    private static void DrawGear(Graphics graphics, PointF center, GearSpec gear, float angle)
    {
        using var state = new GraphicsStateScope(graphics);
        graphics.TranslateTransform(center.X, center.Y);
        graphics.RotateTransform(angle);

        using var path = CreateGearPath(gear.Radius, gear.Teeth);
        using var glowPen = new Pen(Color.FromArgb(90, gear.Color), 9f)
        {
            LineJoin = LineJoin.Round,
        };
        using var edgePen = new Pen(ControlPaint.Light(gear.Color), 2.4f)
        {
            LineJoin = LineJoin.Round,
        };
        using var fillBrush = new LinearGradientBrush(
            new RectangleF(-gear.Radius, -gear.Radius, gear.Radius * 2f, gear.Radius * 2f),
            ControlPaint.Light(gear.Color),
            ControlPaint.Dark(gear.Color),
            LinearGradientMode.ForwardDiagonal);

        graphics.DrawPath(glowPen, path);
        graphics.FillPath(fillBrush, path);
        graphics.DrawPath(edgePen, path);

        var holeRadius = gear.Radius * 0.44f;
        using var holeBrush = new SolidBrush(Color.Black);
        using var holePen = new Pen(Color.FromArgb(140, ControlPaint.Dark(gear.Color)), 3f);
        graphics.FillEllipse(holeBrush, -holeRadius, -holeRadius, holeRadius * 2f, holeRadius * 2f);
        graphics.DrawEllipse(holePen, -holeRadius, -holeRadius, holeRadius * 2f, holeRadius * 2f);
    }

    private static GraphicsPath CreateGearPath(float radius, int teeth)
    {
        var rootRadius = radius * 0.82f;
        var points = new PointF[teeth * 4];
        var step = (Math.PI * 2d) / points.Length;

        for (var i = 0; i < points.Length; i++)
        {
            var toothPart = i % 4;
            var currentRadius = toothPart == 1 || toothPart == 2 ? radius : rootRadius;
            var angle = (i * step) - (Math.PI / 2d);
            points[i] = new PointF(
                (float)(Math.Cos(angle) * currentRadius),
                (float)(Math.Sin(angle) * currentRadius));
        }

        var path = new GraphicsPath();
        path.AddPolygon(points);
        path.CloseFigure();
        return path;
    }

    private void DrawLoadingText(Graphics graphics)
    {
        var dots = new string('.', ((int)(_angle / 30f) % 3) + 1);
        var text = $"{_statusText}{dots}";
        var textBounds = new Rectangle(0, (ClientSize.Height / 2) + 112, ClientSize.Width, 44);

        using var font = new Font("Segoe UI Semibold", 24f, FontStyle.Bold, GraphicsUnit.Point);
        TextRenderer.DrawText(
            graphics,
            text,
            font,
            textBounds,
            Color.FromArgb(178, 178, 184),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private void DrawProgressBar(Graphics graphics)
    {
        var width = Math.Min(560, ClientSize.Width - 180);
        var height = 12;
        var x = (ClientSize.Width - width) / 2;
        var y = (ClientSize.Height / 2) + 188;
        var trackBounds = new Rectangle(x, y, width, height);

        using var trackPath = RoundRectangleHelper.Create(trackBounds, height / 2);
        using var trackBrush = new SolidBrush(Color.FromArgb(31, 31, 31));
        graphics.FillPath(trackBrush, trackPath);

        var fillWidth = Math.Max(height, (int)(width * _progress));
        var fillBounds = new Rectangle(x, y, Math.Min(width, fillWidth), height);
        using var fillPath = RoundRectangleHelper.Create(fillBounds, height / 2);
        using var fillBrush = new LinearGradientBrush(
            trackBounds,
            Color.FromArgb(146, 66, 255),
            Color.FromArgb(113, 220, 70),
            LinearGradientMode.Horizontal);
        graphics.FillPath(fillBrush, fillPath);
    }

    private static Icon LoadApplicationIcon()
    {
        return Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
    }

    private readonly struct GearSpec
    {
        public GearSpec(float offsetX, float offsetY, float radius, int teeth, Color color, int direction)
        {
            OffsetX = offsetX;
            OffsetY = offsetY;
            Radius = radius;
            Teeth = teeth;
            Color = color;
            Direction = direction;
        }

        public float OffsetX { get; }

        public float OffsetY { get; }

        public float Radius { get; }

        public int Teeth { get; }

        public Color Color { get; }

        public int Direction { get; }
    }

    private sealed class GraphicsStateScope : IDisposable
    {
        private readonly Graphics _graphics;
        private readonly GraphicsState _state;

        public GraphicsStateScope(Graphics graphics)
        {
            _graphics = graphics;
            _state = graphics.Save();
        }

        public void Dispose()
        {
            _graphics.Restore(_state);
        }
    }
}
