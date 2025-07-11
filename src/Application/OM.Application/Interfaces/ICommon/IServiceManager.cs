using OM.Application.Interfaces.IServices;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.Interfaces.ICommon;
public interface IServiceManager
{
    ICustomerService CustomerService { get; }
    IOrderService OrderService { get; }
    IDiscountService DiscountService { get; }
    ICacheInvalidationService CacheInvalidationService { get; }

}
