using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DigitalScope.Core;

namespace DigitalScope.Tabs;

public partial class AppearanceTab : UserControl
{
    private AppConfig     _config  = null!;
    private ConfigManager _manager = null!;
    private bool          _loading;
    private string        _activeColorTag = "";

    public event Action? ConfigChanged;

    public AppearanceTab()
    {
        InitializeComponent();
        Picker.ColorPicked += Picker_ColorPicked;
    }

    public void Initialise(AppConfig config, ConfigManager manager)
    {
        _config  = config;
        _manager = manager;
        LoadValues();
    }

    public void Refresh()
    {
        if (_config is null) return;
        LoadValues();
    }

    private void LoadValues()
    {
        _loading = true;

        ChkCrosshair.IsChecked = _config.ShowCrosshair;
        SetSwatch(PrvCrosshair, _config.CrosshairColor);

        _loading = false;
    }

    private void ChkCrosshair_Changed(object s, RoutedEventArgs e)
    {
        if (_loading || _config is null) return;
        _config.ShowCrosshair = ChkCrosshair.IsChecked == true;
        Save();
    }

    private void PrvCrosshair_Click(object s, MouseButtonEventArgs e)
        => OpenPicker("crosshair", _config.CrosshairColor, (UIElement)s);

    private void OpenPicker(string tag, string hex, UIElement target)
    {
        if (_config is null) return;

        if (ColorPickerPopup.IsOpen && _activeColorTag == tag)
        {
            ColorPickerPopup.IsOpen = false;
            return;
        }

        _activeColorTag = tag;
        Picker.SetHex(hex);
        ColorPickerPopup.PlacementTarget = target;
        ColorPickerPopup.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
        ColorPickerPopup.IsOpen = true;
    }

    private void Appearance_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!ColorPickerPopup.IsOpen) return;
        var child = ColorPickerPopup.Child as UIElement;
        if (child != null && child.IsMouseOver) return;
        ColorPickerPopup.IsOpen = false;
    }

    private void Appearance_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && ColorPickerPopup.IsOpen)
        {
            ColorPickerPopup.IsOpen = false;
            e.Handled = true;
        }
    }

    public void Picker_ColorPicked(string hex)
    {
        ColorPickerPopup.IsOpen = false;
        if (_config is null) return;
        if (_activeColorTag == "crosshair")
        {
            _config.CrosshairColor = hex;
            SetSwatch(PrvCrosshair, hex);
        }
        Save();
    }

    private void Save()
    {
        _manager.Save();
        ConfigChanged?.Invoke();
    }

    private static void SetSwatch(Border swatch, string hex)
    {
        var c = TryParseColor(hex);
        if (c is not null)
            swatch.Background = new SolidColorBrush(c.Value);
    }

    private static Color? TryParseColor(string hex)
    {
        try { return (Color)ColorConverter.ConvertFromString(hex); }
        catch { return null; }
    }
}
