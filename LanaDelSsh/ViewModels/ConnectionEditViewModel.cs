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

    public bool IsValid => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Host) && Host.Contains('@');

    public bool ShowHostError => !string.IsNullOrWhiteSpace(Host) && !Host.Contains('@');

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
