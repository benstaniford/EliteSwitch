using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EliteSwitch;

public class ProcessManager
{
    private readonly string _home;
    private readonly string _programFiles;
    private readonly string _programFilesX86;
    private readonly string _localAppData;

    public ProcessManager()
    {
        _home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        _programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }

    public void StopTools(GameMode mode)
    {
        var killList = new List<string>
        {
            "elitedangerous64",
            "edlaunch",
            "dropbox",
            "onedrive",
            "autohotkey",
            "steam",
            "messenger"
        };

        if (mode == GameMode.Monitor)
        {
            killList.Add("virtualdesktop.streamer");
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
        var startList = new List<string>
        {
            Path.Combine(_programFilesX86, "TrackIR5", "TrackIR5.exe"),
            Path.Combine(_programFilesX86, "Frontier", "EDLaunch", "EDLaunch.exe"),
            Path.Combine(_home, "dot-files", "games", "AutoHotKey Scripts", "EliteDangerous.ahk"),
            Path.Combine(_programFilesX86, "Steam", "steamApps", "common", "VoiceAttack", "VoiceAttack.exe"),
            Path.Combine(_programFiles, "EDDiscovery", "EDDiscovery.exe")
        };

        if (mode == GameMode.VR)
        {
            // Start Oculus via PowerShell script
            string startOculusScript = Path.Combine(_home, "dot-files", "games", "Oculus", "StartOculus.ps1");
            if (File.Exists(startOculusScript))
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-ExecutionPolicy Bypass -File \"{startOculusScript}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to start Oculus: {ex.Message}");
                }
            }

            // Add VR Streamer
            startList.Add(Path.Combine(_programFiles, "Virtual Desktop Streamer", "VirtualDesktop.Streamer.exe"));
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
