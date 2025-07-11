using MediatR;

using OM.Application.CQRS.Orders.Commands;
using OM.Application.Interfaces.ICommon;
using OM.Domain.Interfaces.ICommon;
using OM.Domain.Interfaces.IRepositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.CQRS.Orders.CommandHandlers;
public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Unit>
{
    private readonly IServiceManager _serviceManager;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOrderStatusCommandHandler(IServiceManager serviceManager, IUnitOfWork unitOfWork)
    {
        _serviceManager = serviceManager ?? throw new ArgumentNullException(nameof(serviceManager));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    }


    public async Task<Unit> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.OrderRepository.FindByIdAsync(request.OrderId);
        if (order == null)
            throw new ArgumentException("Order not found");

        order.UpdateStatus(request.NewStatus);
        await _unitOfWork.OrderRepository.UpdateAsync(order);

        // Invalidate relevant caches
        await _serviceManager.CacheInvalidationService.InvalidateOrderCacheAsync(order.Id, order.CustomerId);
        await _serviceManager.CacheInvalidationService.InvalidateAnalyticsCacheAsync();

        return Unit.Value;
    }
}
