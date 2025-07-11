using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

using OM.Domain.Common;
using OM.Domain.Entities;
using OM.Domain.Enums;
using OM.Domain.EventDispatchers;
using OM.Domain.Interfaces.ICommon;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OM.Persistence.SQLServer.Context;
public class DBContext : DbContext, IApplicationDBContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly DomainEventDispatcher? _domainEventDispatcher;
    public DBContext(DbContextOptions<DBContext> options) : base(options) { }
    
    public DBContext(DbContextOptions<DBContext> options, DomainEventDispatcher domainEventDispatcher) : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }



    #region Sets

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }



    #endregion


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DBContext).Assembly);

        // Ignore domain events in database mapping
        modelBuilder.Ignore<List<IDomainEvent>>();

        modelBuilder.Entity<Customer>().HasData(
            new Customer
            {
                Id = 1, 
                Name = "John Doe",
                Email = "john.doe@email.com",
                Segment = CustomerSegment.VIP,
                TotalOrders = 12,
                CreatedAt = new DateTime(2024, 1, 1) 
            },
            new Customer
            {
                Id = 2, 
                Name = "Jane Smith",
                Email = "jane.smith@email.com",
                Segment = CustomerSegment.Regular,
                TotalOrders = 5,
                CreatedAt = new DateTime(2024, 1, 1)
            },
            new Customer
            {
                Id = 3, 
                Name = "Bob Johnson",
                Email = "bob.johnson@email.com",
                Segment = CustomerSegment.New,
                TotalOrders = 0,
                CreatedAt = new DateTime(2024, 1, 1)
            }
        );

        base.OnModelCreating(modelBuilder);
    }


    public override int SaveChanges()
    {
        UpdateTimestamps();

        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();

        // Only use transactions and domain events if dispatcher is available
        if (_domainEventDispatcher == null)
        {
            // Simple save without domain events (useful for seeding)
            return await base.SaveChangesAsync(cancellationToken);
        }

        if (Database.IsInMemory())
        {
            // No transactions for in-memory
            var result = await base.SaveChangesAsync(cancellationToken);
            // Still dispatch events if needed
            return result;
        }
        else
        {
            using var transaction = await Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Get entities with domain events before saving
                var entitiesWithEvents = ChangeTracker.Entries<BaseEntity>()
                    .Where(e => e.Entity.DomainEvents.Any())
                    .Select(e => e.Entity)
                    .ToList();

                var result = await base.SaveChangesAsync(cancellationToken); // Ensure Save Entity first

                await transaction.CommitAsync(cancellationToken);

                // Dispatch domain events after successful save
                if (entitiesWithEvents.Any())
                {
                    await _domainEventDispatcher.DispatchEventsAsync(entitiesWithEvents);
                }

                return result;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);  // Rollback in case of error
                throw;
            }
        }
    }

    private void UpdateTimestamps()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.IsInMemory())
        {
            throw new InvalidOperationException("Transactions are not supported with in-memory database");
        }

        return await Database.BeginTransactionAsync(cancellationToken);
    }
}
