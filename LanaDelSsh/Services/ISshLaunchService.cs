using LanaDelSsh.Models;
using System.Diagnostics;

namespace LanaDelSsh.Services;

public interface ISshLaunchService
{
    Process? Connect(string host, int port, AppSettings settings);
    void ConnectKeepOpen(string host, int port, AppSettings settings);
}
