using LanaDelSsh.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LanaDelSsh.Services;

public class ConnectionStorageService : IConnectionStorageService, IDisposable
{
    public static readonly string DefaultFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LanaDelSsh");

    private static readonly string DefaultFilePath = Path.Combine(DefaultFolder, FileName);

    public const string FileName = "LanaDelSsh.connections.json";

    private string? _customFilePath;
    private FileSystemWatcher? _watcher;
    private Timer? _debounceTimer;
    private DateTime _lastWriteUtc = DateTime.MinValue;

    public event EventHandler? FileChanged;

    public string CurrentFilePath => _customFilePath ?? DefaultFilePath;

    private string GetEffectivePath() => CurrentFilePath;

    public void SetFolder(string? folder)
    {
        var newPath = string.IsNullOrWhiteSpace(folder)
            ? null
            : Path.Combine(folder.Trim(), FileName);

        if (newPath == _customFilePath) return;

        _customFilePath = newPath;
        RebuildWatcher();
        FileChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RebuildWatcher()
    {
        _watcher?.Dispose();
        _watcher = null;
        _debounceTimer?.Dispose();
        _debounceTimer = null;

        var path = GetEffectivePath();
        var dir = Path.GetDirectoryName(path);
        var file = Path.GetFileName(path);

        if (dir is null || file is null || !Directory.Exists(dir)) return;

        _watcher = new FileSystemWatcher(dir, file)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true
        };
        _watcher.Changed += OnFileChanged;
        _watcher.Created += OnFileChanged;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Suppress events triggered by our own writes
        if ((DateTime.UtcNow - _lastWriteUtc).TotalMilliseconds < 500) return;

        // Debounce: reset timer on each event to wait for the file to settle
        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(_ =>
        {
            _debounceTimer?.Dispose();
            _debounceTimer = null;
            FileChanged?.Invoke(this, EventArgs.Empty);
        }, null, 300, Timeout.Infinite);
    }

    public async Task<List<SshConnection>> LoadAsync()
    {
        var path = GetEffectivePath();
        if (!File.Exists(path))
            return [];

        try
        {
            var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
            return JsonSerializer.Deserialize(json, AppJsonContext.Default.ListSshConnection) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task SaveAsync(List<SshConnection> connections)
    {
        var path = GetEffectivePath();
        var dir = Path.GetDirectoryName(path) ?? DefaultFolder;
        Directory.CreateDirectory(dir);
        _lastWriteUtc = DateTime.UtcNow;
        var json = JsonSerializer.Serialize(connections, AppJsonContext.Default.ListSshConnection);
        await File.WriteAllTextAsync(path, json).ConfigureAwait(false);
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _debounceTimer?.Dispose();
    }
}
