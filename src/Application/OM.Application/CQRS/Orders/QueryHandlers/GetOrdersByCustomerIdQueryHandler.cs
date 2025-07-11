using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using OM.Application.CQRS.Orders.Queries;
using OM.Application.Dtos.Order;
using OM.Application.Interfaces.ICommon;
using OM.Domain.Interfaces.ICommon;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.CQRS.Orders.QueryHandlers;


public class GetOrdersByCustomerIdQueryHandler : IRequestHandler<GetOrdersByCustomerIdQuery, List<OrderResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetOrdersByCustomerIdQueryHandler> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public GetOrdersByCustomerIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<GetOrdersByCustomerIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<OrderResponse>> Handle(GetOrdersByCustomerIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = GenerateCacheKey(request.CustomerId);

        _logger.LogDebug("Handling GetOrdersByCustomerIdQuery for customer {CustomerId}", request.CustomerId);

        // Try cache first
        var cachedResult = await _cacheService.GetAsync<List<OrderResponse>>(cacheKey);
        if (cachedResult != null)
        {
            _logger.LogDebug("Retrieved {OrderCount} orders for customer {CustomerId} from cache",
                cachedResult.Count, request.CustomerId);
            return cachedResult;
        }

        _logger.LogDebug("Cache miss - fetching orders for customer {CustomerId} from database", request.CustomerId);

        // Get fresh data
        var orders = await _unitOfWork.OrderRepository.FindAll()
            .Where(o => o.CustomerId == request.CustomerId)
            .OrderByDescending(o => o.CreatedAt) // Most recent first
            .ToListAsync(cancellationToken);

        // Map to response DTOs
        var orderResponses = orders.Select(order => new OrderResponse(
            order.Id,
            order.CustomerId,
            order.Amount.Amount,
            order.DiscountAmount.Amount,
            order.FinalAmount.Amount,
            order.Status,
            order.CreatedAt,
            order.FulfilledAt,
            order.Notes
        )).ToList();

        // Cache the result
        await _cacheService.SetAsync(cacheKey, orderResponses, CacheDuration);
        _logger.LogDebug("Cached {OrderCount} orders for customer {CustomerId} for {Duration} minutes",
            orderResponses.Count, request.CustomerId, CacheDuration.TotalMinutes);

        return orderResponses;
    }

    private static string GenerateCacheKey(int customerId)
    {
        return $"customer_{customerId}_orders";
    }
}
