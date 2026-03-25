using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanaDelSsh.Localization;
using LanaDelSsh.Models;
using LanaDelSsh.Services;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LanaDelSsh.ViewModels;

public partial class QuickConnectViewModel : ViewModelBase
{
    private readonly ISshLaunchService _sshLaunchService;
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHostValid))]
    [NotifyPropertyChangedFor(nameof(ShowValidationError))]
    private string _hostInput = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PortText))]
    private int _port = 22;

    public string PortText
    {
        get => Port.ToString();
        set
        {
            if (int.TryParse(value, out var v) && v >= 1 && v <= 65535)
                Port = v;
        }
    }

    public bool IsHostValid => HostInput.IndexOf('@') is var i && i >= 0 && i < HostInput.Length - 1;
    public bool ShowValidationError => HostInput.Length > 0 && !IsHostValid;

    public QuickConnectViewModel(ISshLaunchService sshLaunchService, ISettingsService settingsService)
    {
        _sshLaunchService = sshLaunchService;
        _settingsService = settingsService;
    }

    [RelayCommand]
    private void ClearHostInput() => HostInput = string.Empty;

    public Process? LastLaunchedProcess { get; private set; }

    public async Task ConnectAsync()
    {
        if (!IsHostValid) return;

        var settings = await _settingsService.LoadAsync().ConfigureAwait(false);
        LastLaunchedProcess = _sshLaunchService.Connect(HostInput, Port, settings);
    }
}
