namespace DigitalScope.Core;

public static class AppSettings
{
    public const string AppName         = "DigitalScope";
    public const string AppPublisher    = "PawelKawka";
    public const string AppBaseVersion  = "1.1.2";
    public const string AppBuild        = "1704202601";
    public const string AppVersion      = AppBaseVersion + "." + AppBuild;

    public const int DefaultMagnifierWidth  = 300;
    public const int DefaultMagnifierHeight = 300;

    public const double DefaultZoomFactor = 2.0;
    public const double MinZoomFactor     = 1.5;
    public const double MaxZoomFactor     = 8.0;

    public const bool   DefaultShowCrosshair   = false;
    public const string DefaultCrosshairColor  = "#ffffff";

    public const string DefaultHotkeyToggle = "Ctrl+G";
}
