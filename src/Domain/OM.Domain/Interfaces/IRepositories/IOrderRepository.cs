using OM.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Domain.Interfaces.IRepositories;
public interface IOrderRepository : IBaseRepository<Order>
{
    Task<decimal> GetAverageOrderValueAsync();
    Task<TimeSpan> GetAverageFulfillmentTimeAsync();
}
