using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.Dtos.Order;
public record OrderItemResponse(
    string ProductName,
    decimal Price,
    int Quantity
);

