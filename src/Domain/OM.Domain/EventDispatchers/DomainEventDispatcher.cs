using MediatR;

using Microsoft.Extensions.Logging;

using OM.Domain.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Domain.EventDispatchers;
public class DomainEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IMediator mediator, ILogger<DomainEventDispatcher> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task DispatchEventsAsync(List<BaseEntity> entities)
    {
        var domainEvents = entities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        if (!domainEvents.Any())
            return;

        _logger.LogInformation("Dispatching {Count} domain events", domainEvents.Count);

        foreach (var domainEvent in domainEvents)
        {
            try
            {
                await _mediator.Publish(domainEvent);
                _logger.LogDebug("Dispatched domain event {EventType}", domainEvent.GetType().Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching domain event {EventType}", domainEvent.GetType().Name);
                throw;
            }
        }

        // Clear events after dispatching
        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }
    }
}
