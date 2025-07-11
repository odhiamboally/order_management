using OM.Application.Interfaces.ICommon;
using OM.Application.Interfaces.IServices;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.Implementations.Common;
internal sealed class ServiceManager : IServiceManager
{
    public ServiceManager(
        ICustomerService customerService, 
        IOrderService orderService,
        IDiscountService discountService,
        ICacheInvalidationService cacheInvalidationService)
    {
        CustomerService = customerService;
        OrderService = orderService;
        DiscountService = discountService;
        CacheInvalidationService = cacheInvalidationService;
    }
    public ICustomerService CustomerService { get; }
    public IOrderService OrderService { get; }
    public IDiscountService DiscountService { get; }
    public ICacheInvalidationService CacheInvalidationService { get; }
}
