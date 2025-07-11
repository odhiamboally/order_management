using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.Dtos.Order;


/// <summary>
/// Order analytics data transfer object
/// </summary>
public record OrderAnalyticsResponse
{
    /// <summary>
    /// Average value of all orders
    /// </summary>
    /// <example>156.75</example>
    [Range(0, double.MaxValue)]
    public decimal AverageOrderValue { get; init; }

    /// <summary>
    /// Average time from order creation to fulfillment
    /// </summary>
    /// <example>2.15:30:45</example>
    public TimeSpan AverageFulfillmentTime { get; init; }

    /// <summary>
    /// Total number of orders in the system
    /// </summary>
    /// <example>1247</example>
    [Range(0, int.MaxValue)]
    public int TotalOrders { get; init; }

    /// <summary>
    /// Total revenue from all orders
    /// </summary>
    /// <example>195432.50</example>
    [Range(0, double.MaxValue)]
    public decimal TotalRevenue { get; init; }

    /// <summary>
    /// Distribution of orders by status
    /// </summary>
    /// <example>
    /// {
    ///   "Pending": 45,
    ///   "Confirmed": 123,
    ///   "Processing": 67,
    ///   "Shipped": 234,
    ///   "Delivered": 756,
    ///   "Cancelled": 22
    /// }
    /// </example>
    public Dictionary<string, int> OrdersByStatus { get; init; } 

    public OrderAnalyticsResponse(decimal averageOrderValue,TimeSpan averageFulfillmentTime, int totalOrders, decimal totalRevenue, Dictionary<string, int> ordersByStatus)
    {
        AverageOrderValue = averageOrderValue;
        AverageFulfillmentTime = averageFulfillmentTime;
        TotalOrders = totalOrders;
        TotalRevenue = totalRevenue;
        OrdersByStatus = ordersByStatus;
    }
}
