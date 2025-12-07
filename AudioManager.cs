using System;
using System.Collections.Generic;
using System.Linq;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;

namespace EliteSwitch;

public class AudioManager
{
    private readonly CoreAudioController _audioController;

    public AudioManager()
    {
        _audioController = new CoreAudioController();
    }

    public List<string> GetAvailablePlaybackDevices()
    {
        try
        {
            var devices = _audioController.GetPlaybackDevices(DeviceState.Active);
            return devices.Select(d => d.FullName).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to get playback devices: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Exception type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return new List<string>();
        }
    }

    public List<string> GetAvailableCaptureDevices()
    {
        try
        {
            var devices = _audioController.GetCaptureDevices(DeviceState.Active);
            return devices.Select(d => d.FullName).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to get capture devices: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Exception type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return new List<string>();
        }
    }

    public string? GetDefaultPlaybackDevice()
    {
        try
        {
            var defaultDevice = _audioController.DefaultPlaybackDevice;
            return defaultDevice?.FullName;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to get default playback device: {ex.Message}");
            return null;
        }
    }

    public string? GetDefaultCaptureDevice()
    {
        try
        {
            var defaultDevice = _audioController.DefaultCaptureDevice;
            return defaultDevice?.FullName;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to get default capture device: {ex.Message}");
            return null;
        }
    }

    public async void SetDefaultPlaybackDevice(string deviceNameFragment)
    {
        try
        {
            var playbackDevices = _audioController.GetPlaybackDevices(DeviceState.Active);
            var playbackDevice = playbackDevices.FirstOrDefault(d =>
                d.FullName.Contains(deviceNameFragment, StringComparison.OrdinalIgnoreCase) ||
                d.Name.Contains(deviceNameFragment, StringComparison.OrdinalIgnoreCase));

            if (playbackDevice != null)
            {
                await playbackDevice.SetAsDefaultAsync();
                await playbackDevice.SetAsDefaultCommunicationsAsync();
                System.Diagnostics.Debug.WriteLine($"Set default playback device to: {playbackDevice.FullName}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"No playback device found matching: {deviceNameFragment}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to set playback device: {ex.Message}");
            throw;
        }
    }

    public async void SetDefaultCaptureDevice(string deviceNameFragment)
    {
        try
        {
            var recordingDevices = _audioController.GetCaptureDevices(DeviceState.Active);
            var recordingDevice = recordingDevices.FirstOrDefault(d =>
                d.FullName.Contains(deviceNameFragment, StringComparison.OrdinalIgnoreCase) ||
                d.Name.Contains(deviceNameFragment, StringComparison.OrdinalIgnoreCase));

            if (recordingDevice != null)
            {
                await recordingDevice.SetAsDefaultAsync();
                await recordingDevice.SetAsDefaultCommunicationsAsync();
                System.Diagnostics.Debug.WriteLine($"Set default recording device to: {recordingDevice.FullName}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"No recording device found matching: {deviceNameFragment}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to set capture device: {ex.Message}");
            throw;
        }
    }

    public async void SetDefaultAudioDevice(string deviceNameFragment)
    {
        try
        {
            // Set default playback device
            var playbackDevices = _audioController.GetPlaybackDevices(DeviceState.Active);
            var playbackDevice = playbackDevices.FirstOrDefault(d =>
                d.FullName.Contains(deviceNameFragment, StringComparison.OrdinalIgnoreCase) ||
                d.Name.Contains(deviceNameFragment, StringComparison.OrdinalIgnoreCase));

            if (playbackDevice != null)
            {
                await playbackDevice.SetAsDefaultAsync();
                await playbackDevice.SetAsDefaultCommunicationsAsync();
                System.Diagnostics.Debug.WriteLine($"Set default playback device to: {playbackDevice.FullName}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"No playback device found matching: {deviceNameFragment}");
            }

            // Set default recording device
            var recordingDevices = _audioController.GetCaptureDevices(DeviceState.Active);
            var recordingDevice = recordingDevices.FirstOrDefault(d =>
                d.FullName.Contains(deviceNameFragment, StringComparison.OrdinalIgnoreCase) ||
                d.Name.Contains(deviceNameFragment, StringComparison.OrdinalIgnoreCase));

            if (recordingDevice != null)
            {
                await recordingDevice.SetAsDefaultAsync();
                await recordingDevice.SetAsDefaultCommunicationsAsync();
                System.Diagnostics.Debug.WriteLine($"Set default recording device to: {recordingDevice.FullName}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"No recording device found matching: {deviceNameFragment}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to set audio device: {ex.Message}");
            throw;
        }
    }
}
