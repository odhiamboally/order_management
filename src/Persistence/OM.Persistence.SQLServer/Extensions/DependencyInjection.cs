using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OM.Domain.EventDispatchers;
using OM.Domain.Interfaces.ICommon;
using OM.Domain.Interfaces.IRepositories;
using OM.Persistence.SQLServer.Context;
using OM.Persistence.SQLServer.Implementations.Common;
using OM.Persistence.SQLServer.Implementations.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Persistence.SQLServer.Extensions;
public static class DependencyInjection
{
    public static IServiceCollection AddInMemoryDB(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<DBContext>(options =>
            options.UseInMemoryDatabase("OMDB")); 

        services.AddScoped<IApplicationDBContext>(provider => provider.GetRequiredService<DBContext>());
            

        // Register DomainEventDispatcher before ApplicationDbContext
        services.AddScoped<DomainEventDispatcher>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();

        return services;
    }
}
