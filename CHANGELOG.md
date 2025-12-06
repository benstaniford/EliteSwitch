# Changelog

All notable changes to Elite Switch will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Added
- **VR/Monitor Mode Switching**: Seamlessly switch Elite Dangerous graphics configurations between VR and Monitor modes
  - VR Mode: Windowed, StereoscopicMode 4, VR-optimized refresh rate (59.81Hz), VRUltra preset
  - Monitor Mode: Fullscreen borderless, StereoscopicMode 0, 120Hz refresh rate, Ultra preset
- **System Tray Application**: Runs in the background with easy access via system tray icon
- **Automated Tool Management**:
  - Start tools: TrackIR, EDLaunch, AutoHotkey scripts, VoiceAttack, EDDiscovery
  - VR-specific: Oculus (via PowerShell script), Virtual Desktop Streamer
  - Stop tools: Elite Dangerous, Steam, Dropbox, OneDrive, AutoHotkey, Messenger
- **Audio Device Switching**: Automatically switches Windows default audio devices when changing modes
- **Settings Persistence**: Remembers current mode across application restarts
- **Dual Config File Updates**: Updates both `Settings.xml` and `DisplaySettings.xml` for reliable configuration changes
- **MSI Installer**:
  - Windows Installer with WiX 3.11
  - Optional desktop shortcut
  - Start Menu integration
  - Optional auto-start with Windows (disabled by default)
- **Context Menu Integration**: Quick access to mode switching and tool management
- **Notifications**: System tray notifications for mode changes and tool operations

### Technical Details
- Built with .NET 8.0 and WPF
- Uses H.NotifyIcon.Wpf for system tray functionality
- AudioSwitcher.AudioApi for audio device management
- Requires administrator privileges for process and audio management
- XML-based Elite Dangerous configuration management with LINQ to XML

### System Requirements
- Windows 10 or later (64-bit)
- .NET 8.0 Runtime (Desktop)
- Administrator privileges for full functionality
- Elite Dangerous installed at standard location

## [1.0.0] - Initial Release

_This is the first release of Elite Switch._
