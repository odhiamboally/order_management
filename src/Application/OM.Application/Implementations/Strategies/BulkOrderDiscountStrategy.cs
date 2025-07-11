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
internal sealed class BulkOrderDiscountStrategy : IDiscountStrategy
{
    public BulkOrderDiscountStrategy()
    {
            
    }

    public ApiResponse<int> Priority => ApiResponse<int>.Success(3);

    public Task<ApiResponse<decimal>> CalculateDiscountAsync(Order order, Customer customer)
    {
        try
        {
            // 5% bulk order discount  
            var discountPercentage = 0.05m;
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
            // Apply if order amount is greater than $500  
            bool isApplicable = order.Amount.Amount > 500;
            return Task.FromResult(ApiResponse<bool>.Success(isApplicable));
        }
        catch (Exception)
        {
            throw;
        }
    }
}
