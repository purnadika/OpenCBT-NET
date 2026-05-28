using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OpenCBT.Application.Interfaces;
using OpenCBT.Domain.Entities;
using OpenCBT.Infrastructure.Data;

namespace OpenCBT.Infrastructure.Services;

public class SystemSettingsService : ISystemSettingsService
{
    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public SystemSettingsService(ApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<string?> GetSettingAsync(string key)
    {
        var cacheKey = $"SystemSetting_{key}";
        var cachedValue = await _cache.GetStringAsync(cacheKey);
        
        if (cachedValue != null)
        {
            return cachedValue;
        }

        var setting = await _context.AppSettings.FindAsync(key);
        if (setting?.Value != null)
        {
            await _cache.SetStringAsync(cacheKey, setting.Value, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });
        }

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

        // Invalidate or update cache
        var cacheKey = $"SystemSetting_{key}";
        await _cache.SetStringAsync(cacheKey, value, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        });
    }
}
