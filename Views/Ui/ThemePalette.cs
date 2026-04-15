using System.Drawing;
using DesktopAnimatedWallpaper.Models;

namespace DesktopAnimatedWallpaper.Views.Ui;

internal sealed class ThemePalette
{
    public Color FormBackgroundStart { get; set; }

    public Color FormBackgroundEnd { get; set; }

    public Color GlowPrimary { get; set; }

    public Color GlowSecondary { get; set; }

    public Color CardBackgroundColor { get; set; }

    public Color CardBorderColor { get; set; }

    public Color CardShineColor { get; set; }

    public Color TextPrimary { get; set; }

    public Color TextSecondary { get; set; }

    public Color TextMuted { get; set; }

    public Color AccentText { get; set; }

    public ButtonVisualStyle PrimaryButton { get; set; } = new ButtonVisualStyle();

    public ButtonVisualStyle SecondaryButton { get; set; } = new ButtonVisualStyle();

    public ButtonVisualStyle DangerButton { get; set; } = new ButtonVisualStyle();

    public static ThemePalette Create(AppTheme theme)
    {
        return theme == AppTheme.Dark ? CreateDark() : CreateLight();
    }

    private static ThemePalette CreateDark()
    {
        return new ThemePalette
        {
            FormBackgroundStart = Color.FromArgb(12, 15, 22),
            FormBackgroundEnd = Color.FromArgb(18, 22, 31),
            GlowPrimary = Color.FromArgb(28, 74, 175),
            GlowSecondary = Color.FromArgb(29, 50, 99),
            CardBackgroundColor = Color.FromArgb(28, 33, 45),
            CardBorderColor = Color.FromArgb(57, 66, 84),
            CardShineColor = Color.Transparent,
            TextPrimary = Color.FromArgb(244, 247, 252),
            TextSecondary = Color.FromArgb(185, 193, 208),
            TextMuted = Color.FromArgb(131, 141, 160),
            AccentText = Color.FromArgb(109, 176, 255),
            PrimaryButton = new ButtonVisualStyle
            {
                BackgroundColor = Color.FromArgb(55, 108, 228),
                HoverBackgroundColor = Color.FromArgb(70, 123, 243),
                PressedBackgroundColor = Color.FromArgb(42, 93, 206),
                DisabledBackgroundColor = Color.FromArgb(60, 73, 104),
                BorderColor = Color.FromArgb(55, 108, 228),
                ForegroundColor = Color.FromArgb(245, 247, 252),
                DisabledForegroundColor = Color.FromArgb(178, 186, 201),
            },
            SecondaryButton = new ButtonVisualStyle
            {
                BackgroundColor = Color.FromArgb(54, 61, 77),
                HoverBackgroundColor = Color.FromArgb(65, 74, 93),
                PressedBackgroundColor = Color.FromArgb(44, 51, 66),
                DisabledBackgroundColor = Color.FromArgb(42, 47, 58),
                BorderColor = Color.FromArgb(54, 61, 77),
                ForegroundColor = Color.FromArgb(242, 244, 250),
                DisabledForegroundColor = Color.FromArgb(136, 145, 162),
            },
            DangerButton = new ButtonVisualStyle
            {
                BackgroundColor = Color.FromArgb(123, 60, 71),
                HoverBackgroundColor = Color.FromArgb(140, 71, 83),
                PressedBackgroundColor = Color.FromArgb(104, 50, 60),
                DisabledBackgroundColor = Color.FromArgb(72, 55, 60),
                BorderColor = Color.FromArgb(123, 60, 71),
                ForegroundColor = Color.FromArgb(245, 247, 252),
                DisabledForegroundColor = Color.FromArgb(171, 160, 164),
            },
        };
    }

    private static ThemePalette CreateLight()
    {
        return new ThemePalette
        {
            FormBackgroundStart = Color.FromArgb(247, 249, 253),
            FormBackgroundEnd = Color.FromArgb(237, 242, 249),
            GlowPrimary = Color.FromArgb(180, 204, 246),
            GlowSecondary = Color.FromArgb(221, 230, 244),
            CardBackgroundColor = Color.FromArgb(250, 252, 255),
            CardBorderColor = Color.FromArgb(212, 220, 232),
            CardShineColor = Color.Transparent,
            TextPrimary = Color.FromArgb(28, 34, 46),
            TextSecondary = Color.FromArgb(84, 95, 116),
            TextMuted = Color.FromArgb(111, 122, 144),
            AccentText = Color.FromArgb(55, 101, 209),
            PrimaryButton = new ButtonVisualStyle
            {
                BackgroundColor = Color.FromArgb(57, 106, 224),
                HoverBackgroundColor = Color.FromArgb(72, 119, 237),
                PressedBackgroundColor = Color.FromArgb(46, 91, 200),
                DisabledBackgroundColor = Color.FromArgb(169, 187, 224),
                BorderColor = Color.FromArgb(57, 106, 224),
                ForegroundColor = Color.FromArgb(245, 247, 252),
                DisabledForegroundColor = Color.FromArgb(242, 246, 252),
            },
            SecondaryButton = new ButtonVisualStyle
            {
                BackgroundColor = Color.FromArgb(229, 235, 245),
                HoverBackgroundColor = Color.FromArgb(221, 229, 241),
                PressedBackgroundColor = Color.FromArgb(211, 220, 236),
                DisabledBackgroundColor = Color.FromArgb(232, 236, 243),
                BorderColor = Color.FromArgb(229, 235, 245),
                ForegroundColor = Color.FromArgb(28, 34, 46),
                DisabledForegroundColor = Color.FromArgb(130, 137, 149),
            },
            DangerButton = new ButtonVisualStyle
            {
                BackgroundColor = Color.FromArgb(244, 231, 234),
                HoverBackgroundColor = Color.FromArgb(239, 223, 227),
                PressedBackgroundColor = Color.FromArgb(228, 209, 214),
                DisabledBackgroundColor = Color.FromArgb(239, 234, 236),
                BorderColor = Color.FromArgb(244, 231, 234),
                ForegroundColor = Color.FromArgb(112, 50, 62),
                DisabledForegroundColor = Color.FromArgb(130, 137, 149),
            },
        };
    }
}
