using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NToastNotify;
using Humanizer;

namespace AnnualHealthCheckJs.Controllers
{
    using Data;
    using Models;
    using Results;
    using Services.Core;
    using Tools;
    using ViewModels;

    [Authorize(Roles = "SU,ADMIN,PROVIDER,HR")]
    public class ExcludedEnrolleesController : Controller
    {
        private readonly ExEnrolleeResult2 _eResult;
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification _toastNotification;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IImportProcessorService _importProcessorService;

        public ExcludedEnrolleesController(ApplicationDbContext context,
            ExEnrolleeResult2 eResult, IToastNotification toastNotification,
            UserManager<ApplicationUser> userManager,
            IImportProcessorService importProcessorService)
        {
            _eResult = eResult;
            _context = context;
            _toastNotification = toastNotification;
            _importProcessorService = importProcessorService;
            _userManager = userManager;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        [HttpPost]
        public async Task<IActionResult> List([FromBody] DTParameters param)
        {
            var user = await GetCurrentUserAsync();
            IQueryable<ExcludedEnrollee> dtsource = null;

            if (user == null)
                return ResultData(param, new List<ExcludedEnrollee>().AsQueryable());

            switch (user.ProfileType)
            {
                case ProfileTypes.ADMIN:
                    dtsource = from e in _context.ExcludedEnrollees.Include(e => e.Enrollee).ThenInclude(e => e.HMO)
                               where e.Enrollee.HMOID == user.HMOID
                               select e;
                    break;
                case ProfileTypes.PROVIDER:
                    dtsource = from e in _context.ExcludedEnrollees.Include(e => e.Enrollee).Include(e => e.Enrollee.HMO)
                               join s in _context.SignUps on e.EnrolleeID equals s.EnrolleeID
                               where s.Stage >= Steps.GetRef && _context.ApplicationUserProviders.Select(p => p.HMOID).Contains(e.Enrollee.HMOID)
                               select e;
                    break;
                default:
                    dtsource = _context.ExcludedEnrollees.Include(e => e.Enrollee).ThenInclude(e => e.HMO);
                    break;
            }

            return ResultData(param, dtsource);
        }

        // GET: Admins
        public async Task<IActionResult> Index()
        {
            List<SelectListItem> tmp = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a Profile Type --" } };
            tmp.AddRange((await _context.ImportExcludedEnrolleeSettings.ToListAsync()).OrderByDescending(r => r.DateCreated)
                                .Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }));
            var ptypes = new SelectList(tmp, "Value", "Text");
            ViewBag.ProfileTypes = ptypes;

            tmp = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a HMO --" } };
            tmp.AddRange((await _context.HMOs.ToListAsync()).Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }));
            var hmos = new SelectList(tmp, "Value", "Text");
            ViewBag.HMOs = hmos;

            var user = await GetCurrentUserAsync();
            if (user.ProfileType == ProfileTypes.ADMIN)
                ViewBag.HMOID = user.HMOID;

            return View();
        }

        // GET: Admins/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollee = await _context.ExcludedEnrollees.Include(e => e.Enrollee).ThenInclude(e => e.HMO).FirstOrDefaultAsync(s => s.ID == id);
            if (enrollee == null)
            {
                return NotFound();
            }

            return View(enrollee);
        }

        private JsonResult ResultData(DTParameters param, IQueryable<ExcludedEnrollee> enrollees)
        {
            try
            {
                List<String> columnSearch = new List<string>();

                foreach (var col in param.Columns)
                    columnSearch.Add(col.Search.Value);

                List<ExcludedEnrollee> data = _eResult.GetResult(param.Search.Value, param.SortOrder, param.Start, param.Length, enrollees, columnSearch);
                int count = _eResult.Count(param.Search.Value, enrollees, columnSearch);
                int id = param.Start + 1;
                DTResult<ExcludedEnrollee> result = new DTResult<ExcludedEnrollee>
                {
                    draw = param.Draw,
                    data = data.Select(r => new
                    {
                        sn = id++,
                        id = r.ID.ToString(),
                        empid = r.Enrollee.EmployeeID,
                        names = $"{r.Enrollee.LastName.ToLower().Humanize(LetterCasing.Title)} {r.Enrollee.OtherNames.ToLower().Humanize(LetterCasing.Title)}",
                        enrollid = !string.IsNullOrEmpty(r.Enrollee.EnrollmentID) ? r.Enrollee.EnrollmentID : "",
                        gender = Enum.GetName(typeof(Gender), r.Enrollee.Gender).Humanize(LetterCasing.Title),
                        reason = !string.IsNullOrEmpty(r.Reason) ? r.Reason : "",
                        year = r.Year.ToString().ToLower(),
                        hmo = r.Enrollee.HMO.Name.ToLower().Humanize(LetterCasing.Title)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcludedEnrollees(int profileId, int hmoId, IFormFile file)
        {
            // full path to file in temp location
            var filePath = Path.GetTempFileName();
            if (file == null)
            {
                ImportResultVM vm = new ImportResultVM()
                {
                    SuccessfulRowsImported = 0,
                    TotalRows = 0,
                    TotalErrors = 0,
                    TotalRowsWithErrors = 0,
                    Errors = null,
                    RowsWithErrors = null
                };

                return View(vm);
            }

            if (file.Length > 0)
            {
                string sFileExtension = Path.GetExtension(file.FileName).ToLower();

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);

                    stream.Position = 0;

                    var importSettings = await _context.ImportExcludedEnrolleeSettings.FirstOrDefaultAsync(i => i.ID == profileId);

                    // deserialize the settings
                    ImportTemplate template = Newtonsoft.Json.JsonConvert.DeserializeObject<ImportTemplate>(importSettings.Settings);

                    List<(ISheet, Tools.Sheet)> sheets = null;
                    ExcelTypes etype = ExcelTypes.XLS;

                    if (sFileExtension == ".xls")
                        etype = ExcelTypes.XLS;
                    else
                        etype = ExcelTypes.XLSX;

                    foreach (var tsheet in template.Sheets)
                    {
                        if (sheets == null)
                            sheets = new List<(ISheet, Tools.Sheet)>();

                        ISheet sheet;
                        if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
                        {
                            HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
                            sheet = hssfwb.GetSheet(tsheet.SheetName);
                        }
                        else //This will read 2007 Excel format    
                        {
                            XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
                            sheet = hssfwb.GetSheet(tsheet.SheetName);
                        }

                        sheets.Add((sheet, tsheet));
                    }

                    var result = _importProcessorService.ImportExcludedEnrollees(template, sheets, etype);
                    if (result.excludedEnrollees != null)
                    {
                        // check for duplication and augument.
                        var tmp = result.excludedEnrollees.Where(e =>
                        {
                            if (_context.Enrollees.Any(n => n.EmployeeID == e.EmployeeID && hmoId == n.HMOID))
                            {
                                if (_context.ExcludedEnrollees.Any(n => n.Enrollee.EmployeeID == e.EmployeeID))
                                    return false;
                                else
                                    return true;
                            }
                            else
                                return false;
                        }).Select(e =>
                        {
                            var enrollee = _context.Enrollees.FirstOrDefault(f => f.EmployeeID == e.EmployeeID);
                            var exe = new ExcludedEnrollee()
                            {
                                Enrollee = enrollee,
                                //HMOID = hmoId,
                                Year = DateTime.Now.Year,
                                Reason = "Already Taken part in Excercise"
                            };
                            return exe;
                        });

                        await _context.AddRangeAsync(tmp);
                        await _context.SaveChangesAsync();
                    }

                    ImportResultVM vm = new ImportResultVM()
                    {
                        FileName = file.FileName,
                        SuccessfulRowsImported = result.excludedEnrollees != null ? result.excludedEnrollees.Count() : 0,
                        TotalRows = result.rowCount,
                        TotalErrors = result.errors != null ? result.errors.Count() : 0,
                        TotalRowsWithErrors = result.rowCount - (result.excludedEnrollees != null ? result.excludedEnrollees.Count() : 0),
                        Errors = result.errors,
                        RowsWithErrors = result.errRows
                    };

                    return View(vm);
                }
            }

            ImportResultVM rvm = new ImportResultVM()
            {
                SuccessfulRowsImported = 0,
                TotalRows = 0,
                TotalErrors = 0,
                TotalRowsWithErrors = 0,
                Errors = null,
                RowsWithErrors = null
            };

            return View(rvm);
        }
    }
}
