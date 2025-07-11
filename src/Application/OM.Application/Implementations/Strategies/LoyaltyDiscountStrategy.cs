using OM.Application.Dtos.Common;
using OM.Application.Interfaces.IStrategies;
using OM.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


[assembly: InternalsVisibleTo("OM.UnitTests")]
[assembly: InternalsVisibleTo("OM.IntegrationTests")]

namespace OM.Application.Implementations.Strategies;


internal sealed class LoyaltyDiscountStrategy : IDiscountStrategy
{
    public LoyaltyDiscountStrategy()
    {
            
    }

    public ApiResponse<int> Priority => ApiResponse<int>.Success(2);

    public Task<ApiResponse<decimal>> CalculateDiscountAsync(Order order, Customer customer)
    {
        try
        {
            // 10% loyalty discount  
            var discountPercentage = 0.10m;
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
            // Apply if customer has more than 5 orders  
            bool isApplicable = customer.TotalOrders > 5;
            return Task.FromResult(ApiResponse<bool>.Success(isApplicable));
        }
        catch (Exception)
        {

            throw;
        }
        
    }
}
