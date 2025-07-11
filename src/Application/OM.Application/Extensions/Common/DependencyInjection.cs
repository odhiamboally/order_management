using FluentValidation;

using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using OM.Application.Behaviours;
using OM.Application.Configurations;
using OM.Application.Implementations.Common;
using OM.Application.Implementations.Services;
using OM.Application.Implementations.Strategies;
using OM.Application.Interfaces.ICommon;
using OM.Application.Interfaces.IServices;
using OM.Application.Interfaces.IStrategies;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OM.Application.Extensions.Common;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            services.Configure<JsonSettings>(configuration.GetSection("JsonSettings"));

            
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));


            services.AddScoped<IServiceManager, ServiceManager>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IDiscountService, DiscountService>();
            services.AddScoped<IDiscountStrategy, VipDiscountStrategy>();
            services.AddScoped<IDiscountStrategy, LoyaltyDiscountStrategy>();
            services.AddScoped<IDiscountStrategy, BulkOrderDiscountStrategy>();
            services.AddScoped<IDateTime, DateTimeService>();

            services.AddScoped(serviceProvider => serviceProvider.GetServices<IDiscountStrategy>().ToList());
    

            services.AddMemoryCache();
            services.AddSingleton<ICacheService, InMemoryCacheService>();
            services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();

            return services;
        }
        catch (Exception)
        {

            throw;
        }

    }

}
