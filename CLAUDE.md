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

# Build MSI installer (requires WiX Toolset v3.11)
msbuild EliteSwitch.Installer/EliteSwitch.Installer.wixproj /p:Configuration=Release /p:Platform=x64
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
2. **EliteSwitch.Installer** (WiX v3.11) - MSI installer

### Core Components

**EliteConfigManager.cs**
- Manages Elite Dangerous XML configuration files at `%LOCALAPPDATA%\Frontier Developments\Elite Dangerous\Options\Graphics\`
- Updates both `Settings.xml` and `DisplaySettings.xml`
- Switches between VR settings (StereoscopicMode=4, windowed, VR-specific refresh rate) and Monitor settings (FullScreen=2, 120Hz, Ultra preset)
- Uses LINQ to XML for configuration manipulation
- Loads graphics settings from `%USERPROFILE%\dot-files\.eliteswitch.json` if present, otherwise uses hardcoded defaults
- Automatically creates JSON config file with default settings when first applying a mode

**GraphicsConfig.cs**
- JSON-based configuration for graphics settings, audio devices, and process management
- Stores separate settings for VR and Monitor modes
- Located at `%USERPROFILE%\dot-files\.eliteswitch.json`
- Provides default settings matching the original hardcoded values
- Supports customization via "Edit Config..." menu option
- Includes graphics configuration (nested under "graphics" node):
  - VR mode settings
  - Monitor mode settings
- Includes audio device configuration (nested under "audio" node):
  - Audio output devices (list with name and substring for matching)
  - Microphone devices (list with name and substring for matching)
- Includes tool configuration with three main sections (common, vr, monitor):
  - Each section has "onStart" (tools to launch) and "onStop" (processes to terminate)
  - Common tools run in both modes
  - VR tools run only in VR mode
  - Monitor tools run only in Monitor mode

**ProcessManager.cs**
- Handles starting and stopping gaming tools based on configuration
- Loads tool lists from `%USERPROFILE%\dot-files\.eliteswitch.json`
- Uses new hierarchical structure with common, vr, and monitor sections
- **Default common tools** (onStart - both modes): EDLaunch, AutoHotkey script, VoiceAttack, EDDiscovery
- **Default common processes to stop** (onStop - both modes): Elite Dangerous, EDLaunch, AutoHotkey U64, Messenger
- **Default VR tools** (onStart): Virtual Desktop Streamer
- **Default VR processes to stop** (onStop): Virtual Desktop Streamer
- **Default Monitor tools** (onStart): TrackIR5 (head tracking for monitor gaming)
- **Default Monitor processes to stop** (onStop): TrackIR5
- Tool paths use Environment.SpecialFolder for portability in defaults
- Reloads configuration before each start/stop operation to pick up user edits
- Provides mode-specific methods: `StartModeSpecificTools()` and `StopModeSpecificTools()` for switching between modes without affecting common tools

**AudioManager.cs**
- Uses AudioSwitcher.AudioApi to change default Windows audio devices
- Provides methods to enumerate available playback and capture devices
- Provides methods to get current default playback and capture devices
- Matches devices by substring (case-insensitive) from configuration
- Sets both playback and recording devices, including communications defaults
- Supports independent control of audio output and microphone devices

**AppSettings.cs**
- JSON-based settings persistence at `%APPDATA%\EliteSwitch\settings.json`
- Stores current GameMode (VR/Monitor) and AutoStartTools preference
- Settings persist across application restarts

**MainWindow.xaml/cs**
- Hidden WPF window that hosts the system tray icon (H.NotifyIcon.Wpf)
- Context menu with mode switching, audio device selection, tool management, config editing, and exit options
- Shows balloon notifications for user actions
- Menu items dynamically enable/disable based on current mode
- **Automatic mode-specific tool management**: When switching modes, automatically stops previous mode's tools and starts new mode's tools (leaves common tools running)
- Audio device submenus dynamically populated from config:
  - "Audio Out" submenu for selecting playback devices
  - "Microphone" submenu for selecting capture devices
  - Only devices found on the system (by substring match) appear in menus
  - Current default device shown with bullet indicator (●)
  - Indicators refresh when submenu is opened or device is changed
- "Edit Config..." opens the JSON configuration file in the default system editor
- Automatically reloads configuration before applying mode changes or starting/stopping tools

### Key Design Patterns

**Mode-Based Configuration**: All actions are mode-aware (VR vs Monitor):
- Different XML settings applied to Elite Dangerous configs
- Different tool sets started (VR includes Virtual Desktop Streamer)
- Different audio devices automatically selected
- **Automatic tool switching**: When switching modes, the previous mode's tools are stopped and the new mode's tools are started (common tools remain running)
- Mode-specific processes terminated based on mode (e.g., VR Streamer terminated in VR mode, TrackIR terminated in Monitor mode)

**Dual Config File Updates**: Both `Settings.xml` and `DisplaySettings.xml` are updated identically to ensure Elite Dangerous respects the changes (game checks both files).

## WiX Installer Configuration

The installer (`EliteSwitch.Installer/Product.wxs`) uses WiX Toolset v3.11:
- Installs to `C:\Program Files\EliteSwitch\`
- Creates Start Menu shortcut
- Bundles all NuGet dependencies (H.NotifyIcon, AudioSwitcher, etc.)
- Requires administrator privileges for installation
- Supports major upgrades (keep `UpgradeCode` GUID constant)
- Optional auto-start with Windows (currently commented out - uncomment `RegistryEntries` component to enable)

**Version Management**:
- Version is automatically updated by the GitHub Actions release workflow
- Manual updates: Edit `<?define ProductVersion="1.0.0.0" ?>` in `EliteSwitch.Installer/Product.wxs`

## Important Configuration Details

### Audio Device Configuration

The application now allows selecting audio devices through the tray menu. Audio devices are configured in the JSON file under the "audio" node.

**To customize audio devices:**
1. Right-click the tray icon
2. Select "Edit Config..."
3. Modify the `audio.audioOut.devices` array for playback devices
4. Modify the `audio.microphone.devices` array for capture devices
5. Each device requires:
   - `name`: The display name that appears in the menu
   - `substring`: A unique substring to match the actual device name (case-insensitive)
6. Set `default-vr` and `default-monitor` to the substring of the device to auto-switch when changing modes
7. Save the file
8. Restart the application to rebuild the audio device menus

**Default Audio Devices:**
- Audio Out: "H5 Headphones" (matches "h5 - game"), "Desktop Speakers" (matches "realtek usb audio"), "Oculus Quest 2" (matches "oculus virtual audio")
- Microphone: "Microphone (H5)" (matches "h5 - chat"), "Virtual Desktop Microphone" (matches "virtual desktop audio")
- VR Mode Defaults: Both audio out and microphone default to "h5"
- Monitor Mode Defaults: Both audio out and microphone default to "h5"

**Finding Device Substrings:**
To find the substring for your audio devices, you can check the device name in Windows Sound settings. The substring should be unique enough to match only the desired device (e.g., "h5", "speakers", "headphones", "microphone").

### Graphics Settings Configuration

Graphics settings are stored in a JSON file at `%USERPROFILE%\dot-files\.eliteswitch.json`. If this file doesn't exist, the application uses hardcoded defaults and creates the file when you first switch modes.

**To customize settings:**
1. Right-click the tray icon
2. Select "Edit Config..."
3. Modify the JSON file with your preferred graphics settings, audio devices, and/or tool lists
4. Save the file
5. Graphics and tools: Changes apply on next mode switch or tool start/stop
6. Audio devices: Restart the application to rebuild the audio device menus

**Default VR Mode Settings:**
- ScreenWidth/Height: 3840x2160 (maintains desktop resolution)
- FullScreen: 0 (windowed for VR)
- StereoscopicMode: 4 (VR enabled)
- GammaOffset: 0.240000 (VR-specific gamma correction)
- RefreshRate: 59.810 Hz (59810/1000)
- PresetName: "VRUltra"

**Default Monitor Mode Settings:**
- ScreenWidth/Height: 3840x2160 (4K resolution)
- FullScreen: 2 (fullscreen borderless)
- StereoscopicMode: 0 (VR disabled)
- RefreshRate: 120 Hz (120/1)
- PresetName: "Ultra"

**JSON Configuration Example:**
```json
{
  "graphics": {
    "vr": {
      "ScreenWidth": "3840",
      "ScreenHeight": "2160",
      "FullScreen": "0",
      "StereoscopicMode": "4",
      "GammaOffset": "0.240000",
      "DX11_RefreshRateNumerator": "59810",
      "DX11_RefreshRateDenominator": "1000",
      "PresetName": "VRUltra"
    },
    "monitor": {
      "ScreenWidth": "3840",
      "ScreenHeight": "2160",
      "FullScreen": "2",
      "StereoscopicMode": "0",
      "DX11_RefreshRateNumerator": "120",
      "DX11_RefreshRateDenominator": "1",
      "PresetName": "Ultra"
    }
  },
  "audio": {
    "audioOut": {
      "devices": [
        { "name": "H5 Headphones", "substring": "h5 - game" },
        { "name": "Desktop Speakers", "substring": "realtek usb audio" },
        { "name": "Oculus Quest 2", "substring": "oculus virtual audio" }
      ],
      "default-vr": "h5",
      "default-monitor": "h5"
    },
    "microphone": {
      "devices": [
        { "name": "Microphone (H5)", "substring": "h5 - chat" },
        { "name": "Virtual Desktop Microphone", "substring": "virtual desktop audio" }
      ],
      "default-vr": "h5",
      "default-monitor": "h5"
    }
  },
  "tools": {
    "common": {
      "onStart": [
        "C:\\Program Files (x86)\\Frontier\\EDLaunch\\EDLaunch.exe",
        "C:\\Users\\YourName\\dot-files\\games\\AutoHotKey Scripts\\EliteDangerous.ahk",
        "C:\\Program Files (x86)\\Steam\\steamApps\\common\\VoiceAttack\\VoiceAttack.exe",
        "C:\\Program Files\\EDDiscovery\\EDDiscovery.exe"
      ],
      "onStop": [
        "elitedangerous64",
        "edlaunch",
        "autohotkeyu64",
        "messenger"
      ]
    },
    "vr": {
      "onStart": [
        "C:\\Program Files\\Virtual Desktop Streamer\\VirtualDesktop.Streamer.exe"
      ],
      "onStop": [
        "virtualdesktop.streamer"
      ]
    },
    "monitor": {
      "onStart": [
        "C:\\Program Files (x86)\\TrackIR5\\TrackIR5.exe"
      ],
      "onStop": [
        "trackir5"
      ]
    }
  }
}
```

**Audio Configuration Notes:**
- `audio.audioOut`: Audio output device configuration
  - `devices`: List of audio output devices that will appear in the "Audio Out" submenu
  - `default-vr`: Substring of the device to auto-switch to when entering VR mode
  - `default-monitor`: Substring of the device to auto-switch to when entering Monitor mode
- `audio.microphone`: Microphone device configuration
  - `devices`: List of microphone devices that will appear in the "Microphone" submenu
  - `default-vr`: Substring of the device to auto-switch to when entering VR mode
  - `default-monitor`: Substring of the device to auto-switch to when entering Monitor mode
- Each audio device in the `devices` arrays has:
  - `name`: Display name shown in the menu
  - `substring`: Unique substring used to match the actual device name (case-insensitive)
- Only devices that exist on the system (matched by substring) will appear in the menus
- The substring should be unique enough to identify the device but doesn't need to be the full name
- When switching modes, the application automatically switches to the configured default devices

**Tool Configuration Notes:**
- `tools.common`: Configuration for tools used in both VR and Monitor modes
  - `onStart`: Full paths to executables to start in both modes
  - `onStop`: Process names (lowercase, without .exe) to terminate in both modes
- `tools.vr`: Configuration for VR-specific tools
  - `onStart`: Full paths to executables to start only in VR mode
  - `onStop`: Process names to terminate only when in VR mode
- `tools.monitor`: Configuration for Monitor-specific tools (e.g., TrackIR for head tracking)
  - `onStart`: Full paths to executables to start only in Monitor mode
  - `onStop`: Process names to terminate only when in Monitor mode

### Tool Paths

Default paths are user-specific and may need adjustment:
- AutoHotkey script: `%USERPROFILE%\dot-files\games\AutoHotKey Scripts\EliteDangerous.ahk`

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
- WiX Toolset v3.11 - MSI installer framework
- WixUIExtension - WiX UI dialogs

## Runtime Requirements

- Windows 10/11 (64-bit)
- .NET 8.0 Runtime (can be bundled with installer)
- Administrator privileges
- Audio device containing "h5" in name (or code modification)
- Elite Dangerous installed at standard location

## Customization Scenarios

**Adding Audio Devices:**
1. Use the "Edit Config..." menu option in the tray icon
2. Edit the `%USERPROFILE%\dot-files\.eliteswitch.json` file
3. Add devices to the appropriate array:
   - `audio.audioOut.devices` - for playback/speaker devices
   - `audio.microphone.devices` - for capture/microphone devices
4. Each device needs:
   ```json
   { "name": "Display Name", "substring": "unique_substring" }
   ```
5. Configure mode-specific defaults:
   - `audio.audioOut.default-vr` - substring of device for VR mode
   - `audio.audioOut.default-monitor` - substring of device for Monitor mode
   - `audio.microphone.default-vr` - substring of device for VR mode
   - `audio.microphone.default-monitor` - substring of device for Monitor mode
6. Save the file
7. Restart the application for the new devices to appear in the menu

**Adding New Tools:**
1. Use the "Edit Config..." menu option in the tray icon
2. Edit the `%USERPROFILE%\dot-files\.eliteswitch.json` file
3. Add tool paths or process names to the appropriate section:
   - `tools.common.onStart` - executables to start in both modes
   - `tools.common.onStop` - processes to terminate in both modes
   - `tools.vr.onStart` - executables to start only in VR mode
   - `tools.vr.onStop` - processes to terminate only in VR mode
   - `tools.monitor.onStart` - executables to start only in Monitor mode (e.g., head tracking)
   - `tools.monitor.onStop` - processes to terminate only in Monitor mode
4. Note: `onStart` uses full paths to executables, `onStop` uses process names (lowercase, without .exe)
5. Save the file
6. Changes apply on next tool start/stop operation

Alternatively, you can modify the default settings in `GraphicsConfig.cs` `GetDefaultConfig()` method, but editing the JSON file is recommended for user customization.

**Changing Graphics Presets:**
1. Use the "Edit Config..." menu option in the tray icon
2. Edit the `%USERPROFILE%\dot-files\.eliteswitch.json` file
3. Modify the VR or Monitor settings as needed
4. Element names must match Elite Dangerous XML structure exactly
5. Changes are applied on the next mode switch

Alternatively, you can modify the default settings in `GraphicsConfig.cs` `GetDefaultConfig()` method, but editing the JSON file is recommended for user customization.

**Changing Audio Devices:**
Audio devices are now configured through the JSON file rather than hardcoded. See "Adding Audio Devices" above.

**Enable Auto-Start with Windows:**
1. Open `EliteSwitch.Installer/Product.wxs`
2. Uncomment the `RegistryEntries` component in the `DirectoryRef` section
3. Uncomment the corresponding `ComponentRef` in the Features section
4. Rebuild installer

## Creating Releases

The project uses GitHub Actions for automated releases. The workflow builds the installer and creates GitHub releases with MSI artifacts.

### Release Process

1. **Update CHANGELOG.md**:
   - Move changes from `[Unreleased]` section to a new version section
   - Keep the `[Unreleased]` section header for future changes
   - Example:
     ```markdown
     ## [Unreleased]

     ### Added
     - New feature for next release

     ## [1.0.0] - 2024-12-06

     ### Added
     - Initial release features
     ```

2. **Create and Push a Version Tag**:
   ```bash
   # Create an annotated tag
   git tag -a v1.0.0 -m "Release version 1.0.0"

   # Push the tag to trigger the release workflow
   git push origin v1.0.0
   ```

3. **Monitor the Workflow**:
   - Go to the repository's "Actions" tab on GitHub
   - Watch the "Build and Release Elite Switch" workflow
   - The workflow will:
     - Build the .NET application
     - Build the WiX installer
     - Update version in Product.wxs automatically
     - Extract release notes from CHANGELOG.md
     - Create a GitHub release
     - Upload the MSI installer

### Manual Release Trigger

You can also trigger a release manually from GitHub:

1. Go to **Actions** → **Build and Release Elite Switch**
2. Click **Run workflow**
3. Enter the version (e.g., `v1.0.0`)
4. Click **Run workflow**

### Version Numbering

- Use semantic versioning: `vMAJOR.MINOR.PATCH` (e.g., `v1.0.0`)
- The workflow automatically strips the 'v' prefix for MSI versioning
- MSI filename format: `EliteSwitchInstaller-v1.0.0-x64.msi`

### Release Artifacts

Each release includes:
- **MSI Installer**: `EliteSwitchInstaller-vX.Y.Z-x64.msi`
- **Release Notes**: Auto-generated from CHANGELOG.md with installation instructions
- **System Requirements**: .NET 8.0 Runtime, Windows 10+, Administrator privileges

### CHANGELOG.md Format

The workflow expects this format:

```markdown
## [Unreleased]

### Added
- New feature

### Fixed
- Bug fix

## [1.0.0] - 2024-12-06

### Added
- Initial feature
```

The `[Unreleased]` section is extracted and used for release notes when a new version is tagged.
