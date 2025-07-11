using Asp.Versioning;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using OM.Application.CQRS.Orders.Queries;
using OM.Application.Dtos.Order;
using OM.Application.Interfaces.ICommon;

using System.ComponentModel.DataAnnotations;

namespace OM.Api.Controllers;


/// <summary>
/// Order analytics and reporting endpoints
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Produces("application/json")]
[Tags("Analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly ISender _mediator;

    public AnalyticsController(ISender mediator)
    {
        _mediator = mediator;
    }


    /// <summary>
    /// Get comprehensive order analytics
    /// </summary>
    /// <remarks>
    /// Retrieves comprehensive analytics including:
    /// - Average order value
    /// - Average fulfillment time  
    /// - Order status distribution
    /// - Total revenue metrics
    /// 
    /// Results are cached for 5 minutes for optimal performance.
    /// </remarks>
    /// <returns>Order analytics data</returns>
    /// <response code="200">Analytics retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("order-analytics")]
    [ProducesResponseType<OrderAnalyticsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderAnalytics()
    {
        var query = new GetOrderAnalyticsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get analytics for a specific date range
    /// </summary>
    /// <param name="startDate">Start date for analytics (ISO 8601 format)</param>
    /// <param name="endDate">End date for analytics (ISO 8601 format)</param>
    /// <returns>Analytics data for the specified period</returns>
    /// <response code="200">Analytics retrieved successfully</response>
    /// <response code="400">Invalid date range provided</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("order-analytics/date-range")]
    [ProducesResponseType<OrderAnalyticsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderAnalyticsResponse>> GetAnalyticsByDateRange([FromQuery, Required] DateTimeOffset startDate,[FromQuery, Required] DateTimeOffset endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid date range",
                Detail = "Start date must be before end date",
                Errors = new Dictionary<string, string[]>
                {
                    ["startDate"] = ["Start date must be before end date"],
                    ["endDate"] = ["End date must be after start date"]
                }
            });
        }

        if (endDate > DateTimeOffset.UtcNow)
        {
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Invalid date range",
                Detail = "End date cannot be in the future",
                Errors = new Dictionary<string, string[]>
                {
                    ["endDate"] = ["End date cannot be in the future"]
                }
            });
        }

        var query = new GetOrderAnalyticsByDateRangeQuery(startDate, endDate);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
