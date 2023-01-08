using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace AnnualHealthCheckJs.Results
{
    using Services.Core;
    using ViewModels;

    public class StateResult : ITableResult<StateVM>, IDisposable
    {
        public List<StateVM> GetResult(string search, string sortOrder, int start, int length, IQueryable<StateVM> dtResult, List<string> columnFilters)
        {
            var data = sortOrder.Split(" ");
            PropertyInfo prop;

            if (data.Length == 1)
            {
                prop = getProperty<StateVM>(sortOrder);
                if (prop != null)
                    return FilterResult(search, dtResult, columnFilters).OrderBy(prop.GetValue).Skip(start).Take(length).ToList();
                else
                    return FilterResult(search, dtResult, columnFilters).Skip(start).Take(length).ToList();
            }
            else
            {
                prop = getProperty<StateVM>(data[0]);
                if (prop != null)
                    return FilterResult(search, dtResult, columnFilters).OrderByDescending(prop.GetValue).Skip(start).Take(length).ToList();
                else
                    return FilterResult(search, dtResult, columnFilters).Skip(start).Take(length).ToList();
            }
        }

        public int Count(string search, IQueryable<StateVM> dtResult, List<string> columnFilters)
        {
            return FilterResult(search, dtResult, columnFilters).Count();
        }

        private IQueryable<StateVM> FilterResult(string search, IQueryable<StateVM> dtResult, List<string> columnFilters)
        {
            IQueryable<StateVM> results;   // dtResult.AsQueryable();

            results = dtResult.Where(p => (search == null || (p.Name != null && p.Name.ToLower().Contains(search.ToLower())) || (p.Code != null && p.Code.ToLower().Contains(search.ToLower())) || 
            (p.CountryName != null && p.CountryName.ToLower().Contains(search.ToLower())) || (p.LocationCount.ToString().ToLower().Contains(search.ToLower())))
                && (columnFilters[0] == null || (p.Name != null && p.Name.ToLower().Contains(columnFilters[0].ToLower())))
                && (columnFilters[1] == null || (p.Code != null && p.Code.ToLower().Contains(columnFilters[1].ToLower())))
                && (columnFilters[2] == null || (p.CountryName != null && p.CountryName.ToLower().Contains(columnFilters[2].ToLower())))
                && (columnFilters[3] == null || (p.LocationCount.ToString().ToLower().Contains(columnFilters[3].ToLower())))
            );

            return results;
        }

        private PropertyInfo getProperty<T>(string name)
        {
            var properties = typeof(T).GetProperties();
            PropertyInfo prop = null;
            foreach (var item in properties)
            {
                if (item.Name.ToLower().Equals(name.ToLower()))
                {
                    prop = item;
                    break;
                }
            }
            return prop;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~UserResult() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
