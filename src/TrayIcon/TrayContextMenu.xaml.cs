using System.Windows;
using System.Windows.Forms;

namespace DigitalScope.TrayIcon;

public partial class TrayContextMenu : Window
{
    private bool _closing;

    public event Action? ShowRequested;
    public event Action? CrosshairToggleRequested;
    public event Action? ExitRequested;

    public TrayContextMenu()
    {
        InitializeComponent();
        Deactivated += (_, _) => { if (!_closing) { _closing = true; Close(); } };
        Closing     += (_, _) => _closing = true;
        Loaded      += (_, _) => RepositionNearCursor();
    }

    public void ShowAtCursor()
    {
        Left = -32000;
        Top  = -32000;
        Show();
        Activate();
    }

    private void RepositionNearCursor()
    {
        var rawPos = System.Windows.Forms.Cursor.Position;

        var source = PresentationSource.FromVisual(this);
        double scaleX = source?.CompositionTarget?.TransformFromDevice.M11 ?? 1.0;
        double scaleY = source?.CompositionTarget?.TransformFromDevice.M22 ?? 1.0;

        double dipX   = rawPos.X * scaleX;
        double dipY   = rawPos.Y * scaleY;
        double width  = ActualWidth;
        double height = ActualHeight;

        double left = dipX;
        double top  = dipY - height;

        double vLeft  = SystemParameters.VirtualScreenLeft;
        double vTop   = SystemParameters.VirtualScreenTop;
        double vRight = vLeft + SystemParameters.VirtualScreenWidth;
        double vBot   = vTop  + SystemParameters.VirtualScreenHeight;

        if (left + width  > vRight) left = vRight - width;
        if (left          < vLeft)  left = vLeft;
        if (top           < vTop)   top  = dipY;          // fall below cursor
        if (top  + height > vBot)   top  = vBot - height;

        Left = left;
        Top  = top;
    }

    private void ShowButton_Click(object sender, RoutedEventArgs e)
    {
        if (_closing) return;
        _closing = true;
        Close();
        ShowRequested?.Invoke();
    }

    private void ToggleCrosshairButton_Click(object sender, RoutedEventArgs e)
    {
        if (_closing) return;
        _closing = true;
        Close();
        CrosshairToggleRequested?.Invoke();
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        if (_closing) return;
        _closing = true;
        Close();
        ExitRequested?.Invoke();
    }
}
