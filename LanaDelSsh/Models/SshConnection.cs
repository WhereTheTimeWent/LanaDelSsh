using System;

namespace LanaDelSsh.Models;

public class SshConnection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 22;

    public SshConnection Clone() => new()
    {
        Id = Guid.NewGuid(),
        Name = Name,
        Host = Host,
        Port = Port
    };
}
