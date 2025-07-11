using OM.Application.Dtos.Common;
using OM.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.Interfaces.IServices;
public interface IDiscountService
{
    Task<ApiResponse<decimal>> CalculateDiscountAsync(Order order, Customer customer);
}
