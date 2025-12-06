using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace EliteSwitch;

public class EliteConfigManager
{
    private readonly string _settingsPath;
    private readonly string _displaySettingsPath;

    public EliteConfigManager()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string frontierDir = Path.Combine(localAppData, "Frontier Developments", "Elite Dangerous", "Options", "Graphics");

        _settingsPath = Path.Combine(frontierDir, "Settings.xml");
        _displaySettingsPath = Path.Combine(frontierDir, "DisplaySettings.xml");
    }

    public void ApplyMode(GameMode mode)
    {
        var settings = mode == GameMode.VR ? GetVRSettings() : GetMonitorSettings();

        ApplySettingsToFile(_settingsPath, settings);
        ApplySettingsToFile(_displaySettingsPath, settings);
    }

    private Dictionary<string, string> GetVRSettings()
    {
        return new Dictionary<string, string>
        {
            { "ScreenWidth", "3840" },
            { "ScreenHeight", "2160" },
            { "FullScreen", "0" },
            { "StereoscopicMode", "4" },
            { "GammaOffset", "0.240000" },
            { "DX11_RefreshRateNumerator", "59810" },
            { "DX11_RefreshRateDenominator", "1000" },
            { "PresetName", "VRUltra" }
        };
    }

    private Dictionary<string, string> GetMonitorSettings()
    {
        return new Dictionary<string, string>
        {
            { "ScreenWidth", "3840" },
            { "ScreenHeight", "2160" },
            { "FullScreen", "2" },
            { "StereoscopicMode", "0" },
            { "DX11_RefreshRateNumerator", "120" },
            { "DX11_RefreshRateDenominator", "1" },
            { "PresetName", "Ultra" }
        };
    }

    private void ApplySettingsToFile(string filePath, Dictionary<string, string> settings)
    {
        if (!File.Exists(filePath))
        {
            System.Diagnostics.Debug.WriteLine($"Config file not found: {filePath}");
            return;
        }

        try
        {
            XDocument doc = XDocument.Load(filePath);

            foreach (var setting in settings)
            {
                var element = doc.Descendants(setting.Key).FirstOrDefault();
                if (element != null)
                {
                    element.Value = setting.Value;
                }
            }

            doc.Save(filePath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to update config file {filePath}: {ex.Message}");
            throw;
        }
    }
}
