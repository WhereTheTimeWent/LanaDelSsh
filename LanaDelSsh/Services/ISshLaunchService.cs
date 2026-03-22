using LanaDelSsh.Models;

namespace LanaDelSsh.Services;

public interface ISshLaunchService
{
    void Connect(string host, int port, AppSettings settings);
}
