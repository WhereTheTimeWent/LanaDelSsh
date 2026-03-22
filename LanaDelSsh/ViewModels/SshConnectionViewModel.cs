using CommunityToolkit.Mvvm.ComponentModel;
using LanaDelSsh.Models;
using System;

namespace LanaDelSsh.ViewModels;

/// <summary>
/// Observable wrapper around SshConnection for use in the saved connections list.
/// </summary>
public partial class SshConnectionViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _host;

    [ObservableProperty]
    private int _port;

    private readonly Guid _id;
    public Guid Id => _id;

    public SshConnectionViewModel(SshConnection model)
    {
        _id = model.Id;
        _name = model.Name;
        _host = model.Host;
        _port = model.Port;
    }

    public SshConnection ToModel() => new()
    {
        Id = _id,
        Name = Name,
        Host = Host,
        Port = Port
    };
}
