using OM.Domain.Common;
using OM.Domain.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Domain.Entities;
public class Customer : BaseEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public CustomerSegment Segment { get; set; }
    public int TotalOrders { get; set; }
    public DateTime LastOrderDate { get; set; }

    private readonly List<Order> _orders = new();
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    public void AddOrder(Order order)
    {
        _orders.Add(order);
        TotalOrders++;
        LastOrderDate = DateTime.UtcNow;

        // Business logic for segment upgrade
        UpdateSegmentBasedOnOrderHistory();
    }

    private void UpdateSegmentBasedOnOrderHistory()
    {
        if (TotalOrders >= 10)
            Segment = CustomerSegment.VIP;
        else if (TotalOrders >= 3)
            Segment = CustomerSegment.Regular;
    }
}
