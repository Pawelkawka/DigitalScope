namespace DigitalScope.Core;

public static class AppSettings
{
    public const string AppName         = "DigitalScope";
    public const string AppPublisher    = "PawelKawka";
    public const string AppBaseVersion  = "1.1.3";
    public const string AppBuild        = "1804202601";
    public const string AppVersion      = AppBaseVersion + "." + AppBuild;

    public const int DefaultMagnifierWidth  = 300;
    public const int DefaultMagnifierHeight = 300;

    public const double DefaultZoomFactor = 2.0;
    public const double MinZoomFactor     = 1.5;
    public const double MaxZoomFactor     = 8.0;

    public const bool   DefaultShowCrosshair   = false;
    public const string DefaultCrosshairColor  = "#ffffff";

    // crosshair overlay
    public const string DefaultOverlayCrosshairType      = "Cross";
    public const string DefaultOverlayCrosshairColor     = "#FF0000";
    public const int    DefaultOverlayCrosshairSize      = 15;
    public const double DefaultOverlayCrosshairOpacity   = 1.0;
    public const int    DefaultOverlayCrosshairGap       = 4;
    public const int    MinOverlayCrosshairSize          = 2;
    public const int    MaxOverlayCrosshairSize          = 120;

    public const string DefaultHotkeyToggle     = "Ctrl+G";
    public const string DefaultHotkeyCrosshair  = "Ctrl+H";
}
