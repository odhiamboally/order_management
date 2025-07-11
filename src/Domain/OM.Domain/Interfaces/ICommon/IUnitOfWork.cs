using OM.Domain.Interfaces.IRepositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Domain.Interfaces.ICommon;
public interface IUnitOfWork
{
    ICustomerRepository CustomerRepository { get; }
    IOrderRepository OrderRepository { get; }
}
