using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using OM.Application.Interfaces.ICommon;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.Implementations.Common;
internal class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<InMemoryCacheService> _logger;

    public InMemoryCacheService(IMemoryCache cache, ILogger<InMemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public T? Get<T>(string key)
    {
        return _cache.TryGetValue(key, out T? value) ? value : default!;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        _logger.LogDebug("Getting cache value for key: {Key}", key);

        if (_cache.TryGetValue(key, out var value) && value is T typedValue)
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return Task.FromResult<T?>(typedValue);
        }

        _logger.LogDebug("Cache miss for key: {Key}", key);
        return Task.FromResult<T?>(default);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }

    public Task RemoveAsync(string key)
    {
        _logger.LogDebug("Removing cache value for key: {Key}", key);
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public void Set<T>(string key, T value, TimeSpan expiration)
    {
        _cache.Set(key, value, expiration);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration)
    {
        _logger.LogDebug("Setting cache value for key: {Key}", key);

        var options = new MemoryCacheEntryOptions();

        if (expiration.HasValue)
        {
            options.SetAbsoluteExpiration(expiration.Value);
        }
        else
        {
            options.SetSlidingExpiration(TimeSpan.FromMinutes(30));
        }

        _cache.Set(key, value, options);
        return Task.CompletedTask;
    }
}
