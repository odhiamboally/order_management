using OM.Domain.Enums;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.Dtos.Order;
public record UpdateOrderStatusRequest(
    [Required] OrderStatus NewStatus
    );
