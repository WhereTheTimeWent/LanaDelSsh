using LanaDelSsh.Models;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LanaDelSsh.Services;

public class SshLaunchService : ISshLaunchService
{
    public void Connect(string host, int port, AppSettings settings)
    {
        // Build ssh arguments
        var args = BuildSshArgs(host, port, settings);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Launch cmd.exe which stays open after ssh exits
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c ssh {args}",
                UseShellExecute = true
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            LaunchMacTerminal($"ssh {args}");
        }
        else
        {
            // Linux: try common terminal emulators in order
            LaunchLinuxTerminal($"ssh {args}", settings.LinuxTerminal);
        }
    }

    private static string BuildSshArgs(string host, int port, AppSettings settings)
    {
        var portArg = port != 22 ? $" -p {port}" : string.Empty;
        var strictArg = settings.IgnoreCertificates
            ? " -o StrictHostKeyChecking=no"
            : string.Empty;
        return $"{host}{portArg}{strictArg}";
    }

    private static void LaunchMacTerminal(string command)
    {
        // Write a temporary shell script and open it with Terminal.app
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

    private static void LaunchLinuxTerminal(string command, string? customTerminal = null)
    {
        string[] candidates = string.IsNullOrWhiteSpace(customTerminal)
            ? ["x-terminal-emulator", "gnome-terminal", "xfce4-terminal", "konsole", "xterm"]
            : [customTerminal, "x-terminal-emulator", "gnome-terminal", "xfce4-terminal", "konsole", "xterm"];

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

        throw new InvalidOperationException(
            $"No terminal emulator found. Tried: {string.Join(", ", candidates)}");
    }
}
