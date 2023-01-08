using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Services.Core
{
    public interface ITableResult<T>
    {
        List<T> GetResult(string search, string sortOrder, int start, int length, IQueryable<T> dtResult, List<string> columnFilters);
        int Count(string search, IQueryable<T> dtResult, List<string> columnFilters);
    }
}
