using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LanaDelSsh.Services;

public class PingService : IPingService
{
    public void Ping(string host)
    {
        // Extract hostname/IP from user@host format
        var target = host.Contains('@')
            ? host[(host.IndexOf('@') + 1)..]
            : host;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // ping /t = continuous ping on Windows
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c ping /t {target}",
                UseShellExecute = true
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            LaunchMacTerminal($"ping {target}");
        }
        else
        {
            // Linux: ping runs continuously by default
            LaunchLinuxTerminal($"ping {target}");
        }
    }

    private static void LaunchMacTerminal(string command)
    {
        var script = System.IO.Path.GetTempFileName() + ".sh";
        System.IO.File.WriteAllText(script, $"#!/bin/sh\n{command}\n");
        Process.Start("chmod", $"+x {script}")?.WaitForExit();
        Process.Start(new ProcessStartInfo
        {
            FileName = "open",
            Arguments = $"-a Terminal {script}",
            UseShellExecute = true
        });
    }

    private static void LaunchLinuxTerminal(string command)
    {
        string[] candidates = ["x-terminal-emulator", "gnome-terminal", "xfce4-terminal", "konsole", "xterm"];

        foreach (var terminal in candidates)
        {
            try
            {
                string termArgs = terminal switch
                {
                    "gnome-terminal" => $"-- bash -c \"{command}; exec bash\"",
                    _ => $"-e \"{command}\""
                };

                Process.Start(new ProcessStartInfo
                {
                    FileName = terminal,
                    Arguments = termArgs,
                    UseShellExecute = true
                });
                return;
            }
            catch
            {
                // Try next terminal
            }
        }
    }
}
