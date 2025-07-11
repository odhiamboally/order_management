using OM.Application.Interfaces.ICommon;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.Implementations.Common;
internal class DateTimeService : IDateTime
{
    public DateTimeService()
    {
            
    }

    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}
