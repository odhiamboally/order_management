using MediatR;

using Microsoft.EntityFrameworkCore;

using OM.Application.CQRS.Orders.Commands;
using OM.Application.Dtos.Order;
using OM.Application.Interfaces.ICommon;
using OM.Application.Interfaces.IServices;
using OM.Domain.Entities;
using OM.Domain.Exceptions;
using OM.Domain.Interfaces.ICommon;
using OM.Domain.Interfaces.IRepositories;
using OM.Domain.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.CQRS.Orders.CommandHandlers;
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    private readonly IServiceManager _serviceManager;
    private readonly IUnitOfWork _unitOfWork;
    

    public CreateOrderCommandHandler(IServiceManager serviceManager, IUnitOfWork unitOfWork)
    {
        _serviceManager = serviceManager ?? throw new ArgumentNullException(nameof(serviceManager));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    }

    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await _unitOfWork.CustomerRepository.FindByIdAsync(request.CustomerId);
            if (customer == null)
                throw new BadRequestException($"Customer with ID {request.CustomerId} does not exist.");

            Order order = new()
            {
                CustomerId = request.CustomerId,
                Customer = customer,
                Notes = request.Notes
            };

            // Add items to order
            foreach (var itemDto in request.Items)
            {
                var orderItem = new OrderItem
                {
                    ProductName = itemDto.ProductName,
                    Price = new Money(itemDto.Price),
                    Quantity = itemDto.Quantity
                };
                order.AddItem(orderItem);
            }

            // Apply discounts
            var discountAmountResponse = await _serviceManager.DiscountService.CalculateDiscountAsync(order, customer);
            if (!discountAmountResponse.Successful)
                throw new InvalidOperationException($"Failed to calculate discount: {discountAmountResponse.Message}");

            var discountAmount = discountAmountResponse.Data;
            order.ApplyDiscount(new Money(discountAmount));

            // Save order
            var savedOrder = await _unitOfWork.OrderRepository.CreateAsync(order);
            customer.AddOrder(savedOrder);

            // Invalidate relevant caches
            await _serviceManager.CacheInvalidationService.InvalidateOrderCacheAsync(savedOrder.Id, savedOrder.CustomerId);
            await _serviceManager.CacheInvalidationService.InvalidateAnalyticsCacheAsync();


            return new OrderResponse(
                savedOrder.Id,
                savedOrder.CustomerId,
                savedOrder.Amount.Amount,
                savedOrder.DiscountAmount.Amount,
                savedOrder.FinalAmount.Amount,
                savedOrder.Status,
                savedOrder.CreatedAt,
                savedOrder.FulfilledAt,
                savedOrder.Notes
            );
        }
        catch (Exception)
        {

            throw;
        }
        
    }
}
