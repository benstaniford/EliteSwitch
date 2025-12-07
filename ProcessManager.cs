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
        // Always stop common tools
        var killList = new List<string>(_config.Tools.Common.OnStop);

        // Add mode-specific tools to stop
        if (mode == GameMode.VR)
        {
            killList.AddRange(_config.Tools.VR.OnStop);
        }
        else if (mode == GameMode.Monitor)
        {
            killList.AddRange(_config.Tools.Monitor.OnStop);
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
        // Always start common tools
        var startList = new List<string>(_config.Tools.Common.OnStart);

        // Add mode-specific tools to start
        if (mode == GameMode.VR)
        {
            startList.AddRange(_config.Tools.VR.OnStart);
        }
        else if (mode == GameMode.Monitor)
        {
            startList.AddRange(_config.Tools.Monitor.OnStart);
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
