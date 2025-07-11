using FluentAssertions;
using FluentAssertions.Primitives;

using Moq;

using OM.Application.Dtos.Common;
using OM.Application.Implementations.Services;
using OM.Application.Interfaces.IStrategies;
using OM.Domain.Entities;
using OM.Domain.Enums;
using OM.Domain.ValueObjects;
using OM.UnitTests.TestsModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.UnitTests;
public class DiscountServiceTests
{
    private readonly Mock<IDiscountStrategy> _mockVipStrategy;
    private readonly Mock<IDiscountStrategy> _mockLoyaltyStrategy;
    private readonly Mock<IDiscountStrategy> _mockBulkStrategy;
    private readonly DiscountService _sut; // System Under Test
    private readonly Customer _testCustomer;
    private readonly Order _testOrder;

    public DiscountServiceTests()
    {
        // Initialize mocks
        _mockVipStrategy = new Mock<IDiscountStrategy>(MockBehavior.Strict);
        _mockLoyaltyStrategy = new Mock<IDiscountStrategy>(MockBehavior.Strict);
        _mockBulkStrategy = new Mock<IDiscountStrategy>(MockBehavior.Strict);

        // Setup priority properties (these are not async)
        _mockVipStrategy.Setup(x => x.Priority).Returns(ApiResponse<int>.Success(1));
        _mockLoyaltyStrategy.Setup(x => x.Priority).Returns(ApiResponse<int>.Success(2));
        _mockBulkStrategy.Setup(x => x.Priority).Returns(ApiResponse<int>.Success(3));

        // Create service with mocked strategies
        var strategies = new List<IDiscountStrategy>
        {
            _mockVipStrategy.Object,
            _mockLoyaltyStrategy.Object,
            _mockBulkStrategy.Object
        };

        _sut = new DiscountService(strategies);


        // Test data
        _testCustomer = new Customer
        {
            Id = 1,
            Name = "John Doe",
            Email = "john.doe@test.com",
            Segment = CustomerSegment.VIP,
            TotalOrders = 10
        };

        _testOrder = new Order
        {
            Id = 1,
            CustomerId = 1,
            Amount = new Money(100m),
            Customer = _testCustomer
        };
    }


    [Fact]
    public async Task CalculateDiscountAsync_WithAllStrategiesApplicable_ShouldCombineAllDiscounts()
    {

        // Arrange
        const decimal vipDiscount = 15m;
        const decimal loyaltyDiscount = 10m;
        const decimal bulkDiscount = 5m;
        const decimal expectedTotal = vipDiscount + loyaltyDiscount + bulkDiscount;

        // Setup VIP strategy
        _mockVipStrategy
            .Setup(s => s.IsApplicableAsync(It.Is<Order>(o => o.Id == _testOrder.Id),
                                           It.Is<Customer>(c => c.Id == _testCustomer.Id)))
            .Returns(Task.FromResult(ApiResponse<bool>.Success(true)));

        _mockVipStrategy
            .Setup(s => s.CalculateDiscountAsync(It.Is<Order>(o => o.Id == _testOrder.Id),
                                               It.Is<Customer>(c => c.Id == _testCustomer.Id)))
            .Returns(Task.FromResult(ApiResponse<decimal>.Success(vipDiscount)));

        // Setup Loyalty strategy
        _mockLoyaltyStrategy
            .Setup(s => s.IsApplicableAsync(It.Is<Order>(o => o.Id == _testOrder.Id),
                                           It.Is<Customer>(c => c.Id == _testCustomer.Id)))
            .Returns(Task.FromResult(ApiResponse<bool>.Success(true)));

        _mockLoyaltyStrategy
            .Setup(s => s.CalculateDiscountAsync(It.Is<Order>(o => o.Id == _testOrder.Id),
                                               It.Is<Customer>(c => c.Id == _testCustomer.Id)))
            .Returns(Task.FromResult(ApiResponse<decimal>.Success(loyaltyDiscount)));

        // Setup Bulk strategy
        _mockBulkStrategy
            .Setup(s => s.IsApplicableAsync(It.Is<Order>(o => o.Id == _testOrder.Id),
                                           It.Is<Customer>(c => c.Id == _testCustomer.Id)))
            .Returns(Task.FromResult(ApiResponse<bool>.Success(true)));

        _mockBulkStrategy
            .Setup(s => s.CalculateDiscountAsync(It.Is<Order>(o => o.Id == _testOrder.Id),
                                               It.Is<Customer>(c => c.Id == _testCustomer.Id)))
            .Returns(Task.FromResult(ApiResponse<decimal>.Success(bulkDiscount)));

        // Act
        var result = await _sut.CalculateDiscountAsync(_testOrder, _testCustomer);

        // Assert
        result.Data.Should().Be(expectedTotal);
        result.Data.Should().BeGreaterThan(0);
        result.Data.Should().BeLessThanOrEqualTo(_testOrder.Amount.Amount);

        // Verify all interactions
        _mockVipStrategy.Verify(s => s.IsApplicableAsync(_testOrder, _testCustomer), Times.Once);
        _mockVipStrategy.Verify(s => s.CalculateDiscountAsync(_testOrder, _testCustomer), Times.Once);

        _mockLoyaltyStrategy.Verify(s => s.IsApplicableAsync(_testOrder, _testCustomer), Times.Once);
        _mockLoyaltyStrategy.Verify(s => s.CalculateDiscountAsync(_testOrder, _testCustomer), Times.Once);

        _mockBulkStrategy.Verify(s => s.IsApplicableAsync(_testOrder, _testCustomer), Times.Once);
        _mockBulkStrategy.Verify(s => s.CalculateDiscountAsync(_testOrder, _testCustomer), Times.Once);
    }

    // Alternative: Use MemberData instead of InlineData for complex scenarios
    [Theory]
    [MemberData(nameof(GetDiscountScenarios))]
    public async Task CalculateDiscountAsync_WithComplexScenarios_ShouldReturnExpectedTotal(DiscountTestScenario scenario)

    {
        // Arrange
        _mockVipStrategy
            .Setup(s => s.IsApplicableAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
            .Returns(Task.FromResult(ApiResponse<bool>.Success(scenario.VipApplicable)));

        if (scenario.VipApplicable)
        {
            _mockVipStrategy
                .Setup(s => s.CalculateDiscountAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
                .Returns(Task.FromResult(ApiResponse<decimal>.Success(scenario.VipAmount)));
        }

        _mockLoyaltyStrategy
            .Setup(s => s.IsApplicableAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
            .Returns(Task.FromResult(ApiResponse<bool>.Success(scenario.LoyaltyApplicable)));

        if (scenario.LoyaltyApplicable)
        {
            _mockLoyaltyStrategy
                .Setup(s => s.CalculateDiscountAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
                .Returns(Task.FromResult(ApiResponse<decimal>.Success(scenario.LoyaltyAmount)));
        }

        _mockBulkStrategy
            .Setup(s => s.IsApplicableAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
            .Returns(Task.FromResult(ApiResponse<bool>.Success(scenario.BulkApplicable)));

        if (scenario.BulkApplicable)
        {
            _mockBulkStrategy
                .Setup(s => s.CalculateDiscountAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
                .Returns(Task.FromResult(ApiResponse<decimal>.Success(scenario.BulkAmount)));
        }

        // Act
        var result = await _sut.CalculateDiscountAsync(_testOrder, _testCustomer);

        // Assert
        result.Data.Should().Be(scenario.ExpectedTotal, scenario.Description);
    }

    [Fact]
    public async Task CalculateDiscountAsync_WithDiscountExceedingOrderAmount_ShouldCapAtOrderAmount()
    {
        // Arrange
        var smallOrder = new Order
        {
            Id = 2,
            CustomerId = 1,
            Amount = new Money(10m), // Small order amount
            Customer = _testCustomer
        };

        const decimal excessiveDiscount = 50m; // More than order amount
        const decimal loyaltyDiscount = 5m;

        // Setup strategy to return excessive discount
        _mockVipStrategy
            .Setup(s => s.IsApplicableAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
            .Returns(Task.FromResult(ApiResponse<bool>.Success(true)));
        _mockVipStrategy
            .Setup(s => s.CalculateDiscountAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
            .Returns(Task.FromResult(ApiResponse<decimal>.Success(excessiveDiscount)));

        // Other strategies not applicable
        _mockLoyaltyStrategy
            .Setup(s => s.IsApplicableAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
            .Returns(Task.FromResult(ApiResponse<bool>.Success(true)));
        _mockLoyaltyStrategy
            .Setup(s => s.CalculateDiscountAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
            .Returns(Task.FromResult(ApiResponse<decimal>.Success(loyaltyDiscount))); 

        _mockBulkStrategy
            .Setup(s => s.IsApplicableAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
            .Returns(Task.FromResult(ApiResponse<bool>.Success(false)));

        // Act
        var result = await _sut.CalculateDiscountAsync(smallOrder, _testCustomer);

        // Assert
        result.Data.Should().Be(smallOrder.Amount.Amount); // Should be capped at order amount
        result.Data.Should().BeLessThanOrEqualTo(smallOrder.Amount.Amount);


        // Replace the line with the error
        result.Data.Should().BeLessThanOrEqualTo(smallOrder.Amount.Amount);

        // With this line
        result.Data.Should().BeLessThanOrEqualTo(smallOrder.Amount.Amount);
    }

    [Fact]
    public async Task CalculateDiscountAsync_WithNoApplicableStrategies_ShouldReturnZero()
    {
        // Arrange - All strategies return false for IsApplicable
        _mockVipStrategy
            .Setup(s => s.IsApplicableAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
            .Returns(Task.FromResult(ApiResponse<bool>.Success(false)));

        _mockLoyaltyStrategy
            .Setup(s => s.IsApplicableAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
            .Returns(Task.FromResult(ApiResponse<bool>.Success(false)));

        _mockBulkStrategy
            .Setup(s => s.IsApplicableAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
            .Returns(Task.FromResult(ApiResponse<bool>.Success(false)));

        // Act
        var result = await _sut.CalculateDiscountAsync(_testOrder, _testCustomer);

        // Assert
        result.Data.Should().Be(0m);
        result.Data.Should().BeGreaterThanOrEqualTo(0);
        result.Data.Should().BeLessThanOrEqualTo(_testOrder.Amount.Amount);

        // Verify that IsApplicable was called but CalculateDiscount was never called
        _mockVipStrategy.Verify(s => s.IsApplicableAsync(_testOrder, _testCustomer), Times.Once);
        _mockVipStrategy.Verify(s => s.CalculateDiscountAsync(It.IsAny<Order>(), It.IsAny<Customer>()), Times.Never);

        _mockLoyaltyStrategy.Verify(s => s.IsApplicableAsync(_testOrder, _testCustomer), Times.Once);
        _mockLoyaltyStrategy.Verify(s => s.CalculateDiscountAsync(It.IsAny<Order>(), It.IsAny<Customer>()), Times.Never);

        _mockBulkStrategy.Verify(s => s.IsApplicableAsync(_testOrder, _testCustomer), Times.Once);
        _mockBulkStrategy.Verify(s => s.CalculateDiscountAsync(It.IsAny<Order>(), It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task CalculateDiscountAsync_WithNullCustomer_ShouldThrowArgumentNullException()
    {
        // Arrange
        Customer? nullCustomer = null;

        // Act & Assert
        var act = () => _sut.CalculateDiscountAsync(_testOrder, nullCustomer!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CalculateDiscountAsync_WithNullOrder_ShouldThrowArgumentNullException()
    {
        // Arrange
        Order? nullOrder = null;

        // Act & Assert
        var act = () => _sut.CalculateDiscountAsync(nullOrder!, _testCustomer);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CalculateDiscountAsync_WithOnlyVipApplicable_ShouldReturnOnlyVipDiscount()
    {
        // Arrange
        const decimal vipDiscount = 15m;

        // VIP strategy applicable
        _mockVipStrategy
            .Setup(s => s.IsApplicableAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
            .Returns(Task.FromResult(ApiResponse<bool>.Success(true)));

        _mockVipStrategy
            .Setup(s => s.CalculateDiscountAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
            .Returns(Task.FromResult(ApiResponse<decimal>.Success(vipDiscount)));

        // Other strategies not applicable
        _mockLoyaltyStrategy
            .Setup(s => s.IsApplicableAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
            .Returns(Task.FromResult(ApiResponse<bool>.Success(false)));

        _mockBulkStrategy
            .Setup(s => s.IsApplicableAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
            .Returns(Task.FromResult(ApiResponse<bool>.Success(false)));

        // Act
        var result = await _sut.CalculateDiscountAsync(_testOrder, _testCustomer);

        // Assert
        result.Data.Should().Be(vipDiscount);
        result.Data.Should().BePositive();
        result.Data.Should().BeLessThanOrEqualTo(_testOrder.Amount.Amount);

        // Verify interactions
        _mockVipStrategy.Verify(s => s.IsApplicableAsync(_testOrder, _testCustomer), Times.Once);
        _mockVipStrategy.Verify(s => s.CalculateDiscountAsync(_testOrder, _testCustomer), Times.Once);

        _mockLoyaltyStrategy.Verify(s => s.IsApplicableAsync(_testOrder, _testCustomer), Times.Once);
        _mockLoyaltyStrategy.Verify(s => s.CalculateDiscountAsync(It.IsAny<Order>(), It.IsAny<Customer>()), Times.Never);

        _mockBulkStrategy.Verify(s => s.IsApplicableAsync(_testOrder, _testCustomer), Times.Once);
        _mockBulkStrategy.Verify(s => s.CalculateDiscountAsync(It.IsAny<Order>(), It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task CalculateDiscountAsync_WithStrategyThrowingException_ShouldPropagateException()
    {
        // Arrange
        _mockVipStrategy
            .Setup(s => s.IsApplicableAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
            .ThrowsAsync(new InvalidOperationException("Strategy failed"));

        // Setup other strategies to avoid strict mock failures
        _mockLoyaltyStrategy
            .Setup(s => s.IsApplicableAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
            .Returns(Task.FromResult(ApiResponse<bool>.Success(false)));

        _mockBulkStrategy
            .Setup(s => s.IsApplicableAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
            .Returns(Task.FromResult(ApiResponse<bool>.Success(false)));

        // Act & Assert
        var act = () => _sut.CalculateDiscountAsync(_testOrder, _testCustomer);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Strategy failed");
    }


    [Theory]
    [InlineData(true, true, true, 15.0, 10.0, 5.0, 30.0)]
    [InlineData(true, false, false, 15.0, 0.0, 0.0, 15.0)]
    [InlineData(false, true, false, 0.0, 10.0, 0.0, 10.0)]
    [InlineData(false, false, true, 0.0, 0.0, 5.0, 5.0)]
    [InlineData(false, false, false, 0.0, 0.0, 0.0, 0.0)]
    public async Task CalculateDiscountAsync_WithVariousStrategyCombinations_ShouldReturnExpectedTotal(
        bool vipApplicable, bool loyaltyApplicable, bool bulkApplicable,
        decimal vipAmount, decimal loyaltyAmount, decimal bulkAmount,
        decimal expectedTotal)
    {
        // Arrange
        _mockVipStrategy
            .Setup(s => s.IsApplicableAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
            .Returns(Task.FromResult(ApiResponse<bool>.Success(vipApplicable)));

        if (vipApplicable)
        {
            _mockVipStrategy
                .Setup(s => s.CalculateDiscountAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
                .Returns(Task.FromResult(ApiResponse<decimal>.Success(vipAmount)));
        }

        _mockLoyaltyStrategy
            .Setup(s => s.IsApplicableAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
            .Returns(Task.FromResult(ApiResponse<bool>.Success(loyaltyApplicable)));

        if (loyaltyApplicable)
        {
            _mockLoyaltyStrategy
                .Setup(s => s.CalculateDiscountAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
                .Returns(Task.FromResult(ApiResponse<decimal>.Success(loyaltyAmount)));
        }

        _mockBulkStrategy
            .Setup(s => s.IsApplicableAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
            .Returns(Task.FromResult(ApiResponse<bool>.Success(bulkApplicable)));

        if (bulkApplicable)
        {
            _mockBulkStrategy
                .Setup(s => s.CalculateDiscountAsync(It.IsAny<Order>(), It.IsAny<Customer>()))
                .Returns(Task.FromResult(ApiResponse<decimal>.Success(bulkAmount)));
        }

        // Act
        var result = await _sut.CalculateDiscountAsync(_testOrder, _testCustomer);

        // Assert
        result.Data.Should().Be(expectedTotal);
    }

    

    
    

    

    public static IEnumerable<object[]> GetDiscountScenarios()
    {
        yield return new object[]
        {
            new DiscountTestScenario
            {
                VipApplicable = true,
                LoyaltyApplicable = true,
                BulkApplicable = true,
                VipAmount = 15m,
                LoyaltyAmount = 10m,
                BulkAmount = 5m,
                ExpectedTotal = 30m,
                Description = "All discounts applicable"
            }
        };

        yield return new object[]
        {
            new DiscountTestScenario
            {
                VipApplicable = true,
                LoyaltyApplicable = false,
                BulkApplicable = false,
                VipAmount = 15m,
                LoyaltyAmount = 0m,
                BulkAmount = 0m,
                ExpectedTotal = 15m,
                Description = "Only VIP discount applicable"
            }
        };

        yield return new object[]
        {
            new DiscountTestScenario
            {
                VipApplicable = false,
                LoyaltyApplicable = false,
                BulkApplicable = false,
                VipAmount = 0m,
                LoyaltyAmount = 0m,
                BulkAmount = 0m,
                ExpectedTotal = 0m,
                Description = "No discounts applicable"
            }
        };
    }

    public void Dispose()
    {
        // Verify all setups were used (with MockBehavior.Strict)
        _mockVipStrategy.VerifyAll();
        _mockLoyaltyStrategy.VerifyAll();
        _mockBulkStrategy.VerifyAll();
    }
}
