using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LanaDelSsh.Models;

namespace LanaDelSsh.Services;

public interface IConnectionStorageService
{
    Task<List<SshConnection>> LoadAsync();
    Task SaveAsync(List<SshConnection> connections);

    /// <summary>Sets a custom folder. The file 'LanaDelSsh.connections.json' inside that folder will be used. Pass null or empty to use the default location.</summary>
    void SetFolder(string? folder);

    /// <summary>The full path of the currently active connections file.</summary>
    string CurrentFilePath { get; }

    /// <summary>Raised when the connections file is changed externally or the path is switched.</summary>
    event EventHandler? FileChanged;
}
