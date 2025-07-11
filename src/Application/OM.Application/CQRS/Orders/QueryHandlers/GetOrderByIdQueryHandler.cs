using MediatR;

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
public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);


    public GetOrderByIdQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public async Task<OrderResponse> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = GenerateCacheKey(request.Id);

        var cachedResult = await _cacheService.GetAsync<OrderResponse>(cacheKey);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        var order = await _unitOfWork.OrderRepository.FindByIdAsync(request.Id);
        if (order == null)
            throw new ArgumentException($"Order with ID {request.Id} not found");

        await _cacheService.SetAsync(cacheKey, order, CacheDuration);

        return new OrderResponse(
            order.Id,
            order.CustomerId,
            order.Amount.Amount,
            order.DiscountAmount.Amount,
            order.FinalAmount.Amount,
            order.Status,
            order.CreatedAt,
            order.FulfilledAt,
            order.Notes
        );
    }

    private static string GenerateCacheKey(int orderId)
    {
        return $"order_{orderId}";
    }
}
