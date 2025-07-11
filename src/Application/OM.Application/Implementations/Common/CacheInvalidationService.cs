using Microsoft.Extensions.Logging;

using OM.Application.Interfaces.ICommon;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.Implementations.Common;


internal sealed class CacheInvalidationService : ICacheInvalidationService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheInvalidationService> _logger;

    public CacheInvalidationService(ICacheService cacheService, ILogger<CacheInvalidationService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task InvalidateOrderCacheAsync(int orderId, int customerId)
    {
        var tasks = new List<Task>
        {
            _cacheService.RemoveAsync($"order_{orderId}"),
            _cacheService.RemoveAsync($"customer_{customerId}_orders"),
            _cacheService.RemoveAsync("all_orders")
        };

        await Task.WhenAll(tasks);

        _logger.LogDebug("Invalidated cache for order {OrderId} and customer {CustomerId}", orderId, customerId);
    }

    public async Task InvalidateAllOrdersCacheAsync()
    {
        await _cacheService.RemoveAsync("all_orders");
        _logger.LogDebug("Invalidated all orders cache");
    }

    public async Task InvalidateAnalyticsCacheAsync()
    {
        var tasks = new List<Task>
        {
            _cacheService.RemoveAsync("order_analytics"),
            // Could also remove pattern-based analytics cache keys here
            // _cacheService.RemoveByPatternAsync("order_analytics_*")
        };

        await Task.WhenAll(tasks);

        _logger.LogDebug("Invalidated analytics cache");
    }
}