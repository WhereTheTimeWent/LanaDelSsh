using LanaDelSsh.Models;
using Xunit;

namespace LanaDelSsh.Tests;

public class SshConnectionTests
{
    [Fact]
    public void Clone_AssignsNewId()
    {
        var original = new SshConnection { Name = "test", Host = "user@host", Port = 22 };
        var clone = original.Clone();
        Assert.NotEqual(original.Id, clone.Id);
    }

    [Fact]
    public void Clone_CopiesProperties()
    {
        var original = new SshConnection { Name = "prod", Host = "admin@10.0.0.1", Port = 2222 };
        var clone = original.Clone();
        Assert.Equal(original.Name, clone.Name);
        Assert.Equal(original.Host, clone.Host);
        Assert.Equal(original.Port, clone.Port);
    }
}
