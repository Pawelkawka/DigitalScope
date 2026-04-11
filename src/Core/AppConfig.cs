namespace DigitalScope.Core;

public class AppConfig
{
    public int MagnifierWidth  { get; set; } = AppSettings.DefaultMagnifierWidth;
    public int MagnifierHeight { get; set; } = AppSettings.DefaultMagnifierHeight;

    public double ZoomFactor { get; set; } = AppSettings.DefaultZoomFactor;

    public bool   ShowCrosshair  { get; set; } = AppSettings.DefaultShowCrosshair;
    public string CrosshairColor { get; set; } = AppSettings.DefaultCrosshairColor;

    public string HotkeyToggle { get; set; } = AppSettings.DefaultHotkeyToggle;
}
