# Elite Switch

A Windows system tray application for switching between VR and Monitor modes for Elite Dangerous. This replaces the Python `elite-switch` script with a modern C# WPF application.

## Features

- **Mode Switching**: Toggle between VR and Monitor modes
  - Automatically updates Elite Dangerous config files (`Settings.xml` and `DisplaySettings.xml`)
  - Adjusts graphics settings (resolution, fullscreen, stereoscopic mode, refresh rate, preset)
  - Switches default audio devices to "h5"
  - Persists current mode selection

- **Tool Management**: Start and stop helper programs
  - **Common tools**: TrackIR, EDLaunch, AutoHotkey scripts, VoiceAttack, EDDiscovery
  - **VR mode**: Also starts Oculus runtime and Virtual Desktop Streamer
  - **Monitor mode**: Terminates VR-specific processes
  - **Stop Tools**: Terminates Elite Dangerous, Steam, Dropbox, OneDrive, AutoHotkey, Messenger

- **System Tray Integration**: Runs quietly in the background with context menu access
  - Mode indicator shows current mode
  - Toggle options disabled for current mode
  - Notification balloon tips for actions

## Requirements

- .NET 8.0 Runtime
- Windows 10/11 (64-bit)
- Administrator privileges (required for process management and audio device switching)

## Building

```bash
dotnet restore
dotnet build -c Release
```

## Running

```bash
dotnet run
```

Or run the compiled executable from `bin/Release/net8.0-windows/EliteSwitch.exe`

## Configuration

Settings are automatically saved to:
`%APPDATA%\EliteSwitch\settings.json`

The application modifies Elite Dangerous configuration files at:
`%LOCALAPPDATA%\Frontier Developments\Elite Dangerous\Options\Graphics\`

## Tool Paths

The following paths are expected for the various tools:
- TrackIR: `C:\Program Files (x86)\TrackIR5\TrackIR5.exe`
- EDLaunch: `C:\Program Files (x86)\Frontier\EDLaunch\EDLaunch.exe`
- AutoHotkey Script: `%USERPROFILE%\dot-files\games\AutoHotKey Scripts\EliteDangerous.ahk`
- VoiceAttack: `C:\Program Files (x86)\Steam\steamApps\common\VoiceAttack\VoiceAttack.exe`
- EDDiscovery: `C:\Program Files\EDDiscovery\EDDiscovery.exe`
- Virtual Desktop Streamer: `C:\Program Files\Virtual Desktop Streamer\VirtualDesktop.Streamer.exe`
- Oculus Start Script: `%USERPROFILE%\dot-files\games\Oculus\StartOculus.ps1`

## Setup Instructions

1. **Icon File**: Create or obtain an `icon.ico` file and place it in the project root directory (see `ICON_NOTE.txt` for details)

2. **Restore NuGet Packages**:
   ```bash
   dotnet restore
   ```

3. **Build the Application**:
   ```bash
   dotnet build -c Release
   ```

4. **Run as Administrator**: Right-click the executable and select "Run as administrator"

## Compared to Original Python Script

This application replaces the functionality of the `~/dot-files/scripts/elite-switch` Python script:

| Feature | Python Script | C# WPF App |
|---------|--------------|------------|
| Mode switching | Command-line arguments | System tray context menu |
| Config management | XML parsing with ElementTree | LINQ to XML |
| Process management | psutil library | System.Diagnostics.Process |
| Audio switching | Custom MyAudio module | AudioSwitcher.AudioApi |
| User interface | Zenity/dialog prompts | Native Windows tray icon |
| Startup | Manual execution | Can add to Windows startup |

## Notes

- The application requires administrator privileges to manage processes and change audio devices
- Audio device switching requires the "h5" audio device to be available (configurable in code)
- Settings are automatically saved to `%APPDATA%\EliteSwitch\settings.json`
- The application runs hidden with only a system tray icon visible
- Tool paths can be customized in `ProcessManager.cs`
- Elite Dangerous config files are expected at the standard location: `%LOCALAPPDATA%\Frontier Developments\Elite Dangerous\Options\Graphics\`

## Future Enhancements

Potential improvements for future versions:
- Settings UI for customizing tool paths and audio device names
- Automatic startup with Windows option
- Support for additional game modes
- Logging of mode switches and errors
- Hotkey support for quick mode switching
