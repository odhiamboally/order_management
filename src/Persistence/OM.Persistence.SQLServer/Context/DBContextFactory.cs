using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using OM.Domain.EventDispatchers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Persistence.SQLServer.Context;

public class DBContextFactory : IDesignTimeDbContextFactory<DBContext>
{
    private readonly DomainEventDispatcher _domainEventDispatcher;

    public DBContextFactory(IMediator mediator, ILogger<DomainEventDispatcher> logger)
    {
        _domainEventDispatcher = new DomainEventDispatcher(mediator, logger);
    }

    public DBContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<DBContext>();
        var connectionString = configuration.GetConnectionString("LocalIdConn");

        optionsBuilder.UseSqlServer(connectionString);

        return new DBContext(optionsBuilder.Options, _domainEventDispatcher);
    }
}
