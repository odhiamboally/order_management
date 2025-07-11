using FluentAssertions;

using OM.Application.Implementations.Strategies;
using OM.Application.Interfaces.IStrategies;
using OM.Domain.Entities;
using OM.Domain.Enums;
using OM.Domain.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.UnitTests;
public class VipDiscountStrategyTests
{
    private readonly VipDiscountStrategy _strategy = new();

    [Fact]
    public async Task CalculateDiscountAsync_WithVipCustomer_ShouldReturn15PercentDiscount()
    {
        // Arrange
        var vipCustomer = new Customer { Segment = CustomerSegment.VIP };
        var order = new Order { Amount = new Money(100) };

        // Act
        var result = await _strategy.CalculateDiscountAsync(order, vipCustomer);

        // Assert
        result.Data.Should().Be(15m); // 15% of 100
    }

    [Fact]
    public async Task IsApplicableAsync_WithVipCustomer_ShouldReturnTrue()
    {
        // Arrange
        var vipCustomer = new Customer { Segment = CustomerSegment.VIP };
        var order = new Order();

        // Act
        var result = await _strategy.IsApplicableAsync(order, vipCustomer);

        // Assert
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task IsApplicableAsync_WithNonVipCustomer_ShouldReturnFalse()
    {
        // Arrange
        var regularCustomer = new Customer { Segment = CustomerSegment.Regular };
        var order = new Order();

        // Act
        var result = await _strategy.IsApplicableAsync(order, regularCustomer);

        // Assert
        result.Data.Should().BeFalse();
    }

    
}

