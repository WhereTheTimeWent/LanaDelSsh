using LanaDelSsh.Models;
using System.Threading.Tasks;

namespace LanaDelSsh.Services;

public interface ISettingsService
{
    Task<AppSettings> LoadAsync();
    Task SaveAsync(AppSettings settings);
}
