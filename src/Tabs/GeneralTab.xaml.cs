using System.Windows.Controls;
using DigitalScope.Core;

namespace DigitalScope.Tabs;

public partial class GeneralTab : UserControl
{
    private AppConfig?     _config;
    private ConfigManager? _manager;
    private bool           _loading;

    public event Action? ConfigChanged;

    public GeneralTab() => InitializeComponent();

    public void Initialise(AppConfig config, ConfigManager manager)
    {
        _config  = config;
        _manager = manager;
        LoadValues();
    }

    public void Refresh()
    {
        if (_config is not null) LoadValues();
    }

    private void LoadValues()
    {
        _loading = true;

        SetSlider(SlWidth,  LblWidth,  _config!.MagnifierWidth);
        SetSlider(SlHeight, LblHeight, _config!.MagnifierHeight);

        SlZoom.Value  = (int)(_config!.ZoomFactor * 10);
        LblZoom.Text  = $"{_config!.ZoomFactor:F1}x";

        _loading = false;
    }

    private static void SetSlider(System.Windows.Controls.Slider sl,
                                  TextBlock lbl, double value)
    {
        sl.Value = Math.Clamp(value, sl.Minimum, sl.Maximum);
        lbl.Text = ((int)sl.Value).ToString();
    }

    private void SlWidth_ValueChanged(object s, System.Windows.RoutedPropertyChangedEventArgs<double> e)
    {
        if (LblWidth != null) LblWidth.Text = ((int)e.NewValue).ToString();
        if (_loading || _config is null) return;
        _config.MagnifierWidth = (int)SlWidth.Value;
        Save();
    }

    private void SlHeight_ValueChanged(object s, System.Windows.RoutedPropertyChangedEventArgs<double> e)
    {
        if (LblHeight != null) LblHeight.Text = ((int)e.NewValue).ToString();
        if (_loading || _config is null) return;
        _config.MagnifierHeight = (int)SlHeight.Value;
        Save();
    }

    private void SlZoom_ValueChanged(object s, System.Windows.RoutedPropertyChangedEventArgs<double> e)
    {
        double zoom = e.NewValue / 10.0;
        if (LblZoom != null) LblZoom.Text = $"{zoom:F1}x";
        if (_loading || _config is null) return;
        _config.ZoomFactor = zoom;
        Save();
    }

    private void Save()
    {
        _manager?.Save();
        ConfigChanged?.Invoke();
    }
}
