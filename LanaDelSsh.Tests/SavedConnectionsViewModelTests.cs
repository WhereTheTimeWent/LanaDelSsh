using LanaDelSsh.Models;
using LanaDelSsh.Services;
using LanaDelSsh.ViewModels;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LanaDelSsh.Tests;

public class SavedConnectionsViewModelTests
{
    private static SavedConnectionsViewModel CreateVm(IConnectionStorageService? storage = null)
    {
        storage ??= Substitute.For<IConnectionStorageService>();
        storage.SaveAsync(Arg.Any<List<SshConnection>>()).Returns(Task.CompletedTask);
        return new SavedConnectionsViewModel(
            storage,
            Substitute.For<ISshLaunchService>(),
            Substitute.For<IPingService>(),
            Substitute.For<ISettingsService>());
    }

    private static SshConnectionViewModel Conn(string name, string host = "user@host") =>
        new(new SshConnection { Name = name, Host = host, Port = 22 });

    // --- FilteredConnections ---

    [Fact]
    public void FilteredConnections_EmptySearch_ReturnsAll()
    {
        var vm = CreateVm();
        vm.Connections.Add(Conn("alpha"));
        vm.Connections.Add(Conn("beta"));
        Assert.Equal(2, vm.FilteredConnections.Count());
    }

    [Fact]
    public void FilteredConnections_MatchesByName()
    {
        var vm = CreateVm();
        vm.Connections.Add(Conn("production"));
        vm.Connections.Add(Conn("staging"));
        vm.SearchText = "prod";
        var results = vm.FilteredConnections.ToList();
        Assert.Single(results);
        Assert.Equal("production", results[0].Name);
    }

    [Fact]
    public void FilteredConnections_MatchesByHost()
    {
        var vm = CreateVm();
        vm.Connections.Add(Conn("server1", "admin@10.0.0.1"));
        vm.Connections.Add(Conn("server2", "admin@10.0.0.2"));
        vm.SearchText = "10.0.0.1";
        Assert.Single(vm.FilteredConnections);
    }

    [Fact]
    public void FilteredConnections_IsCaseInsensitive()
    {
        var vm = CreateVm();
        vm.Connections.Add(Conn("Production"));
        vm.SearchText = "PRODUCTION";
        Assert.Single(vm.FilteredConnections);
    }

    [Fact]
    public void FilteredConnections_NoMatch_ReturnsEmpty()
    {
        var vm = CreateVm();
        vm.Connections.Add(Conn("alpha"));
        vm.SearchText = "zzz";
        Assert.Empty(vm.FilteredConnections);
    }

    [Fact]
    public void FilteredConnections_UpdatesWhenConnectionAdded()
    {
        var vm = CreateVm();
        vm.Connections.Add(Conn("alpha"));
        vm.SearchText = "beta";
        Assert.Empty(vm.FilteredConnections);
        vm.Connections.Add(Conn("beta"));
        Assert.Single(vm.FilteredConnections);
    }

    // --- HasSelection ---

    [Fact]
    public void HasSelection_FalseWhenNone()
    {
        var vm = CreateVm();
        Assert.False(vm.HasSelection);
    }

    [Fact]
    public void HasSelection_TrueWhenSet()
    {
        var vm = CreateVm();
        var conn = Conn("x");
        vm.Connections.Add(conn);
        vm.SelectedConnection = conn;
        Assert.True(vm.HasSelection);
    }

    // --- MoveItemAsync ---

    [Fact]
    public async Task MoveItemAsync_MovesItemToNewIndex()
    {
        var vm = CreateVm();
        vm.Connections.Add(Conn("a"));
        vm.Connections.Add(Conn("b"));
        vm.Connections.Add(Conn("c"));
        await vm.MoveItemAsync(0, 2);
        Assert.Equal("b", vm.Connections[0].Name);
        Assert.Equal("c", vm.Connections[1].Name);
        Assert.Equal("a", vm.Connections[2].Name);
    }

    [Fact]
    public async Task MoveItemAsync_SameIndex_DoesNothing()
    {
        var vm = CreateVm();
        vm.Connections.Add(Conn("a"));
        vm.Connections.Add(Conn("b"));
        await vm.MoveItemAsync(0, 0);
        Assert.Equal("a", vm.Connections[0].Name);
        Assert.Equal("b", vm.Connections[1].Name);
    }

    [Fact]
    public async Task MoveItemAsync_OutOfBounds_DoesNothing()
    {
        var vm = CreateVm();
        vm.Connections.Add(Conn("a"));
        await vm.MoveItemAsync(0, 5);
        Assert.Single(vm.Connections);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_RemovesSelectedItem()
    {
        var vm = CreateVm();
        var conn = Conn("x");
        vm.Connections.Add(conn);
        vm.SelectedConnection = conn;
        await vm.DeleteCommand.ExecuteAsync(null);
        Assert.Empty(vm.Connections);
    }

    [Fact]
    public async Task DeleteAsync_SelectsNextItemAfterDeletion()
    {
        var vm = CreateVm();
        var a = Conn("a");
        var b = Conn("b");
        var c = Conn("c");
        vm.Connections.Add(a);
        vm.Connections.Add(b);
        vm.Connections.Add(c);
        vm.SelectedConnection = b;
        await vm.DeleteCommand.ExecuteAsync(null);
        Assert.Equal(c, vm.SelectedConnection);
    }

    [Fact]
    public async Task DeleteAsync_SelectsNullWhenLastItemDeleted()
    {
        var vm = CreateVm();
        var conn = Conn("only");
        vm.Connections.Add(conn);
        vm.SelectedConnection = conn;
        await vm.DeleteCommand.ExecuteAsync(null);
        Assert.Null(vm.SelectedConnection);
    }
}
