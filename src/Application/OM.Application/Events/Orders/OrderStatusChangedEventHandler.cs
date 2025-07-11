using MediatR;

using Microsoft.Extensions.Logging;

using OM.Domain.Events;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.Events.Orders;
public class OrderStatusChangedEventHandler : INotificationHandler<OrderStatusChangedEvent>
{
    private readonly ILogger<OrderStatusChangedEventHandler> _logger;

    public OrderStatusChangedEventHandler(ILogger<OrderStatusChangedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Order {OrderId} status changed from {OldStatus} to {NewStatus} at {OccurredOn}",
            notification.OrderId,
            notification.OldStatus,
            notification.NewStatus,
            notification.OccurredOn);

        // Additional business logic can go here:
        // - Send notification emails
        // - Update analytics
        // - Trigger inventory updates
        // - etc.

        await Task.CompletedTask; // Placeholder for async operations
    }
}
