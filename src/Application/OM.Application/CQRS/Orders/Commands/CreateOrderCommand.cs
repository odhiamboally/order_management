using MediatR;

using OM.Application.Dtos.Order;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.CQRS.Orders.Commands;
public record CreateOrderCommand(
    int CustomerId,
    List<OrderItemResponse> Items,
    string Notes = ""
) : IRequest<OrderResponse>;
