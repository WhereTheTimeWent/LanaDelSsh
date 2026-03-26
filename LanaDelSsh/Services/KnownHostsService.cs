using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace LanaDelSsh.Services;

public class KnownHostsService : IKnownHostsService
{
    private readonly string _knownHostsPath;

    public KnownHostsService(string? knownHostsPath = null)
    {
        _knownHostsPath = knownHostsPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".ssh", "known_hosts");
    }

    public bool RemoveHost(string hostname, int port)
    {
        if (string.IsNullOrWhiteSpace(hostname)) return false;

        var path = _knownHostsPath;
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

        // Plain-text entry
        if (!entry.StartsWith('|'))
            return entry.Equals(expected, StringComparison.OrdinalIgnoreCase);

        // Hashed entry: |1|<salt_base64>|<hash_base64>
        var parts = entry.Split('|');
        if (parts.Length != 4 || parts[1] != "1") return false;

        byte[] salt, storedHash;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            storedHash = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var computed = HMACSHA1.HashData(salt, System.Text.Encoding.UTF8.GetBytes(expected));
        return computed.AsSpan().SequenceEqual(storedHash);
    }
}
