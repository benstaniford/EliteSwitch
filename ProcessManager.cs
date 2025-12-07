using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EliteSwitch;

public class ProcessManager
{
    private GraphicsConfig _config;

    public ProcessManager()
    {
        _config = GraphicsConfig.Load();
    }

    public void ReloadConfig()
    {
        _config = GraphicsConfig.Load();
    }

    public void StopTools(GameMode mode)
    {
        var killList = new List<string>(_config.Tools.StopAlways);

        if (mode == GameMode.Monitor)
        {
            killList.AddRange(_config.Tools.StopInMonitorMode);
        }

        foreach (var processName in killList)
        {
            try
            {
                var processes = Process.GetProcesses()
                    .Where(p => p.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase));

                foreach (var process in processes)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(5000);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to kill process {processName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error enumerating processes for {processName}: {ex.Message}");
            }
        }
    }

    public void StartTools(GameMode mode)
    {
        var startList = new List<string>(_config.Tools.CommonTools);

        if (mode == GameMode.VR)
        {
            // Add VR-only tools
            startList.AddRange(_config.Tools.VROnlyTools);
        }

        foreach (var executable in startList)
        {
            if (File.Exists(executable))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = executable,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to start {executable}: {ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine($"Executable not found: {executable}");
            }
        }
    }
}
