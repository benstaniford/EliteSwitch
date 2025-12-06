# Elite Switch Installer

This WiX installer project creates an MSI package for the Elite Switch application.

## Prerequisites

To build the installer, you need:

1. **.NET 8.0 SDK** - For building the main application
2. **WiX Toolset v4** - Install via:
   ```bash
   dotnet tool install --global wix
   ```

## Building the Installer

### From Command Line

```bash
# Build the main application first
dotnet build ../EliteSwitch.csproj -c Release

# Build the installer
dotnet build EliteSwitch.Installer.wixproj -c Release
```

The installer MSI will be created at:
`bin/Release/x64/en-US/EliteSwitchSetup.msi`

### From Visual Studio

1. Open `EliteSwitch.sln`
2. Set build configuration to **Release** and platform to **x64**
3. Right-click the `EliteSwitch.Installer` project and select **Build**

## What the Installer Does

- Installs EliteSwitch to `C:\Program Files\EliteSwitch\`
- Creates a Start Menu shortcut
- Adds an entry to Add/Remove Programs with icon
- Includes all required dependencies:
  - .NET 8.0 runtime components
  - AudioSwitcher.AudioApi libraries
  - H.NotifyIcon.Wpf library
  - Application icon file
- **Requires administrator privileges** to install

## Optional Features

### Auto-Start with Windows

By default, the application does NOT start with Windows. To enable this:

1. Open `Package.wxs`
2. Uncomment the `RegistryEntries` component (lines with `<!--` and `-->`)
3. Uncomment the ComponentRef in the Feature section
4. Rebuild the installer

This will add a registry entry to start Elite Switch when Windows boots.

## Customization

### Changing Install Location

Users can choose a custom install location during installation using the InstallDir dialog.

### Updating Version

Update the version in `Package.wxs`:
```xml
<Package Name="Elite Switch"
         Version="1.0.0.0"  <!-- Change this -->
```

### Changing License

Edit `License.rtf` to customize the license agreement shown during installation.

## Uninstallation

Users can uninstall via:
- Windows Settings > Apps > Elite Switch
- Control Panel > Programs and Features

The uninstaller will remove all installed files but preserve user settings in `%APPDATA%\EliteSwitch\`.

## Upgrading

The installer supports major upgrades. When installing a newer version:
- The old version is automatically uninstalled
- User settings are preserved
- The new version is installed in the same location

The `UpgradeCode` GUID must remain constant across versions for upgrades to work.
