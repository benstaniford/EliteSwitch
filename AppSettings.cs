using System.IO;
using System.Text.Json;

namespace EliteSwitch;

public class AppSettings
{
    public GameMode CurrentMode { get; set; } = GameMode.Monitor;
    public bool AutoStartTools { get; set; } = false;

    private static string SettingsFilePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EliteSwitch", "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                string json = File.ReadAllText(SettingsFilePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
        }

        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            string directory = Path.GetDirectoryName(SettingsFilePath)!;
            Directory.CreateDirectory(directory);

            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }
}
