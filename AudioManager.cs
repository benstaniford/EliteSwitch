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
