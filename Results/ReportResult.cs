using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace AnnualHealthCheckJs.Results
{
    using Services.Core;
    using Models;
    using Tools;

    public class GenderUtilizationReportResult : ITableResult<GenderUtilizationReportModel>, IDisposable
    {
        public List<GenderUtilizationReportModel> GetResult(string search, string sortOrder, int start, int length, IQueryable<GenderUtilizationReportModel> dtResult, List<string> columnFilters)
        {
            var data = sortOrder.Split(" ");
            PropertyInfo prop;

            if (data.Length == 1)
            {
                prop = getProperty<GenderUtilizationReportModel>(sortOrder);
                if (prop != null)
                    return FilterResult(search, dtResult, columnFilters).OrderBy(prop.GetValue).Skip(start).Take(length).ToList();
                else
                    return FilterResult(search, dtResult, columnFilters).Skip(start).Take(length).ToList();
            }
            else
            {
                prop = getProperty<GenderUtilizationReportModel>(data[0]);
                if (prop != null)
                    return FilterResult(search, dtResult, columnFilters).OrderByDescending(prop.GetValue).Skip(start).Take(length).ToList();
                else
                    return FilterResult(search, dtResult, columnFilters).Skip(start).Take(length).ToList();
            }
        }

        public int Count(string search, IQueryable<GenderUtilizationReportModel> dtResult, List<string> columnFilters)
        {
            return FilterResult(search, dtResult, columnFilters).Count();
        }

        private IEnumerable<GenderUtilizationReportModel> FilterResult(string search, IEnumerable<GenderUtilizationReportModel> dtResult, List<string> columnFilters)
        {
            IEnumerable<GenderUtilizationReportModel> results;   // dtResult.AsQueryable();

            results = dtResult.Where(p => (search == null || (p.Name != null && p.Name.ToLower().Contains(search.ToLower())) ||
            (p.PercentUtilization.ToString().ToLower().Contains(search.ToLower())) || (p.UtilizationCount.ToString().ToLower().Contains(search.ToLower())) ||
            (p.TotalCount.ToString().ToLower().Contains(search.ToLower())))
                && (columnFilters[0] == null || (p.Name != null && p.Name.ToLower().Contains(columnFilters[0].ToLower())))
                && (columnFilters[1] == null || (p.PercentUtilization.ToString().ToLower().Contains(columnFilters[1].ToLower())))
                && (columnFilters[2] == null || (p.UtilizationCount.ToString().ToLower().Contains(columnFilters[2].ToLower())))
                && (columnFilters[3] == null || (p.TotalCount.ToString().ToLower().Contains(columnFilters[3].ToLower())))
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

    public class UtilizationByAgeRangeReportResult : ITableResult<GenderUtilizationReportModel>, IDisposable
    {
        public List<GenderUtilizationReportModel> GetResult(string search, string sortOrder, int start, int length, IQueryable<GenderUtilizationReportModel> dtResult, List<string> columnFilters)
        {
            var data = sortOrder.Split(" ");
            PropertyInfo prop;

            if (data.Length == 1)
            {
                prop = getProperty<GenderUtilizationReportModel>(sortOrder);
                if (prop != null)
                    return FilterResult(search, dtResult, columnFilters).OrderBy(prop.GetValue).Skip(start).Take(length).ToList();
                else
                    return FilterResult(search, dtResult, columnFilters).Skip(start).Take(length).ToList();
            }
            else
            {
                prop = getProperty<GenderUtilizationReportModel>(data[0]);
                if (prop != null)
                    return FilterResult(search, dtResult, columnFilters).OrderByDescending(prop.GetValue).Skip(start).Take(length).ToList();
                else
                    return FilterResult(search, dtResult, columnFilters).Skip(start).Take(length).ToList();
            }
        }

        public int Count(string search, IQueryable<GenderUtilizationReportModel> dtResult, List<string> columnFilters)
        {
            return FilterResult(search, dtResult, columnFilters).Count();
        }

        private IEnumerable<GenderUtilizationReportModel> FilterResult(string search, IEnumerable<GenderUtilizationReportModel> dtResult, List<string> columnFilters)
        {
            IEnumerable<GenderUtilizationReportModel> results;   // dtResult.AsQueryable();

            results = dtResult.Where(p => (search == null || (p.Name != null && p.Name.ToLower().Contains(search.ToLower())) || (p.UtilizationCount.ToString().ToLower().Contains(search.ToLower())))
                && (columnFilters[0] == null || (p.Name != null && p.Name.ToLower().Contains(columnFilters[0].ToLower())))
                && (columnFilters[1] == null || (p.UtilizationCount.ToString().ToLower().Contains(columnFilters[1].ToLower())))
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

    public class RatingReportResult : ITableResult<RatingReportModel>, IDisposable
    {
        public List<RatingReportModel> GetResult(string search, string sortOrder, int start, int length, IQueryable<RatingReportModel> dtResult, List<string> columnFilters)
        {
            var data = sortOrder.Split(" ");
            PropertyInfo prop;

            if (data.Length == 1)
            {
                prop = getProperty<RatingReportModel>(sortOrder);
                if (prop != null)
                    return FilterResult(search, dtResult, columnFilters).OrderBy(prop.GetValue).Skip(start).Take(length).ToList();
                else
                    return FilterResult(search, dtResult, columnFilters).Skip(start).Take(length).ToList();
            }
            else
            {
                prop = getProperty<RatingReportModel>(data[0]);
                if (prop != null)
                    return FilterResult(search, dtResult, columnFilters).OrderByDescending(prop.GetValue).Skip(start).Take(length).ToList();
                else
                    return FilterResult(search, dtResult, columnFilters).Skip(start).Take(length).ToList();
            }
        }

        public int Count(string search, IQueryable<RatingReportModel> dtResult, List<string> columnFilters)
        {
            return FilterResult(search, dtResult, columnFilters).Count();
        }

        private IEnumerable<RatingReportModel> FilterResult(string search, IEnumerable<RatingReportModel> dtResult, List<string> columnFilters)
        {
            IEnumerable<RatingReportModel> results;   // dtResult.AsQueryable();

            results = dtResult.Where(p => (search == null || (p.Name != null && p.Name.ToLower().Contains(search.ToLower())) || (p.AverageRating.ToString().ToLower().Contains(search.ToLower())))
                && (columnFilters[0] == null || (p.Name != null && p.Name.ToLower().Contains(columnFilters[0].ToLower())))
                && (columnFilters[1] == null || (p.AverageRating.ToString().ToLower().Contains(columnFilters[1].ToLower())))
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
