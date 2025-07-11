using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Application.Helpers;

public static class DateRangeHelper
{
    public static (DateTime start, DateTime end) Normalize(DateTimeOffset start, DateTimeOffset end)
    {
        return (start.UtcDateTime.Date, end.UtcDateTime.Date.AddDays(1));
    }
}
