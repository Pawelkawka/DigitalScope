using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DigitalScope.Controls;

public partial class ColorPickerControl : UserControl
{
    public event Action<string>? ColorPicked;

    private double _hue = 0;
    private double _sat = 1;
    private double _val = 1;

    private bool _draggingSv  = false;
    private bool _draggingHue = false;
    private bool _suppressHex = false;

    private const double SvW  = 224;
    private const double SvH  = 150;
    private const double HueW = 224;

    public ColorPickerControl()
    {
        InitializeComponent();
        UpdateAll();
    }

    public void SetHex(string hex)
    {
        var c = TryParseHex(hex);
        if (c is null) return;
        RgbToHsv(c.Value.R / 255.0, c.Value.G / 255.0, c.Value.B / 255.0,
                 out _hue, out _sat, out _val);
        UpdateAll();
    }

    private void SvCanvas_MouseDown(object s, MouseButtonEventArgs e)
    {
        _draggingSv = true;
        SvCanvas.CaptureMouse();
        SetSvFromPoint(e.GetPosition(SvCanvas));
    }

    private void SvCanvas_MouseUp(object s, MouseButtonEventArgs e)
    {
        _draggingSv = false;
        SvCanvas.ReleaseMouseCapture();
    }

    private void SvCanvas_MouseMove(object s, MouseEventArgs e)
    {
        if (!_draggingSv || e.LeftButton != MouseButtonState.Pressed) return;
        SetSvFromPoint(e.GetPosition(SvCanvas));
    }

    private void SetSvFromPoint(System.Windows.Point p)
    {
        _sat = Math.Clamp(p.X / SvW, 0, 1);
        _val = Math.Clamp(1 - p.Y / SvH, 0, 1);
        UpdateSvCursor();
        UpdatePreview();
    }

    private void HueCanvas_MouseDown(object s, MouseButtonEventArgs e)
    {
        _draggingHue = true;
        HueCanvas.CaptureMouse();
        SetHueFromPoint(e.GetPosition(HueCanvas));
    }

    private void HueCanvas_MouseUp(object s, MouseButtonEventArgs e)
    {
        _draggingHue = false;
        HueCanvas.ReleaseMouseCapture();
    }

    private void HueCanvas_MouseMove(object s, MouseEventArgs e)
    {
        if (!_draggingHue || e.LeftButton != MouseButtonState.Pressed) return;
        SetHueFromPoint(e.GetPosition(HueCanvas));
    }

    private void SetHueFromPoint(System.Windows.Point p)
    {
        _hue = Math.Clamp(p.X / HueW * 360.0, 0, 360);
        UpdateHueCursor();
        UpdateHueBackground();
        UpdatePreview();
    }

    private void TbPickerHex_TextChanged(object s, TextChangedEventArgs e)
    {
        if (_suppressHex) return;
        var c = TryParseHex(TbPickerHex.Text);
        if (c is null) return;
        RgbToHsv(c.Value.R / 255.0, c.Value.G / 255.0, c.Value.B / 255.0,
                 out _hue, out _sat, out _val);
        UpdateHueCursor();
        UpdateHueBackground();
        UpdateSvCursor();
        ColorPreviewBorder.Background = new SolidColorBrush(c.Value);
    }

    private void BtnOk_Click(object s, RoutedEventArgs e)
        => ColorPicked?.Invoke(GetHex());

    private void UpdateAll()
    {
        UpdateHueCursor();
        UpdateHueBackground();
        UpdateSvCursor();
        UpdatePreview();
    }

    private void UpdateHueBackground()
        => GsHue.Color = HsvToRgb(_hue, 1, 1);

    private void UpdateSvCursor()
    {
        Canvas.SetLeft(SvCursor, _sat * SvW - 6);
        Canvas.SetTop(SvCursor,  (1 - _val) * SvH - 6);
    }

    private void UpdateHueCursor()
        => Canvas.SetLeft(HueCursor, _hue / 360.0 * HueW - 2);

    private void UpdatePreview()
    {
        var c = HsvToRgb(_hue, _sat, _val);
        ColorPreviewBorder.Background = new SolidColorBrush(c);
        _suppressHex     = true;
        TbPickerHex.Text = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        _suppressHex     = false;
    }

    private string GetHex()
    {
        var c = HsvToRgb(_hue, _sat, _val);
        return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    }

    private static Color HsvToRgb(double h, double s, double v)
    {
        if (s == 0)
        {
            byte gray = (byte)(v * 255);
            return Color.FromRgb(gray, gray, gray);
        }

        double sector = h / 60.0;
        int    i      = (int)Math.Floor(sector);
        double f      = sector - i;
        double p      = v * (1 - s);
        double q      = v * (1 - s * f);
        double t      = v * (1 - s * (1 - f));

        (double r, double g, double b) = (i % 6) switch
        {
            0 => (v, t, p),
            1 => (q, v, p),
            2 => (p, v, t),
            3 => (p, q, v),
            4 => (t, p, v),
            _ => (v, p, q),
        };

        return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }

    private static void RgbToHsv(double r, double g, double b,
                                   out double h, out double s, out double v)
    {
        double max   = Math.Max(r, Math.Max(g, b));
        double min   = Math.Min(r, Math.Min(g, b));
        double delta = max - min;
        v = max;
        s = max == 0 ? 0 : delta / max;

        if (delta == 0) { h = 0; return; }

        if      (max == r) h = 60 * (((g - b) / delta) % 6);
        else if (max == g) h = 60 * ((b - r) / delta + 2);
        else               h = 60 * ((r - g) / delta + 4);

        if (h < 0) h += 360;
    }

    private static Color? TryParseHex(string? hex)
    {
        if (string.IsNullOrEmpty(hex)) return null;
        hex = hex.TrimStart('#');
        if (hex.Length == 6 &&
            uint.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out uint val))
        {
            return Color.FromRgb(
                (byte)(val >> 16),
                (byte)(val >> 8 & 0xFF),
                (byte)(val & 0xFF));
        }
        return null;
    }
}
