# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

Elite Switch is a Windows .NET 8.0 WPF system tray application for switching between VR and Monitor modes for Elite Dangerous. It replaces a Python script with a native Windows application that provides:
- Elite Dangerous graphics configuration management (XML file manipulation)
- Process management for starting/stopping gaming tools
- Audio device switching
- WiX-based MSI installer

## Common Development Commands

### Building the Solution

```bash
# Full automated build (application + installer)
./build.sh      # Linux/Mac
build.cmd       # Windows

# Build application only
dotnet restore
dotnet build EliteSwitch.csproj -c Release

# Build MSI installer (requires WiX Toolset v4)
dotnet tool install --global wix  # One-time setup
dotnet build EliteSwitch.Installer/EliteSwitch.Installer.wixproj -c Release
```

**Build outputs:**
- Application: `bin/Release/net8.0-windows/EliteSwitch.exe`
- Installer: `EliteSwitch.Installer/bin/Release/x64/en-US/EliteSwitchSetup.msi`

### Running the Application

```bash
# Development mode
dotnet run

# Must be run with administrator privileges for process and audio management
```

### Important Build Note

The project requires an `icon.ico` file in the root directory. If missing, builds may fail. See `ICON_NOTE.txt` for guidance.

## Architecture Overview

### Solution Structure

Two-project solution:
1. **EliteSwitch** (.NET 8.0 WPF) - Main application
2. **EliteSwitch.Installer** (WiX v4) - MSI installer

### Core Components

**EliteConfigManager.cs**
- Manages Elite Dangerous XML configuration files at `%LOCALAPPDATA%\Frontier Developments\Elite Dangerous\Options\Graphics\`
- Updates both `Settings.xml` and `DisplaySettings.xml`
- Switches between VR settings (StereoscopicMode=4, windowed, VR-specific refresh rate) and Monitor settings (FullScreen=2, 120Hz, Ultra preset)
- Uses LINQ to XML for configuration manipulation

**ProcessManager.cs**
- Handles starting and stopping gaming tools
- **Common tools** (both modes): TrackIR, EDLaunch, AutoHotkey script, VoiceAttack, EDDiscovery
- **VR-specific tools**: Oculus (via PowerShell script at `%USERPROFILE%\dot-files\games\Oculus\StartOculus.ps1`), Virtual Desktop Streamer
- **Stop list**: Elite Dangerous, Steam, Dropbox, OneDrive, AutoHotkey, Messenger (+ VR Streamer in Monitor mode)
- Tool paths are hardcoded using Environment.SpecialFolder for portability

**AudioManager.cs**
- Uses AudioSwitcher.AudioApi to change default Windows audio devices
- Currently configured to switch to device containing "h5" in the name
- Sets both playback and recording devices, including communications defaults

**AppSettings.cs**
- JSON-based settings persistence at `%APPDATA%\EliteSwitch\settings.json`
- Stores current GameMode (VR/Monitor) and AutoStartTools preference
- Settings persist across application restarts

**MainWindow.xaml/cs**
- Hidden WPF window that hosts the system tray icon (H.NotifyIcon.Wpf)
- Context menu with mode switching, tool management, and exit options
- Shows balloon notifications for user actions
- Menu items dynamically enable/disable based on current mode

### Key Design Patterns

**Administrator Privileges**: Application requires admin rights (declared in `app.manifest`) to:
- Kill processes via Process.Kill()
- Change system default audio devices
- Install via MSI (installer requirement)

**Mode-Based Configuration**: All actions are mode-aware (VR vs Monitor):
- Different XML settings applied to Elite Dangerous configs
- Different tool sets started (VR includes additional Oculus/VR Streamer)
- VR Streamer terminated only in Monitor mode

**Dual Config File Updates**: Both `Settings.xml` and `DisplaySettings.xml` are updated identically to ensure Elite Dangerous respects the changes (game checks both files).

## WiX Installer Configuration

The installer (`EliteSwitch.Installer/Package.wxs`) uses WiX Toolset v4:
- Installs to `C:\Program Files\EliteSwitch\`
- Creates Start Menu shortcut
- Bundles all NuGet dependencies (H.NotifyIcon, AudioSwitcher, etc.)
- Requires administrator privileges for installation
- Supports major upgrades (keep `UpgradeCode` GUID constant)
- Optional auto-start with Windows (currently commented out - uncomment `RegistryEntries` component to enable)

**Version Management**: Update version in TWO places:
1. `EliteSwitch.Installer/Package.wxs` - `<Package Version="1.0.0.0" />`
2. Optionally in `EliteSwitch.csproj` - `<AssemblyVersion>` and `<FileVersion>`

## Important Configuration Details

### Elite Dangerous Settings

**VR Mode:**
- ScreenWidth/Height: 3840x2160 (maintains desktop resolution)
- FullScreen: 0 (windowed for VR)
- StereoscopicMode: 4 (VR enabled)
- GammaOffset: 0.240000 (VR-specific gamma correction)
- RefreshRate: 59.810 Hz (59810/1000)
- PresetName: "VRUltra"

**Monitor Mode:**
- ScreenWidth/Height: 3840x2160 (4K resolution)
- FullScreen: 2 (fullscreen borderless)
- StereoscopicMode: 0 (VR disabled)
- RefreshRate: 120 Hz (120/1)
- PresetName: "Ultra"

### Tool Paths

Default paths are user-specific and may need adjustment:
- AutoHotkey script: `%USERPROFILE%\dot-files\games\AutoHotKey Scripts\EliteDangerous.ahk`
- Oculus start script: `%USERPROFILE%\dot-files\games\Oculus\StartOculus.ps1`

Standard installation paths:
- TrackIR: `C:\Program Files (x86)\TrackIR5\TrackIR5.exe`
- EDLaunch: `C:\Program Files (x86)\Frontier\EDLaunch\EDLaunch.exe`
- VoiceAttack: `C:\Program Files (x86)\Steam\steamApps\common\VoiceAttack\VoiceAttack.exe`
- EDDiscovery: `C:\Program Files\EDDiscovery\EDDiscovery.exe`
- VR Streamer: `C:\Program Files\Virtual Desktop Streamer\VirtualDesktop.Streamer.exe`

### Audio Device Configuration

The application switches to audio device containing "h5" in the name. To customize, modify `MainWindow.xaml.cs` lines where `SetDefaultAudioDevice("h5")` is called.

## Dependencies

**NuGet Packages:**
- H.NotifyIcon.Wpf 2.1.4 - System tray icon support
- AudioSwitcher.AudioApi 4.0.0-alpha5 - Audio device management
- AudioSwitcher.AudioApi.CoreAudio 4.0.0-alpha5 - CoreAudio implementation

**Installer:**
- WixToolset.UI.wixext 4.0.5 - WiX UI dialogs

## Runtime Requirements

- Windows 10/11 (64-bit)
- .NET 8.0 Runtime (can be bundled with installer)
- Administrator privileges
- Audio device containing "h5" in name (or code modification)
- Elite Dangerous installed at standard location

## Customization Scenarios

**Adding New Tools:**
1. Add path to `ProcessManager.cs` `StartTools()` method's `startList`
2. If tool should only run in specific mode, add conditional check like VR Streamer
3. If tool should be terminated, add process name (lowercase, without .exe) to `StopTools()` `killList`

**Changing Graphics Presets:**
1. Modify `EliteConfigManager.cs` `GetVRSettings()` or `GetMonitorSettings()` dictionaries
2. Add/remove XML element names and values as needed
3. Element names must match Elite Dangerous XML structure exactly

**Changing Audio Device:**
1. Modify `AudioManager.cs` `SetDefaultAudioDevice()` parameter in `MainWindow.xaml.cs`
2. Change from "h5" to desired device name fragment (case-insensitive partial match)

**Enable Auto-Start with Windows:**
1. Open `EliteSwitch.Installer/Package.wxs`
2. Uncomment lines 52-58 (`RegistryEntries` component)
3. Uncomment line 73 (`ComponentRef` for RegistryEntries)
4. Rebuild installer
