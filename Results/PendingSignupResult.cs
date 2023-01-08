using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace AnnualHealthCheckJs.Results
{
    using Services.Core;
    using Models;

    public class PendingSignupResult : ITableResult<SignUp>, IDisposable
    {
        public List<SignUp> GetResult(string search, string sortOrder, int start, int length, IQueryable<SignUp> dtResult, List<string> columnFilters)
        {
            var data = sortOrder.Split(" ");
            PropertyInfo prop;

            if (data.Length == 1)
            {
                prop = getProperty<SignUp>(sortOrder);
                if (prop != null)
                    return FilterResult(search, dtResult, columnFilters).OrderBy(prop.GetValue).Skip(start).Take(length).ToList();
                else
                    return FilterResult(search, dtResult, columnFilters).Skip(start).Take(length).ToList();
            }
            else
            {
                prop = getProperty<SignUp>(data[0]);
                if (prop != null)
                    return FilterResult(search, dtResult, columnFilters).OrderByDescending(prop.GetValue).Skip(start).Take(length).ToList();
                else
                    return FilterResult(search, dtResult, columnFilters).Skip(start).Take(length).ToList();
            }
        }

        public int Count(string search, IQueryable<SignUp> dtResult, List<string> columnFilters)
        {
            return FilterResult(search, dtResult, columnFilters).Count();
        }

        private IQueryable<SignUp> FilterResult(string search, IQueryable<SignUp> dtResult, List<string> columnFilters)
        {
            IQueryable<SignUp> results;   // dtResult.AsQueryable();

            results = dtResult.Where(p => (search == null || (p.Enrollee.EmployeeID != null && p.Enrollee.EmployeeID.ToLower().Contains(search.ToLower())) ||
            (p.Enrollee.LastName != null && p.Enrollee.LastName.ToLower().Contains(search.ToLower())) || (p.Enrollee.OtherNames != null && p.Enrollee.OtherNames.ToLower().Contains(search.ToLower())) || 
            (p.Enrollee.EnrollmentID != null && p.Enrollee.EnrollmentID.ToLower().Contains(search.ToLower())) ||
            ((Enum.GetName(typeof(Gender), p.Enrollee.Gender).ToLower().Contains(search.ToLower()))))
                && (columnFilters[0] == null || (p.Enrollee.EmployeeID != null && p.Enrollee.EmployeeID.ToLower().Contains(columnFilters[0].ToLower())))
                && (columnFilters[1] == null || ($"{p.Enrollee.LastName.ToLower()} {p.Enrollee.OtherNames.ToLower()}".Contains(columnFilters[1].ToLower())))
                && (columnFilters[2] == null || (p.Enrollee.EnrollmentID != null ? p.Enrollee.EnrollmentID.ToLower().Contains(columnFilters[2].ToLower()) : true))
                && (columnFilters[3] == null || (Enum.GetName(typeof(Gender), p.Enrollee.Gender).ToLower().Contains(columnFilters[3].ToLower())))
                && (columnFilters[4] == null || (p.DateUpdated != null ? p.DateUpdated.Value.ToString("d MMM yyyy h:mm tt").Contains(columnFilters[4].ToLower()) : true))
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
