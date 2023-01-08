using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
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
    public class ServicesController : Controller
    {
        private readonly ServiceResult2 _sResult;
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification _toastNotification;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IImportProcessorService _importProcessorService;

        public ServicesController(ApplicationDbContext context,
            ServiceResult2 sResult, IToastNotification toastNotification,
            UserManager<ApplicationUser> userManager,
            IImportProcessorService importProcessorService)
        {
            _sResult = sResult;
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
            IQueryable<Service> dtsource = null;

            if (user == null)
                return ResultData(param, new List<Service>().AsQueryable());

            switch (user.ProfileType)
            {
                case ProfileTypes.ADMIN:
                    dtsource = from e in _context.Services.Include(e => e.HMO)
                               where e.Enabled != false && e.HMOID == user.HMOID
                               select e;
                    break;
                case ProfileTypes.PROVIDER:
                    dtsource = from e in _context.Services.Include(e => e.HMO)
                               where e.Enabled != false && user.Providers.Select(p => p.HMOID).Contains(e.HMOID)
                               select e;
                    break;
                default:
                    dtsource = _context.Services.Include(s => s.HMO).Where(s => s.Enabled != false);
                    break;
            }

            return ResultData(param, dtsource);
        }

        // GET: Admins
        public async Task<IActionResult> Index()
        {
            List<SelectListItem> tmp = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a Profile Type --" } };
            tmp.AddRange((await _context.ImportServiceSettings.ToListAsync()).OrderByDescending(r => r.DateCreated)
                                .Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }));
            var ptypes = new SelectList(tmp, "Value", "Text");
            ViewBag.ProfileTypes = ptypes;

            tmp = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a HMO --" } };
            tmp.AddRange((await _context.HMOs.ToListAsync()).Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }));
            var hmos = new SelectList(tmp, "Value", "Text");
            ViewBag.HMOs = hmos;

            return View();
        }

        // GET: Admins/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services.Include(s => s.HMO).FirstOrDefaultAsync(s => s.ID == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            if (ModelState.IsValid)
            {
                _context.Add(service);
                await _context.SaveChangesAsync();

                _toastNotification.AddSuccessToastMessage("A new provider has been created successfully!");
                return RedirectToAction(nameof(ProvidersController.Index));
            }

            var hmos = new SelectList((await _context.HMOs.ToListAsync()).Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }), "Value", "Text");
            ViewBag.HMOs = hmos;

            return View(service);
        }

        // GET: Admins/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services.Include(s => s.HMO).FirstOrDefaultAsync(s => s.ID == id);
            if (service == null)
            {
                return NotFound();
            }

            var hmos = new SelectList((await _context.HMOs.ToListAsync()).Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }), "Value", "Text");
            ViewBag.HMOs = hmos;

            return View(service);
        }

        // POST: Admins/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Service service)
        {
            if (id != service.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!service.GTE_Age.HasValue)
                        service.GTE_Age = null;

                    _context.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.ID))
                    {
                        _toastNotification.AddInfoToastMessage("Resource requested not found!");
                        return NotFound();
                    }
                    else
                    {
                        _toastNotification.AddErrorToastMessage("Oop, something went wrong, Please try again later.");
                        return View();
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var hmos = new SelectList((await _context.HMOs.ToListAsync()).Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }), "Value", "Text", service.HMOID);
            ViewBag.HMOs = hmos;

            return View(service);
        }

        // GET: Admins/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _toastNotification.AddInfoToastMessage("Resource requested not found!");
                return NotFound();
            }

            var provider = await _context.Services.Include(s => s.HMO).FirstOrDefaultAsync(s => s.ID == id);
            if (provider == null)
            {
                _toastNotification.AddInfoToastMessage("Resource requested not found!");
                return NotFound();
            }

            return View(provider);
        }

        // POST: Admins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FirstOrDefaultAsync(p => p.ID == id);
            service.Enabled = false;
            await _context.SaveChangesAsync();

            _toastNotification.AddSuccessToastMessage("Deleted Successfully!");
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.ID == id);
        }

        private JsonResult ResultData(DTParameters param, IQueryable<Service> services)
        {
            try
            {
                List<String> columnSearch = new List<string>();

                foreach (var col in param.Columns)
                    columnSearch.Add(col.Search.Value);

                List<Service> data = _sResult.GetResult(param.Search.Value, param.SortOrder, param.Start, param.Length, services, columnSearch);
                int count = _sResult.Count(param.Search.Value, services, columnSearch);
                int id = param.Start + 1;
                DTResult<Service> result = new DTResult<Service>
                {
                    draw = param.Draw,
                    data = data.Select(r => new
                    {
                        sn = id++,
                        id = r.ID.ToString(),
                        name = r.Name.ToUpper(),
                        gender = Enum.GetName(typeof(GenderX), r.Gender).Humanize(LetterCasing.Title),
                        gteage = r.GTE_Age.HasValue ? r.GTE_Age.Value.ToString() : "",
                        hmo = r.HMO.Name.Humanize(LetterCasing.Title)
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
        public async Task<IActionResult> ImportServices(int profileId, int hmoId, IFormFile file)
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

                    var importSettings = await _context.ImportServiceSettings.FirstOrDefaultAsync(i => i.ID == profileId);

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

                    var result = _importProcessorService.ImportServices(template, sheets, etype);
                    if (result.services != null)
                    {
                        // check for duplication and augument.
                        var tmp = result.services.Where(e =>
                        {
                            if (_context.Services.Any(n => n.Name.ToUpper().Trim() == e.Test.ToUpper().Trim() && n.HMOID == hmoId))
                                return false;
                            else
                                return true;
                        }).Select(e =>
                        {
                            var s = new Service()
                            {
                                Name = e.Test.ToUpper(),
                                Gender = e.Gender,
                                HMOID = hmoId
                            };
                            return s;
                        });

                        await _context.AddRangeAsync(tmp);
                        await _context.SaveChangesAsync();
                    }

                    ImportResultVM vm = new ImportResultVM()
                    {
                        FileName = file.FileName,
                        SuccessfulRowsImported = result.services != null ? result.services.Count() : 0,
                        TotalRows = result.rowCount,
                        TotalErrors = result.errors != null ? result.errors.Count() : 0,
                        TotalRowsWithErrors = result.rowCount - (result.services != null ? result.services.Count() : 0),
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
