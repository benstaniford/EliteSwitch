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
    private GraphicsConfig _config;

    public EliteConfigManager()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string frontierDir = Path.Combine(localAppData, "Frontier Developments", "Elite Dangerous", "Options", "Graphics");

        _settingsPath = Path.Combine(frontierDir, "Settings.xml");
        _displaySettingsPath = Path.Combine(frontierDir, "DisplaySettings.xml");

        // Load configuration from file or use defaults
        _config = GraphicsConfig.Load();
    }

    public void ApplyMode(GameMode mode)
    {
        var settings = mode == GameMode.VR ? _config.Graphics.VRSettings : _config.Graphics.MonitorSettings;

        ApplySettingsToFile(_settingsPath, settings);
        ApplySettingsToFile(_displaySettingsPath, settings);

        // Save the configuration after applying (creates the file with current settings if it doesn't exist)
        _config.Save();
    }

    public void ReloadConfig()
    {
        _config = GraphicsConfig.Load();
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
