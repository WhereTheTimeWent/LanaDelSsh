using System;
using System.IO;
using System.Linq;

namespace LanaDelSsh.Services;

public class KnownHostsService : IKnownHostsService
{
    private static string KnownHostsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".ssh", "known_hosts");

    public bool RemoveHost(string hostname, int port)
    {
        if (string.IsNullOrWhiteSpace(hostname)) return false;

        var path = KnownHostsPath;
        if (!File.Exists(path)) return false;

        string[] lines;
        try
        {
            lines = File.ReadAllLines(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new IOException($"Could not read known_hosts: {ex.Message}", ex);
        }

        var filtered = lines.Where(line => !LineMatchesHost(line, hostname, port)).ToArray();

        if (filtered.Length == lines.Length) return false;

        try
        {
            File.WriteAllLines(path, filtered);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new IOException($"Could not write known_hosts: {ex.Message}", ex);
        }

        return true;
    }

    private static bool LineMatchesHost(string line, string hostname, int port)
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) return false;
        try
        {
            var keyPart = line.Split(' ')[0];
            return keyPart.Split(',').Any(entry => MatchesEntry(entry, hostname, port));
        }
        catch
        {
            return false;
        }
    }

    private static bool MatchesEntry(string entry, string hostname, int port)
    {
        var expected = port == 22
            ? hostname
            : $"[{hostname}]:{port}";
        return entry.Equals(expected, StringComparison.OrdinalIgnoreCase);
    }
}
