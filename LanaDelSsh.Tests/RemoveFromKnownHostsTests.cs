using LanaDelSsh.Models;
using LanaDelSsh.Services;
using LanaDelSsh.ViewModels;
using NSubstitute;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace LanaDelSsh.Tests;

public class RemoveFromKnownHostsTests
{
    private static (SavedConnectionsViewModel vm, IKnownHostsService knownHosts) CreateVm()
    {
        var storage = Substitute.For<IConnectionStorageService>();
        storage.SaveAsync(Arg.Any<List<SshConnection>>()).Returns(Task.CompletedTask);
        var knownHosts = Substitute.For<IKnownHostsService>();
        var vm = new SavedConnectionsViewModel(
            storage,
            Substitute.For<ISshLaunchService>(),
            Substitute.For<IPingService>(),
            Substitute.For<ISettingsService>(),
            knownHosts);
        return (vm, knownHosts);
    }

    private static SshConnectionViewModel Conn(string host, int port = 22) =>
        new(new SshConnection { Name = "test", Host = host, Port = port });

    [Fact]
    public void RemoveSelectedFromKnownHosts_ExtractsHostnameAfterAt()
    {
        var (vm, knownHosts) = CreateVm();
        vm.Connections.Add(Conn("root@10.0.1.5"));
        vm.SelectedConnection = vm.Connections[0];

        vm.RemoveSelectedFromKnownHosts();

        knownHosts.Received(1).RemoveHost("10.0.1.5", 22);
    }

    [Fact]
    public void RemoveSelectedFromKnownHosts_NoAtSign_UsesFullHost()
    {
        var (vm, knownHosts) = CreateVm();
        vm.Connections.Add(Conn("10.0.1.5"));
        vm.SelectedConnection = vm.Connections[0];

        vm.RemoveSelectedFromKnownHosts();

        knownHosts.Received(1).RemoveHost("10.0.1.5", 22);
    }

    [Fact]
    public void RemoveSelectedFromKnownHosts_PassesPortToService()
    {
        var (vm, knownHosts) = CreateVm();
        vm.Connections.Add(Conn("root@10.0.1.5", port: 2222));
        vm.SelectedConnection = vm.Connections[0];

        vm.RemoveSelectedFromKnownHosts();

        knownHosts.Received(1).RemoveHost("10.0.1.5", 2222);
    }

    [Fact]
    public void RemoveSelectedFromKnownHosts_NoSelection_ReturnsFalseWithoutCallingService()
    {
        var (vm, knownHosts) = CreateVm();

        var result = vm.RemoveSelectedFromKnownHosts();

        Assert.False(result);
        knownHosts.DidNotReceive().RemoveHost(Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public void RemoveSelectedFromKnownHosts_ReturnsServiceResult()
    {
        var (vm, knownHosts) = CreateVm();
        vm.Connections.Add(Conn("root@10.0.1.5"));
        vm.SelectedConnection = vm.Connections[0];
        knownHosts.RemoveHost(Arg.Any<string>(), Arg.Any<int>()).Returns(true);

        Assert.True(vm.RemoveSelectedFromKnownHosts());

        knownHosts.RemoveHost(Arg.Any<string>(), Arg.Any<int>()).Returns(false);
        Assert.False(vm.RemoveSelectedFromKnownHosts());
    }
}
