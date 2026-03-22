using LanaDelSsh.Models;
using LanaDelSsh.ViewModels;
using Xunit;

namespace LanaDelSsh.Tests;

public class ConnectionEditViewModelTests
{
    // --- IsValid ---

    [Fact]
    public void IsValid_FalseWhenNameEmpty()
    {
        var vm = new ConnectionEditViewModel { Host = "user@host" };
        Assert.False(vm.IsValid);
    }

    [Fact]
    public void IsValid_FalseWhenHostEmpty()
    {
        var vm = new ConnectionEditViewModel { Name = "prod" };
        Assert.False(vm.IsValid);
    }

    [Fact]
    public void IsValid_FalseWhenHostMissingAt()
    {
        var vm = new ConnectionEditViewModel { Name = "prod", Host = "justhost" };
        Assert.False(vm.IsValid);
    }

    [Fact]
    public void IsValid_TrueWhenNameAndHostWithAt()
    {
        var vm = new ConnectionEditViewModel { Name = "prod", Host = "user@host" };
        Assert.True(vm.IsValid);
    }

    // --- ShowHostError ---

    [Fact]
    public void ShowHostError_TrueWhenHostNonEmptyWithoutAt()
    {
        var vm = new ConnectionEditViewModel { Host = "justhost" };
        Assert.True(vm.ShowHostError);
    }

    [Fact]
    public void ShowHostError_FalseWhenHostEmpty()
    {
        var vm = new ConnectionEditViewModel();
        Assert.False(vm.ShowHostError);
    }

    [Fact]
    public void ShowHostError_FalseWhenHostContainsAt()
    {
        var vm = new ConnectionEditViewModel { Host = "user@host" };
        Assert.False(vm.ShowHostError);
    }

    // --- BuildModel ---

    [Fact]
    public void BuildModel_TrimsNameAndHost()
    {
        var vm = new ConnectionEditViewModel { Name = "  prod  ", Host = "  user@host  ", Port = 22 };
        var result = vm.BuildModel();
        Assert.Equal("prod", result.Name);
        Assert.Equal("user@host", result.Host);
    }

    [Fact]
    public void BuildModel_PreservesPort()
    {
        var vm = new ConnectionEditViewModel { Name = "n", Host = "u@h", Port = 2222 };
        Assert.Equal(2222, vm.BuildModel().Port);
    }

    // --- Constructor / IsEditMode ---

    [Fact]
    public void Constructor_NoExisting_IsEditModeFalse()
    {
        var vm = new ConnectionEditViewModel();
        Assert.False(vm.IsEditMode);
    }

    [Fact]
    public void Constructor_WithExisting_IsEditModeTrue()
    {
        var vm = new ConnectionEditViewModel(new SshConnection { Name = "x", Host = "u@h", Port = 22 });
        Assert.True(vm.IsEditMode);
    }

    [Fact]
    public void Constructor_WithExisting_PopulatesFields()
    {
        var existing = new SshConnection { Name = "prod", Host = "admin@10.0.0.1", Port = 2222 };
        var vm = new ConnectionEditViewModel(existing);
        Assert.Equal("prod", vm.Name);
        Assert.Equal("admin@10.0.0.1", vm.Host);
        Assert.Equal(2222, vm.Port);
    }
}
