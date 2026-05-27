using Microsoft.EntityFrameworkCore;
using OpenCBT.Application.Interfaces;
using OpenCBT.Domain.Entities;
using OpenCBT.Infrastructure.Data;

namespace OpenCBT.Infrastructure.Services;

public class SystemSettingsService : ISystemSettingsService
{
    private readonly ApplicationDbContext _context;

    public SystemSettingsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetSettingAsync(string key)
    {
        var setting = await _context.AppSettings.FindAsync(key);
        return setting?.Value;
    }

    public async Task SetSettingAsync(string key, string value, string? description = null)
    {
        var setting = await _context.AppSettings.FindAsync(key);
        if (setting == null)
        {
            _context.AppSettings.Add(new AppSetting
            {
                Key = key,
                Value = value,
                Description = description
            });
        }
        else
        {
            setting.Value = value;
            if (description != null) setting.Description = description;
        }

        await _context.SaveChangesAsync();
    }
}
