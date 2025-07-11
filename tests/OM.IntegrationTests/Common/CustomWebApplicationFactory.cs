using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OM.Domain.Entities;


using OM.Domain.Enums;

//using Microsoft.VisualStudio.TestPlatform.TestHost;

using OM.Persistence.SQLServer.Context;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.IntegrationTests.Common;
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing database registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add a database context using an in-memory database for testing

            // Add test-specific database
            services.AddDbContext<DBContext>(options =>
            {
                //var dbName = $"TestDb_{Guid.NewGuid()}";
                options.UseInMemoryDatabase("SharedTestDb");
                options.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                    
            });


            // Build service provider and ensure database is created
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DBContext>();
            context.Database.EnsureCreated();

            if (!context.Customers.Any(c => c.Id == 1))
            {
                context.Customers.Add(new Customer
                {
                    Id = 1,
                    Name = "Test Customer",
                    Email = "test@email.com",
                    Segment = CustomerSegment.Regular
                });
                context.SaveChanges();
            }

        });
    }
}
