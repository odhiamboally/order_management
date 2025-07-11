using OM.Application.Dtos.Common;
using OM.Application.Interfaces.IServices;
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

namespace OM.Application.Implementations.Services;
internal sealed class DiscountService : IDiscountService
{
    private readonly List<IDiscountStrategy> _strategies;

    public DiscountService(List<IDiscountStrategy> strategies)
    {
        _strategies = strategies;
    }

    public async Task<ApiResponse<decimal>> CalculateDiscountAsync(Order order, Customer customer)
    {
        try
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            decimal totalDiscount = 0;

            foreach (var strategy in _strategies)
            {
                var isApplicableResponse = await strategy.IsApplicableAsync(order, customer);
                if (isApplicableResponse.Successful && isApplicableResponse.Data)
                {
                    var discountResponse = await strategy.CalculateDiscountAsync(order, customer);
                    if (discountResponse.Successful)
                    {
                        totalDiscount += discountResponse.Data;
                    }
                }
            }

            // Ensure discount doesn't exceed order amount
            var discountAmount = Math.Min(totalDiscount, order.Amount.Amount);
            if (discountAmount < 0)
                throw new ArgumentException("Calculated discount cannot be negative");

            return ApiResponse<decimal>.Success("Success", discountAmount);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
