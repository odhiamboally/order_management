using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using OM.Domain.Entities;
using OM.Domain.Enums;
using OM.Domain.Interfaces.ICommon;
using OM.Domain.ValueObjects;
using OM.Persistence.SQLServer.Context;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OM.Persistence.SQLServer.Implementations.Common;
internal sealed class DatabaseSeeder : IDatabaseSeeder
{
    private readonly DBContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly IConfiguration _configuration;
    public DatabaseSeeder(DBContext context, ILogger<DatabaseSeeder> logger, IConfiguration configuration)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }


    public async Task SeedAsync()
    {
        var seedingEnabled = _configuration.GetValue<bool>("DatabaseSeeding:Enabled");
        if (!seedingEnabled)
        {
            _logger.LogInformation("Database seeding is disabled in configuration");
            return;
        }

        var environment = _configuration.GetValue<string>("DatabaseSeeding:Environment");
        if (environment != "Development" && environment != Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
        {
            _logger.LogInformation("Skipping seeding - environment mismatch");
            return;
        }

        await SeedAsync(CancellationToken.None);
    }

    public async Task SeedAsync(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            // Ensure database exists
            await _context.Database.EnsureCreatedAsync(ct);

            // Seed in dependency order
            await SeedCustomersAsync(ct);
            await SeedSampleOrdersAsync(ct);

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during database seeding");
            throw;
        }
    }

    private async Task SeedCustomersAsync(CancellationToken cancellationToken)
    {
        // Idempotent seeding - safe to run multiple times
        var existingEmails = await _context.Customers
            .Select(c => c.Email)
            .ToListAsync(cancellationToken);

        var seedData = new[]
        {
            new { Name = "John Doe", Email = "john.doe@email.com", Segment = CustomerSegment.VIP, TotalOrders = 12 },
            new { Name = "Jane Smith", Email = "jane.smith@email.com", Segment = CustomerSegment.Regular, TotalOrders = 5 },
            new { Name = "Bob Johnson", Email = "bob.johnson@email.com", Segment = CustomerSegment.New, TotalOrders = 0 }
        };

        var newCustomers = seedData
            .Where(data => !existingEmails.Contains(data.Email))
            .Select(data => new Customer
            {
                Name = data.Name,
                Email = data.Email,
                Segment = data.Segment,
                TotalOrders = data.TotalOrders
            })
            .ToList();

        if (newCustomers.Any())
        {
            _context.Customers.AddRange(newCustomers);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded {CustomerCount} new customers", newCustomers.Count);
        }
        else
        {
            _logger.LogInformation("All seed customers already exist");
        }
    }

    private async Task SeedSampleOrdersAsync(CancellationToken cancellationToken)
    {
        // Only create sample orders if none exist
        if (await _context.Orders.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Orders already exist, skipping sample order seeding");
            return;
        }

        var customers = await _context.Customers
            .Take(2)
            .ToListAsync(cancellationToken);

        if (!customers.Any())
        {
            _logger.LogWarning("No customers found for sample order seeding");
            return;
        }

        var sampleOrders = customers.Select(customer => new Order
        {
            CustomerId = customer.Id,
            Customer = customer,
            Amount = new Money(Random.Shared.Next(50, 500)),
            Notes = $"Sample order for {customer.Name}"
        }).ToList();

        _context.Orders.AddRange(sampleOrders);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {OrderCount} sample orders", sampleOrders.Count);
    }
}
