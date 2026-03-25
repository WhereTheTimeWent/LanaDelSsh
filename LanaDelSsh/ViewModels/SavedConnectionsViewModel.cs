using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanaDelSsh.Models;
using LanaDelSsh.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LanaDelSsh.ViewModels;

public partial class SavedConnectionsViewModel : ViewModelBase
{
    private readonly IConnectionStorageService _storageService;
    private readonly ISshLaunchService _sshLaunchService;
    private readonly IPingService _pingService;
    private readonly ISettingsService _settingsService;
    private readonly IKnownHostsService _knownHostsService;

    public ObservableCollection<SshConnectionViewModel> Connections { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    private SshConnectionViewModel? _selectedConnection;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredConnections))]
    private string _searchText = string.Empty;

    [RelayCommand]
    private void ClearSearch() => SearchText = string.Empty;

    public IEnumerable<SshConnectionViewModel> FilteredConnections =>
        string.IsNullOrWhiteSpace(SearchText)
            ? Connections
            : Connections.Where(c =>
                c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                c.Host.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

    public bool HasSelection => SelectedConnection is not null;

    public SavedConnectionsViewModel(
        IConnectionStorageService storageService,
        ISshLaunchService sshLaunchService,
        IPingService pingService,
        ISettingsService settingsService,
        IKnownHostsService knownHostsService)
    {
        _storageService = storageService;
        _sshLaunchService = sshLaunchService;
        _pingService = pingService;
        _settingsService = settingsService;
        _knownHostsService = knownHostsService;
        storageService.FileChanged += OnStorageFileChanged;
        Connections.CollectionChanged += (_, _) => OnPropertyChanged(nameof(FilteredConnections));
    }

    private void OnStorageFileChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(LoadAsync);
    }

    public async Task LoadAsync()
    {
        var models = await _storageService.LoadAsync().ConfigureAwait(false);
        Connections.Clear();
        foreach (var model in models)
            Connections.Add(new SshConnectionViewModel(model));
    }

    private async Task PersistAsync()
    {
        var models = Connections.Select(c => c.ToModel()).ToList();
        await _storageService.SaveAsync(models).ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (SelectedConnection is null) return;
        var settings = await _settingsService.LoadAsync().ConfigureAwait(false);
        _sshLaunchService.Connect(SelectedConnection.Host, SelectedConnection.Port, settings);
    }

    [RelayCommand]
    private void Ping()
    {
        if (SelectedConnection is null) return;
        _pingService.Ping(SelectedConnection.Host);
    }

    public bool RemoveSelectedFromKnownHosts()
    {
        if (SelectedConnection is null) return false;
        var host = SelectedConnection.Host;
        var atIndex = host.IndexOf('@');
        var hostname = atIndex >= 0 ? host[(atIndex + 1)..] : host;
        return _knownHostsService.RemoveHost(hostname, SelectedConnection.Port);
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedConnection is null) return;
        var index = Connections.IndexOf(SelectedConnection);
        Connections.Remove(SelectedConnection);
        SelectedConnection = Connections.Count > 0
            ? Connections[System.Math.Min(index, Connections.Count - 1)]
            : null;
        await PersistAsync().ConfigureAwait(false);
    }

    /// <summary>Called by the view after add/edit dialog returns a model.</summary>
    public async Task AddConnectionAsync(SshConnection model)
    {
        var vm = new SshConnectionViewModel(model);
        Connections.Add(vm);
        SelectedConnection = vm;
        await PersistAsync().ConfigureAwait(false);
    }

    /// <summary>Called by the view after editing an existing connection.</summary>
    public async Task UpdateConnectionAsync(SshConnectionViewModel vm, SshConnection updated)
    {
        vm.Name = updated.Name;
        vm.Host = updated.Host;
        vm.Port = updated.Port;
        await PersistAsync().ConfigureAwait(false);
    }

    /// <summary>Moves an item from one index to another (drag-and-drop support).</summary>
    public async Task MoveItemAsync(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || toIndex < 0 || fromIndex == toIndex) return;
        if (fromIndex >= Connections.Count || toIndex >= Connections.Count) return;

        var item = Connections[fromIndex];
        Connections.RemoveAt(fromIndex);
        Connections.Insert(toIndex, item);
        await PersistAsync().ConfigureAwait(false);
    }
}
