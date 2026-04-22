using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DigitalScope.Core;

namespace DigitalScope.Crosshair;

public static class CrosshairRenderer
{
    public static void Draw(Canvas canvas, AppConfig config)
    {
        canvas.Children.Clear();

        double w     = canvas.Width;
        double h     = canvas.Height;
        double cx    = w / 2;
        double cy    = h / 2;
        double size  = Math.Clamp(config.OverlayCrosshairSize, AppSettings.MinOverlayCrosshairSize, AppSettings.MaxOverlayCrosshairSize);
        double thick = Math.Clamp(size * 0.12, 1.5, 3.5);
        double gap   = Math.Clamp(config.OverlayCrosshairGap, 0, size - 1);

        var brush = ParseBrush(config.OverlayCrosshairColor) ?? Brushes.Red;

        switch (config.OverlayCrosshairType)
        {
            case "Dot":
                DrawDot(canvas, cx, cy, size, brush);
                break;
            default: // "Cross"
                DrawCross(canvas, cx, cy, size, thick, gap, brush);
                break;
        }
    }

    private static void DrawCross(Canvas canvas, double cx, double cy, double size, double thick, double gap, Brush brush)
    {
        // Left
        AddLine(canvas, cx - size, cy, cx - gap, cy, thick, brush);
        // Right
        AddLine(canvas, cx + gap, cy, cx + size, cy, thick, brush);
        // Top
        AddLine(canvas, cx, cy - size, cx, cy - gap, thick, brush);
        // Bottom
        AddLine(canvas, cx, cy + gap, cx, cy + size, thick, brush);
    }

    private static void DrawDot(Canvas canvas, double cx, double cy, double size, Brush brush)
    {
        double r = Math.Max(size * 0.4, 1.0);
        var el = new Ellipse
        {
            Width  = r * 2,
            Height = r * 2,
            Fill   = brush
        };
        Canvas.SetLeft(el, cx - r);
        Canvas.SetTop(el,  cy - r);
        canvas.Children.Add(el);
    }

    private static void AddLine(Canvas canvas, double x1, double y1, double x2, double y2, double thick, Brush brush)
    {
        var line = new Line
        {
            X1              = x1,
            Y1              = y1,
            X2              = x2,
            Y2              = y2,
            Stroke          = brush,
            StrokeThickness = thick,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap   = PenLineCap.Round
        };
        canvas.Children.Add(line);
    }

    private static SolidColorBrush? ParseBrush(string hex)
    {
        try
        {
            var c = (Color)ColorConverter.ConvertFromString(hex);
            return new SolidColorBrush(c);
        }
        catch { return null; }
    }

    public static double CanvasSize(AppConfig config)
    {
        double size = Math.Clamp(config.OverlayCrosshairSize, AppSettings.MinOverlayCrosshairSize, AppSettings.MaxOverlayCrosshairSize);
        return Math.Max(size * 2 + 20, 30);
    }
}
