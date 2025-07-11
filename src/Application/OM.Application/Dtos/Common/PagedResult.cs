using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OM.Application.Dtos.Common;
public class PagedResult<T>
{
    public List<T> Items { get; init; } = new();
    public int TotalCount { get; set; }
    public int? Cursor { get; set; }
    public int? PreviousCursor => Cursor;
    public int? NextCursor { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
    public bool IsFirstPage { get; set; }
    public bool IsLastPage { get; set; }
    public int TotalPages { get; set; }

}