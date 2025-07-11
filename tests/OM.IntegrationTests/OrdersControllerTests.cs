using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using OM.Application.CQRS.Orders.Commands;
using OM.Application.Dtos.Order;
using OM.Domain.Entities;
using OM.Domain.Enums;
using OM.IntegrationTests.Common;
using OM.Persistence.SQLServer.Context;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OM.IntegrationTests;
public class OrdersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions;


    public OrdersControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();

        // Configure JSON options to match your API
        _jsonOptions = new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = false,
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };
    }

    [Fact]
    public async Task CreateOrder_WithInvalidCustomerId_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new CreateOrderCommand(
            CustomerId: 999, // Non-existent customer
            Items: new List<OrderItemResponse>
            {
                new("Test Product", 50.00m, 2)
            }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1.0/order/create", command, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_WithValidCustomerId_ShouldReturnCreatedOrder()
    {
        // Arrange
        await SeedTestCustomer();

        var command = new CreateOrderCommand(
            CustomerId: 1,
            Items: new List<OrderItemResponse>
            {
                new("Test Product_1", 50.00m, 2)
            },
            Notes: "Test order"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1.0/order/create", command, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        //var order = await response.Content.ReadFromJsonAsync<OrderResponse>();

        // Use custom JSON options for deserialization
        var jsonString = await response.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<OrderResponse>(jsonString, _jsonOptions);

        order.Should().NotBeNull();
        order!.Amount.Should().Be(100.00m); // 2 * $50
        order.CustomerId.Should().Be(1);
    }

    [Fact]
    public async Task GetOrderById_WithExistingOrder_ShouldReturnOrder()
    {
        // Arrange
        await SeedTestCustomer();
        var orderId = await CreateTestOrder();

        // Act
        var response = await _client.GetAsync($"/api/v1.0/order/getbyId/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var jsonString = await response.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<OrderResponse>(jsonString, _jsonOptions);

        order.Should().NotBeNull();
        order!.Id.Should().Be(orderId);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithValidTransition_ShouldReturnNoContent()
    {
        // Arrange
        await SeedTestCustomer();
        var orderId = await CreateTestOrder();
        var updateRequest = new { NewStatus = OrderStatus.Confirmed };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1.0/order/updateOrderStatus/{orderId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task SeedTestCustomer()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DBContext>();

        if (!context.Customers.Any(c => c.Id == 1))
        {
            var customer = new Customer
            {
                Id = 1,
                Name = "Test Customer",
                Email = "test@email.com",
                Segment = CustomerSegment.Regular
            };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
        }
    }

    private async Task<int> CreateTestOrder()
    {
        var command = new CreateOrderCommand(
            CustomerId: 1,
            Items: new List<OrderItemResponse>
            {
                new("Test Product", 50.00m, 1)
            }
        );

        var response = await _client.PostAsJsonAsync("/api/v1.0/order/create", command);
        var jsonString = await response.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<OrderResponse>(jsonString, _jsonOptions);
        return order!.Id;
    }
}
