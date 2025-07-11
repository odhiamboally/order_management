using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.Interfaces.ICommon;
public interface ICacheInvalidationService
{
    Task InvalidateOrderCacheAsync(int orderId, int customerId);
    Task InvalidateAllOrdersCacheAsync();
    Task InvalidateAnalyticsCacheAsync();
}
