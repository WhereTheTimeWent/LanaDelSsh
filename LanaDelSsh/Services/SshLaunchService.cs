using LanaDelSsh.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace LanaDelSsh.Services;

public class SshLaunchService : ISshLaunchService
{
    public Process? Connect(string host, int port, AppSettings settings)
    {
        var command = $"ssh {BuildSshArgs(host, port, settings)}";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                UseShellExecute = true
            });

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            LaunchMacTerminal(command, keepOpen: false);
            return null; // 'open' exits immediately — not monitorable
        }

        LaunchLinuxTerminal(command, keepOpen: false, settings.LinuxTerminal);
        return null; // terminal process model too complex to monitor reliably
    }

    public void ConnectKeepOpen(string host, int port, AppSettings settings)
    {
        var command = $"ssh {BuildSshArgs(host, port, settings)}";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/k {command}",
                UseShellExecute = true
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            LaunchMacTerminal(command, keepOpen: true);
        }
        else
        {
            LaunchLinuxTerminal(command, keepOpen: true, settings.LinuxTerminal);
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

    private static void LaunchMacTerminal(string command, bool keepOpen)
    {
        var script = Path.GetTempFileName() + ".sh";
        var content = keepOpen
            ? $"#!/bin/sh\n{command}\nexec \"$SHELL\"\n"
            : $"#!/bin/sh\n{command}\n";
        File.WriteAllText(script, content);
        Process.Start("chmod", $"+x {script}")?.WaitForExit();
        Process.Start(new ProcessStartInfo
        {
            FileName = "open",
            Arguments = $"-a Terminal {script}",
            UseShellExecute = true
        });
    }

    private static void LaunchLinuxTerminal(string command, bool keepOpen, string? customTerminal = null)
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
                    // gnome-terminal already stays open via exec bash
                    "gnome-terminal" => $"-- bash -c \"{command}; exec bash\"",
                    // Other terminals: use a temp script to avoid quoting issues
                    _ when keepOpen => $"-e {WriteKeepOpenScript(command)}",
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

    private static string WriteKeepOpenScript(string command)
    {
        var script = Path.GetTempFileName() + ".sh";
        File.WriteAllText(script, $"#!/bin/sh\n{command}\nexec \"$SHELL\"\n");
        try { Process.Start("chmod", $"+x {script}")?.WaitForExit(); } catch { }
        return script;
    }
}
