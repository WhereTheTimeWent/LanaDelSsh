using LanaDelSsh.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace LanaDelSsh.Services;

public class SettingsService : ISettingsService
{
    private static readonly string DataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LanaDelSsh");

    private static readonly string FilePath = Path.Combine(DataFolder, "settings.json");

    public async Task<AppSettings> LoadAsync()
    {
        if (!File.Exists(FilePath))
            return new AppSettings();

        try
        {
            var json = await File.ReadAllTextAsync(FilePath).ConfigureAwait(false);
            return JsonSerializer.Deserialize(json, AppJsonContext.Default.AppSettings) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public async Task SaveAsync(AppSettings settings)
    {
        Directory.CreateDirectory(DataFolder);
        var json = JsonSerializer.Serialize(settings, AppJsonContext.Default.AppSettings);
        await File.WriteAllTextAsync(FilePath, json).ConfigureAwait(false);
    }
}
