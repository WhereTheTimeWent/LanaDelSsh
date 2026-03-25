namespace LanaDelSsh.Services;

public interface IKnownHostsService
{
    bool RemoveHost(string hostname, int port);
}
