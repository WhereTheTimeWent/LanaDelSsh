using CommunityToolkit.Mvvm.ComponentModel;
using LanaDelSsh.Localization;
using LanaDelSsh.Services;
using System;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace LanaDelSsh.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public QuickConnectViewModel QuickConnect { get; }
    public SavedConnectionsViewModel SavedConnections { get; }
    public SettingsViewModel Settings { get; }

    private readonly ISettingsService _settingsService;

    public MainWindowViewModel(
        QuickConnectViewModel quickConnect,
        SavedConnectionsViewModel savedConnections,
        SettingsViewModel settings,
        ISettingsService settingsService)
    {
        QuickConnect = quickConnect;
        SavedConnections = savedConnections;
        Settings = settings;
        _settingsService = settingsService;
    }

    public async Task InitializeAsync()
    {
        // Load settings first so language is applied before anything else
        var appSettings = await _settingsService.LoadAsync().ConfigureAwait(false);
        LocalizationService.Instance.ApplyLanguage(appSettings.LanguageCode);

        await Settings.LoadAsync(appSettings).ConfigureAwait(false);
        await SavedConnections.LoadAsync().ConfigureAwait(false);

        if (appSettings.AutoUpdate)
            _ = CheckForUpdatesAsync();
    }

    private static async Task CheckForUpdatesAsync()
    {
        try
        {
            var mgr = new UpdateManager(new GithubSource("https://github.com/WhereTheTimeWent/LanaDelSsh", null, false));
            if (!mgr.IsInstalled)
                return;

            var update = await mgr.CheckForUpdatesAsync().ConfigureAwait(false);
            if (update is null)
                return;

            await mgr.DownloadUpdatesAsync(update).ConfigureAwait(false);
            // Update will be applied automatically on next launch via Velopack's auto-apply-on-startup
        }
        catch (Exception)
        {
            // Silently ignore update errors (no internet, no releases yet, etc.)
        }
    }
}
