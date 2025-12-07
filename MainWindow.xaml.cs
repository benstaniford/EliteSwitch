using System;
using System.Windows;

namespace EliteSwitch;

public partial class MainWindow : Window
{
    private readonly EliteConfigManager _configManager;
    private readonly ProcessManager _processManager;
    private AudioManager? _audioManager;
    private readonly AppSettings _settings;
    private bool _audioManagerFailed = false;

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

            // AudioManager initialization is deferred - will be created on first use
            System.Diagnostics.Debug.WriteLine("MainWindow: AudioManager will be initialized on first use");

            System.Diagnostics.Debug.WriteLine("MainWindow: Loading settings...");
            _settings = AppSettings.Load();

            System.Diagnostics.Debug.WriteLine("MainWindow: Updating mode display...");
            UpdateModeDisplay();

            System.Diagnostics.Debug.WriteLine("MainWindow: Initialization complete!");

            // Additional debugging for the window and tray icon
            this.Loaded += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("=== Lambda Loaded Event ===");
                System.Diagnostics.Debug.WriteLine($"Window.IsLoaded: {this.IsLoaded}");
                System.Diagnostics.Debug.WriteLine($"Window.IsVisible: {this.IsVisible}");
                System.Diagnostics.Debug.WriteLine($"Window.Visibility: {this.Visibility}");
            };
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

    private AudioManager? GetAudioManager()
    {
        if (_audioManagerFailed)
        {
            return null;
        }

        if (_audioManager == null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Initializing AudioManager...");
                _audioManager = new AudioManager();
                System.Diagnostics.Debug.WriteLine("AudioManager initialized successfully");
            }
            catch (Exception ex)
            {
                _audioManagerFailed = true;
                System.Diagnostics.Debug.WriteLine($"Failed to initialize AudioManager: {ex.Message}");
                MessageBox.Show(
                    "Audio device switching is not available on this system.\n" +
                    "This feature requires Windows CoreAudio APIs.\n\n" +
                    "All other features will work normally.",
                    "Audio Manager Unavailable",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        return _audioManager;
    }

    private void SwitchToVR_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _settings.CurrentMode = GameMode.VR;
            _settings.Save();

            _configManager.ApplyMode(GameMode.VR);

            var audioManager = GetAudioManager();
            audioManager?.SetDefaultAudioDevice("h5");

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

            var audioManager = GetAudioManager();
            audioManager?.SetDefaultAudioDevice("h5");

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
        System.Diagnostics.Debug.WriteLine("=== Window_Loaded event fired ===");
        System.Diagnostics.Debug.WriteLine($"TrayIcon is null: {TrayIcon == null}");

        if (TrayIcon != null)
        {
            System.Diagnostics.Debug.WriteLine($"TrayIcon.IconSource: {TrayIcon.IconSource}");
            System.Diagnostics.Debug.WriteLine($"TrayIcon.ToolTipText: {TrayIcon.ToolTipText}");

            // Try to manually show/create the icon
            try
            {
                System.Diagnostics.Debug.WriteLine("Attempting to force icon visibility...");

                // Try setting icon from absolute file path as fallback
                var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
                System.Diagnostics.Debug.WriteLine($"Icon file path: {iconPath}");
                System.Diagnostics.Debug.WriteLine($"Icon file exists: {System.IO.File.Exists(iconPath)}");

                if (!System.IO.File.Exists(iconPath))
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: icon.ico not found in output directory!");
                    MessageBox.Show($"Icon file not found at: {iconPath}\n\nThe tray icon may not display correctly.",
                        "Icon Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    // Try setting the icon from the absolute file path
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("Trying to set icon from absolute path...");
                        var iconUri = new Uri(iconPath, UriKind.Absolute);
                        TrayIcon.IconSource = iconUri;
                        System.Diagnostics.Debug.WriteLine("Icon set from absolute path successfully");
                    }
                    catch (Exception iconEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to set icon from absolute path: {iconEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking icon: {ex.Message}");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("ERROR: TrayIcon is NULL after window loaded!");
            MessageBox.Show("TrayIcon control failed to initialize!\n\nThis may be a compatibility issue with H.NotifyIcon.Wpf.",
                "TrayIcon Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
