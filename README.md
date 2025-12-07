# Elite Switch

Elite Switch is a Windows system tray application that makes it easy to switch between VR and Monitor modes for Elite Dangerous. With a simple click, it automatically adjusts your game settings, audio devices, and launches the right tools for your gaming session.

## Features

- **One-Click Mode Switching**: Instantly switch between VR and Monitor modes
  - Automatically updates Elite Dangerous graphics settings
  - Switches to the appropriate audio devices
  - Starts mode-specific tools (VR headset software or head tracking)

- **Audio Device Management**:
  - Quick access to switch between audio outputs and microphones
  - Automatically switches to the right devices when changing modes

- **Tool Management**:
  - Launches Elite Dangerous and supporting tools with one click
  - Automatically manages VR-specific tools (Virtual Desktop) or Monitor-specific tools (TrackIR)
  - Smart process detection - won't start tools that are already running

- **System Tray Integration**:
  - Runs quietly in the background
  - Right-click or left-click the tray icon to access the menu
  - Shows your current mode at a glance
  - Notification popups confirm your actions

## Installation

1. Download the latest `EliteSwitchInstaller-vX.X.X-x64.msi` from the [Releases](https://github.com/yourusername/EliteSwitch/releases) page
2. Double-click the installer and follow the installation wizard
3. The application will be installed to `C:\Program Files\EliteSwitch\`
4. Launch "Elite Switch" from the Start Menu

**Note**: Elite Switch requires administrator privileges to manage processes and audio devices. You may see a User Account Control (UAC) prompt when launching.

## System Requirements

- Windows 10 or Windows 11 (64-bit)
- .NET 8.0 Runtime (the installer will prompt you to download it if needed)
- Elite Dangerous installed

## How to Use

### First Launch

When you first launch Elite Switch, you'll see a new icon in your system tray (notification area). The app has no visible window - everything is accessed through this tray icon.

### Switching Modes

1. **Right-click or left-click** the Elite Switch tray icon
2. Choose either:
   - **Switch to VR Mode** - Configures Elite Dangerous for VR headset play
   - **Switch to Monitor Mode** - Configures Elite Dangerous for monitor play

The app will:
- Update your Elite Dangerous graphics settings
- Switch to the appropriate audio devices
- Stop the previous mode's tools and start the new mode's tools

### Starting Elite Dangerous

1. Right-click the tray icon
2. Select **Start Elite Dangerous**
   - This launches Elite Dangerous along with companion tools (EDLaunch, VoiceAttack, EDDiscovery, etc.)
   - VR mode also starts Virtual Desktop Streamer
   - Monitor mode also starts TrackIR (if installed)
   - Tools already running won't be started again

### Stopping Tools

1. Right-click the tray icon
2. Select **Stop**
   - This closes Elite Dangerous and related tools

### Changing Audio Devices

The tray menu includes two submenus for audio:

- **Audio Out**: Select your speakers or headphones
- **Microphone**: Select your microphone

Available devices are automatically detected from your configuration. The currently selected device is marked with a bullet (●).

### Exiting the Application

1. Right-click the tray icon
2. Select **Exit**

## Configuration

### Customizing Settings

You can customize graphics settings, audio devices, and which tools to launch by editing the configuration file.

1. Right-click the Elite Switch tray icon
2. Select **Edit Config...**
3. The configuration file will open in your default JSON editor (usually Notepad)

The configuration file is located at: `%USERPROFILE%\dot-files\.eliteswitch.json`

### Configuration Options

**Graphics Settings**: Customize resolution, refresh rate, and quality presets for each mode under the `graphics.vr` and `graphics.monitor` sections.

**Audio Devices**: Define which audio devices appear in the menus and which devices to automatically select for each mode under the `audio.audioOut` and `audio.microphone` sections.

**Tool Management**: Specify which programs to launch and stop for each mode:
- `tools.common`: Tools used in both VR and Monitor modes (Elite Dangerous, VoiceAttack, etc.)
- `tools.vr`: VR-specific tools (Virtual Desktop Streamer)
- `tools.monitor`: Monitor-specific tools (TrackIR for head tracking)

For detailed configuration examples, see the [CLAUDE.md](CLAUDE.md) file.

### Default Tool Paths

Elite Switch expects tools to be installed at these standard locations:

- **Elite Dangerous**: `C:\Program Files (x86)\Frontier\EDLaunch\EDLaunch.exe`
- **VoiceAttack**: `C:\Program Files (x86)\Steam\steamApps\common\VoiceAttack\VoiceAttack.exe`
- **EDDiscovery**: `C:\Program Files\EDDiscovery\EDDiscovery.exe`
- **Virtual Desktop Streamer**: `C:\Program Files\Virtual Desktop Streamer\VirtualDesktop.Streamer.exe`
- **TrackIR**: `C:\Program Files (x86)\TrackIR5\TrackIR5.exe`

If your tools are installed elsewhere, you can update the paths in the configuration file.

## Troubleshooting

### The tray icon doesn't appear
- Make sure the application is running (check Task Manager)
- Try restarting the application
- Check if the icon is hidden - click the arrow in the system tray to show hidden icons

### Audio devices don't appear in the menu
- Open the configuration file (Edit Config...) and verify your audio device substrings
- The substring must match part of your device name (case-insensitive)
- Restart the application after making configuration changes to audio devices

### Tools aren't starting
- Verify the tool paths in the configuration file point to the correct locations
- Check if you have the tools installed
- Make sure Elite Switch is running with administrator privileges

### Graphics settings aren't changing
- Ensure Elite Dangerous is closed when switching modes
- Check that Elite Dangerous is installed in the standard location: `%LOCALAPPDATA%\Frontier Developments\Elite Dangerous\`

### "Access Denied" or permission errors
- Make sure to run Elite Switch as administrator (it should prompt automatically)
- Some anti-virus software may block process management - you may need to add an exception

## Getting Help

If you encounter issues:
1. Check the troubleshooting section above
2. Review your configuration file for errors (invalid JSON syntax, incorrect paths)
3. Report issues on the [GitHub Issues](https://github.com/yourusername/EliteSwitch/issues) page

## License

This project is open source. See the LICENSE file for details.
