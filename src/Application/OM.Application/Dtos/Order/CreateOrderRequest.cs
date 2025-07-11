using OM.Domain.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.Dtos.Order;
public record CreateOrderRequest(
    int Id,
    int CustomerId,
    decimal Amount,
    decimal DiscountAmount,
    decimal FinalAmount,
    OrderStatus Status,
    DateTime CreatedAt,
    DateTime? FulfilledAt,
    string Notes
);