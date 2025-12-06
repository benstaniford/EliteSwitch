# Elite Switch - Project Structure

## Overview

This solution contains two projects:
1. **EliteSwitch** - The main WPF system tray application (.NET 8.0)
2. **EliteSwitch.Installer** - WiX-based MSI installer project (WiX v4)

## Directory Structure

```
EliteSwitch/
├── EliteSwitch.sln                      # Visual Studio solution file
├── EliteSwitch.csproj                   # Main application project
├── README.md                            # Project documentation
├── PROJECT_STRUCTURE.md                 # This file
├── ICON_NOTE.txt                        # Instructions for icon creation
├── .gitignore                           # Git ignore rules
├── build.cmd                            # Windows build script
├── build.sh                             # Linux/Mac build script
│
├── Application Files
│   ├── App.xaml                         # WPF application definition
│   ├── App.xaml.cs                      # Application code-behind
│   ├── MainWindow.xaml                  # Main (hidden) window with tray icon
│   ├── MainWindow.xaml.cs               # Main window code-behind
│   ├── app.manifest                     # Application manifest (requires admin)
│   └── icon.ico                         # Application icon (user-provided)
│
├── Core Components
│   ├── GameMode.cs                      # Enum: VR/Monitor modes
│   ├── AppSettings.cs                   # Settings persistence (JSON)
│   ├── EliteConfigManager.cs            # Elite Dangerous XML config management
│   ├── ProcessManager.cs                # Tool start/stop management
│   └── AudioManager.cs                  # Audio device switching
│
└── EliteSwitch.Installer/               # Installer project directory
    ├── EliteSwitch.Installer.wixproj    # WiX project file
    ├── Package.wxs                      # WiX installer definition
    ├── License.rtf                      # License agreement text
    ├── README.md                        # Installer-specific documentation
    └── .gitignore                       # Installer build artifacts
```

## Build Outputs

### Application Build
- **Location**: `bin/Release/net8.0-windows/`
- **Main executable**: `EliteSwitch.exe`
- **Dependencies**: All NuGet packages (H.NotifyIcon, AudioSwitcher, etc.)

### Installer Build
- **Location**: `EliteSwitch.Installer/bin/Release/x64/en-US/`
- **Installer**: `EliteSwitchSetup.msi`
- **Debug symbols**: `EliteSwitchSetup.wixpdb`

## Key Technologies

### Application
- **.NET 8.0** - Latest .NET runtime
- **WPF** - Windows Presentation Foundation for UI
- **H.NotifyIcon.Wpf** - System tray icon support
- **AudioSwitcher.AudioApi** - Audio device management
- **System.Xml.Linq** - XML configuration file manipulation

### Installer
- **WiX Toolset v4** - Windows Installer XML toolset
- **MSI** - Windows Installer package format

## Dependencies

### NuGet Packages (Application)
```xml
<PackageReference Include="H.NotifyIcon.Wpf" Version="2.1.4" />
<PackageReference Include="AudioSwitcher.AudioApi" Version="4.0.0-alpha5" />
<PackageReference Include="AudioSwitcher.AudioApi.CoreAudio" Version="4.0.0-alpha5" />
```

### NuGet Packages (Installer)
```xml
<PackageReference Include="WixToolset.UI.wixext" Version="4.0.5" />
```

## Runtime Requirements

### End User Installation
- **Windows 10/11** (64-bit)
- **.NET 8.0 Runtime** (bundled with installer or available from Microsoft)
- **Administrator privileges** (required for process management and audio switching)

### Development Environment
- **Visual Studio 2022** (recommended) or **VS Code**
- **.NET 8.0 SDK**
- **WiX Toolset v4** (via `dotnet tool install --global wix`)

## Configuration Files

### Application Runtime
- **User settings**: `%APPDATA%\EliteSwitch\settings.json`
- **Elite Dangerous configs**:
  - `%LOCALAPPDATA%\Frontier Developments\Elite Dangerous\Options\Graphics\Settings.xml`
  - `%LOCALAPPDATA%\Frontier Developments\Elite Dangerous\Options\Graphics\DisplaySettings.xml`

### Build Configuration
- **EliteSwitch.csproj**: Application dependencies and build settings
- **EliteSwitch.Installer.wixproj**: Installer build settings
- **Package.wxs**: Installer component definitions and features

## Build Process

### Quick Build
```bash
# Windows
build.cmd

# Linux/Mac
./build.sh
```

### Manual Build
```bash
# Restore packages
dotnet restore

# Build application
dotnet build EliteSwitch.csproj -c Release

# Build installer (requires WiX)
dotnet build EliteSwitch.Installer/EliteSwitch.Installer.wixproj -c Release
```

### Visual Studio
1. Open `EliteSwitch.sln`
2. Set configuration to **Release**
3. Set platform to **x64** (for installer)
4. Build > Build Solution

## Version Management

To update the version:

1. **Application**: Update in `EliteSwitch.csproj`
   ```xml
   <AssemblyVersion>1.0.0.0</AssemblyVersion>
   <FileVersion>1.0.0.0</FileVersion>
   ```

2. **Installer**: Update in `Package.wxs`
   ```xml
   <Package Version="1.0.0.0" ... />
   ```

**Important**: Keep the `UpgradeCode` GUID constant to support automatic upgrades.

## Distribution

### MSI Installer (Recommended)
- Single file: `EliteSwitchSetup.msi`
- Includes all dependencies
- Automatic start menu shortcut
- Add/Remove Programs integration
- Clean uninstall support

### Portable Executable
- Copy entire `bin/Release/net8.0-windows/` folder
- Requires .NET 8.0 Runtime installed
- Must run as administrator
- No installer experience

## Future Enhancements

Potential additions to the project structure:
- **Tests/**: Unit tests and integration tests
- **Docs/**: Additional documentation and screenshots
- **Scripts/**: PowerShell deployment scripts
- **Assets/**: Additional icons and resources
- **Localization/**: Multi-language support files
