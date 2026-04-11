using System.Windows.Media;
using Microsoft.Win32;

namespace DigitalScope.Core;

public static class AccentColorHelper
{
    private const string DwmKey      = @"SOFTWARE\Microsoft\Windows\DWM";
    private const string AccentValue = "AccentColor";

    public static Color GetSystemAccentColor()
    {
        var fromReg = TryReadRegistry();
        if (fromReg.HasValue) return fromReg.Value;

        var glass = System.Windows.SystemParameters.WindowGlassColor;
        if (glass.A > 0)
        {
            AppLogger.Info("AccentColorHelper: using WindowGlassColor fallback.");
            return Color.FromRgb(glass.R, glass.G, glass.B);
        }

        AppLogger.Warn("AccentColorHelper: all sources failed / using fallback.");
        return Color.FromRgb(0x00, 0x78, 0xD4);
    }

    public static Color Lighten(Color c, byte amount = 24) =>
        Color.FromRgb(
            (byte)Math.Min(c.R + amount, 255),
            (byte)Math.Min(c.G + amount, 255),
            (byte)Math.Min(c.B + amount, 255));

    public static void Apply(System.Windows.ResourceDictionary resources)
    {
        var accent      = GetSystemAccentColor();
        var accentHover = Lighten(accent);

        resources["AccentColor"]      = accent;
        resources["AccentHoverColor"] = accentHover;
        resources["AccentBrush"]      = new SolidColorBrush(accent);
        resources["AccentHoverBrush"] = new SolidColorBrush(accentHover);

        AppLogger.Info($"Accent colour applied: #{accent.R:X2}{accent.G:X2}{accent.B:X2}");
    }

    private static Color? TryReadRegistry()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(DwmKey);
            var raw = key?.GetValue(AccentValue);
            if (raw is null) return null;

            uint v = (uint)Convert.ToInt32(raw);
            byte r = (byte)( v        & 0xFF);
            byte g = (byte)((v >>  8) & 0xFF);
            byte b = (byte)((v >> 16) & 0xFF);

            var c = Color.FromRgb(r, g, b);
            AppLogger.Info($"AccentColorHelper: registry → #{r:X2}{g:X2}{b:X2}");
            return c;
        }
        catch (Exception ex)
        {
            AppLogger.Warn($"AccentColorHelper: registry read failed – {ex.Message}");
            return null;
        }
    }
}
