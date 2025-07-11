using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.Dtos.Order;
public record OrderItemRequest(
    string ProductName,
    decimal Price,
    int Quantity
);
