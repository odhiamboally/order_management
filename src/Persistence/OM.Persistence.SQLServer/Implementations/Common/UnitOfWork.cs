using OM.Domain.Interfaces.ICommon;
using OM.Domain.Interfaces.IRepositories;
using OM.Persistence.SQLServer.Context;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Persistence.SQLServer.Implementations.Common;


public class UnitOfWork : IUnitOfWork
{
    public ICustomerRepository CustomerRepository { get; private set; }
    public IOrderRepository OrderRepository { get; private set; }
    

    private readonly DBContext _context;

    public UnitOfWork(
        ICustomerRepository customerRepository,
        IOrderRepository orderRepository,

        DBContext Context


        )
    {
        CustomerRepository = customerRepository;
        OrderRepository = orderRepository;

        _context = Context;



    }

    public async Task<int> CompleteAsync()
    {
        var result = await _context.SaveChangesAsync();
        return result!;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);

    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _context.Dispose();
        }
    }
}

