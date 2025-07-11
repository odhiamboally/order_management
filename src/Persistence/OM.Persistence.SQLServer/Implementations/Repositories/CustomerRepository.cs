using OM.Domain.Entities;
using OM.Domain.Interfaces.IRepositories;
using OM.Persistence.SQLServer.Context;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Persistence.SQLServer.Implementations.Repositories;
internal sealed class CustomerRepository : BaseRepository<Customer>,  ICustomerRepository
{
    public CustomerRepository(DBContext context) : base(context)
    {
    }
}
