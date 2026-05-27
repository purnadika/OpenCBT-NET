namespace OpenCBT.Application.Interfaces;

public interface ISystemSettingsService
{
    Task<string?> GetSettingAsync(string key);
    Task SetSettingAsync(string key, string value, string? description = null);
}
