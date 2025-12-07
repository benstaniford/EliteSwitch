using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EliteSwitch;

public class ToolsConfig
{
    [JsonPropertyName("common")]
    public List<string> CommonTools { get; set; } = new();

    [JsonPropertyName("vrOnly")]
    public List<string> VROnlyTools { get; set; } = new();

    [JsonPropertyName("monitorOnly")]
    public List<string> MonitorOnlyTools { get; set; } = new();

    [JsonPropertyName("stopAlways")]
    public List<string> StopAlways { get; set; } = new();

    [JsonPropertyName("stopInVrMode")]
    public List<string> StopInVrMode { get; set; } = new();

    [JsonPropertyName("stopInMonitorMode")]
    public List<string> StopInMonitorMode { get; set; } = new();
}

public class GraphicsSettings
{
    [JsonPropertyName("vr")]
    public Dictionary<string, string> VRSettings { get; set; } = new();

    [JsonPropertyName("monitor")]
    public Dictionary<string, string> MonitorSettings { get; set; } = new();
}

public class GraphicsConfig
{
    [JsonPropertyName("graphics")]
    public GraphicsSettings Graphics { get; set; } = new();

    [JsonPropertyName("tools")]
    public ToolsConfig Tools { get; set; } = new();

    private static readonly string ConfigFilePath = Path.Combine(
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
        "dot-files",
        ".eliteswitch.json"
    );

    public static GraphicsConfig GetDefaultConfig()
    {
        var home = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
        var programFiles = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);
        var programFilesX86 = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86);

        return new GraphicsConfig
        {
            Graphics = new GraphicsSettings
            {
                VRSettings = new Dictionary<string, string>
                {
                    { "ScreenWidth", "3840" },
                    { "ScreenHeight", "2160" },
                    { "FullScreen", "0" },
                    { "StereoscopicMode", "4" },
                    { "GammaOffset", "0.240000" },
                    { "DX11_RefreshRateNumerator", "59810" },
                    { "DX11_RefreshRateDenominator", "1000" },
                    { "PresetName", "VRUltra" }
                },
                MonitorSettings = new Dictionary<string, string>
                {
                    { "ScreenWidth", "3840" },
                    { "ScreenHeight", "2160" },
                    { "FullScreen", "2" },
                    { "StereoscopicMode", "0" },
                    { "DX11_RefreshRateNumerator", "120" },
                    { "DX11_RefreshRateDenominator", "1" },
                    { "PresetName", "Ultra" }
                }
            },
            Tools = new ToolsConfig
            {
                CommonTools = new List<string>
                {
                    Path.Combine(programFilesX86, "Frontier", "EDLaunch", "EDLaunch.exe"),
                    Path.Combine(home, "dot-files", "games", "AutoHotKey Scripts", "EliteDangerous.ahk"),
                    Path.Combine(programFilesX86, "Steam", "steamApps", "common", "VoiceAttack", "VoiceAttack.exe"),
                    Path.Combine(programFiles, "EDDiscovery", "EDDiscovery.exe")
                },
                VROnlyTools = new List<string>
                {
                    Path.Combine(programFiles, "Virtual Desktop Streamer", "VirtualDesktop.Streamer.exe")
                },
                MonitorOnlyTools = new List<string>
                {
                    Path.Combine(programFilesX86, "TrackIR5", "TrackIR5.exe")
                },
                StopAlways = new List<string>
                {
                    "elitedangerous64",
                    "edlaunch",
                    "dropbox",
                    "onedrive",
                    "autohotkey",
                    "steam",
                    "messenger"
                },
                StopInVrMode = new List<string>
                {
                },
                StopInMonitorMode = new List<string>
                {
                    "virtualdesktop.streamer"
                }
            }
        };
    }

    public static GraphicsConfig Load()
    {
        if (File.Exists(ConfigFilePath))
        {
            try
            {
                string json = File.ReadAllText(ConfigFilePath);
                var config = JsonSerializer.Deserialize<GraphicsConfig>(json);
                if (config != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Loaded graphics config from {ConfigFilePath}");
                    return config;
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load graphics config: {ex.Message}");
            }
        }

        System.Diagnostics.Debug.WriteLine("Using default graphics configuration");
        return GetDefaultConfig();
    }

    public void Save()
    {
        try
        {
            // Ensure directory exists
            string? directory = Path.GetDirectoryName(ConfigFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(ConfigFilePath, json);
            System.Diagnostics.Debug.WriteLine($"Saved graphics config to {ConfigFilePath}");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save graphics config: {ex.Message}");
            throw;
        }
    }

    public static string GetConfigFilePath()
    {
        return ConfigFilePath;
    }
}
