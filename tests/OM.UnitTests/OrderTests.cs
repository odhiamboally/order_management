using FluentAssertions;

using OM.Domain.Entities;
using OM.Domain.Enums;
using OM.Domain.Events;
using OM.Domain.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.UnitTests;
public class OrderTests
{
    [Fact]
    public void ApplyDiscount_ExceedingOrderAmount_ShouldThrowException()
    {
        // Arrange
        var order = new Order { Amount = new Money(100) };
        var discountAmount = new Money(150);

        // Act & Assert
        Action act = () => order.ApplyDiscount(discountAmount);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Discount cannot exceed order amount");
    }

    [Fact]
    public void ApplyDiscount_WithNegativeAmount_ShouldThrowException()
    {
        // Arrange
        var order = new Order { Amount = new Money(100) };
        var discountAmount = new Money(-10);

        // Act & Assert
        Action act = () => order.ApplyDiscount(discountAmount);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Discount amount cannot be negative");
    }

    [Fact]
    public void ApplyDiscount_WithValidAmount_ShouldSetDiscountAmount()
    {
        // Arrange
        var order = new Order { Amount = new Money(100) };
        var discountAmount = new Money(15);

        // Act
        order.ApplyDiscount(discountAmount);

        // Assert
        order.DiscountAmount.Should().Be(discountAmount);
        order.FinalAmount.Amount.Should().Be(85);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var order = new Order();
        order.UpdateStatus(OrderStatus.Confirmed);
        order.UpdateStatus(OrderStatus.Processing);

        order.DomainEvents.Should().HaveCount(2);

        // Act
        order.ClearDomainEvents();

        // Assert
        order.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void GetFulfillmentTime_WithFulfilledOrder_ShouldReturnCorrectTimeSpan()
    {
        // Arrange
        var order = new Order();
        var startTime = DateTime.UtcNow;
        order.UpdateStatus(OrderStatus.Confirmed);
        order.UpdateStatus(OrderStatus.Processing);
        order.UpdateStatus(OrderStatus.Shipped);
        order.UpdateStatus(OrderStatus.Delivered);

        // Act
        var fulfillmentTime = order.GetFulfillmentTime();

        // Assert
        fulfillmentTime.Should().NotBeNull();
        fulfillmentTime.Should().BeCloseTo(DateTime.UtcNow - startTime, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateStatus_ToDelivered_ShouldSetFulfilledAt()
    {
        // Arrange
        var order = new Order();
        order.UpdateStatus(OrderStatus.Confirmed);
        order.UpdateStatus(OrderStatus.Processing);
        order.UpdateStatus(OrderStatus.Shipped);

        // Act
        order.UpdateStatus(OrderStatus.Delivered);

        // Assert
        order.Status.Should().Be(OrderStatus.Delivered);
        order.FulfilledAt.Should().NotBeNull();
        order.FulfilledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateStatus_ToDelivered_ShouldSetFulfilledAtAndRaiseDomainEvent()
    {
        // Arrange
        var order = new Order();
        order.UpdateStatus(OrderStatus.Confirmed);
        order.UpdateStatus(OrderStatus.Processing);
        order.UpdateStatus(OrderStatus.Shipped);

        // Clear events to test final transition
        order.ClearDomainEvents();

        // Act
        order.UpdateStatus(OrderStatus.Delivered);

        // Assert
        order.Status.Should().Be(OrderStatus.Delivered);
        order.FulfilledAt.Should().NotBeNull();
        order.FulfilledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Test domain event for delivery
        order.DomainEvents.Should().HaveCount(1);
        var domainEvent = order.DomainEvents.First().Should().BeOfType<OrderStatusChangedEvent>().Subject;
        domainEvent.NewStatus.Should().Be(OrderStatus.Delivered);
    }


    [Fact]
    public void UpdateStatus_WithInvalidTransition_ShouldThrowException()
    {
        // Arrange
        var order = new Order(); // Status is Pending by default

        // Act & Assert
        Action act = () => order.UpdateStatus(OrderStatus.Shipped);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Cannot transition from Pending to Shipped");
    }

    [Fact]
    public void UpdateStatus_WithInvalidTransition_ShouldThrowExceptionAndNotRaiseDomainEvent()
    {
        // Arrange
        var order = new Order(); // Status is Pending by default

        // Act & Assert
        Action act = () => order.UpdateStatus(OrderStatus.Shipped);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Cannot transition from Pending to Shipped");

        // Verify no domain event was raised for invalid transition
        order.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdateStatus_WithValidTransition_ShouldUpdateStatus()
    {
        // Arrange
        var order = new Order();
        order.UpdateStatus(OrderStatus.Confirmed); // First transition to Confirmed

        // Clear any existing events to test the next transition
        order.ClearDomainEvents();

        // Act
        order.UpdateStatus(OrderStatus.Processing);

        // Assert
        order.Status.Should().Be(OrderStatus.Processing);

        // Test domain event was raised
        order.DomainEvents.Should().HaveCount(1);
        var domainEvent = order.DomainEvents.First().Should().BeOfType<OrderStatusChangedEvent>().Subject;

        domainEvent.OrderId.Should().Be(order.Id);
        domainEvent.OldStatus.Should().Be(OrderStatus.Confirmed);
        domainEvent.NewStatus.Should().Be(OrderStatus.Processing);
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    

    


    

    

    

    

    

    
}
