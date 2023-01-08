using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Humanizer;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace AnnualHealthCheckJs.Controllers
{
    using Data;
    using Models;
    using Results;
    using ViewModels;

    public class HomeController : Controller
    {
        private readonly PendingSignupResult _pResult;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHostingEnvironment _hostingEnvironment;

        public HomeController(ApplicationDbContext context, PendingSignupResult pResult,
            IHostingEnvironment hostingEnvironment, UserManager<ApplicationUser> userManager)
        {
            _pResult = pResult;
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Widget(int duration)
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

            return Json(new { enrollees, sall, signeds, excluded, providers, locations });
        }

        public async Task<IActionResult> PendingSignups([FromBody] DTParameters param)
        {
            var user = await GetCurrentUserAsync();
            IQueryable<SignUp> dtsource = null;

            if (user == null)
                return PendingSignupsResultData(param, new List<SignUp>().AsQueryable());

            switch (user.ProfileType)
            {
                case ProfileTypes.ADMIN:
                    dtsource = from e in _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO)
                               where e.Stage < Steps.GetRef && e.Enrollee.HMOID == user.HMOID
                               select e;
                    break;
                case ProfileTypes.PROVIDER:
                    dtsource = from e in _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO)
                               where e.Stage == Steps.GetRef && _context.ApplicationUserProviders.Select(p => p.HMOID).Contains(e.Enrollee.HMOID)
                               select e;
                    break;
                default:
                    dtsource = from s in _context.SignUps.Include(s => s.Enrollee)
                               where s.Stage < Steps.GetRef
                               select s;
                    break;
            }


            return PendingSignupsResultData(param, dtsource);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        private JsonResult PendingSignupsResultData(DTParameters param, IQueryable<SignUp> invoices)
        {
            try
            {
                List<String> columnSearch = new List<string>();

                foreach (var col in param.Columns)
                    columnSearch.Add(col.Search.Value);

                List<SignUp> data = _pResult.GetResult(param.Search.Value, param.SortOrder, param.Start, param.Length, invoices, columnSearch);
                int count = _pResult.Count(param.Search.Value, invoices, columnSearch);
                int id = param.Start + 1;
                DTResult<SignUp> result = new DTResult<SignUp>
                {
                    draw = param.Draw,
                    data = data.Select(r => new
                    {
                        sn = id++,
                        id = r.EnrolleeID.ToString(),
                        empid = r.Enrollee.EmployeeID,
                        names = $"{r.Enrollee.LastName.ToLower().Humanize(LetterCasing.Title)} {r.Enrollee.OtherNames.ToLower().Humanize(LetterCasing.Title)}",
                        enrollid = !string.IsNullOrEmpty(r.Enrollee.EnrollmentID) ? r.Enrollee.EnrollmentID : "",
                        gender = Enum.GetName(typeof(Gender), r.Enrollee.Gender).Humanize(LetterCasing.Title),
                        lastupdated = r.DateUpdated.Value.ToString("d MMM yyyy h:mm tt")
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

        public async Task<IActionResult> ExportEnrolleesWithPin()
        {
            var qgenr = from e in _context.Enrollees
                        group e by e.ClientPlan into grp
                        select grp;

            string sWebRootFolder = _hostingEnvironment.WebRootPath;
            string sFileName = $"ubn_pins_{DateTime.Now.Ticks.ToString()}.xlsx";
            string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, sFileName);
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            var memory = new MemoryStream();

            using (var fs = new FileStream(Path.Combine(sWebRootFolder, sFileName), FileMode.Create, FileAccess.Write))
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();

                foreach (var grp in qgenr)
                {
                    ISheet excelSheet = workbook.CreateSheet(grp.Key.ToUpper());
                    IRow row = excelSheet.CreateRow(0);

                    // Create the Header
                    row.CreateCell(0).SetCellValue("S/N");
                    row.CreateCell(1).SetCellValue("STAFF NO");
                    row.CreateCell(2).SetCellValue("ENROLLEE NO");
                    row.CreateCell(3).SetCellValue("FULL NAME");
                    row.CreateCell(4).SetCellValue("PIN");

                    int sn = 1;
                    foreach (var enr in grp)
                    {
                        row = excelSheet.CreateRow(sn);
                        row.CreateCell(0).SetCellValue(sn);
                        row.CreateCell(1).SetCellValue(enr.EmployeeID);
                        row.CreateCell(2).SetCellValue(enr.EnrollmentID);
                        row.CreateCell(3).SetCellValue($"{enr.LastName} {enr.OtherNames}".Trim());
                        row.CreateCell(4).SetCellValue(enr.PIN);

                        sn++;
                    }
                }

                workbook.Write(fs);
            }
            using (var stream = new FileStream(Path.Combine(sWebRootFolder, sFileName), FileMode.Open))
                await stream.CopyToAsync(memory);

            memory.Position = 0;
            return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", sFileName);
        }
    }
}
