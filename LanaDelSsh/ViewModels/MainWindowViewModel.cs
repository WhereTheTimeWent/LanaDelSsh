using CommunityToolkit.Mvvm.ComponentModel;
using LanaDelSsh.Localization;
using LanaDelSsh.Services;
using System.Threading.Tasks;

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
    }
}
