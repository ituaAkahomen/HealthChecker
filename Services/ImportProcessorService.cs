using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.Reflection;
using System.Text;

namespace AnnualHealthCheckJs.Services
{
    using Core;
    using Models;
    using Tools;

    public class ImportProcessorService : IImportProcessorService, IDisposable
    {
        public (List<Enrollee> enrollees, List<string> errors, List<string> errRows, int rowCount) ImportEnrollees(ImportTemplate settings, List<(ISheet, Tools.Sheet)> sheets, ExcelTypes etype)
        {
            if (sheets == null)
                throw new Exception($"No sheets could not be found!");

            List<string> errs = null;
            List<string> erows = null;
            List<Enrollee> eels = null;
            int rowcount = 0;

            foreach (var sheet in sheets)
            {
                if (sheet.Item1 == null)
                    throw new Exception($"The sheet {sheet.Item1.SheetName} could not be found!");

                System.Collections.IEnumerator rows;
                if ((rows = sheet.Item1.GetRowEnumerator()) == null)
                    throw new Exception($"No rows in the sheet {sheet.Item1.SheetName}.");

                List<string> hdr = new List<string>();

                while (rows.MoveNext())
                {
                    IRow row;
                    if (etype == ExcelTypes.XLSX)
                        row = (XSSFRow)rows.Current;
                    else if (etype == ExcelTypes.XLS)
                        row = (HSSFRow)rows.Current;
                    else
                        row = (HSSFRow)rows.Current;

                    //List<string> hdrs = new List<string>();
                    if (row.RowNum == sheet.Item2.HeaderRow)
                    {
                        int cellCount = row.LastCellNum;
                        for (int j = 0; j < cellCount; j++)
                        {
                            ICell cell = row.GetCell(j);
                            if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                                continue;

                            hdr.Add(cell.ToString());
                        }

                        // Validation
                        List<string> missingColumns = null;
                        foreach (var column in settings.Rows)
                        {
                            if (!hdr.Contains(column.ColumnName))
                            {
                                // could not find the specified column
                                // Deal breaker

                                if (missingColumns == null)
                                    missingColumns = new List<string>();
                                missingColumns.Add(column.ColumnName);
                            }
                        }

                        if (missingColumns != null)
                        {
                            if (errs == null)
                                errs = new List<string>();

                            errs.Add($"Could not find the following Columns expected: {string.Join(" ,", missingColumns)}");
                            break;
                        }
                    }
                    else if (row.RowNum > sheet.Item2.HeaderRow)
                    {
                        bool isDirty = true;
                        Enrollee item = new Enrollee();
                        foreach (var column in settings.Rows)
                        {
                            object val = GetColumn(hdr, row, column.ColumnName);

                            if (val == null)
                            {
                                if (errs == null)
                                    errs = new List<string>();
                                errs.Add($"Could not find the column '{column.ColumnName}' in the following row: {RowToString(row, hdr)}");
                                continue;
                            }

                            try
                            {
                                AttributeTypes type = AttributeTypes.STRING;
                                if (Enum.TryParse<AttributeTypes>(column.Type.ToUpper(), true, out type))
                                {
                                    // if the type is of DATETIME also look at the format
                                    if (type != AttributeTypes.DATETIME)
                                    {
                                        // hack for now
                                        if (column.ColumnName.ToLower() == "sex")
                                        {
                                            if (val.ToString().ToLower().Trim() == "m" || val.ToString().ToLower().Trim() == "male")
                                                typeof(Enrollee).GetProperty(column.AttributeName).SetValue(item, Gender.MALE, null);
                                            else if (val.ToString().ToLower().Trim() == "f" || val.ToString().ToLower().Trim() == "female")
                                                typeof(Enrollee).GetProperty(column.AttributeName).SetValue(item, Gender.FEMALE, null);
                                            else
                                                typeof(Enrollee).GetProperty(column.AttributeName).SetValue(item, Gender.UNKNOWN, null);
                                        }
                                        else
                                            typeof(Enrollee).GetProperty(column.AttributeName).SetValue(item, Convert.ChangeType(val, type.ConvertToTypeCode()), null);
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(column.Format))
                                        {
                                            DateTime resultDate = DateTime.MinValue;
                                            if (!DateTime.TryParseExact(val.ToString(), new string[] { column.Format, "dd-MMM-yyyy" }, System.Globalization.CultureInfo.CurrentUICulture, System.Globalization.DateTimeStyles.None, out resultDate))
                                            {
                                                // if parsing fails
                                                resultDate = Convert.ToDateTime(Convert.ChangeType(val, type.ConvertToTypeCode()));
                                            }
                                            //var resultDate = DateTime.ParseExact(val.ToString(), column.Format, System.Globalization.CultureInfo.CurrentUICulture);
                                            typeof(Enrollee).GetProperty(column.AttributeName).SetValue(item, resultDate, null);
                                        }
                                        else
                                            typeof(Enrollee).GetProperty(column.AttributeName).SetValue(item, Convert.ChangeType(val, type.ConvertToTypeCode()), null);
                                    }
                                    isDirty = false;
                                }
                                else
                                {
                                    if (errs == null)
                                        errs = new List<string>();
                                    errs.Add($"Could not convert to the appropriate type for '{column.AttributeName}'. Error ocurred in the column '{column.ColumnName}' and the following row: {RowToString(row, hdr)}");
                                }
                            }
                            catch (Exception e)
                            {
                                if (errs == null)
                                    errs = new List<string>();
                                errs.Add($"{e.Message}. Error ocurred in the column '{column.ColumnName}' and the following row: {RowToString(row, hdr)}");
                            }
                        }
                        if (!isDirty)
                        {
                            if (eels == null)
                                eels = new List<Enrollee>();
                            eels.Add(item);
                        }

                        if (errs != null && isDirty)
                        {
                            if (erows == null)
                                erows = new List<string>();
                            erows.Add(RowToString(row, hdr));
                        }

                        rowcount++;
                    }
                }
            }

            return (eels, errs, erows, rowcount);
        }

        public (List<ExEnrolleeModel> excludedEnrollees, List<string> errors, List<string> errRows, int rowCount) ImportExcludedEnrollees(ImportTemplate settings, List<(ISheet, Tools.Sheet)> sheets, ExcelTypes etype)
        {
            if (sheets == null)
                throw new Exception($"No sheets could not be found!");

            List<string> errs = null;
            List<string> erows = null;
            List<ExEnrolleeModel> eels = null;
            int rowcount = 0;

            foreach (var sheet in sheets)
            {
                if (sheet.Item1 == null)
                    throw new Exception($"The sheet {sheet.Item1.SheetName} could not be found!");

                System.Collections.IEnumerator rows;
                if ((rows = sheet.Item1.GetRowEnumerator()) == null)
                    throw new Exception($"No rows in the sheet {sheet.Item1.SheetName}.");

                List<string> hdr = new List<string>();

                while (rows.MoveNext())
                {
                    IRow row;
                    if (etype == ExcelTypes.XLSX)
                        row = (XSSFRow)rows.Current;
                    else if (etype == ExcelTypes.XLS)
                        row = (HSSFRow)rows.Current;
                    else
                        row = (HSSFRow)rows.Current;

                    //List<string> hdrs = new List<string>();
                    if (row.RowNum == sheet.Item2.HeaderRow)
                    {
                        int cellCount = row.LastCellNum;
                        for (int j = 0; j < cellCount; j++)
                        {
                            ICell cell = row.GetCell(j);
                            if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                                continue;

                            hdr.Add(cell.ToString());
                        }

                        // Validation
                        List<string> missingColumns = null;
                        foreach (var column in settings.Rows)
                        {
                            if (!hdr.Contains(column.ColumnName))
                            {
                                // could not find the specified column
                                // Deal breaker

                                if (missingColumns == null)
                                    missingColumns = new List<string>();
                                missingColumns.Add(column.ColumnName);
                            }
                        }

                        if (missingColumns != null)
                        {
                            if (errs == null)
                                errs = new List<string>();

                            errs.Add($"Could not find the following Columns expected: {string.Join(" ,", missingColumns)}");
                            break;
                        }
                    }
                    else if (row.RowNum > sheet.Item2.HeaderRow)
                    {
                        bool isDirty = true;
                        ExEnrolleeModel item = new ExEnrolleeModel();
                        foreach (var column in settings.Rows)
                        {
                            object val = GetColumn(hdr, row, column.ColumnName);

                            if (val == null)
                            {
                                if (errs == null)
                                    errs = new List<string>();
                                errs.Add($"Could not find the column '{column.ColumnName}' in the following row: {RowToString(row, hdr)}");
                                continue;
                            }

                            try
                            {
                                AttributeTypes type = AttributeTypes.STRING;
                                if (Enum.TryParse<AttributeTypes>(column.Type.ToUpper(), true, out type))
                                {
                                    // if the type is of DATETIME also look at the format
                                    typeof(ExEnrolleeModel).GetProperty(column.AttributeName).SetValue(item, Convert.ChangeType(val, type.ConvertToTypeCode()), null);
                                    isDirty = false;
                                }
                                else
                                {
                                    if (errs == null)
                                        errs = new List<string>();
                                    errs.Add($"Could not convert to the appropriate type for '{column.AttributeName}'. Error ocurred in the column '{column.ColumnName}' and the following row: {RowToString(row, hdr)}");
                                }
                            }
                            catch (Exception e)
                            {
                                if (errs == null)
                                    errs = new List<string>();
                                errs.Add($"{e.Message}. Error ocurred in the column '{column.ColumnName}' and the following row: {RowToString(row, hdr)}");
                            }
                        }
                        if (!isDirty)
                        {
                            if (eels == null)
                                eels = new List<ExEnrolleeModel>();
                            eels.Add(item);
                        }

                        if (errs != null && isDirty)
                        {
                            if (erows == null)
                                erows = new List<string>();
                            erows.Add(RowToString(row, hdr));
                        }

                        rowcount++;
                    }
                }
            }

            return (eels, errs, erows, rowcount);
        }

        public (List<ProviderModel> providers, List<string> errors, List<string> errRows, int rowCount) ImportProviders(ImportTemplate settings, List<(ISheet, Tools.Sheet)> sheets, ExcelTypes etype)
        {
            if (sheets == null)
                throw new Exception($"No sheets could not be found!");

            List<string> errs = null;
            List<string> erows = null;
            List<ProviderModel> eels = null;
            int rowcount = 0;

            foreach (var sheet in sheets)
            {
                if (sheet.Item1 == null)
                    throw new Exception($"The sheet {sheet.Item1.SheetName} could not be found!");

                System.Collections.IEnumerator rows;
                if ((rows = sheet.Item1.GetRowEnumerator()) == null)
                    throw new Exception($"No rows in the sheet {sheet.Item1.SheetName}.");

                List<string> hdr = new List<string>();

                while (rows.MoveNext())
                {
                    IRow row;
                    if (etype == ExcelTypes.XLSX)
                        row = (XSSFRow)rows.Current;
                    else if (etype == ExcelTypes.XLS)
                        row = (HSSFRow)rows.Current;
                    else
                        row = (HSSFRow)rows.Current;

                    //List<string> hdrs = new List<string>();
                    if (row.RowNum == sheet.Item2.HeaderRow)
                    {
                        int cellCount = row.LastCellNum;
                        for (int j = 0; j < cellCount; j++)
                        {
                            ICell cell = row.GetCell(j);
                            if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                                continue;

                            hdr.Add(cell.ToString());
                        }

                        // Validation
                        List<string> missingColumns = null;
                        foreach (var column in settings.Rows)
                        {
                            if (!hdr.Contains(column.ColumnName))
                            {
                                // could not find the specified column
                                // Deal breaker

                                if (missingColumns == null)
                                    missingColumns = new List<string>();
                                missingColumns.Add(column.ColumnName);
                            }
                        }

                        if (missingColumns != null)
                        {
                            if (errs == null)
                                errs = new List<string>();

                            errs.Add($"Could not find the following Columns expected: {string.Join(" ,", missingColumns)}");
                            break;
                        }
                    }
                    else if (row.RowNum > sheet.Item2.HeaderRow)
                    {
                        bool isDirty = true;
                        ProviderModel item = new ProviderModel();
                        foreach (var column in settings.Rows)
                        {
                            object val = GetColumn(hdr, row, column.ColumnName);

                            if (val == null)
                            {
                                if (errs == null)
                                    errs = new List<string>();
                                errs.Add($"Could not find the column '{column.ColumnName}' in the following row: {RowToString(row, hdr)}");
                                continue;
                            }

                            try
                            {
                                AttributeTypes type = AttributeTypes.STRING;
                                if (Enum.TryParse<AttributeTypes>(column.Type.ToUpper(), true, out type))
                                {
                                    typeof(ProviderModel).GetProperty(column.AttributeName).SetValue(item, Convert.ChangeType(val, type.ConvertToTypeCode()), null);
                                    isDirty = false;
                                }
                                else
                                {
                                    if (errs == null)
                                        errs = new List<string>();
                                    errs.Add($"Could not convert to the appropriate type for '{column.AttributeName}'. Error ocurred in the column '{column.ColumnName}' and the following row: {RowToString(row, hdr)}");
                                }
                            }
                            catch (Exception e)
                            {
                                if (errs == null)
                                    errs = new List<string>();
                                errs.Add($"{e.Message}. Error ocurred in the column '{column.ColumnName}' and the following row: {RowToString(row, hdr)}");
                            }
                        }
                        if (!isDirty)
                        {
                            if (eels == null)
                                eels = new List<ProviderModel>();
                            eels.Add(item);
                        }

                        if (errs != null && isDirty)
                        {
                            if (erows == null)
                                erows = new List<string>();
                            erows.Add(RowToString(row, hdr));
                        }

                        rowcount++;
                    }
                }
            }

            return (eels, errs, erows, rowcount);
        }

        public (List<ServiceModel> services, List<string> errors, List<string> errRows, int rowCount) ImportServices(ImportTemplate settings, List<(ISheet, Tools.Sheet)> sheets, ExcelTypes etype)
        {
            if (sheets == null)
                throw new Exception($"No sheets could not be found!");

            List<string> errs = null;
            List<string> erows = null;
            List<ServiceModel> eels = null;
            int rowcount = 0;

            foreach (var sheet in sheets)
            {
                if (sheet.Item1 == null)
                    throw new Exception($"The sheet {sheet.Item1.SheetName} could not be found!");

                System.Collections.IEnumerator rows;
                if ((rows = sheet.Item1.GetRowEnumerator()) == null)
                    throw new Exception($"No rows in the sheet {sheet.Item1.SheetName}.");

                List<string> hdr = new List<string>();

                while (rows.MoveNext())
                {
                    IRow row;
                    if (etype == ExcelTypes.XLSX)
                        row = (XSSFRow)rows.Current;
                    else if (etype == ExcelTypes.XLS)
                        row = (HSSFRow)rows.Current;
                    else
                        row = (HSSFRow)rows.Current;

                    //List<string> hdrs = new List<string>();
                    if (row.RowNum == sheet.Item2.HeaderRow)
                    {
                        int cellCount = row.LastCellNum;
                        for (int j = 0; j < cellCount; j++)
                        {
                            ICell cell = row.GetCell(j);
                            if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                                continue;

                            hdr.Add(cell.ToString());
                        }

                        // Validation
                        List<string> missingColumns = null;
                        foreach (var column in settings.Rows)
                        {
                            if (!hdr.Contains(column.ColumnName))
                            {
                                // could not find the specified column
                                // Deal breaker

                                if (missingColumns == null)
                                    missingColumns = new List<string>();
                                missingColumns.Add(column.ColumnName);
                            }
                        }

                        if (missingColumns != null)
                        {
                            if (errs == null)
                                errs = new List<string>();

                            errs.Add($"Could not find the following Columns expected: {string.Join(" ,", missingColumns)}");
                            break;
                        }
                    }
                    else if (row.RowNum > sheet.Item2.HeaderRow)
                    {
                        bool isDirty = true;
                        ServiceModel item = new ServiceModel();
                        foreach (var column in settings.Rows)
                        {
                            object val = GetColumn(hdr, row, column.ColumnName);

                            if (val == null)
                            {
                                if (errs == null)
                                    errs = new List<string>();
                                errs.Add($"Could not find the column '{column.ColumnName}' in the following row: {RowToString(row, hdr)}");
                                continue;
                            }

                            try
                            {
                                AttributeTypes type = AttributeTypes.STRING;
                                if (Enum.TryParse<AttributeTypes>(column.Type.ToUpper(), true, out type))
                                {
                                    if (column.ColumnName.ToLower() == "gender")
                                    {
                                        if (val.ToString().ToLower().Trim() == "m" || val.ToString().ToLower().Trim() == "male")
                                            typeof(ServiceModel).GetProperty(column.AttributeName).SetValue(item, GenderX.MALE, null);
                                        else if (val.ToString().ToLower().Trim() == "f" || val.ToString().ToLower().Trim() == "female")
                                            typeof(ServiceModel).GetProperty(column.AttributeName).SetValue(item, GenderX.FEMALE, null);
                                        else
                                            typeof(ServiceModel).GetProperty(column.AttributeName).SetValue(item, GenderX.BOTH, null);
                                    }
                                    else
                                        typeof(ServiceModel).GetProperty(column.AttributeName).SetValue(item, Convert.ChangeType(val, type.ConvertToTypeCode()), null);
                                    isDirty = false;
                                }
                                else
                                {
                                    if (errs == null)
                                        errs = new List<string>();
                                    errs.Add($"Could not convert to the appropriate type for '{column.AttributeName}'. Error ocurred in the column '{column.ColumnName}' and the following row: {RowToString(row, hdr)}");
                                }
                            }
                            catch (Exception e)
                            {
                                if (errs == null)
                                    errs = new List<string>();
                                errs.Add($"{e.Message}. Error ocurred in the column '{column.ColumnName}' and the following row: {RowToString(row, hdr)}");
                            }
                        }
                        if (!isDirty)
                        {
                            if (eels == null)
                                eels = new List<ServiceModel>();
                            eels.Add(item);
                        }

                        if (errs != null && isDirty)
                        {
                            if (erows == null)
                                erows = new List<string>();
                            erows.Add(RowToString(row, hdr));
                        }

                        rowcount++;
                    }
                }
            }

            return (eels, errs, erows, rowcount);
        }


        private string RowToString(IRow row, IList<string> header)
        {
            StringBuilder sb = new StringBuilder();
            string rtn = string.Empty;
            int i = 0;

            sb.Append("|");
            foreach (var s in header)
            {
                if (row != null)
                {
                    ICell cell = row.GetCell(i);

                    if (cell == null)
                    {
                        sb.Append("|");
                        continue;
                    }

                    sb.Append(cell.ToString());
                }
                else
                    sb.Append("|");
                i++;
            }
            sb.Append("|");

            return sb.ToString();
        }

        private object GetColumn(List<string> header, IRow row, string attrib)
        {
            return GetColumn(header, row, attrib, true);
        }

        private object GetColumn(List<string> header, IRow row, string attrib, bool firstval)
        {
            int i = 0;

            foreach (var s in header)
            {
                if (s.Trim().ToLower() == attrib.Trim().ToLower())
                {
                    if (row != null)
                    {
                        ICell cell = row.GetCell(i);

                        if (cell == null)
                            return null;

                        return cell.ToString();
                    }
                    else
                        return null;
                }
                i++;
            }

            return null;
        }

        private object GetColumn2(List<string> header, IRow row, string attrib)
        {
            return GetColumn2(header, row, attrib, true);
        }

        private object GetColumn2(List<string> header, IRow row, string attrib, bool firstval)
        {
            int i = 0;

            foreach (var s in header)
            {
                if (s.Trim().ToLower() == attrib.Trim().ToLower())
                {
                    if (row != null)
                    {
                        ICell cell = row.GetCell(i);

                        if (cell == null)
                            return string.Empty;

                        return cell.ToString();
                    }
                    else
                        return string.Empty;
                }
                i++;
            }

            return string.Empty;
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
        // ~ImportProcessorService()
        // {
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
