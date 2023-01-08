using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using NToastNotify;
using Humanizer;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;

namespace AnnualHealthCheckJs.Controllers
{
    using Data;
    using Models;
    using Services.Core;
    using Tools;
    using Results;
    using ViewModels;

    public class ReportsController : Controller
    {
        private readonly GenderUtilizationReportResult _gurResult;
        private readonly UtilizationByAgeRangeReportResult _uarResult;
        private readonly RatingReportResult _rResult;

        private ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportsController(GenderUtilizationReportResult gurResult,
            UtilizationByAgeRangeReportResult uarResult,
            RatingReportResult rResult,
            ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _uarResult = uarResult;
            _gurResult = gurResult;
            _rResult = rResult;
            _context = context;
            _userManager = userManager;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);


        public async Task<IActionResult> Index()
        {
            var rvm = TempData.Get<ReportViewModel>("rvm");
            if (rvm == null)
            {
                var config = await _context.ProjectConfig.FirstOrDefaultAsync();
                rvm = new ReportViewModel() { StartDate = config.StartDate, EndDate = config.EndDate };
            }

            TempData.Put("rvm", rvm);

            return View(rvm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string range)
        {
            var rvm = TempData.Get<ReportViewModel>("rvm");

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "There was an error. Please select your date range again!");

                if (rvm == null)
                {
                    var config = await _context.ProjectConfig.FirstOrDefaultAsync();
                    rvm = new ReportViewModel() { StartDate = config.StartDate, EndDate = config.EndDate };
                }
                return View(rvm);
            }

            if (string.IsNullOrEmpty(range))
            {
                ModelState.AddModelError("", "Please select your date range again!");

                if (rvm == null)
                {
                    var config = await _context.ProjectConfig.FirstOrDefaultAsync();
                    rvm = new ReportViewModel() { StartDate = config.StartDate, EndDate = config.EndDate };
                }
                return View(rvm);
            }

            DateTime tmp;
            var rangeX = range.Split("to").Where(s => DateTime.TryParse(s.Trim(), out tmp)).Select(s => DateTime.Parse(s.Trim()));
            if (!rangeX.Any())
            {
                ModelState.AddModelError("", "Please select a date range!");

                var config = await _context.ProjectConfig.FirstOrDefaultAsync();
                rvm = new ReportViewModel() { StartDate = config.StartDate, EndDate = config.EndDate };
            }

            if (rangeX.Count() != 2)
            {
                ModelState.AddModelError("", "Your date range is wrong, Please select a proper date range!");

                var config = await _context.ProjectConfig.FirstOrDefaultAsync();
                rvm = new ReportViewModel() { StartDate = config.StartDate, EndDate = config.EndDate };
            }
            else
                rvm = new ReportViewModel() { StartDate = rangeX.ElementAt(0), EndDate = rangeX.ElementAt(1) };

            TempData.Put("rvm", rvm);
            return View(rvm);
        }

        public async Task<IActionResult> ExportReport()
        {
            var rvm = TempData.Get<ReportViewModel>("rvm");
            if (rvm == null)
            {
                var config = await _context.ProjectConfig.FirstOrDefaultAsync();
                rvm = new ReportViewModel() { StartDate = config.StartDate, EndDate = config.EndDate };
            }

            IWorkbook workbook;
            workbook = new XSSFWorkbook();

            var memory = new MemoryStream();

            ISheet excelSheet = workbook.CreateSheet("Reports");

            int rowCount = 0;
            IRow row = excelSheet.CreateRow(rowCount);

            XSSFCellStyle titlecellStyle = (XSSFCellStyle)workbook.CreateCellStyle();       //.CreateCellStyle();
            var titlefont = titlecellStyle.GetFont(workbook);
            titlefont.FontHeightInPoints = (short)15;
            titlefont.IsBold = true;
            titlecellStyle.SetFont(titlefont);

            string title = string.Empty;
            var user = await GetCurrentUserAsync();
            switch (user.ProfileType)
            {
                case ProfileTypes.ADMIN:
                    var hmo = _context.HMOs.FirstOrDefault(h => h.ID == user.HMOID);
                    title = hmo.Name.ToUpper();
                    break;
                case ProfileTypes.PROVIDER:
                    break;
                default:
                    title = "ALL HMOS";
                    break;
            }

            var titlecell = row.CreateCell(2);
            titlecell.SetCellValue($"REPORT FOR {title} BETWEEN {rvm.StartDate.ToString("dd MMM, yyyy").ToUpper()} TO {rvm.EndDate.ToString("dd MMM, yyyy").ToUpper()}");
            titlecell.CellStyle = titlecellStyle;

            XSSFCellStyle subtitlecellStyle = (XSSFCellStyle)workbook.CreateCellStyle();       //.CreateCellStyle();
            var subtitlefont = subtitlecellStyle.GetFont(workbook);
            subtitlefont.FontHeightInPoints = (short)13;
            subtitlefont.IsBold = true;
            subtitlecellStyle.SetFont(subtitlefont);

            // Statistics
            rowCount += 3;
            row = excelSheet.CreateRow(rowCount);
            var statscell = row.CreateCell(1);
            statscell.SetCellValue("STATISTICS");
            statscell.CellStyle = subtitlecellStyle;

            XSSFCellStyle cellStyle = (XSSFCellStyle)workbook.CreateCellStyle();       //.CreateCellStyle();
            var font = cellStyle.GetFont(workbook);
            font.FontHeightInPoints = (short)11;
            font.IsBold = false;
            cellStyle.SetFont(font);

            var stats = await getStatistics();

            rowCount += 1;
            row = excelSheet.CreateRow(rowCount);

            var icell = row.CreateCell(1);
            icell.SetCellValue("TOTAL ENROLLEES");
            icell.CellStyle = cellStyle;
            icell = row.CreateCell(2);
            icell.SetCellValue(stats.enrollees);
            icell.CellStyle = cellStyle;

            rowCount += 1;
            row = excelSheet.CreateRow(rowCount);

            icell = row.CreateCell(1);
            icell.SetCellValue("ALL SIGNED UPS");
            icell.CellStyle = cellStyle;
            icell = row.CreateCell(2);
            icell.SetCellValue(stats.sall);
            icell.CellStyle = cellStyle;

            rowCount += 1;
            row = excelSheet.CreateRow(rowCount);

            icell = row.CreateCell(1);
            icell.SetCellValue("SIGNUP WITH APPOINTMENT");
            icell.CellStyle = cellStyle;
            icell = row.CreateCell(2);
            icell.SetCellValue(stats.signeds);
            icell.CellStyle = cellStyle;

            rowCount += 1;
            row = excelSheet.CreateRow(rowCount);

            icell = row.CreateCell(1);
            icell.SetCellValue("EXCLUDED ENROLLEES");
            icell.CellStyle = cellStyle;
            icell = row.CreateCell(2);
            icell.SetCellValue(stats.excluded);
            icell.CellStyle = cellStyle;

            rowCount += 1;
            row = excelSheet.CreateRow(rowCount);

            icell = row.CreateCell(1);
            icell.SetCellValue("PROVIDERS");
            icell.CellStyle = cellStyle;
            icell = row.CreateCell(2);
            icell.SetCellValue(stats.providers);
            icell.CellStyle = cellStyle;

            rowCount += 1;
            row = excelSheet.CreateRow(rowCount);

            icell = row.CreateCell(1);
            icell.SetCellValue("LOCATIONS");
            icell.CellStyle = cellStyle;
            icell = row.CreateCell(2);
            icell.SetCellValue(stats.locations);
            icell.CellStyle = cellStyle;

            // Gender Report
            rowCount += 3;

            row = excelSheet.CreateRow(rowCount);
            var sectioncell = row.CreateCell(1);
            sectioncell.SetCellValue("UTILIZATION BY GENDER");
            sectioncell.CellStyle = subtitlecellStyle;

            rowCount += 1;
            row = excelSheet.CreateRow(rowCount);

            XSSFCellStyle headercellStyle = (XSSFCellStyle)workbook.CreateCellStyle();      //.CreateCellStyle();         
            var headerfont = headercellStyle.GetFont(workbook);
            headerfont.FontHeightInPoints = (short)11;
            headerfont.IsBold = true;
            headercellStyle.SetFont(headerfont);

            var headercell = row.CreateCell(0);
            headercell.SetCellValue("S/N");
            headercell.CellStyle = headercellStyle;

            headercell = row.CreateCell(1);
            headercell.SetCellValue("NAME");
            headercell.CellStyle = headercellStyle;

            headercell = row.CreateCell(2);
            headercell.SetCellValue("PERCENTAGE UTILIZATION");
            headercell.CellStyle = headercellStyle;

            headercell = row.CreateCell(3);
            headercell.SetCellValue("UTILIZATION COUNT");
            headercell.CellStyle = headercellStyle;

            headercell = row.CreateCell(4);
            headercell.SetCellValue("TOTAL");
            headercell.CellStyle = headercellStyle;


            var dtsource = getUtilizationByGender(await GetCurrentUserAsync(), rvm.StartDate, rvm.EndDate);

            int sn = 0;
            foreach (var dt in dtsource)
            {
                row = excelSheet.CreateRow(++rowCount);

                var cell = row.CreateCell(0);
                cell.SetCellValue(++sn);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(1);
                cell.SetCellValue(dt.Name.ToUpper());
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(2);
                cell.SetCellValue($"{dt.PercentUtilization}%");
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(3);
                cell.SetCellValue($"{dt.UtilizationCount}");
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(4);
                cell.SetCellValue($"{dt.TotalCount}");
                cell.CellStyle = cellStyle;
            }

            // Age Report
            rowCount += 3;

            row = excelSheet.CreateRow(rowCount);
            sectioncell = row.CreateCell(1);
            sectioncell.SetCellValue("UTILIZATION BY AGE RANGE");
            sectioncell.CellStyle = subtitlecellStyle;

            rowCount += 1;
            row = excelSheet.CreateRow(rowCount);

            headercell = row.CreateCell(0);
            headercell.SetCellValue("S/N");
            headercell.CellStyle = headercellStyle;

            headercell = row.CreateCell(1);
            headercell.SetCellValue("NAME");
            headercell.CellStyle = headercellStyle;

            headercell = row.CreateCell(2);
            headercell.SetCellValue("PERCENTAGE UTILIZATION");
            headercell.CellStyle = headercellStyle;

            headercell = row.CreateCell(3);
            headercell.SetCellValue("UTILIZATION COUNT");
            headercell.CellStyle = headercellStyle;

            headercell = row.CreateCell(4);
            headercell.SetCellValue("TOTAL");
            headercell.CellStyle = headercellStyle;

            dtsource = getUtilizationByAgeRange(await GetCurrentUserAsync(), rvm.StartDate, rvm.EndDate);

            sn = 0;
            foreach (var dt in dtsource)
            {
                row = excelSheet.CreateRow(++rowCount);

                var cell = row.CreateCell(0);
                cell.SetCellValue(++sn);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(1);
                cell.SetCellValue(dt.Name.ToUpper());
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(2);
                cell.SetCellValue($"{dt.PercentUtilization}%");
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(3);
                cell.SetCellValue($"{dt.UtilizationCount}");
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(4);
                cell.SetCellValue($"{dt.TotalCount}");
                cell.CellStyle = cellStyle;
            }

            // Rating Report
            rowCount += 3;

            row = excelSheet.CreateRow(rowCount);
            sectioncell = row.CreateCell(1);
            sectioncell.SetCellValue("AVERAGE RATING");
            sectioncell.CellStyle = subtitlecellStyle;

            rowCount += 1;
            row = excelSheet.CreateRow(rowCount);

            headercell = row.CreateCell(0);
            headercell.SetCellValue("S/N");
            headercell.CellStyle = headercellStyle;

            headercell = row.CreateCell(1);
            headercell.SetCellValue("PROVIDER NAME");
            headercell.CellStyle = headercellStyle;

            headercell = row.CreateCell(2);
            headercell.SetCellValue("AVERAGE RATING");
            headercell.CellStyle = headercellStyle;

            var ratingsource = getRating(await GetCurrentUserAsync(), rvm.StartDate, rvm.EndDate);

            sn = 0;
            foreach (var dt in ratingsource)
            {
                row = excelSheet.CreateRow(++rowCount);

                var cell = row.CreateCell(0);
                cell.SetCellValue(++sn);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(1);
                cell.SetCellValue(dt.Name.ToUpper());
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(2);
                cell.SetCellValue($"{dt.AverageRating}");
                cell.CellStyle = cellStyle;
            }

            workbook.Write(memory);
            var buffer = memory.ToArray();

            TempData.Put("rvm", rvm);

            //memory.Position = 0;
            var sFileName = $"reports_{DateTime.Now.Ticks.ToString()}.xlsx";
            return File(buffer, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", sFileName);
        }

        #region Tables

        [HttpPost]
        public async Task<IActionResult> GenderUtilizationList([FromBody] DTParameters param)
        {
            var rvm = TempData.Get<ReportViewModel>("rvm");
            if (rvm == null)
            {
                var config = await _context.ProjectConfig.FirstOrDefaultAsync();
                rvm = new ReportViewModel() { StartDate = config.StartDate, EndDate = config.EndDate };
            }

            var dtsource = getUtilizationByGender(await GetCurrentUserAsync(), rvm.StartDate, rvm.EndDate);

            return UtilizationByGenderResultData(param, dtsource);
        }

        [HttpPost]
        public async Task<IActionResult> AgeRangeUtilizationList([FromBody] DTParameters param)
        {
            var rvm = TempData.Get<ReportViewModel>("rvm");
            if (rvm == null)
            {
                var config = await _context.ProjectConfig.FirstOrDefaultAsync();
                rvm = new ReportViewModel() { StartDate = config.StartDate, EndDate = config.EndDate };
            }

            var dtsource = getUtilizationByAgeRange(await GetCurrentUserAsync(), rvm.StartDate, rvm.EndDate);

            return UtilizationByGenderResultData(param, dtsource);
        }

        [HttpPost]
        public async Task<IActionResult> RatingList([FromBody] DTParameters param)
        {
            var rvm = TempData.Get<ReportViewModel>("rvm");
            if (rvm == null)
            {
                var config = await _context.ProjectConfig.FirstOrDefaultAsync();
                rvm = new ReportViewModel() { StartDate = config.StartDate, EndDate = config.EndDate };
            }

            var dtsource = getRating(await GetCurrentUserAsync(), rvm.StartDate, rvm.EndDate);

            return RatingResultData(param, dtsource);
        }

        #endregion

        #region Graphs



        #endregion

        #region Pie

        public async Task<IActionResult> GenderPie()
        {
            var rvm = TempData.Get<ReportViewModel>("rvm");
            if (rvm == null)
            {
                var config = await _context.ProjectConfig.FirstOrDefaultAsync();
                rvm = new ReportViewModel() { StartDate = config.StartDate, EndDate = config.EndDate };
            }

            var piesource = getUtilizationByGenderPie(await GetCurrentUserAsync(), rvm.StartDate, rvm.EndDate).Where(r => r.UtilizationCount > 0).Select(r => new PieChartModel { Key = r.Name, Value = r.UtilizationCount });

            return Json(piesource);
        }

        public async Task<IActionResult> AgePie()
        {
            var rvm = TempData.Get<ReportViewModel>("rvm");
            if (rvm == null)
            {
                var config = await _context.ProjectConfig.FirstOrDefaultAsync();
                rvm = new ReportViewModel() { StartDate = config.StartDate, EndDate = config.EndDate };
            }

            var piesource = getUtilizationByAgeRangePie(await GetCurrentUserAsync(), rvm.StartDate, rvm.EndDate).Where(r => r.UtilizationCount > 0).Select(r => new PieChartModel { Key = r.Name, Value = r.UtilizationCount });

            return Json(piesource);
        }

        #endregion

        #region Private


        private async Task<(string enrollees, string sall, string signeds, string excluded, string providers, string locations)> getStatistics()
        {
            var user = await GetCurrentUserAsync();

            string enrollees = "", sall = "", signeds = "", excluded = "", providers = "", locations = "";

            switch (user.ProfileType)
            {
                case ProfileTypes.ADMIN:
                    enrollees = (await (from e in _context.Enrollees
                                        where e.ClientPlan != "demo" && e.HMOID == user.HMOID
                                        select e).CountAsync()).ToString();
                    sall = (await (from e in _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO)
                                   where e.Enrollee.HMOID == user.HMOID
                                   select e).CountAsync()).ToString();
                    signeds = (await (from e in _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO)
                                      where e.Stage >= Steps.GetRef && e.Enrollee.HMOID == user.HMOID
                                      select e).CountAsync()).ToString();
                    excluded = (await (from e in _context.ExcludedEnrollees.Include(e => e.Enrollee).ThenInclude(e => e.HMO)
                                       where e.Enrollee.HMOID == user.HMOID
                                       select e).CountAsync()).ToString();
                    providers = (await (from e in _context.Providers.Include(p => p.HMO).Include(p => p.Location).Include(p => p.State)
                                        where e.HMOID == user.HMOID && e.Enabled != false
                                        select e).CountAsync()).ToString();
                    locations = (await _context.Providers.Where(p => p.HMOID == user.HMOID && p.Enabled != false).Select(p => p.Location).Distinct().CountAsync()).ToString();
                    //locations = (await _context.Locations.CountAsync()).ToString();
                    break;

                case ProfileTypes.PROVIDER:
                    enrollees = (await (from e in _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO)
                                        where e.Enrollee.ClientPlan != "demo" && e.Stage >= Steps.GetRef && _context.ApplicationUserProviders.Select(p => p.HMOID).Contains(e.Enrollee.HMOID)
                                        select e).CountAsync()).ToString();
                    signeds = (await (from e in _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO)
                                      where _context.ApplicationUserProviders.Select(p => p.HMOID).Contains(e.Enrollee.HMOID)
                                      select e).CountAsync()).ToString();
                    signeds = (await (from e in _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO)
                                      where e.Stage >= Steps.GetRef && _context.ApplicationUserProviders.Select(p => p.HMOID).Contains(e.Enrollee.HMOID)
                                      select e).CountAsync()).ToString();
                    //excluded = (await (from e in _context.ExcludedEnrollees.Include(e => e.Enrollee).Include(e => e.Enrollee.HMO)
                    //                   join s in _context.SignUps on e.EnrolleeID equals s.EnrolleeID
                    //                   where s.Stage >= Steps.GetRef && _context.ApplicationUserProviders.Select(p => p.HMOID).Contains(e.Enrollee.HMOID)
                    //                   select e).CountAsync()).ToString();
                    excluded = "0";
                    providers = "0";
                    locations = "0";

                    break;
                default:
                    enrollees = (await _context.Enrollees.Where(e => e.ClientPlan != "demo").CountAsync()).ToString();
                    sall = (await _context.SignUps.CountAsync()).ToString();
                    signeds = (await (from s in _context.SignUps
                                      where s.Stage == Steps.GetRef
                                      select s).CountAsync()).ToString();
                    excluded = (await _context.ExcludedEnrollees.CountAsync()).ToString();
                    providers = (await _context.Providers.Where(p => p.Enabled != false).CountAsync()).ToString();
                    locations = (await _context.Locations.CountAsync()).ToString();
                    break;
            }

            return (enrollees, sall, signeds, excluded, providers, locations);
        }

        private IEnumerable<GenderUtilizationReportModel> getUtilizationByGender(ApplicationUser user, DateTime start, DateTime end)
        {
            var profiletype = user.ProfileType;
            switch (profiletype)
            {
                case ProfileTypes.ADMIN:
                    var qryAdmin = from s in _context.SignUps.Include(s => s.Enrollee)
                                   where s.Stage >= Steps.GetRef && s.Enrollee.HMOID == user.HMOID &&
                                       s.AppointmentDate >= start && s.AppointmentDate <= end //&&
                                                                                              //(s.CheckedOn_ByAdmin ?? s.CheckedOn_ByProvider ?? s.CheckedOn).HasValue
                                   group s by s.Enrollee.Gender into grp
                                   select new GenderUtilizationReportModel
                                   {
                                       Name = grp.Key.ToString(),
                                       PercentUtilization = grp.Count(r => true) > 0 ? Math.Round((decimal)grp.Count(r => (r.CheckedOn_ByAdmin ?? r.CheckedOn_ByProvider ?? r.CheckedOn).HasValue) / (decimal)grp.Count(r => true) * 100M, 2) : 0,
                                       UtilizationCount = (decimal)grp.Count(r => (r.CheckedOn_ByAdmin ?? r.CheckedOn_ByProvider ?? r.CheckedOn).HasValue),
                                       TotalCount = (decimal)grp.Count(r => true)
                                   };

                    var totalutilizationCountAdmin = qryAdmin.Sum(u => u.UtilizationCount);
                    var totalCountAdmin = qryAdmin.Sum(u => u.TotalCount);

                    var oneAdmin = new GenderUtilizationReportModel() { Name = "Total", PercentUtilization = (totalCountAdmin > 0) ? Math.Round(totalutilizationCountAdmin / totalCountAdmin * 100M, 2) : 0, TotalCount = totalCountAdmin, UtilizationCount = totalutilizationCountAdmin };

                    var tmpAdmin = new List<GenderUtilizationReportModel>(qryAdmin);
                    tmpAdmin.Add(oneAdmin);

                    return tmpAdmin;

                case ProfileTypes.PROVIDER:
                    var qryProv = from s in _context.SignUps.Include(s => s.Enrollee)
                                  where s.Stage >= Steps.GetRef && _context.ApplicationUserProviders.Select(p => p.HMOID).Contains(user.HMOID.Value) &&
                                      s.AppointmentDate >= start && s.AppointmentDate <= end //&&
                                                                                             //(s.CheckedOn_ByAdmin ?? s.CheckedOn_ByProvider ?? s.CheckedOn).HasValue
                                  group s by s.Enrollee.Gender into grp
                                  select new GenderUtilizationReportModel
                                  {
                                      Name = grp.Key.ToString(),
                                      PercentUtilization = grp.Count(r => true) > 0 ? Math.Round((decimal)grp.Count(r => (r.CheckedOn_ByAdmin ?? r.CheckedOn_ByProvider ?? r.CheckedOn).HasValue) / (decimal)grp.Count(r => true) * 100M, 2) : 0,
                                      UtilizationCount = (decimal)grp.Count(r => (r.CheckedOn_ByAdmin ?? r.CheckedOn_ByProvider ?? r.CheckedOn).HasValue),
                                      TotalCount = (decimal)grp.Count(r => true)
                                  };

                    var totalutilizationCountProv = qryProv.Sum(u => u.UtilizationCount);
                    var totalCountProv = qryProv.Sum(u => u.TotalCount);

                    var oneProv = new GenderUtilizationReportModel() { Name = "Total", PercentUtilization = (totalCountProv > 0) ? Math.Round(totalutilizationCountProv / totalCountProv * 100M, 2) : 0, TotalCount = totalCountProv, UtilizationCount = totalutilizationCountProv };

                    var tmpProv = new List<GenderUtilizationReportModel>(qryProv);
                    tmpProv.Add(oneProv);

                    return tmpProv;

                default:
                    var qry = from s in _context.SignUps.Include(s => s.Enrollee)
                              where s.Stage >= Steps.GetRef &&
                                  s.AppointmentDate >= start && s.AppointmentDate <= end //&& 
                                                                                         //(s.CheckedOn_ByAdmin ?? s.CheckedOn_ByProvider ?? s.CheckedOn).HasValue
                              group s by s.Enrollee.Gender into grp
                              select new GenderUtilizationReportModel
                              {
                                  Name = grp.Key.ToString(),
                                  PercentUtilization = grp.Count(r => true) > 0 ? Math.Round((decimal)grp.Count(r => (r.CheckedOn_ByAdmin ?? r.CheckedOn_ByProvider ?? r.CheckedOn).HasValue) / (decimal)grp.Count(r => true) * 100M, 2) : 0,
                                  UtilizationCount = (decimal)grp.Count(r => (r.CheckedOn_ByAdmin ?? r.CheckedOn_ByProvider ?? r.CheckedOn).HasValue),
                                  TotalCount = (decimal)grp.Count(r => true)
                              };

                    var totalutilizationCount = qry.Sum(u => u.UtilizationCount);
                    var totalCount = qry.Sum(u => u.TotalCount);

                    var one = new GenderUtilizationReportModel() { Name = "Total", PercentUtilization = (totalCount > 0) ? Math.Round(totalutilizationCount / totalCount * 100M, 2) : 0, TotalCount = totalCount, UtilizationCount = totalutilizationCount };

                    var tmp = new List<GenderUtilizationReportModel>(qry);
                    tmp.Add(one);

                    return tmp;
            }
        }

        private IEnumerable<GenderUtilizationReportModel> getUtilizationByGenderPie(ApplicationUser user, DateTime start, DateTime end)
        {
            var profiletype = user.ProfileType;
            switch (profiletype)
            {
                case ProfileTypes.ADMIN:
                    var qryAdmin = from s in _context.SignUps.Include(s => s.Enrollee)
                                   where s.Stage >= Steps.GetRef && s.Enrollee.HMOID == user.HMOID &&
                                       s.AppointmentDate >= start && s.AppointmentDate <= end
                                   //&& (s.CheckedOn_ByAdmin ?? s.CheckedOn_ByProvider ?? s.CheckedOn).HasValue
                                   group s by s.Enrollee.Gender into grp
                                   select new GenderUtilizationReportModel
                                   {
                                       Name = grp.Key.ToString(),
                                       UtilizationCount = (decimal)grp.Count(r => (r.CheckedOn_ByAdmin ?? r.CheckedOn_ByProvider ?? r.CheckedOn).HasValue),
                                   };

                    var ntotAdmin = _context.Enrollees.Count() - qryAdmin.Sum(r => r.UtilizationCount);
                    var oneAdmin = new GenderUtilizationReportModel() { Name = "NOT CHECKED", UtilizationCount = ntotAdmin };

                    var tmpAdmin = new List<GenderUtilizationReportModel>(qryAdmin);
                    tmpAdmin.Add(oneAdmin);

                    return tmpAdmin;

                case ProfileTypes.PROVIDER:
                    var qryProv = from s in _context.SignUps.Include(s => s.Enrollee)
                                  where s.Stage >= Steps.GetRef && _context.ApplicationUserProviders.Select(p => p.HMOID).Contains(user.HMOID.Value) &&
                                      s.AppointmentDate >= start && s.AppointmentDate <= end
                                  //&& (s.CheckedOn_ByAdmin ?? s.CheckedOn_ByProvider ?? s.CheckedOn).HasValue
                                  group s by s.Enrollee.Gender into grp
                                  select new GenderUtilizationReportModel
                                  {
                                      Name = grp.Key.ToString(),
                                      UtilizationCount = (decimal)grp.Count(r => (r.CheckedOn_ByAdmin ?? r.CheckedOn_ByProvider ?? r.CheckedOn).HasValue),
                                  };

                    var ntotProv = _context.Enrollees.Count() - qryProv.Sum(r => r.UtilizationCount);
                    var oneProv = new GenderUtilizationReportModel() { Name = "NOT CHECKED", UtilizationCount = ntotProv };

                    var tmpProv = new List<GenderUtilizationReportModel>(qryProv);
                    tmpProv.Add(oneProv);

                    return tmpProv;

                default:
                    var qry = from s in _context.SignUps.Include(s => s.Enrollee)
                              where s.Stage >= Steps.GetRef &&
                                  s.AppointmentDate >= start && s.AppointmentDate <= end
                              //&& (s.CheckedOn_ByAdmin ?? s.CheckedOn_ByProvider ?? s.CheckedOn).HasValue
                              group s by s.Enrollee.Gender into grp
                              select new GenderUtilizationReportModel
                              {
                                  Name = grp.Key.ToString(),
                                  UtilizationCount = (decimal)grp.Count(r => (r.CheckedOn_ByAdmin ?? r.CheckedOn_ByProvider ?? r.CheckedOn).HasValue),
                              };

                    var ntot = _context.Enrollees.Count() - qry.Sum(r => r.UtilizationCount);
                    var one = new GenderUtilizationReportModel() { Name = "NOT CHECKED", UtilizationCount = ntot };

                    var tmp = new List<GenderUtilizationReportModel>(qry);
                    tmp.Add(one);

                    return tmp;
            }

        }

        private IEnumerable<GenderUtilizationReportModel> getUtilizationByAgeRange(ApplicationUser user, DateTime start, DateTime end)
        {
            var profiletype = user.ProfileType;
            switch (profiletype)
            {
                case ProfileTypes.ADMIN:

                    var qryAdmin = from s in _context.SignUps.Include(s => s.Enrollee)
                                   where s.Stage >= Steps.GetRef && s.Enrollee.HMOID == user.HMOID &&
                                       s.AppointmentDate >= start && s.AppointmentDate <= end //&&
                                                                                              //(s.CheckedOn_ByAdmin ?? s.CheckedOn_ByProvider ?? s.CheckedOn).HasValue
                                   let age = (DateTime.Now.Date - s.Enrollee.DOB.Value).TotalDays / 365.2425
                                   group s by (s.Enrollee.DOB.HasValue ? (age < 30 ? AgeGrouping.LESSTHAN_30 : (age >= 30 && age < 40 ? AgeGrouping.BETWEEN_30_TO_39 : (age >= 40 && age < 50 ? AgeGrouping.BETWEEN_40_TO_49 : AgeGrouping.GREATERTHAN_50))) : AgeGrouping.UNCATEGORIZED) into grp
                                   select new GenderUtilizationReportModel
                                   {
                                       Name = grp.Key.ToString(),
                                       PercentUtilization = grp.Count(r => true) > 0 ? Math.Round((decimal)grp.Count(r => (r.CheckedOn_ByAdmin ?? r.CheckedOn_ByProvider ?? r.CheckedOn).HasValue) / (decimal)grp.Count(r => true) * 100, 2) : 0,
                                       UtilizationCount = (decimal)grp.Count(r => (r.CheckedOn_ByAdmin ?? r.CheckedOn_ByProvider ?? r.CheckedOn).HasValue),
                                       TotalCount = (decimal)grp.Count(r => true)
                                   };

                    var totalutilizationCountAdmin = qryAdmin.Sum(u => u.UtilizationCount);
                    var totalCountAdmin = qryAdmin.Sum(u => u.TotalCount);

                    var oneAdmin = new GenderUtilizationReportModel() { Name = "Total", PercentUtilization = (totalCountAdmin > 0) ? Math.Round(totalutilizationCountAdmin / totalCountAdmin * 100M, 2) : 0, TotalCount = totalCountAdmin, UtilizationCount = totalutilizationCountAdmin };

                    var tmpAdmin = new List<GenderUtilizationReportModel>(qryAdmin);
                    tmpAdmin.Add(oneAdmin);

                    return tmpAdmin;

                case ProfileTypes.PROVIDER:

                    var qryProv = from s in _context.SignUps.Include(s => s.Enrollee)
                                  where s.Stage >= Steps.GetRef && _context.ApplicationUserProviders.Select(p => p.HMOID).Contains(user.HMOID.Value) &&
                                      s.AppointmentDate >= start && s.AppointmentDate <= end //&&
                                                                                             //(s.CheckedOn_ByAdmin ?? s.CheckedOn_ByProvider ?? s.CheckedOn).HasValue
                                  let age = (DateTime.Now.Date - s.Enrollee.DOB.Value).TotalDays / 365.2425
                                  group s by (s.Enrollee.DOB.HasValue ? (age < 30 ? AgeGrouping.LESSTHAN_30 : (age >= 30 && age < 40 ? AgeGrouping.BETWEEN_30_TO_39 : (age >= 40 && age < 50 ? AgeGrouping.BETWEEN_40_TO_49 : AgeGrouping.GREATERTHAN_50))) : AgeGrouping.UNCATEGORIZED) into grp
                                  select new GenderUtilizationReportModel
                                  {
                                      Name = grp.Key.ToString(),
                                      PercentUtilization = grp.Count(r => true) > 0 ? Math.Round((decimal)grp.Count(r => (r.CheckedOn_ByAdmin ?? r.CheckedOn_ByProvider ?? r.CheckedOn).HasValue) / (decimal)grp.Count(r => true) * 100, 2) : 0,
                                      UtilizationCount = (decimal)grp.Count(r => (r.CheckedOn_ByAdmin ?? r.CheckedOn_ByProvider ?? r.CheckedOn).HasValue),
                                      TotalCount = (decimal)grp.Count(r => true)
                                  };

                    var totalutilizationCountProv = qryProv.Sum(u => u.UtilizationCount);
                    var totalCountProv = qryProv.Sum(u => u.TotalCount);

                    var oneProv = new GenderUtilizationReportModel() { Name = "Total", PercentUtilization = (totalCountProv > 0) ? Math.Round(totalutilizationCountProv / totalCountProv * 100M, 2) : 0, TotalCount = totalCountProv, UtilizationCount = totalutilizationCountProv };

                    var tmpProv = new List<GenderUtilizationReportModel>(qryProv);
                    tmpProv.Add(oneProv);

                    return tmpProv;

                default:
                    var qry = from s in _context.SignUps.Include(s => s.Enrollee)
                              where s.Stage >= Steps.GetRef &&
                                  s.AppointmentDate >= start && s.AppointmentDate <= end //&& 
                                                                                         //(s.CheckedOn_ByAdmin ?? s.CheckedOn_ByProvider ?? s.CheckedOn).HasValue
                              let age = (DateTime.Now.Date - s.Enrollee.DOB.Value).TotalDays / 365.2425
                              group s by (s.Enrollee.DOB.HasValue ? (age < 30 ? AgeGrouping.LESSTHAN_30 : (age >= 30 && age < 40 ? AgeGrouping.BETWEEN_30_TO_39 : (age >= 40 && age < 50 ? AgeGrouping.BETWEEN_40_TO_49 : AgeGrouping.GREATERTHAN_50))) : AgeGrouping.UNCATEGORIZED) into grp
                              select new GenderUtilizationReportModel
                              {
                                  Name = grp.Key.ToString(),
                                  PercentUtilization = grp.Count(r => true) > 0 ? Math.Round((decimal)grp.Count(r => (r.CheckedOn_ByAdmin ?? r.CheckedOn_ByProvider ?? r.CheckedOn).HasValue) / (decimal)grp.Count(r => true) * 100, 2) : 0,
                                  UtilizationCount = (decimal)grp.Count(r => (r.CheckedOn_ByAdmin ?? r.CheckedOn_ByProvider ?? r.CheckedOn).HasValue),
                                  TotalCount = (decimal)grp.Count(r => true)
                              };

                    var totalutilizationCount = qry.Sum(u => u.UtilizationCount);
                    var totalCount = qry.Sum(u => u.TotalCount);

                    var one = new GenderUtilizationReportModel() { Name = "Total", PercentUtilization = (totalCount > 0) ? Math.Round(totalutilizationCount / totalCount * 100M) : 0, TotalCount = totalCount, UtilizationCount = totalutilizationCount };

                    var tmp = new List<GenderUtilizationReportModel>(qry);
                    tmp.Add(one);

                    return tmp;
            }
        }

        private IEnumerable<GenderUtilizationReportModel> getUtilizationByAgeRangePie(ApplicationUser user, DateTime start, DateTime end)
        {
            var profiletype = user.ProfileType;
            switch (profiletype)
            {
                case ProfileTypes.ADMIN:
                    var qryAdmin = from s in _context.SignUps.Include(s => s.Enrollee)
                                   where s.Stage >= Steps.GetRef && s.Enrollee.HMOID == user.HMOID &&
                                       s.AppointmentDate >= start && s.AppointmentDate <= end
                                   //&& (s.CheckedOn_ByAdmin ?? s.CheckedOn_ByProvider ?? s.CheckedOn).HasValue
                                   let age = (DateTime.Now.Date - s.Enrollee.DOB.Value).TotalDays / 365.2425
                                   group s by (s.Enrollee.DOB.HasValue ? (age < 30 ? AgeGrouping.LESSTHAN_30 : (age >= 30 && age < 40 ? AgeGrouping.BETWEEN_30_TO_39 : (age >= 40 && age < 50 ? AgeGrouping.BETWEEN_40_TO_49 : AgeGrouping.GREATERTHAN_50))) : AgeGrouping.UNCATEGORIZED) into grp
                                   select new GenderUtilizationReportModel
                                   {
                                       Name = grp.Key.ToString(),
                                       UtilizationCount = (decimal)grp.Count(r => (r.CheckedOn_ByAdmin ?? r.CheckedOn_ByProvider ?? r.CheckedOn).HasValue),
                                   };

                    var ntotAdmin = _context.Enrollees.Count() - qryAdmin.Sum(r => r.UtilizationCount);
                    var oneAdmin = new GenderUtilizationReportModel() { Name = "NOT CHECKED", UtilizationCount = ntotAdmin };

                    var tmpAdmin = new List<GenderUtilizationReportModel>(qryAdmin);
                    tmpAdmin.Add(oneAdmin);

                    return tmpAdmin;

                case ProfileTypes.PROVIDER:

                    var qryProv = from s in _context.SignUps.Include(s => s.Enrollee)
                                  where s.Stage >= Steps.GetRef && _context.ApplicationUserProviders.Select(p => p.HMOID).Contains(user.HMOID.Value) &&
                                      s.AppointmentDate >= start && s.AppointmentDate <= end
                                  //&& (s.CheckedOn_ByAdmin ?? s.CheckedOn_ByProvider ?? s.CheckedOn).HasValue
                                  let age = (DateTime.Now.Date - s.Enrollee.DOB.Value).TotalDays / 365.2425
                                  group s by (s.Enrollee.DOB.HasValue ? (age < 30 ? AgeGrouping.LESSTHAN_30 : (age >= 30 && age < 40 ? AgeGrouping.BETWEEN_30_TO_39 : (age >= 40 && age < 50 ? AgeGrouping.BETWEEN_40_TO_49 : AgeGrouping.GREATERTHAN_50))) : AgeGrouping.UNCATEGORIZED) into grp
                                  select new GenderUtilizationReportModel
                                  {
                                      Name = grp.Key.ToString(),
                                      UtilizationCount = (decimal)grp.Count(r => (r.CheckedOn_ByAdmin ?? r.CheckedOn_ByProvider ?? r.CheckedOn).HasValue),
                                  };

                    var ntotProv = _context.Enrollees.Count() - qryProv.Sum(r => r.UtilizationCount);
                    var oneProv = new GenderUtilizationReportModel() { Name = "NOT CHECKED", UtilizationCount = ntotProv };

                    var tmpProv = new List<GenderUtilizationReportModel>(qryProv);
                    tmpProv.Add(oneProv);

                    return tmpProv;

                default:

                    var qry = from s in _context.SignUps.Include(s => s.Enrollee)
                              where s.Stage >= Steps.GetRef &&
                                  s.AppointmentDate >= start && s.AppointmentDate <= end
                              //&& (s.CheckedOn_ByAdmin ?? s.CheckedOn_ByProvider ?? s.CheckedOn).HasValue
                              let age = (DateTime.Now.Date - s.Enrollee.DOB.Value).TotalDays / 365.2425
                              group s by (s.Enrollee.DOB.HasValue ? (age < 30 ? AgeGrouping.LESSTHAN_30 : (age >= 30 && age < 40 ? AgeGrouping.BETWEEN_30_TO_39 : (age >= 40 && age < 50 ? AgeGrouping.BETWEEN_40_TO_49 : AgeGrouping.GREATERTHAN_50))) : AgeGrouping.UNCATEGORIZED) into grp
                              select new GenderUtilizationReportModel
                              {
                                  Name = grp.Key.ToString(),
                                  UtilizationCount = (decimal)grp.Count(r => (r.CheckedOn_ByAdmin ?? r.CheckedOn_ByProvider ?? r.CheckedOn).HasValue),
                              };

                    var ntot = _context.Enrollees.Count() - qry.Sum(r => r.UtilizationCount);
                    var one = new GenderUtilizationReportModel() { Name = "NOT CHECKED", UtilizationCount = ntot };

                    var tmp = new List<GenderUtilizationReportModel>(qry);
                    tmp.Add(one);

                    return tmp;
            }
        }

        private IEnumerable<RatingReportModel> getRating(ApplicationUser user, DateTime start, DateTime end)
        {
            var profiletype = user.ProfileType;
            switch (profiletype)
            {
                case ProfileTypes.ADMIN:
                    var qryAdmin = from s in _context.SignUps.Include(s => s.Provider)
                                   where s.Rating > 0 && s.Enrollee.HMOID == user.HMOID &&
                                       s.AppointmentDate >= start && s.AppointmentDate <= end
                                   group s by s.Provider into grp
                                   select new RatingReportModel
                                   {
                                       Name = grp.Key.Name.ToString(),
                                       AverageRating = Math.Round(grp.Average(r => (decimal)r.Rating), 2)
                                   };
                    return qryAdmin;

                case ProfileTypes.PROVIDER:

                    var qryProv = from s in _context.SignUps.Include(s => s.Provider)
                                  where s.Rating > 0 && _context.ApplicationUserProviders.Select(p => p.HMOID).Contains(user.HMOID.Value) &&
                                      s.AppointmentDate >= start && s.AppointmentDate <= end
                                  group s by s.Provider into grp
                                  select new RatingReportModel
                                  {
                                      Name = grp.Key.Name.ToString(),
                                      AverageRating = Math.Round(grp.Average(r => (decimal)r.Rating), 2)
                                  };
                    return qryProv;

                default:
                    var qry = from s in _context.SignUps.Include(s => s.Provider)
                              where s.Rating > 0 &&
                                  s.AppointmentDate >= start && s.AppointmentDate <= end
                              group s by s.Provider into grp
                              select new RatingReportModel
                              {
                                  Name = grp.Key.Name.ToString(),
                                  AverageRating = Math.Round(grp.Average(r => (decimal)r.Rating), 2)
                              };
                    return qry;
            }


        }


        private JsonResult UtilizationByGenderResultData(DTParameters param, IEnumerable<GenderUtilizationReportModel> dataResult)
        {
            try
            {
                List<String> columnSearch = new List<string>();

                foreach (var col in param.Columns)
                    columnSearch.Add(col.Search.Value);

                List<GenderUtilizationReportModel> data = _gurResult.GetResult(param.Search.Value, param.SortOrder, param.Start, param.Length, dataResult.AsQueryable(), columnSearch);
                int count = _gurResult.Count(param.Search.Value, dataResult.AsQueryable(), columnSearch);
                int id = param.Start + 1;
                DTResult<GenderUtilizationReportModel> result = new DTResult<GenderUtilizationReportModel>
                {
                    draw = param.Draw,
                    data = data.Select(r => new
                    {
                        sn = id++,
                        name = r.Name.ToLower().Humanize(LetterCasing.Title),
                        percent = $"{r.PercentUtilization}%",
                        utilization = r.UtilizationCount,
                        total = r.TotalCount
                    }),
                    recordsFiltered = count,
                    recordsTotal = count
                };
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private JsonResult AgeRangeUtilizationResultData(DTParameters param, IEnumerable<GenderUtilizationReportModel> dataResult)
        {
            try
            {
                List<String> columnSearch = new List<string>();

                foreach (var col in param.Columns)
                    columnSearch.Add(col.Search.Value);

                List<GenderUtilizationReportModel> data = _uarResult.GetResult(param.Search.Value, param.SortOrder, param.Start, param.Length, dataResult.AsQueryable(), columnSearch);
                int count = _uarResult.Count(param.Search.Value, dataResult.AsQueryable(), columnSearch);
                int id = param.Start + 1;
                DTResult<GenderUtilizationReportModel> result = new DTResult<GenderUtilizationReportModel>
                {
                    draw = param.Draw,
                    data = data.Select(r => new
                    {
                        sn = id++,
                        name = r.Name.ToLower().Humanize(LetterCasing.Title),
                        utilization = r.UtilizationCount,
                    }),
                    recordsFiltered = count,
                    recordsTotal = count
                };
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private JsonResult RatingResultData(DTParameters param, IEnumerable<RatingReportModel> dataResult)
        {
            try
            {
                List<String> columnSearch = new List<string>();

                foreach (var col in param.Columns)
                    columnSearch.Add(col.Search.Value);

                List<RatingReportModel> data = _rResult.GetResult(param.Search.Value, param.SortOrder, param.Start, param.Length, dataResult.AsQueryable(), columnSearch);
                int count = _rResult.Count(param.Search.Value, dataResult.AsQueryable(), columnSearch);
                int id = param.Start + 1;
                DTResult<RatingReportModel> result = new DTResult<RatingReportModel>
                {
                    draw = param.Draw,
                    data = data.Select(r => new
                    {
                        sn = id++,
                        name = r.Name.ToLower().Humanize(LetterCasing.Title),
                        rating = r.AverageRating,
                    }),
                    recordsFiltered = count,
                    recordsTotal = count
                };
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        #endregion

    }
}
