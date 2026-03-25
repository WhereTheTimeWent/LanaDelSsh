using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanaDelSsh.Models;

namespace LanaDelSsh.ViewModels;

public partial class ConnectionEditViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [NotifyPropertyChangedFor(nameof(ShowHostError))]
    private string _host = string.Empty;

    [ObservableProperty]
    private int _port = 22;

    private static bool IsValidHost(string host) =>
        host.IndexOf('@') is var i && i >= 0 && i < host.Length - 1;

    public bool IsValid => !string.IsNullOrWhiteSpace(Name) && IsValidHost(Host);

    public bool ShowHostError => !string.IsNullOrWhiteSpace(Host) && !IsValidHost(Host);

    public bool IsEditMode { get; }

    public string DialogTitle => IsEditMode
        ? Localization.Loc.Instance.ConnectionEdit_Title_Edit
        : Localization.Loc.Instance.ConnectionEdit_Title_Add;

    public ConnectionEditViewModel(SshConnection? existing = null)
    {
        if (existing is not null)
        {
            _name = existing.Name;
            _host = existing.Host;
            _port = existing.Port;
            IsEditMode = true;
        }
        else
        {
            IsEditMode = false;
        }
    }

    public SshConnection BuildModel() => new()
    {
        Name = Name.Trim(),
        Host = Host.Trim(),
        Port = Port
    };
}
