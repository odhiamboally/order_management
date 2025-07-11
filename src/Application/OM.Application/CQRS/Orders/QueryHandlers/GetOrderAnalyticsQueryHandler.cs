using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using OM.Application.CQRS.Orders.Queries;
using OM.Application.Dtos.Order;
using OM.Application.Interfaces.ICommon;
using OM.Domain.Interfaces.ICommon;
using OM.Domain.Interfaces.IRepositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.CQRS.Orders.QueryHandlers;
public class GetOrderAnalyticsQueryHandler : IRequestHandler<GetOrderAnalyticsQuery, OrderAnalyticsResponse>
{
    private readonly IServiceManager _serviceManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetOrderAnalyticsQueryHandler> _logger;

    private const string CacheKey = "order_analytics";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public GetOrderAnalyticsQueryHandler(
        IServiceManager serviceManager,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<GetOrderAnalyticsQueryHandler> logger)
    {
        _serviceManager = serviceManager ?? throw new ArgumentNullException(nameof(serviceManager));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OrderAnalyticsResponse> Handle(GetOrderAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var cachedResult = await _cacheService.GetAsync<OrderAnalyticsResponse>(CacheKey);
        if (cachedResult != null)
        {
            _logger.LogDebug("Analytics data retrieved from cache");
            return cachedResult;
        }

        // Calculate fresh analytics
        var analytics = await CalculateAnalyticsAsync(cancellationToken);

        // Cache the result
        await _cacheService.SetAsync(CacheKey, analytics, CacheDuration);
        _logger.LogDebug("Analytics data cached for {Duration} minutes", CacheDuration.TotalMinutes);

        return analytics;
    }

    private async Task<OrderAnalyticsResponse> CalculateAnalyticsAsync(CancellationToken cancellationToken)
    {
        // Get all orders efficiently - consider using a more optimized query for large datasets
        var orders = await _unitOfWork.OrderRepository.FindAll()
            .AsNoTracking() // Performance optimization for read-only operations
            .ToListAsync(cancellationToken);

        // Get aggregated data from repository methods
        var averageOrderValue = await _unitOfWork.OrderRepository.GetAverageOrderValueAsync();
        var averageFulfillmentTime = await _unitOfWork.OrderRepository.GetAverageFulfillmentTimeAsync();

        // Calculate order distribution by status
        var ordersByStatus = orders
            .GroupBy(o => o.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Calculate total revenue
        var totalRevenue = orders.Sum(o => o.FinalAmount.Amount);

        _logger.LogInformation(
            "Calculated analytics: {TotalOrders} orders, {TotalRevenue:C} revenue, {AverageValue:C} average",
            orders.Count, totalRevenue, averageOrderValue);

        return new OrderAnalyticsResponse(
            averageOrderValue,
            averageFulfillmentTime,
            orders.Count,
            totalRevenue,
            ordersByStatus
        );
    }



}
