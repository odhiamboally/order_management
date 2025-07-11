using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Domain.Interfaces.ICommon;


public interface IDatabaseSeeder
{
    Task SeedAsync();
    Task SeedAsync(CancellationToken ct);
}
