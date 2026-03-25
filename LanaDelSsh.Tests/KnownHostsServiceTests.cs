using LanaDelSsh.Services;
using System;
using System.IO;
using Xunit;

namespace LanaDelSsh.Tests;

public class KnownHostsServiceTests : IDisposable
{
    private readonly string _path;
    private readonly KnownHostsService _sut;

    public KnownHostsServiceTests()
    {
        _path = Path.GetTempFileName();
        _sut = new KnownHostsService(_path);
    }

    public void Dispose() => File.Delete(_path);

    private void Write(string content) => File.WriteAllText(_path, content);
    private string Read() => File.ReadAllText(_path);

    // --- File / input guards ---

    [Fact]
    public void RemoveHost_FileNotExists_ReturnsFalse()
    {
        File.Delete(_path);
        Assert.False(_sut.RemoveHost("10.0.0.1", 22));
    }

    [Fact]
    public void RemoveHost_EmptyHostname_ReturnsFalse()
    {
        Write("10.0.0.1 ssh-ed25519 AAAA\n");
        Assert.False(_sut.RemoveHost("", 22));
        Assert.False(_sut.RemoveHost("   ", 22));
    }

    // --- Match / no match ---

    [Fact]
    public void RemoveHost_HostFound_ReturnsTrueAndRemovesLine()
    {
        Write("10.0.0.1 ssh-ed25519 AAAA\n");
        Assert.True(_sut.RemoveHost("10.0.0.1", 22));
        Assert.DoesNotContain("10.0.0.1", Read());
    }

    [Fact]
    public void RemoveHost_HostNotFound_ReturnsFalse()
    {
        Write("10.0.0.2 ssh-ed25519 AAAA\n");
        Assert.False(_sut.RemoveHost("10.0.0.1", 22));
    }

    [Fact]
    public void RemoveHost_OnlyMatchingLineRemoved_OthersPreserved()
    {
        Write("10.0.0.1 ssh-ed25519 AAAA\n10.0.0.2 ssh-ed25519 BBBB\n");
        _sut.RemoveHost("10.0.0.1", 22);
        var result = Read();
        Assert.DoesNotContain("10.0.0.1", result);
        Assert.Contains("10.0.0.2", result);
    }

    [Fact]
    public void RemoveHost_MatchIsCaseInsensitive()
    {
        Write("MyHost ssh-ed25519 AAAA\n");
        Assert.True(_sut.RemoveHost("myhost", 22));
    }

    // --- Port handling ---

    [Fact]
    public void RemoveHost_NonDefaultPort_MatchesBracketFormat()
    {
        Write("[10.0.0.1]:2222 ssh-ed25519 AAAA\n");
        Assert.True(_sut.RemoveHost("10.0.0.1", 2222));
    }

    [Fact]
    public void RemoveHost_NonDefaultPort_DoesNotMatchPlainFormat()
    {
        Write("10.0.0.1 ssh-ed25519 AAAA\n");
        Assert.False(_sut.RemoveHost("10.0.0.1", 2222));
    }

    [Fact]
    public void RemoveHost_Port22_DoesNotMatchBracketFormat()
    {
        Write("[10.0.0.1]:22 ssh-ed25519 AAAA\n");
        Assert.False(_sut.RemoveHost("10.0.0.1", 22));
    }

    // --- Comma-separated hosts ---

    [Fact]
    public void RemoveHost_CommaSeparated_MatchesFirstEntry()
    {
        Write("10.0.0.1,10.0.0.2 ssh-ed25519 AAAA\n");
        Assert.True(_sut.RemoveHost("10.0.0.1", 22));
    }

    [Fact]
    public void RemoveHost_CommaSeparated_MatchesSecondEntry()
    {
        Write("10.0.0.1,10.0.0.2 ssh-ed25519 AAAA\n");
        Assert.True(_sut.RemoveHost("10.0.0.2", 22));
    }

    // --- Special lines ---

    [Fact]
    public void RemoveHost_CommentLine_IsNotRemoved()
    {
        Write("# 10.0.0.1 ssh-ed25519 AAAA\n");
        Assert.False(_sut.RemoveHost("10.0.0.1", 22));
        Assert.Contains("# 10.0.0.1", Read());
    }

    [Fact]
    public void RemoveHost_BlankLines_ArePreserved()
    {
        Write("\n10.0.0.1 ssh-ed25519 AAAA\n\n");
        _sut.RemoveHost("10.0.0.1", 22);
        Assert.Contains("\n", Read());
    }

    [Fact]
    public void RemoveHost_MalformedLine_IsIgnoredAndPreserved()
    {
        Write("not-a-valid-entry\n10.0.0.1 ssh-ed25519 AAAA\n");
        _sut.RemoveHost("10.0.0.1", 22);
        Assert.Contains("not-a-valid-entry", Read());
    }
}
