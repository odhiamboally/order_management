using MediatR;

using OM.Application.Dtos.Order;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.CQRS.Orders.Queries;
public record GetOrderAnalyticsQuery() : IRequest<OrderAnalyticsResponse>;
