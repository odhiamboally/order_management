using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using OM.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Domain.Interfaces.ICommon;
public interface IApplicationDBContext
{
    DbSet<Order> Orders { get; }
    DbSet<Customer> Customers { get; }
    DbSet<OrderItem> OrderItems { get; }


    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    

    // Additional methods for advanced scenarios (optional)
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    // For transaction support (optional)
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        
}
