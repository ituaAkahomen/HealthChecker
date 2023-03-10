using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace AnnualHealthCheckJs.Results
{
    using Services.Core;
    using Models;

    public class EnrolleeResult : ITableResult<Enrollee>, IDisposable
    {
        public List<Enrollee> GetResult(string search, string sortOrder, int start, int length, IQueryable<Enrollee> dtResult, List<string> columnFilters)
        {
            var data = sortOrder.Split(" ");
            PropertyInfo prop;

            if (data.Length == 1)
            {
                prop = getProperty<Enrollee>(sortOrder);
                if (prop != null)
                    return FilterResult(search, dtResult, columnFilters).OrderBy(prop.GetValue).Skip(start).Take(length).ToList();
                else
                    return FilterResult(search, dtResult, columnFilters).Skip(start).Take(length).ToList();
            }
            else
            {
                prop = getProperty<Enrollee>(data[0]);
                if (prop != null)
                    return FilterResult(search, dtResult, columnFilters).OrderByDescending(prop.GetValue).Skip(start).Take(length).ToList();
                else
                    return FilterResult(search, dtResult, columnFilters).Skip(start).Take(length).ToList();
            }
        }

        public int Count(string search, IQueryable<Enrollee> dtResult, List<string> columnFilters)
        {
            return FilterResult(search, dtResult, columnFilters).Count();
        }

        private IQueryable<Enrollee> FilterResult(string search, IQueryable<Enrollee> dtResult, List<string> columnFilters)
        {
            IQueryable<Enrollee> results;   // dtResult.AsQueryable();

            results = dtResult.Where(p => (search == null || (p.EmployeeID != null && p.EmployeeID.ToLower().Contains(search.ToLower())) ||
            (p.LastName != null && p.LastName.ToLower().Contains(search.ToLower())) || (p.OtherNames != null && p.OtherNames.ToLower().Contains(search.ToLower())) || (p.EnrollmentID != null && p.EnrollmentID.ToLower().Contains(search.ToLower())) ||
            ((Enum.GetName(typeof(Gender), p.Gender).ToLower().Contains(search.ToLower()))) || (p.DOB != null && p.DOB.Value.ToString("d MMM yyyy").ToLower().Contains(search.ToLower())) ||
            (p.MobileNumber != null && p.MobileNumber.ToLower().Contains(search.ToLower())) || (p.Email != null && p.Email.ToLower().Contains(search.ToLower())) ||
            (p.Enabled.ToString().ToLower().Contains(search.ToLower())))
                && (columnFilters[0] == null || (p.EmployeeID != null && p.EmployeeID.ToLower().Contains(columnFilters[0].ToLower())))
                && (columnFilters[1] == null || ($"{(!string.IsNullOrEmpty(p.LastName) ? p.LastName.ToLower() : "")} {(!string.IsNullOrEmpty(p.OtherNames) ? p.OtherNames.ToLower() : "")}".Contains(columnFilters[1].ToLower())))
                && (columnFilters[2] == null || (p.EnrollmentID != null ? p.EnrollmentID.ToLower().Contains(columnFilters[2].ToLower()) : true))
                && (columnFilters[3] == null || (Enum.GetName(typeof(Gender), p.Gender).ToLower().Contains(columnFilters[3].ToLower())))
                && (columnFilters[4] == null || (p.DOB != null ? p.DOB.Value.ToString("d MMM yyyy").ToLower().Contains(columnFilters[4].ToLower()) : true))
                && (columnFilters[5] == null || (p.MobileNumber != null ? p.MobileNumber.ToLower().Contains(columnFilters[5].ToLower()) : true))
                && (columnFilters[6] == null || (p.Email != null ? p.Email.ToLower().Contains(columnFilters[6].ToLower()) : true))
                && (columnFilters[7] == null || (p.Enabled.ToString().ToLower().Contains(columnFilters[7].ToLower())))
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

    public class EnrolleeResult2 : ITableResult<Enrollee>, IDisposable
    {
        public List<Enrollee> GetResult(string search, string sortOrder, int start, int length, IQueryable<Enrollee> dtResult, List<string> columnFilters)
        {
            var data = sortOrder.Split(" ");
            PropertyInfo prop;

            if (data.Length == 1)
            {
                prop = getProperty<Enrollee>(sortOrder);
                if (prop != null)
                    return FilterResult(search, dtResult, columnFilters).OrderBy(prop.GetValue).Skip(start).Take(length).ToList();
                else
                    return FilterResult(search, dtResult, columnFilters).Skip(start).Take(length).ToList();
            }
            else
            {
                prop = getProperty<Enrollee>(data[0]);
                if (prop != null)
                    return FilterResult(search, dtResult, columnFilters).OrderByDescending(prop.GetValue).Skip(start).Take(length).ToList();
                else
                    return FilterResult(search, dtResult, columnFilters).Skip(start).Take(length).ToList();
            }
        }

        public int Count(string search, IQueryable<Enrollee> dtResult, List<string> columnFilters)
        {
            return FilterResult(search, dtResult, columnFilters).Count();
        }

        private IQueryable<Enrollee> FilterResult(string search, IQueryable<Enrollee> dtResult, List<string> columnFilters)
        {
            IQueryable<Enrollee> results;   // dtResult.AsQueryable();

            results = dtResult.Where(p => (search == null || (p.EmployeeID != null && p.EmployeeID.ToLower().Contains(search.ToLower())) ||
                        (p.LastName != null && p.LastName.ToLower().Contains(search.ToLower())) || (p.OtherNames != null && p.OtherNames.ToLower().Contains(search.ToLower())) || (p.EnrollmentID != null && p.EnrollmentID.ToLower().Contains(search.ToLower())) ||
                        ((Enum.GetName(typeof(Gender), p.Gender).ToLower().Contains(search.ToLower()))) || (p.DOB != null && p.DOB.Value.ToString("d MMM yyyy").ToLower().Contains(search.ToLower())) ||
                        (p.MobileNumber != null && p.MobileNumber.ToLower().Contains(search.ToLower())) || (p.Email != null && p.Email.ToLower().Contains(search.ToLower())) ||
                        (p.Enabled.ToString().ToLower().Contains(search.ToLower())) || (p.HMO != null && p.HMO.Name.ToLower().Contains(search.ToLower())))
                            && (columnFilters[0] == null || (p.EmployeeID != null && p.EmployeeID.ToLower().Contains(columnFilters[0].ToLower())))
                            && (columnFilters[1] == null || ($"{(!string.IsNullOrEmpty(p.LastName) ? p.LastName.ToLower() : "")} {(!string.IsNullOrEmpty(p.OtherNames) ? p.OtherNames.ToLower() : "")}".Contains(columnFilters[1].ToLower())))
                            && (columnFilters[2] == null || (p.EnrollmentID != null ? p.EnrollmentID.ToLower().Contains(columnFilters[2].ToLower()) : true))
                            && (columnFilters[3] == null || (Enum.GetName(typeof(Gender), p.Gender).ToLower().Contains(columnFilters[3].ToLower())))
                            && (columnFilters[4] == null || (p.DOB != null ? p.DOB.Value.ToString("d MMM yyyy").ToLower().Contains(columnFilters[4].ToLower()) : true))
                            && (columnFilters[5] == null || (p.MobileNumber != null ? p.MobileNumber.ToLower().Contains(columnFilters[5].ToLower()) : true))
                            && (columnFilters[6] == null || (p.Email != null ? p.Email.ToLower().Contains(columnFilters[6].ToLower()) : true))
                            && (columnFilters[7] == null || (p.Enabled.ToString().ToLower().Contains(columnFilters[7].ToLower())))
                            && (columnFilters[8] == null || (p.HMO != null ? p.HMO.Name.ToLower().Contains(columnFilters[8].ToLower()) : true))
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
