using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Animation;
using DigitalScope.Core;

namespace DigitalScope.Views;

public partial class UpdateAvailableWindow : Window
{
    private readonly AppUpdateInfo _updateInfo;

    public UpdateAvailableWindow(AppUpdateInfo updateInfo)
    {
        _updateInfo = updateInfo;
        InitializeComponent();

        TbVersion.Text      = $"Version {updateInfo.DisplayVersion}";
        TbReleaseNotes.Text = FormatReleaseNotes(updateInfo.ReleaseNotes);

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        PositionWindow();
        BeginAnimation(
            OpacityProperty,
            new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
            });
    }

    private void PositionWindow()
    {
        var area = SystemParameters.WorkArea;
        Left = area.Right - ActualWidth - 18;
        Top  = area.Bottom - ActualHeight - 18;
    }

    private static string FormatReleaseNotes(string releaseNotes)
    {
        if (string.IsNullOrWhiteSpace(releaseNotes))
            return "No release notes were provided for this version.";

        var lines = releaseNotes
            .Replace("\r", string.Empty)
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Take(8)
            .ToArray();

        return lines.Length == 0
            ? "No release notes were provided for this version."
            : string.Join(Environment.NewLine, lines);
    }

    private void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(_updateInfo.ReleaseUrl) { UseShellExecute = true });
            AppLogger.Info($"Updater: opening {_updateInfo.ReleaseUrl}");
            Close();
        }
        catch (Exception ex)
        {
            AppLogger.Warn($"Updater: could not open browser: {ex.Message}");
        }
    }

    private void BtnLater_Click(object sender, RoutedEventArgs e) => Close();
    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
}
