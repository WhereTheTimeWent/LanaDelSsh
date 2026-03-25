using LanaDelSsh.Services;
using LanaDelSsh.ViewModels;
using NSubstitute;
using Xunit;

namespace LanaDelSsh.Tests;

public class QuickConnectViewModelTests
{
    private static QuickConnectViewModel Create() => new(
        Substitute.For<ISshLaunchService>(),
        Substitute.For<ISettingsService>());

    // --- IsHostValid ---

    [Fact]
    public void IsHostValid_TrueWhenContainsAt()
    {
        var vm = Create();
        vm.HostInput = "user@host";
        Assert.True(vm.IsHostValid);
    }

    [Fact]
    public void IsHostValid_FalseWhenAtWithNothingAfter()
    {
        var vm = Create();
        vm.HostInput = "root@";
        Assert.False(vm.IsHostValid);
    }

    [Fact]
    public void IsHostValid_FalseWhenNoAt()
    {
        var vm = Create();
        vm.HostInput = "justhost";
        Assert.False(vm.IsHostValid);
    }

    [Fact]
    public void IsHostValid_FalseWhenEmpty()
    {
        var vm = Create();
        Assert.False(vm.IsHostValid);
    }

    // --- ShowValidationError ---

    [Fact]
    public void ShowValidationError_TrueWhenNonEmptyWithoutAt()
    {
        var vm = Create();
        vm.HostInput = "justhost";
        Assert.True(vm.ShowValidationError);
    }

    [Fact]
    public void ShowValidationError_FalseWhenEmpty()
    {
        var vm = Create();
        Assert.False(vm.ShowValidationError);
    }

    [Fact]
    public void ShowValidationError_FalseWhenValid()
    {
        var vm = Create();
        vm.HostInput = "user@host";
        Assert.False(vm.ShowValidationError);
    }

    [Fact]
    public void ShowValidationError_TrueWhenAtWithNothingAfter()
    {
        var vm = Create();
        vm.HostInput = "root@";
        Assert.True(vm.ShowValidationError);
    }

    // --- PortText ---

    [Fact]
    public void PortText_ValidValueUpdatesPort()
    {
        var vm = Create();
        vm.PortText = "8022";
        Assert.Equal(8022, vm.Port);
    }

    [Fact]
    public void PortText_NonNumericDoesNotChangePort()
    {
        var vm = Create();
        vm.PortText = "abc";
        Assert.Equal(22, vm.Port);
    }

    [Fact]
    public void PortText_ZeroDoesNotChangePort()
    {
        var vm = Create();
        vm.PortText = "0";
        Assert.Equal(22, vm.Port);
    }

    [Fact]
    public void PortText_AboveMaxDoesNotChangePort()
    {
        var vm = Create();
        vm.PortText = "65536";
        Assert.Equal(22, vm.Port);
    }

    [Fact]
    public void PortText_BoundaryValuesAreAccepted()
    {
        var vm = Create();
        vm.PortText = "1";
        Assert.Equal(1, vm.Port);
        vm.PortText = "65535";
        Assert.Equal(65535, vm.Port);
    }

    // --- ClearHostInput ---

    [Fact]
    public void ClearHostInput_ClearsInput()
    {
        var vm = Create();
        vm.HostInput = "user@host";
        vm.ClearHostInputCommand.Execute(null);
        Assert.Equal(string.Empty, vm.HostInput);
    }
}
