namespace LanaDelSsh.Services;

public interface IPingService
{
    /// <summary>
    /// Extracts the hostname/IP from a user@host string and launches ping in a terminal.
    /// </summary>
    void Ping(string host);
}
