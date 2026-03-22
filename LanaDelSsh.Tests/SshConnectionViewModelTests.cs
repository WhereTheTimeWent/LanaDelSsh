using LanaDelSsh.Models;
using LanaDelSsh.ViewModels;
using System;
using Xunit;

namespace LanaDelSsh.Tests;

public class SshConnectionViewModelTests
{
    [Fact]
    public void ToModel_ReturnsCorrectValues()
    {
        var model = new SshConnection { Name = "dev", Host = "user@dev.host", Port = 2222 };
        var vm = new SshConnectionViewModel(model);
        var result = vm.ToModel();
        Assert.Equal("dev", result.Name);
        Assert.Equal("user@dev.host", result.Host);
        Assert.Equal(2222, result.Port);
    }

    [Fact]
    public void ToModel_PreservesId()
    {
        var model = new SshConnection { Name = "x", Host = "a@b", Port = 22 };
        var vm = new SshConnectionViewModel(model);
        Assert.Equal(model.Id, vm.ToModel().Id);
    }

    [Fact]
    public void ToModel_ReflectsUpdatedProperties()
    {
        var model = new SshConnection { Name = "old", Host = "user@old", Port = 22 };
        var vm = new SshConnectionViewModel(model) { Name = "new", Host = "user@new", Port = 8022 };
        var result = vm.ToModel();
        Assert.Equal("new", result.Name);
        Assert.Equal("user@new", result.Host);
        Assert.Equal(8022, result.Port);
    }
}
