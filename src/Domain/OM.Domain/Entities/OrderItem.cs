using OM.Domain.Common;
using OM.Domain.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Domain.Entities;
public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public string ProductName { get; set; } = string.Empty;
    public Money Price { get; set; } = Money.Zero;
    public int Quantity { get; set; }
    public Money TotalPrice => Price * Quantity;
}
