using MediatR;

using OM.Domain.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.CQRS.Orders.Commands;
public record UpdateOrderStatusCommand(
    int OrderId,
    OrderStatus NewStatus
) : IRequest<Unit>;
