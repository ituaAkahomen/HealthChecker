using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NPOI.SS.UserModel;

namespace AnnualHealthCheckJs.Services.Core
{
    using Models;
    using Tools;

    public interface IImportProcessorService
    {
        (List<Enrollee> enrollees, List<string> errors, List<string> errRows, int rowCount) ImportEnrollees(ImportTemplate settings, List<(ISheet, Tools.Sheet)> sheets, ExcelTypes etype);
        (List<ExEnrolleeModel> excludedEnrollees, List<string> errors, List<string> errRows, int rowCount) ImportExcludedEnrollees(ImportTemplate settings, List<(ISheet, Tools.Sheet)> sheets, ExcelTypes etype);
        (List<ProviderModel> providers, List<string> errors, List<string> errRows, int rowCount) ImportProviders(ImportTemplate settings, List<(ISheet, Tools.Sheet)> sheets, ExcelTypes etype);
        (List<ServiceModel> services, List<string> errors, List<string> errRows, int rowCount) ImportServices(ImportTemplate settings, List<(ISheet, Tools.Sheet)> sheets, ExcelTypes etype);
    }
}
