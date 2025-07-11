using OM.Application.Dtos.Common;
using OM.Application.Interfaces.IStrategies;
using OM.Domain.Entities;
using OM.Domain.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


[assembly: InternalsVisibleTo("OM.UnitTests")]
[assembly: InternalsVisibleTo("OM.IntegrationTests")]

namespace OM.Application.Implementations.Strategies;
internal sealed class VipDiscountStrategy : IDiscountStrategy
{
    public VipDiscountStrategy()
    {
            
    }

    public ApiResponse<int> Priority => ApiResponse<int>.Success(1);

    public Task<ApiResponse<decimal>> CalculateDiscountAsync(Order order, Customer customer)
    {
        try
        {
            // 15 % discount for VIP customers  
            var discountPercentage = 0.15m;
            var discount = order.Amount.Amount * discountPercentage;
            return Task.FromResult(ApiResponse<decimal>.Success(discount));
        }
        catch (Exception)
        {

            throw;
        }
        
    }

    public Task<ApiResponse<bool>> IsApplicableAsync(Order order, Customer customer)
    {
        try
        {
            return Task.FromResult(ApiResponse<bool>.Success(customer.Segment == CustomerSegment.VIP));
        }
        catch (Exception)
        {

            throw;
        }
        
    }
}
