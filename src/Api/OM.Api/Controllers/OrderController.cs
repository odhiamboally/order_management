using Asp.Versioning;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using OM.Application.CQRS.Orders.Commands;
using OM.Application.CQRS.Orders.Queries;
using OM.Application.Dtos.Order;

namespace OM.Api.Controllers;



/// <summary>
/// Orders management endpoints
/// </summary>
/// 
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Produces("application/json")]
[Tags("Orders")]
public class OrderController : BaseController
{
    private readonly ISender _mediator;
    public OrderController(ISender mediator)
    {
        _mediator = mediator;

    }

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <remarks>
    /// Creates a new order with automatic discount calculation based on:
    /// - Customer segment (VIP gets 15% discount)
    /// - Order history (Loyal customers with 5+ orders get 10% discount)
    /// - Order amount (Orders over $500 get 5% bulk discount)
    /// 
    /// Discounts are automatically combined and capped at the order total.
    /// </remarks>
    /// <param name="command">Order creation details</param>
    /// <returns>Created order details</returns>
    /// <response code="201">Order created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Customer not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("create")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtRoute(nameof(GetOrderById), new { version = "1.0", id = result.Id }, result);
            
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details including items, discounts, and current status</returns>
    /// <response code="200">Order found</response>
    /// <response code="404">Order not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("getbyId/{id:int}", Name = "GetOrderById")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderById([FromRoute] int id)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get all orders
    /// </summary>
    /// <returns>List of all orders in the system</returns>
    /// <response code="200">Orders retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("getAll")]
    [ProducesResponseType<List<OrderResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllOrders()
    {
        var query = new GetAllOrdersQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Update order status
    /// </summary>
    /// <remarks>
    /// Updates the status of an existing order with proper state validation:
    /// - Pending → Confirmed or Cancelled
    /// - Confirmed → Processing or Cancelled  
    /// - Processing → Shipped or Cancelled
    /// - Shipped → Delivered
    /// - Delivered and Cancelled are final states
    /// 
    /// Status changes trigger domain events for notifications and analytics.
    /// </remarks>
    /// <param name="id">Order ID</param>
    /// <param name="request">Status update request</param>
    /// <returns>Success response</returns>
    /// <response code="204">Status updated successfully</response>
    /// <response code="400">Invalid status transition</response>
    /// <response code="404">Order not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("updateOrderStatus/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOrderStatus([FromRoute] int id,[FromBody] UpdateOrderStatusRequest request)
    {
        var command = new UpdateOrderStatusCommand(id, request.NewStatus);
        await _mediator.Send(command);
        return NoContent();
    }


}
