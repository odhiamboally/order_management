using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using OM.Application.CQRS.Orders.Queries;
using OM.Application.Dtos.Order;
using OM.Application.Helpers;
using OM.Application.Interfaces.ICommon;
using OM.Domain.Interfaces.ICommon;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.CQRS.Orders.QueryHandlers;


public class GetOrderAnalyticsByDateRangeQueryHandler : IRequestHandler<GetOrderAnalyticsByDateRangeQuery, OrderAnalyticsResponse>
{
    private readonly IServiceManager _serviceManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetOrderAnalyticsByDateRangeQueryHandler> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1); // Longer cache for date ranges

    public GetOrderAnalyticsByDateRangeQueryHandler(
        IServiceManager serviceManager,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<GetOrderAnalyticsByDateRangeQueryHandler> logger)
    {
        _serviceManager = serviceManager ?? throw new ArgumentNullException(nameof(serviceManager));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OrderAnalyticsResponse> Handle(GetOrderAnalyticsByDateRangeQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = GenerateCacheKey(request.StartDate, request.EndDate);

        _logger.LogDebug("Handling GetOrderAnalyticsByDateRangeQuery for {StartDate} to {EndDate}",
            request.StartDate, request.EndDate);

        // Try cache first
        var cachedResult = await _cacheService.GetAsync<OrderAnalyticsResponse>(cacheKey);
        if (cachedResult != null)
        {
            _logger.LogDebug("Date range analytics retrieved from cache");
            return cachedResult;
        }

        // Calculate fresh analytics for date range
        var analytics = await CalculateAnalyticsForDateRangeAsync(request.StartDate, request.EndDate, cancellationToken);
        if (analytics.TotalOrders == 0)
        {
            _logger.LogWarning("No analytics data found for date range {StartDate} to {EndDate}", request.StartDate, request.EndDate);
            return new OrderAnalyticsResponse(0, TimeSpan.Zero, 0, 0, []);
        }

        // Cache the result - longer cache for historical data
        var cacheDuration = IsHistoricalData(request.EndDate) ? TimeSpan.FromDays(1) : CacheDuration;
        await _cacheService.SetAsync(cacheKey, analytics, cacheDuration);

        _logger.LogDebug("Date range analytics cached for {Duration}", cacheDuration);

        return analytics;
    }

    private async Task<OrderAnalyticsResponse> CalculateAnalyticsForDateRangeAsync(DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken ct)
    {
        var (normalizedStart, normalizedEnd) = DateRangeHelper.Normalize(startDate, endDate);

        var orders = await _unitOfWork.OrderRepository.FindAll()
            .Where(o => o.CreatedAt >= normalizedStart && o.CreatedAt < normalizedEnd)
            .ToListAsync(ct);
    
        if (!orders.Any())
        {
            _logger.LogInformation("No orders found for date range {StartDate} to {EndDate}", startDate, endDate);
            return new OrderAnalyticsResponse(0, TimeSpan.Zero, 0, 0, []);
        }


        // Calculate metrics for the filtered data
        var averageOrderValue = orders.Average(o => o.FinalAmount.Amount);
        var fulfilledOrders = orders.Where(o => o.FulfilledAt.HasValue).ToList();
        var averageFulfillmentTime = fulfilledOrders.Any()
            ? TimeSpan.FromTicks((long)fulfilledOrders.Average(o => (o.FulfilledAt!.Value - o.CreatedAt).Ticks))
            : TimeSpan.Zero;

        var ordersByStatus = orders
            .GroupBy(o => o.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var totalRevenue = orders.Sum(o => o.FinalAmount.Amount);

        _logger.LogInformation(
            "Calculated date range analytics: {TotalOrders} orders, {TotalRevenue:C} revenue for {StartDate} to {EndDate}",
            orders.Count, totalRevenue, startDate, endDate);

        return new OrderAnalyticsResponse(
            averageOrderValue,
            averageFulfillmentTime,
            orders.Count,
            totalRevenue,
            ordersByStatus
        );
    }

    private static string GenerateCacheKey(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        return $"order_analytics_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}";
    }

    private static bool IsHistoricalData(DateTimeOffset endDate)
    {
        // Data older than 24 hours is considered historical and can be cached longer
        return endDate < DateTime.UtcNow.AddDays(-1);
    }
}
