using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

            // Check TrayIcon immediately after XAML loads
            System.Diagnostics.Debug.WriteLine($"MainWindow: TrayIcon null after InitializeComponent? {TrayIcon == null}");
            if (TrayIcon != null)
            {
                System.Diagnostics.Debug.WriteLine($"MainWindow: TrayIcon.IconSource = {TrayIcon.IconSource}");
                System.Diagnostics.Debug.WriteLine($"MainWindow: TrayIcon.ToolTipText = {TrayIcon.ToolTipText}");
                System.Diagnostics.Debug.WriteLine($"MainWindow: TrayIcon.ContextMenu = {TrayIcon.ContextMenu}");

                // Try to ensure the icon is visible
                try
                {
                    System.Diagnostics.Debug.WriteLine("MainWindow: Attempting to ensure TrayIcon visibility...");
                    // Force the icon to show by triggering a property change
                    var currentTooltip = TrayIcon.ToolTipText;
                    TrayIcon.ToolTipText = currentTooltip + " "; // Trigger update
                    TrayIcon.ToolTipText = currentTooltip; // Restore
                    System.Diagnostics.Debug.WriteLine("MainWindow: TrayIcon visibility check complete");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"MainWindow: Error forcing TrayIcon visibility: {ex.Message}");
                }
            }

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
                
                // Build audio menus after window is fully loaded to avoid COM threading issues
                System.Diagnostics.Debug.WriteLine("MainWindow: Building audio device menus...");
                BuildAudioDeviceMenus();
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
            catch (ArgumentNullException ex)
            {
                _audioManagerFailed = true;
                System.Diagnostics.Debug.WriteLine($"AudioManager initialization failed with ArgumentNullException: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"This may indicate an issue with a specific audio device on the system.");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Don't show message box here - will show when building menus
            }
            catch (Exception ex)
            {
                _audioManagerFailed = true;
                System.Diagnostics.Debug.WriteLine($"Failed to initialize AudioManager: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Exception type: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        return _audioManager;
    }

    private void BuildAudioDeviceMenus()
    {
        // Must be called on UI thread after window is loaded to avoid COM threading issues
        try
        {
            var config = GraphicsConfig.Load();
            
            // Try to initialize audio manager
            var audioManager = GetAudioManager();
            
            if (audioManager == null)
            {
                System.Diagnostics.Debug.WriteLine("AudioManager unavailable - disabling audio device menus");
                AudioOutMenuItem.IsEnabled = false;
                MicrophoneMenuItem.IsEnabled = false;
                
                // Show informational message
                if (_audioManagerFailed)
                {
                    TrayIcon.ShowNotification(
                        "Elite Switch - Audio Unavailable",
                        "Audio device menus disabled due to system compatibility issue.");
                }
                return;
            }
            
            // Build Audio Out submenu
            AudioOutMenuItem.Items.Clear();
            
            try
            {
                var availablePlaybackDevices = audioManager.GetAvailablePlaybackDevices();
                var currentDefaultDevice = audioManager.GetDefaultPlaybackDevice();
                
                foreach (var device in config.Audio.AudioOut.Devices)
                {
                    // Check if this device exists on the system
                    var matchingDevice = availablePlaybackDevices.FirstOrDefault(d => 
                        d.Contains(device.Substring, StringComparison.OrdinalIgnoreCase));
                    
                    if (matchingDevice != null)
                    {
                        // Check if THIS specific device is the current default
                        // by comparing the actual matching device name with the current default
                        bool isDefault = !string.IsNullOrEmpty(currentDefaultDevice) &&
                                       matchingDevice.Equals(currentDefaultDevice, StringComparison.OrdinalIgnoreCase);
                        
                        var menuItem = new MenuItem
                        {
                            Header = isDefault ? $"● {device.Name}" : device.Name,
                            Tag = device.Substring
                        };
                        menuItem.Click += AudioOut_Click;
                        AudioOutMenuItem.Items.Add(menuItem);
                    }
                }
                
                if (AudioOutMenuItem.Items.Count == 0)
                {
                    AudioOutMenuItem.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to enumerate playback devices: {ex.Message}");
                AudioOutMenuItem.IsEnabled = false;
            }

            // Build Microphone submenu
            MicrophoneMenuItem.Items.Clear();
            
            try
            {
                var availableCaptureDevices = audioManager.GetAvailableCaptureDevices();
                var currentDefaultDevice = audioManager.GetDefaultCaptureDevice();
                
                foreach (var device in config.Audio.Microphone.Devices)
                {
                    // Check if this device exists on the system
                    var matchingDevice = availableCaptureDevices.FirstOrDefault(d => 
                        d.Contains(device.Substring, StringComparison.OrdinalIgnoreCase));
                    
                    if (matchingDevice != null)
                    {
                        // Check if THIS specific device is the current default
                        // by comparing the actual matching device name with the current default
                        bool isDefault = !string.IsNullOrEmpty(currentDefaultDevice) &&
                                       matchingDevice.Equals(currentDefaultDevice, StringComparison.OrdinalIgnoreCase);
                        
                        var menuItem = new MenuItem
                        {
                            Header = isDefault ? $"● {device.Name}" : device.Name,
                            Tag = device.Substring
                        };
                        menuItem.Click += Microphone_Click;
                        MicrophoneMenuItem.Items.Add(menuItem);
                    }
                }
                
                if (MicrophoneMenuItem.Items.Count == 0)
                {
                    MicrophoneMenuItem.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to enumerate capture devices: {ex.Message}");
                MicrophoneMenuItem.IsEnabled = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to build audio device menus: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Exception details: {ex}");
            AudioOutMenuItem.IsEnabled = false;
            MicrophoneMenuItem.IsEnabled = false;
        }
    }

    private void AudioOut_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.Tag is string substring)
        {
            try
            {
                var audioManager = GetAudioManager();
                audioManager?.SetDefaultPlaybackDevice(substring);
                
                // Refresh the menu to update indicators
                BuildAudioDeviceMenus();
                
                TrayIcon.ShowNotification("Elite Switch", $"Switched audio output to {menuItem.Header}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to switch audio device: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void Microphone_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.Tag is string substring)
        {
            try
            {
                var audioManager = GetAudioManager();
                audioManager?.SetDefaultCaptureDevice(substring);
                
                // Refresh the menu to update indicators
                BuildAudioDeviceMenus();
                
                TrayIcon.ShowNotification("Elite Switch", $"Switched microphone to {menuItem.Header}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to switch microphone: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void AudioOutMenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
    {
        // Refresh the menu to show current indicators when opened
        BuildAudioDeviceMenus();
    }

    private void MicrophoneMenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
    {
        // Refresh the menu to show current indicators when opened
        BuildAudioDeviceMenus();
    }

    private void SwitchAudioForMode(GameMode mode)
    {
        var audioManager = GetAudioManager();
        if (audioManager == null)
        {
            return;
        }

        try
        {
            var config = GraphicsConfig.Load();
            
            // Switch audio output device
            var audioOutDefault = mode == GameMode.VR ? config.Audio.AudioOut.DefaultVR : config.Audio.AudioOut.DefaultMonitor;
            if (!string.IsNullOrEmpty(audioOutDefault))
            {
                audioManager.SetDefaultPlaybackDevice(audioOutDefault);
                System.Diagnostics.Debug.WriteLine($"Switched audio output to default for {mode} mode: {audioOutDefault}");
            }
            
            // Switch microphone device
            var micDefault = mode == GameMode.VR ? config.Audio.Microphone.DefaultVR : config.Audio.Microphone.DefaultMonitor;
            if (!string.IsNullOrEmpty(micDefault))
            {
                audioManager.SetDefaultCaptureDevice(micDefault);
                System.Diagnostics.Debug.WriteLine($"Switched microphone to default for {mode} mode: {micDefault}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to switch audio for mode {mode}: {ex.Message}");
        }
    }

    private void SwitchToVR_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var previousMode = _settings.CurrentMode;
            
            _settings.CurrentMode = GameMode.VR;
            _settings.Save();

            // Reload config in case user edited it
            _configManager.ReloadConfig();
            _configManager.ApplyMode(GameMode.VR);

            // Switch audio devices to VR defaults
            SwitchAudioForMode(GameMode.VR);

            // Handle mode-specific tool switching
            _processManager.ReloadConfig();
            if (previousMode != GameMode.VR)
            {
                // Stop the previous mode's tools and start VR tools
                _processManager.StopModeSpecificTools(previousMode);
                _processManager.StartModeSpecificTools(GameMode.VR);
            }

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
            var previousMode = _settings.CurrentMode;
            
            _settings.CurrentMode = GameMode.Monitor;
            _settings.Save();

            // Reload config in case user edited it
            _configManager.ReloadConfig();
            _configManager.ApplyMode(GameMode.Monitor);

            // Switch audio devices to Monitor defaults
            SwitchAudioForMode(GameMode.Monitor);

            // Handle mode-specific tool switching
            _processManager.ReloadConfig();
            if (previousMode != GameMode.Monitor)
            {
                // Stop the previous mode's tools and start Monitor tools
                _processManager.StopModeSpecificTools(previousMode);
                _processManager.StartModeSpecificTools(GameMode.Monitor);
            }

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
            // Reload config in case user edited it
            _processManager.ReloadConfig();
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
            // Reload config in case user edited it
            _processManager.ReloadConfig();
            _processManager.StopTools(_settings.CurrentMode);
            TrayIcon.ShowNotification("Elite Switch", "Stopping tools");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to stop tools: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void EditConfig_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string configPath = GraphicsConfig.GetConfigFilePath();

            // Ensure the config file exists (create with defaults if it doesn't)
            if (!System.IO.File.Exists(configPath))
            {
                var defaultConfig = GraphicsConfig.GetDefaultConfig();
                defaultConfig.Save();
                TrayIcon.ShowNotification("Elite Switch", "Created default configuration file");
            }

            // Open the file with the default JSON editor
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = configPath,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);

            // Show a notification explaining that the app will reload the config
            TrayIcon.ShowNotification("Elite Switch",
                "Config opened for editing.\n\nChanges will be applied when you next switch modes or start/stop tools.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to open config file: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void Window_ContentRendered(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("=== Window_ContentRendered event fired ===");
        CheckTrayIcon();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("=== Window_Loaded event fired ===");
        CheckTrayIcon();
    }

    private void CheckTrayIcon()
    {
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
                var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icons", "favicon.ico");
                System.Diagnostics.Debug.WriteLine($"Icon file path: {iconPath}");
                System.Diagnostics.Debug.WriteLine($"Icon file exists: {System.IO.File.Exists(iconPath)}");

                if (!System.IO.File.Exists(iconPath))
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: favicon.ico not found in output directory!");
                    MessageBox.Show($"Icon file not found at: {iconPath}\n\nThe tray icon may not display correctly.",
                        "Icon Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    // Try setting the icon from the absolute file path
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("Trying to set icon from absolute path...");
                        var bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = new Uri(iconPath, UriKind.Absolute);
                        bitmapImage.EndInit();
                        TrayIcon.IconSource = bitmapImage;
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
