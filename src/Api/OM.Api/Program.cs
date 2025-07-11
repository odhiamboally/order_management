using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using OM.Api.Extensions;
using OM.Api.Filters;
using OM.Application.Extensions.Common;
using OM.Domain.Entities;
using OM.Domain.Enums;
using OM.Domain.Interfaces.ICommon;
using OM.Domain.ValueObjects;
using OM.Persistence.SQLServer.Context;
using OM.Persistence.SQLServer.Extensions;

using Scalar.AspNetCore;

using System.Text.Json;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

string corsPolicy = "ApiCorsPolicy";

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddInMemoryDB(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);


builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiExceptionFilterAttribute>();
})
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// CORS for API clients
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["https://localhost:3000", "https://localhost:5173"]; // Default for dev

        if (builder.Environment.IsDevelopment())
        {
            // More permissive in development
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            // Strict in production
            policy.WithOrigins(allowedOrigins)
                  .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                  .WithHeaders("Content-Type", "Authorization", "X-Requested-With")
                  .AllowCredentials();
        }
    });
});


builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, _) =>
    {
        document.Info = new()
        {
            Title = "Order Management API",
            Version = "v1.0",
            Description = """
                A comprehensive order management system 
                with discounting and analytics features
                """,
            Contact = new()
            {
                Name = "Development Team",
                Email = "dev@orderManagement.com",
            }
        };

        document.Servers = new List<OpenApiServer>
        {
            new() { Url = "https://localhost:7208", Description = "Development HTTPS" },
            new() { Url = "http://localhost:5170", Description = "Development HTTP" }
        };

        return Task.CompletedTask;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    //app.UseDeveloperExceptionPage();
    app.UseExceptionHandler((_ => { }));
}
else
{
    app.UseExceptionHandler((_ => { }));
    //app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors(corsPolicy);

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().CacheOutput();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Order Management API")
               .WithSidebar(true)
               .WithTheme(ScalarTheme.Purple)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
               .AddPreferredSecuritySchemes("https");
    });

    // Redirect root to API docs in development
    app.MapGet("/", () => Results.Redirect("/scalar/v1"))
       .ExcludeFromDescription()
       .AllowAnonymous();
}

app.MapControllers();

try
{
    //await SeedDatabaseAsync(app);
    await SeedDatabaseWithSeederServiceAsync(app);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while seeding the database");
    // Don't throw - let the app continue without seeding
}

app.Run();



static async Task SeedDatabaseWithSeederServiceAsync(WebApplication app)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database seeding failed");
        // Don't crash the app, just log the error
    }
}

static async Task SeedDatabaseAsync(WebApplication app)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Starting database seeding...");

            // Get the context using the actual registered type name
            var context = serviceProvider.GetRequiredService<DBContext>();

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Call seeding method that's separate from SaveChanges
            await SeedCustomersAsync(context, logger);
            await SeedOrdersAsync(context, logger);


            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            // Log but don't crash the application
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Error occurred during database seeding");
            throw; // Re-throw to be caught by the caller
        }
    }

static async Task SeedCustomersAsync(DBContext context, ILogger logger)
    {
        // Protection #1: Check if ANY customers exist
        if (await context.Customers.AnyAsync())
        {
            logger.LogInformation("Customers already exist, skipping customer seeding");
            return;
        }

        logger.LogInformation("Seeding customers...");

        var customersToSeed = new[]
        {
        new { Name = "John Doe", Email = "john.doe@email.com", Segment = CustomerSegment.VIP, TotalOrders = 12 },
        new { Name = "Jane Smith", Email = "jane.smith@email.com", Segment = CustomerSegment.Regular, TotalOrders = 5 },
        new { Name = "Bob Johnson", Email = "bob.johnson@email.com", Segment = CustomerSegment.New, TotalOrders = 0 }
    };

        var customers = new List<Customer>();

        foreach (var customerData in customersToSeed)
        {
            // Protection #2: Check individual email uniqueness
            var existingCustomer = await context.Customers
                .FirstOrDefaultAsync(c => c.Email == customerData.Email);

            if (existingCustomer == null)
            {
                customers.Add(new Customer
                {
                    Name = customerData.Name,
                    Email = customerData.Email,
                    Segment = customerData.Segment,
                    TotalOrders = customerData.TotalOrders
                });
            }
            else
            {
                logger.LogWarning("Customer with email {Email} already exists, skipping", customerData.Email);
            }
        }

        if (customers.Any())
        {
            context.Customers.AddRange(customers);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {CustomerCount} new customers", customers.Count);
        }
        else
        {
            logger.LogInformation("No new customers to seed");
        }
    }

static async Task SeedOrdersAsync(DBContext context, ILogger logger)
{
    // Only seed orders if we have customers but no orders
    var customerCount = await context.Customers.CountAsync();
    var orderCount = await context.Orders.CountAsync();

    if (customerCount == 0)
    {
        logger.LogWarning("No customers found, skipping order seeding");
        return;
    }

    if (orderCount > 0)
    {
        logger.LogInformation("Orders already exist, skipping order seeding");
        return;
    }

    logger.LogInformation("Seeding sample orders...");

    var customers = await context.Customers.ToListAsync();
    var orders = new List<Order>();

    // Create sample orders for existing customers
    foreach (var customer in customers.Take(2)) // Only first 2 customers
    {
        var order = new Order
        {
            CustomerId = customer.Id,
            Customer = customer,
            Amount = new Money(100m),
            Notes = $"Sample order for {customer.Name}"
        };

        // Add sample items
        order.AddItem(new OrderItem
        {
            ProductName = "Sample Product",
            Price = new Money(50m),
            Quantity = 2
        });

        orders.Add(order);
    }

    if (orders.Any())
    {
        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {OrderCount} sample orders", orders.Count);
    }
}


public partial class Program { }
