using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DesktopAnimatedWallpaper.Models;
using DesktopAnimatedWallpaper.Views.Ui;

namespace DesktopAnimatedWallpaper.Views;

internal sealed class MainForm : Form, IMainView
{
    private readonly TableLayoutPanel _rootLayout;
    private readonly GlassCardPanel _headerCard;
    private readonly GlassCardPanel _fileCard;
    private readonly GlassCardPanel _actionCard;
    private readonly ThemeButton _themeToggleButton;
    private readonly ThemeButton _browseButton;
    private readonly ThemeButton _applyButton;
    private readonly ThemeButton _stopButton;
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _trayMenu;
    private readonly Label _titleLabel;
    private readonly Label _subtitleLabel;
    private readonly Label _fileSectionLabel;
    private readonly Label _fileStateLabel;
    private readonly Label _fileHintLabel;
    private readonly Label _actionSectionLabel;
    private readonly Label _statusLabel;
    private string _selectedVideoPath = string.Empty;
    private AppTheme _currentTheme = AppTheme.Dark;
    private ThemePalette _palette = ThemePalette.Create(AppTheme.Dark);

    public MainForm()
    {
        SuspendLayout();

        DoubleBuffered = true;
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        Text = "Animated Wallpaper";
        Icon = LoadApplicationIcon();
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(860, 560);
        Size = new Size(960, 648);
        Padding = new Padding(20);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;

        _rootLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = new Padding(0),
            Padding = new Padding(0),
            BackColor = Color.Transparent,
        };
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 162));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        _headerCard = CreateCard(new Padding(0, 0, 0, 14));
        _fileCard = CreateCard(new Padding(0, 0, 0, 14));
        _actionCard = CreateCard(Padding.Empty);

        _titleLabel = CreateAutoLabel(new Font("Segoe UI Semibold", 24F, FontStyle.Bold, GraphicsUnit.Point));
        _titleLabel.Text = "Animated Wallpaper";

        _subtitleLabel = CreateParagraphLabel(new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point), 42);
        _subtitleLabel.Text = "Запускайте локальные ролики как обои без звука.";

        _fileSectionLabel = CreateAutoLabel(new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point));
        _fileSectionLabel.Text = "Видео";

        _fileStateLabel = CreateAutoLabel(new Font("Segoe UI Semibold", 18F, FontStyle.Bold, GraphicsUnit.Point));
        _fileStateLabel.Text = "Файл не добавлен";

        _fileHintLabel = CreateParagraphLabel(new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point), 40);
        _fileHintLabel.AutoEllipsis = true;
        _fileHintLabel.Text = "Нажмите «Добавить видео». Показывается только имя файла.";

        _actionSectionLabel = CreateAutoLabel(new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point));
        _actionSectionLabel.Text = "Управление";

        _statusLabel = CreateParagraphLabel(new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point), 24);
        _statusLabel.Text = "Добавьте видео и нажмите «Запустить обои».";

        _themeToggleButton = CreateButton("Светлая тема", 162, 42);
        _themeToggleButton.Click += (_, _) => ThemeToggleRequested?.Invoke(this, EventArgs.Empty);

        _browseButton = CreateButton("Добавить видео", 164, 44);
        _browseButton.Click += (_, _) => BrowseRequested?.Invoke(this, EventArgs.Empty);

        _applyButton = CreateButton("Запустить обои", 176, 44);
        _applyButton.Click += (_, _) => ApplyRequested?.Invoke(this, EventArgs.Empty);

        _stopButton = CreateButton("Остановить", 144, 44);
        _stopButton.Click += (_, _) => StopRequested?.Invoke(this, EventArgs.Empty);

        _trayMenu = new ContextMenuStrip();
        _trayMenu.Items.Add("Открыть", null, (_, _) => RestoreFromTray());
        _trayMenu.Items.Add("Выход", null, (_, _) => Close());

        _notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = _trayMenu,
            Icon = Icon,
            Text = "Animated Wallpaper",
            Visible = false,
        };
        _notifyIcon.MouseDoubleClick += (_, args) =>
        {
            if (args.Button == MouseButtons.Left)
            {
                RestoreFromTray();
            }
        };

        _headerCard.Controls.AddRange(new Control[] { _titleLabel, _subtitleLabel, _themeToggleButton });
        _fileCard.Controls.AddRange(new Control[] { _fileSectionLabel, _fileStateLabel, _fileHintLabel });
        _actionCard.Controls.AddRange(new Control[] { _actionSectionLabel, _browseButton, _applyButton, _stopButton, _statusLabel });

        _rootLayout.Controls.Add(_headerCard, 0, 0);
        _rootLayout.Controls.Add(_fileCard, 0, 1);
        _rootLayout.Controls.Add(_actionCard, 0, 2);
        Controls.Add(_rootLayout);

        Resize += (_, _) => UpdateLayoutForCurrentSize();
        _headerCard.Resize += (_, _) => LayoutHeaderCard();
        _fileCard.Resize += (_, _) => LayoutFileCard();
        _actionCard.Resize += (_, _) => LayoutActionCard();

        ApplyThemePalette();
        UpdateSelectedFileView();
        UpdateLayoutForCurrentSize();

        ResumeLayout(true);
    }

    public event EventHandler? BrowseRequested;

    public event EventHandler? ApplyRequested;

    public event EventHandler? StopRequested;

    public event EventHandler? ThemeToggleRequested;

    public event EventHandler? VideoPathChanged;

    public string SelectedVideoPath
    {
        get => _selectedVideoPath;
        set
        {
            _selectedVideoPath = value ?? string.Empty;
            UpdateSelectedFileView();
            VideoPathChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public AppTheme CurrentTheme
    {
        set
        {
            _currentTheme = value;
            _palette = ThemePalette.Create(value);
            ApplyThemePalette();
            UpdateLayoutForCurrentSize();
            Invalidate();
        }
    }

    public string StatusText
    {
        set => _statusLabel.Text = value;
    }

    public string? SelectVideoFile(string? currentPath)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Выбор видео для обоев",
            Filter = "Видео и GIF (*.mp4;*.wmv;*.avi;*.mov;*.m4v;*.mkv;*.gif)|*.mp4;*.wmv;*.avi;*.mov;*.m4v;*.mkv;*.gif|Все файлы (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false,
            RestoreDirectory = true,
        };

        if (!string.IsNullOrWhiteSpace(currentPath) && File.Exists(currentPath))
        {
            dialog.InitialDirectory = Path.GetDirectoryName(currentPath);
            dialog.FileName = Path.GetFileName(currentPath);
        }

        return dialog.ShowDialog(this) == DialogResult.OK ? dialog.FileName : null;
    }

    public void ShowError(string message)
    {
        MessageBox.Show(this, message, "Animated Wallpaper", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public void SetApplyEnabled(bool enabled)
    {
        _applyButton.Enabled = enabled;
    }

    public void SetStopEnabled(bool enabled)
    {
        _stopButton.Enabled = enabled;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
            ClientRectangle,
            _palette.FormBackgroundStart,
            _palette.FormBackgroundEnd,
            System.Drawing.Drawing2D.LinearGradientMode.Vertical);
        e.Graphics.FillRectangle(brush, ClientRectangle);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        if (WindowState == FormWindowState.Minimized)
        {
            HideToTray();
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _notifyIcon.Visible = false;
        base.OnFormClosing(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _notifyIcon.Dispose();
            _trayMenu.Dispose();
        }

        base.Dispose(disposing);
    }

    private static GlassCardPanel CreateCard(Padding margin)
    {
        return new GlassCardPanel
        {
            Dock = DockStyle.Fill,
            Margin = margin,
        };
    }

    private static Label CreateAutoLabel(Font font)
    {
        return new Label
        {
            AutoSize = true,
            Font = font,
            BackColor = Color.Transparent,
        };
    }

    private static Label CreateParagraphLabel(Font font, int height)
    {
        return new Label
        {
            AutoSize = false,
            Height = height,
            Font = font,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.TopLeft,
        };
    }

    private static ThemeButton CreateButton(string text, int width, int height)
    {
        return new ThemeButton
        {
            Text = text,
            Width = width,
            Height = height,
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point),
        };
    }

    private static Icon LoadApplicationIcon()
    {
        return Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
    }

    private void HideToTray()
    {
        _notifyIcon.Visible = true;
        ShowInTaskbar = false;
        Hide();
    }

    private void RestoreFromTray()
    {
        Show();
        ShowInTaskbar = true;
        WindowState = FormWindowState.Normal;
        _notifyIcon.Visible = false;
        Activate();
    }

    private void UpdateSelectedFileView()
    {
        if (string.IsNullOrWhiteSpace(_selectedVideoPath))
        {
            _fileStateLabel.Text = "Файл не добавлен";
            _fileHintLabel.Text = "Нажмите «Добавить видео». Показывается только имя файла.";
            return;
        }

        _fileStateLabel.Text = "Файл добавлен";
        _fileHintLabel.Text = Path.GetFileName(_selectedVideoPath);
    }

    private void ApplyThemePalette()
    {
        BackColor = _palette.FormBackgroundEnd;
        ForeColor = _palette.TextPrimary;

        ApplyCardTheme(_headerCard);
        ApplyCardTheme(_fileCard);
        ApplyCardTheme(_actionCard);

        ApplyLabelTheme(_titleLabel, _palette.TextPrimary);
        ApplyLabelTheme(_subtitleLabel, _palette.TextSecondary);
        ApplyLabelTheme(_fileSectionLabel, _palette.TextMuted);
        ApplyLabelTheme(_fileStateLabel, _palette.TextPrimary);
        ApplyLabelTheme(_fileHintLabel, _palette.TextSecondary);
        ApplyLabelTheme(_actionSectionLabel, _palette.TextMuted);
        ApplyLabelTheme(_statusLabel, _palette.AccentText);

        _browseButton.Style = _palette.SecondaryButton;
        _applyButton.Style = _palette.PrimaryButton;
        _stopButton.Style = _palette.DangerButton;
        _themeToggleButton.Style = _palette.SecondaryButton;
        _themeToggleButton.Text = _currentTheme == AppTheme.Dark ? "Светлая тема" : "Тёмная тема";
    }

    private void ApplyCardTheme(GlassCardPanel card)
    {
        card.FillColor = _palette.CardBackgroundColor;
        card.BorderColor = _palette.CardBorderColor;
        card.ShineColor = _palette.CardShineColor;
        card.BackColor = _palette.CardBackgroundColor;
    }

    private static void ApplyLabelTheme(Label label, Color foreColor)
    {
        label.ForeColor = foreColor;
        label.BackColor = Color.Transparent;
    }

    private void UpdateLayoutForCurrentSize()
    {
        LayoutHeaderCard();
        LayoutFileCard();
        LayoutActionCard();
    }

    private void LayoutHeaderCard()
    {
        var padding = _headerCard.Padding;
        var themeButtonX = _headerCard.ClientSize.Width - padding.Right - _themeToggleButton.Width;
        var textWidth = Math.Max(260, themeButtonX - padding.Left - 28);

        _titleLabel.MaximumSize = new Size(textWidth, 0);
        _titleLabel.Location = new Point(padding.Left, padding.Top - 2);

        _subtitleLabel.Width = textWidth;
        _subtitleLabel.Location = new Point(padding.Left, _titleLabel.Bottom + 10);

        _themeToggleButton.Location = new Point(themeButtonX, padding.Top + 2);
    }

    private void LayoutFileCard()
    {
        var padding = _fileCard.Padding;
        var contentWidth = Math.Max(260, _fileCard.ClientSize.Width - padding.Horizontal);

        _fileSectionLabel.Location = new Point(padding.Left, padding.Top);
        _fileStateLabel.MaximumSize = new Size(contentWidth, 0);
        _fileStateLabel.Location = new Point(padding.Left, _fileSectionLabel.Bottom + 18);

        _fileHintLabel.Width = contentWidth;
        _fileHintLabel.Location = new Point(padding.Left, _fileStateLabel.Bottom + 12);
    }

    private void LayoutActionCard()
    {
        var padding = _actionCard.Padding;
        var gap = 14;
        var buttonY = _actionSectionLabel.Bottom + 22;
        var rowWidth = _browseButton.Width + _applyButton.Width + _stopButton.Width + (gap * 2);
        var availableWidth = _actionCard.ClientSize.Width - padding.Horizontal;
        var startX = padding.Left;

        if (rowWidth > availableWidth)
        {
            startX = padding.Left + Math.Max(0, (availableWidth - rowWidth) / 2);
        }

        _actionSectionLabel.Location = new Point(padding.Left, padding.Top);

        _browseButton.Location = new Point(startX, buttonY);
        _applyButton.Location = new Point(_browseButton.Right + gap, buttonY);
        _stopButton.Location = new Point(_applyButton.Right + gap, buttonY);

        _statusLabel.Width = Math.Max(320, availableWidth);
        _statusLabel.Location = new Point(padding.Left, _browseButton.Bottom + 18);
    }
}
