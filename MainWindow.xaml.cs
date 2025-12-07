using System;
using System.Windows;

namespace EliteSwitch;

public partial class MainWindow : Window
{
    private readonly EliteConfigManager _configManager;
    private readonly ProcessManager _processManager;
    private readonly AudioManager _audioManager;
    private readonly AppSettings _settings;

    public MainWindow()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("MainWindow: Starting initialization...");
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("MainWindow: InitializeComponent completed");

            System.Diagnostics.Debug.WriteLine("MainWindow: Creating EliteConfigManager...");
            _configManager = new EliteConfigManager();

            System.Diagnostics.Debug.WriteLine("MainWindow: Creating ProcessManager...");
            _processManager = new ProcessManager();

            System.Diagnostics.Debug.WriteLine("MainWindow: Creating AudioManager...");
            _audioManager = new AudioManager();

            System.Diagnostics.Debug.WriteLine("MainWindow: Loading settings...");
            _settings = AppSettings.Load();

            System.Diagnostics.Debug.WriteLine("MainWindow: Updating mode display...");
            UpdateModeDisplay();

            System.Diagnostics.Debug.WriteLine("MainWindow: Initialization complete!");
        }
        catch (ArgumentNullException ex)
        {
            System.Diagnostics.Debug.WriteLine($"ArgumentNullException in MainWindow constructor:");
            System.Diagnostics.Debug.WriteLine($"  Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"  ParamName: {ex.ParamName}");
            System.Diagnostics.Debug.WriteLine($"  StackTrace: {ex.StackTrace}");
            MessageBox.Show($"ArgumentNullException during startup:\n\nParameter: {ex.ParamName}\nMessage: {ex.Message}\n\nSee Debug Output for details.",
                "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception in MainWindow constructor:");
            System.Diagnostics.Debug.WriteLine($"  Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"  Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"  StackTrace: {ex.StackTrace}");
            MessageBox.Show($"Error during startup:\n\n{ex.GetType().Name}: {ex.Message}\n\nSee Debug Output for details.",
                "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    private void UpdateModeDisplay()
    {
        ModeMenuItem.Header = $"Current Mode: {_settings.CurrentMode}";
        VRModeMenuItem.IsEnabled = _settings.CurrentMode != GameMode.VR;
        MonitorModeMenuItem.IsEnabled = _settings.CurrentMode != GameMode.Monitor;
    }

    private void SwitchToVR_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _settings.CurrentMode = GameMode.VR;
            _settings.Save();

            _configManager.ApplyMode(GameMode.VR);
            _audioManager.SetDefaultAudioDevice("h5");

            UpdateModeDisplay();

            TrayIcon.ShowNotification("Elite Switch", "Switched to VR Mode");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to switch to VR mode: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SwitchToMonitor_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _settings.CurrentMode = GameMode.Monitor;
            _settings.Save();

            _configManager.ApplyMode(GameMode.Monitor);
            _audioManager.SetDefaultAudioDevice("h5");

            UpdateModeDisplay();

            TrayIcon.ShowNotification("Elite Switch", "Switched to Monitor Mode");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to switch to monitor mode: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void StartTools_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _processManager.StartTools(_settings.CurrentMode);
            TrayIcon.ShowNotification("Elite Switch", $"Starting tools for {_settings.CurrentMode} mode");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to start tools: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void StopTools_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _processManager.StopTools(_settings.CurrentMode);
            TrayIcon.ShowNotification("Elite Switch", "Stopping tools");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to stop tools: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Window_Loaded event fired");
        System.Diagnostics.Debug.WriteLine($"TrayIcon is null: {TrayIcon == null}");

        if (TrayIcon != null)
        {
            System.Diagnostics.Debug.WriteLine($"TrayIcon.IconSource: {TrayIcon.IconSource}");
            System.Diagnostics.Debug.WriteLine($"TrayIcon.ToolTipText: {TrayIcon.ToolTipText}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("ERROR: TrayIcon is NULL after window loaded!");
        }
    }
}
