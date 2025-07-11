using Asp.Versioning;

using FluentValidation;

using OM.Api.Middleware;
using OM.Application.Configurations;

namespace OM.Api.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            var assembly = typeof(Program).Assembly;

            services.Configure<JsonSettings>(configuration.GetSection(""));

            services.AddExceptionHandler<ApiExceptionHandler>();
            services.AddProblemDetails();

            // Basic API services
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddWebEncoders();
            services.AddHttpClient();
            services.AddHttpContextAccessor();

            services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
                config.ApiVersionReader = new UrlSegmentApiVersionReader();
                config.ApiVersionReader = ApiVersionReader.Combine(
                        new UrlSegmentApiVersionReader(),
                        new QueryStringApiVersionReader("version"),
                        new HeaderApiVersionReader("X-Version"));
            });

            services.AddValidatorsFromAssembly(assembly);

            return services;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
