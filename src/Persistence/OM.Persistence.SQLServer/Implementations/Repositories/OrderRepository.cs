using Microsoft.EntityFrameworkCore;

using OM.Domain.Entities;
using OM.Domain.Interfaces.IRepositories;
using OM.Persistence.SQLServer.Context;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Persistence.SQLServer.Implementations.Repositories;
internal sealed class OrderRepository : BaseRepository<Order>, IOrderRepository
{
    public OrderRepository(DBContext context) : base(context)
    {
    }

    public async Task<TimeSpan> GetAverageFulfillmentTimeAsync()
    {
        var fulfilledOrders = await _context.Orders
            .Where(o => o.FulfilledAt.HasValue)
            .Select(o => new { o.CreatedAt, o.FulfilledAt })
            .ToListAsync();

        if (!fulfilledOrders.Any())
            return TimeSpan.Zero;

        var averageTicks = fulfilledOrders
            .Select(o => (o.FulfilledAt!.Value - o.CreatedAt).Ticks)
            .Average();

        return new TimeSpan((long)averageTicks);
    }

    public async Task<decimal> GetAverageOrderValueAsync()
    {
        return await Task.FromResult(
        _context.Orders
            .AsEnumerable() // Switch to LINQ-to-Objects
            .Select(o => o.Amount.Amount)
            .DefaultIfEmpty(0)
            .Average()
    );
    }
}
