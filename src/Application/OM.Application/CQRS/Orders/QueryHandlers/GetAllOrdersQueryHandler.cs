using MediatR;

using Microsoft.EntityFrameworkCore;

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
public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, List<OrderResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    private const string CacheKey = "all_orders";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    public GetAllOrdersQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public async Task<List<OrderResponse>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var cachedResult = await _cacheService.GetAsync<List<OrderResponse>>(CacheKey);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        var orders = await _unitOfWork.OrderRepository.FindAll().ToListAsync();

        return orders.Select(order => new OrderResponse(
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

        await _cacheService.SetAsync(CacheKey, orders, CacheDuration);
    }
}
