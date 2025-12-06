# Elite Switch - Quick Start Guide

## First Time Setup

### 1. Create Icon File
You need to provide an `icon.ico` file in the project root:
- Use any PNG/JPG to ICO converter online
- Recommended size: 256x256 pixels
- Save as `icon.ico` in `/home/ben/Code/EliteSwitch/`

### 2. Install WiX Toolset (for installer builds)
```bash
dotnet tool install --global wix
```

### 3. Build Everything
```bash
# Windows
build.cmd

# Linux/Mac
./build.sh
```

## Common Tasks

### Build Application Only
```bash
dotnet build EliteSwitch.csproj -c Release
```
Output: `bin/Release/net8.0-windows/EliteSwitch.exe`

### Build Installer Only
```bash
dotnet build EliteSwitch.Installer/EliteSwitch.Installer.wixproj -c Release
```
Output: `EliteSwitch.Installer/bin/Release/x64/en-US/EliteSwitchSetup.msi`

### Run Application (Development)
```bash
dotnet run
```
Must be run with administrator privileges.

### Install via MSI
1. Double-click `EliteSwitchSetup.msi`
2. Follow installation wizard
3. Application installs to `C:\Program Files\EliteSwitch\`
4. Start menu shortcut created automatically

## Using the Application

### System Tray Menu
Right-click the tray icon for options:
- **Current Mode**: Shows VR or Monitor mode (indicator only)
- **Switch to VR Mode**: Changes to VR configuration
- **Switch to Monitor Mode**: Changes to Monitor configuration
- **Start Tools**: Launches helper programs for current mode
- **Stop Tools**: Terminates running game and helper programs
- **Exit**: Closes the application

### What Each Mode Does

**VR Mode:**
- Sets Elite Dangerous to VR mode (StereoscopicMode=4)
- Windowed mode with VR-optimized refresh rate
- Starts: Oculus, Virtual Desktop Streamer, + common tools
- Switches audio to "h5" device

**Monitor Mode:**
- Sets Elite Dangerous to fullscreen 4K mode
- 120Hz refresh rate, Ultra preset
- Starts: Common tools only (no VR apps)
- Stops: Any running VR Streamer processes
- Switches audio to "h5" device

### Common Tools (Both Modes)
- TrackIR
- Elite Dangerous Launcher (EDLaunch)
- AutoHotkey scripts
- VoiceAttack
- EDDiscovery

### Settings Location
Application settings are saved to:
`%APPDATA%\EliteSwitch\settings.json`

Current mode is remembered between sessions.

## Troubleshooting

### Build Errors

**"icon.ico not found"**
- Create an icon file (see ICON_NOTE.txt)
- Or temporarily create a blank .ico file to test

**"WiX command not found"**
```bash
dotnet tool install --global wix
# Then close and reopen terminal
```

**"Access denied" when building**
- Close any running instances of EliteSwitch.exe
- Run Visual Studio as administrator

### Runtime Errors

**"Access denied" or permissions errors**
- Application requires administrator privileges
- Right-click and "Run as administrator"

**Audio device not switching**
- Ensure "h5" device is connected and enabled
- Edit `AudioManager.cs` to change device name if needed

**Elite Dangerous config files not found**
- Ensure Elite Dangerous has been run at least once
- Check path: `%LOCALAPPDATA%\Frontier Developments\Elite Dangerous\`

**Tools not starting**
- Verify tool paths in `ProcessManager.cs`
- Default paths may differ on your system

## Customization

### Change Audio Device Name
Edit `AudioManager.cs` line 15 and 39:
```csharp
_audioManager.SetDefaultAudioDevice("h5");  // Change "h5" to your device
```

### Modify Tool Paths
Edit `ProcessManager.cs` lines 22-29 (startList) to customize paths.

### Enable Auto-Start with Windows
Uncomment the registry entry in `EliteSwitch.Installer/Package.wxs`:
```xml
<Component Id="RegistryEntries" Bitness="always64">
  <RegistryKey Root="HKLM" Key="SOFTWARE\Microsoft\Windows\CurrentVersion\Run">
    <RegistryValue Name="EliteSwitch" Value="[INSTALLFOLDER]EliteSwitch.exe" Type="string" />
  </RegistryKey>
</Component>
```
Then uncomment in Feature section and rebuild installer.

## Project Files

- **EliteSwitch.sln** - Open in Visual Studio
- **README.md** - Full documentation
- **PROJECT_STRUCTURE.md** - Architecture overview
- **build.cmd / build.sh** - Automated build scripts
- **EliteSwitch.Installer/** - MSI installer project

## Next Steps

1. Create the icon file
2. Build the project
3. Test the application
4. Build the installer
5. Distribute the MSI to other PCs

For detailed information, see README.md
