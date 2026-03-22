using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using LanaDelSsh.Localization;
using LanaDelSsh.Models;
using LanaDelSsh.Services;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace LanaDelSsh.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private bool _isLoading;
    private AppSettings _cachedSettings = new();

    [ObservableProperty]
    private bool _ignoreCertificates;

    /// <summary>0 = Auto, 1 = English, 2 = German</summary>
    [ObservableProperty]
    private int _selectedLanguageIndex;

    /// <summary>0 = Auto, 1 = Light, 2 = Dark, 3 = Dark: Did you know that there's a tunnel under Ocean Blvd</summary>
    [ObservableProperty]
    private int _selectedThemeIndex;

    [ObservableProperty]
    private string? _linuxTerminal;

    [ObservableProperty]
    private string? _connectionsFolder;

    [ObservableProperty]
    private string? _connectionsFolderError;

    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public string CurrentConnectionsFilePath => _connectionStorageService.CurrentFilePath;

    private readonly IConnectionStorageService _connectionStorageService;

    public SettingsViewModel(ISettingsService settingsService, IConnectionStorageService connectionStorageService)
    {
        _settingsService = settingsService;
        _connectionStorageService = connectionStorageService;
    }

    public async Task LoadAsync(AppSettings settings)
    {
        _cachedSettings = settings;
        _isLoading = true;
        IgnoreCertificates = settings.IgnoreCertificates;
        LinuxTerminal = settings.LinuxTerminal;
        ConnectionsFolder = settings.ConnectionsFolder;
        SelectedLanguageIndex = settings.LanguageCode switch
        {
            "en" => 1,
            "de" => 2,
            _ => 0
        };
        SelectedThemeIndex = settings.ThemeVariant switch
        {
            "light"                 => 1,
            "dark"                  => 2,
            DesignService.OceanBlvd => 3,
            _                       => 0
        };
        ApplyTheme(SelectedThemeIndex);
        _isLoading = false;
    }

    partial void OnIgnoreCertificatesChanged(bool value) => _ = SaveAsync();
    partial void OnSelectedLanguageIndexChanged(int value) => _ = ApplyLanguageAndSaveAsync(value);
    partial void OnSelectedThemeIndexChanged(int value) => _ = ApplyThemeAndSaveAsync(value);
    partial void OnLinuxTerminalChanged(string? value) => _ = SaveAsync();
    partial void OnConnectionsFolderChanged(string? value)
    {
        if (_isLoading) return;
        ConnectionsFolderError = null;
        if (string.IsNullOrWhiteSpace(value))
            _ = MoveToDefaultFolderAsync();
        else if (Directory.Exists(value))
        {
            _connectionStorageService.SetFolder(value);
            _ = SaveAsync();
        }
        else
        {
            ConnectionsFolderError = LocalizationService.Instance.Get("Settings_ConnectionsFolder_InvalidPath");
        }
    }

    [CommunityToolkit.Mvvm.Input.RelayCommand]
    private void ClearConnectionsFolder() => ConnectionsFolder = string.Empty;

    /// <summary>Resets an invalid folder input back to the last saved value. Call before reopening the settings dialog.</summary>
    public void ResetInvalidFolderInput()
    {
        if (ConnectionsFolderError is null) return;
        _isLoading = true;
        ConnectionsFolder = _cachedSettings.ConnectionsFolder;
        _isLoading = false;
        ConnectionsFolderError = null;
    }

    private async Task MoveToDefaultFolderAsync()
    {
        var currentPath = _connectionStorageService.CurrentFilePath;
        var defaultPath = Path.Combine(ConnectionStorageService.DefaultFolder, ConnectionStorageService.FileName);

        if (currentPath != defaultPath && File.Exists(currentPath))
        {
            Directory.CreateDirectory(ConnectionStorageService.DefaultFolder);
            File.Move(currentPath, defaultPath, overwrite: true);
        }

        _connectionStorageService.SetFolder(null);
        await SaveAsync().ConfigureAwait(false);
    }

    private async Task ApplyLanguageAndSaveAsync(int index)
    {
        if (_isLoading) return;
        var code = index switch
        {
            1 => "en",
            2 => "de",
            _ => null
        };
        LocalizationService.Instance.ApplyLanguage(code);
        await SaveAsync().ConfigureAwait(false);
    }

    private async Task ApplyThemeAndSaveAsync(int index)
    {
        if (_isLoading) return;
        ApplyTheme(index);
        await SaveAsync().ConfigureAwait(false);
    }

    private static void ApplyTheme(int index)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (Application.Current is null) return;
            if (index == 3)
            {
                DesignService.Apply(DesignService.OceanBlvd, Application.Current);
            }
            else
            {
                DesignService.Apply(null, Application.Current);
                Application.Current.RequestedThemeVariant = index switch
                {
                    1 => ThemeVariant.Light,
                    2 => ThemeVariant.Dark,
                    _ => ThemeVariant.Default
                };
            }
        });
    }

    private async Task SaveAsync()
    {
        if (_isLoading) return;
        _cachedSettings.IgnoreCertificates = IgnoreCertificates;
        _cachedSettings.LanguageCode = SelectedLanguageIndex switch
        {
            1 => "en",
            2 => "de",
            _ => null
        };
        _cachedSettings.ThemeVariant = SelectedThemeIndex switch
        {
            1 => "light",
            2 => "dark",
            3 => DesignService.OceanBlvd,
            _ => null
        };
        _cachedSettings.LinuxTerminal = string.IsNullOrWhiteSpace(LinuxTerminal) ? null : LinuxTerminal;
        _cachedSettings.ConnectionsFolder = string.IsNullOrWhiteSpace(ConnectionsFolder) ? null : ConnectionsFolder;
        await _settingsService.SaveAsync(_cachedSettings).ConfigureAwait(false);
    }

    public async Task SaveWindowSizeAsync(double width, double height)
    {
        _cachedSettings.WindowWidth = width;
        _cachedSettings.WindowHeight = height;
        await _settingsService.SaveAsync(_cachedSettings).ConfigureAwait(false);
    }

    public (double Width, double Height) GetSavedWindowSize() =>
        (_cachedSettings.WindowWidth, _cachedSettings.WindowHeight);

    public async Task ResetWindowSizeAsync()
    {
        _cachedSettings.WindowWidth = 550;
        _cachedSettings.WindowHeight = 620;
        await _settingsService.SaveAsync(_cachedSettings).ConfigureAwait(false);
    }
}
