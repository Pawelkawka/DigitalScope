using System.Threading;
using System.Windows;
using DigitalScope.Core;
using DigitalScope.TrayIcon;
using DigitalScope.Views;

namespace DigitalScope;

public partial class App : Application
{
    private const string SingleInstanceMutexName = "DigitalScope_SingleInstance_Mutex";
    private Mutex?                         _singleInstanceMutex;
    private readonly AppUpdateService      _updateService = new();
    private readonly CancellationTokenSource _updateCts   = new();
    private TrayIconManager?               _tray;
    private MainWindow?                    _mainWindow;
    private UpdateAvailableWindow?         _updateWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        _singleInstanceMutex = new Mutex(initiallyOwned: true, name: SingleInstanceMutexName, createdNew: out var createdNew);
        if (!createdNew)
        {
            _singleInstanceMutex.Dispose();
            _singleInstanceMutex = null;
            Shutdown();
            return;
        }

        AppLogger.Initialise();
        AccentColorHelper.Apply(Resources);

        AppDomain.CurrentDomain.UnhandledException += (_, ev) =>
            AppLogger.Error($"[UnhandledException] {ev.ExceptionObject}");

        DispatcherUnhandledException += (_, ev) =>
        {
            AppLogger.Error($"[DispatcherUnhandled] {ev.Exception}");
            ev.Handled = true;
            MessageBox.Show(
                $"An unexpected error occurred:\n\n{ev.Exception.Message}\n\nSee log.txt for details.",
                AppSettings.AppName,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        };

        base.OnStartup(e);

        _tray = new TrayIconManager();
        _tray.ShowRequested += OnTrayShow;
        _tray.ExitRequested += OnTrayExit;

        _mainWindow = new MainWindow();
        MainWindow  = _mainWindow;
        _mainWindow.Show();

        _ = CheckForUpdatesAsync();
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var updateInfo = await _updateService.CheckForUpdateAsync(_updateCts.Token);
            if (updateInfo is null) return;

            await Dispatcher.InvokeAsync(() => ShowUpdateWindow(updateInfo));
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            AppLogger.Warn($"Updater check failed: {ex.Message}");
        }
    }

    private void ShowUpdateWindow(AppUpdateInfo updateInfo)
    {
        if (_updateWindow?.IsVisible == true) return;
        _updateWindow = new UpdateAvailableWindow(updateInfo);
        _updateWindow.Closed += (_, _) => _updateWindow = null;
        _updateWindow.Show();
    }

    private void OnTrayShow()
    {
        Dispatcher.Invoke(() =>
        {
            if (_mainWindow is null) return;
            _mainWindow.Show();
            if (_mainWindow.WindowState == WindowState.Minimized)
                _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
        });
    }

    private void OnTrayExit()
    {
        Dispatcher.Invoke(() =>
        {
            _tray?.Dispose();
            _tray = null;
            _mainWindow?.RequestExit();
            Shutdown();
        });
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _updateCts.Cancel();
        _updateCts.Dispose();
        _updateWindow?.Close();
        _tray?.Dispose();
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();
        _singleInstanceMutex = null;
        AppLogger.Info("Application exiting.");
        AppLogger.Close();
        base.OnExit(e);
    }
}
