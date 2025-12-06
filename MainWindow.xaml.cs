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
        InitializeComponent();

        _configManager = new EliteConfigManager();
        _processManager = new ProcessManager();
        _audioManager = new AudioManager();
        _settings = AppSettings.Load();

        UpdateModeDisplay();
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
}
