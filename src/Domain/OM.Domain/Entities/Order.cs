using OM.Domain.Common;
using OM.Domain.Enums;
using OM.Domain.Events;
using OM.Domain.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Domain.Entities;
public class Order : BaseEntity, IAggregateRoot
{
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Money Amount { get; set; } = Money.Zero;
    public Money DiscountAmount { get; set; } = Money.Zero;
    public Money FinalAmount => Amount - DiscountAmount;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public DateTimeOffset? FulfilledAt { get; private set; }
    public string Notes { get; set; } = string.Empty;

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public void AddItem(OrderItem item)
    {
        _items.Add(item);
        RecalculateAmount();
    }

    public void ApplyDiscount(Money discountAmount)
    {
        if (discountAmount.Amount < 0)
            throw new ArgumentException("Discount amount cannot be negative");

        if (discountAmount.Amount > Amount.Amount)
            throw new ArgumentException("Discount cannot exceed order amount");

        DiscountAmount = discountAmount;
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        if (!IsValidStatusTransition(Status, newStatus))
            throw new InvalidOperationException($"Cannot transition from {Status} to {newStatus}");

        var oldStatus = Status;
        Status = newStatus;

        if (newStatus == OrderStatus.Delivered)
            FulfilledAt = DateTimeOffset.UtcNow;

        // Domain event for status change
        AddDomainEvent(new OrderStatusChangedEvent(Id, oldStatus, newStatus));
    }

    private static bool IsValidStatusTransition(OrderStatus current, OrderStatus target)
    {
        return current switch
        {
            OrderStatus.Pending => target is OrderStatus.Confirmed or OrderStatus.Cancelled,
            OrderStatus.Confirmed => target is OrderStatus.Processing or OrderStatus.Cancelled,
            OrderStatus.Processing => target is OrderStatus.Shipped or OrderStatus.Cancelled,
            OrderStatus.Shipped => target is OrderStatus.Delivered,
            OrderStatus.Delivered => false,
            OrderStatus.Cancelled => false,
            _ => false
        };
    }

    private void RecalculateAmount()
    {
        Amount = new Money(_items.Sum(item => item.Price.Amount * item.Quantity));
    }

    public TimeSpan? GetFulfillmentTime()
    {
        return FulfilledAt?.Subtract(CreatedAt);
    }
}
