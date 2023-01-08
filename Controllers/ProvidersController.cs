using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using NToastNotify;
using Humanizer;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;


namespace AnnualHealthCheckJs.Controllers
{
    using Data;
    using Models;
    using Results;
    using Services.Core;
    using Tools;
    using ViewModels;

    [Authorize(Roles = "SU,ADMIN,PROVIDER,HR")]
    public class ProvidersController : Controller
    {
        private readonly ProviderResult2 _pResult;
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification _toastNotification;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IImportProcessorService _importProcessorService;

        public ProvidersController(ApplicationDbContext context,
            ProviderResult2 pResult, IToastNotification toastNotification,
            UserManager<ApplicationUser> userManager,
            IImportProcessorService importProcessorService)
        {
            _pResult = pResult;
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
            IQueryable<Provider> dtsource = null;

            if (user == null)
                return ResultData(param, new List<Provider>().AsQueryable());

            switch (user.ProfileType)
            {
                case ProfileTypes.ADMIN:
                    dtsource = from e in _context.Providers.Include(p => p.HMO).Include(p => p.Location).Include(p => p.State)
                               where e.Enabled != false && e.HMOID == user.HMOID
                               select e;
                    break;
                case ProfileTypes.PROVIDER:
                    dtsource = from e in _context.Providers.Include(e => e.HMO).Include(p => p.Location).Include(p => p.State)
                               where e.Enabled != false && _context.ApplicationUserProviders.Select(p => p.HMOID).Contains(e.HMOID)
                               select e;
                    break;
                default:
                    dtsource = _context.Providers.Include(p => p.HMO).Include(p => p.Location).Include(p => p.State).Where(p => p.Enabled != false);
                    break;
            }

            return ResultData(param, dtsource);
        }

        // GET: Admins
        public async Task<IActionResult> Index()
        {
            List<SelectListItem> tmp = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a Profile Type --" } };
            tmp.AddRange((await _context.ImportProviderSettings.ToListAsync()).OrderByDescending(r => r.DateCreated)
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

            var provider = await _context.Providers
                                            .Include(p => p.HMO)
                                            .Include(p => p.State)
                                            .Include(s => s.Location)
                                            .FirstOrDefaultAsync(s => s.ID == id);
            if (provider == null)
            {
                return NotFound();
            }

            return View(provider);
        }


        public async Task<IActionResult> StateLocations(string stateId)
        {
            var locs = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a Location --" } };
            if (string.IsNullOrEmpty(stateId))
                return Json(locs);

            int id = 0;
            bool isValid = int.TryParse(stateId, out id);
            if (!isValid)
                return Json(locs);

            var locations = _context.Locations.Where(l => l.StateID == id)
                                        .Select(cp => new SelectListItem
                                        {
                                            Value = cp.ID.ToString(),
                                            Text = cp.Name.ToLower().Humanize(LetterCasing.Title)
                                        });

            locs = await locations.ToListAsync();

            return Json(new { locations = locs });
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.States = new SelectList((await _context.States.ToListAsync())
                .Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }), "Value", "Text");

            ViewBag.Locations = new SelectList(new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a location --" } }, "Value", "Text");

            var tmp = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a HMO --" } };
            tmp.AddRange((await _context.HMOs.ToListAsync()).Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }));
            var hmos = new SelectList(tmp, "Value", "Text");
            ViewBag.HMOs = hmos;

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Provider provider)
        {
            if (ModelState.IsValid)
            {
                _context.Add(provider);
                await _context.SaveChangesAsync();
            }

            ViewBag.States = new SelectList((await _context.States.ToListAsync())
                .Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }), "Value", "Text", provider.StateID);

            ViewBag.Locations = new SelectList((await _context.Locations.Where(l => l.StateID == provider.Location.StateID).ToListAsync())
                .Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }), "Value", "Text", provider.LocationID);

            var tmp = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a HMO --" } };
            tmp.AddRange((await _context.HMOs.ToListAsync()).Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }));
            var hmos = new SelectList(tmp, "Value", "Text", provider.HMOID);
            ViewBag.HMOs = hmos;

            return View(provider);
        }


        // GET: Admins/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var provider = await _context.Providers
                                    .Include(p => p.State)
                                    .Include(s => s.Location)
                                    .Include(s => s.HMO)
                                    .FirstOrDefaultAsync(s => s.ID == id);
            if (provider == null)
            {
                return NotFound();
            }

            ViewBag.States = new SelectList((await _context.States.ToListAsync())
                .Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }), "Value", "Text", provider.StateID);

            ViewBag.Locations = new SelectList((await _context.Locations.Where(l => l.StateID == provider.Location.StateID).ToListAsync())
                .Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }), "Value", "Text", provider.LocationID);

            var tmp = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a HMO --" } };
            tmp.AddRange((await _context.HMOs.ToListAsync()).Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }));
            var hmos = new SelectList(tmp, "Value", "Text", provider.HMOID);
            ViewBag.HMOs = hmos;

            return View(provider);
        }

        // POST: Admins/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Provider provider)
        {
            if (id != provider.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(provider);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProviderExists(provider.ID))
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

            ViewBag.States = new SelectList((await _context.States.ToListAsync())
                .Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }), "Value", "Text", provider.StateID);

            ViewBag.Locations = new SelectList((await _context.Locations.Where(l => l.StateID == provider.Location.StateID).ToListAsync())
                .Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }), "Value", "Text", provider.LocationID);

            var tmp = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a HMO --" } };
            tmp.AddRange((await _context.HMOs.ToListAsync()).Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }));
            var hmos = new SelectList(tmp, "Value", "Text", provider.HMOID);
            ViewBag.HMOs = hmos;

            return View(provider);
        }

        private bool ProviderExists(int id)
        {
            return _context.Providers.Any(e => e.ID == id);
        }

        // GET: Admins/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _toastNotification.AddInfoToastMessage("Resource requested not found!");
                return NotFound();
            }

            var provider = await _context.Providers
                                    .Include(p => p.State)
                                    .Include(s => s.Location)
                                    .Include(s => s.HMO)
                                    .FirstOrDefaultAsync(s => s.ID == id);
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
            var provider = await _context.Providers.FirstOrDefaultAsync(p => p.ID == id);
            provider.Enabled = false;
            await _context.SaveChangesAsync();

            _toastNotification.AddSuccessToastMessage("Deleted Successfully!");
            return RedirectToAction(nameof(Index));
        }

        private JsonResult ResultData(DTParameters param, IQueryable<Provider> providers)
        {
            try
            {
                List<String> columnSearch = new List<string>();

                foreach (var col in param.Columns)
                    columnSearch.Add(col.Search.Value);

                List<Provider> data = _pResult.GetResult(param.Search.Value, param.SortOrder, param.Start, param.Length, providers, columnSearch);
                int count = _pResult.Count(param.Search.Value, providers, columnSearch);
                int id = param.Start + 1;
                DTResult<Provider> result = new DTResult<Provider>
                {
                    draw = param.Draw,
                    data = data.Select(r => new
                    {
                        sn = id++,
                        id = r.ID.ToString(),
                        name = r.Name.ToLower().Humanize(LetterCasing.Title),
                        address = r.Address.ToUpper(),
                        location = r.Location.Name.ToUpper(),
                        state = r.State.Name.ToUpper(),
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
        public async Task<IActionResult> ImportProviders(int profileId, int hmoId, IFormFile file)
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

                    var importSettings = await _context.ImportProviderSettings.FirstOrDefaultAsync(i => i.ID == profileId);

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

                    var result = _importProcessorService.ImportProviders(template, sheets, etype);
                    HashSet<Location> locations = new HashSet<Location>();
                    if (result.providers != null)
                    {
                        // check for duplication and augument.
                        var tmp = result.providers.Where(e =>
                        {
                            if (_context.Providers.Any(n => n.Name.Trim().ToUpper() == e.Name.Trim().ToUpper() && n.Address.Trim().ToUpper() == e.Address.Trim().ToUpper() && n.HMOID == hmoId))
                                return false;
                            else
                                return true;
                        }).Select(e =>
                        {
                            // set the state and location
                            var state = _context.States.FirstOrDefault(s => s.Name.Trim().ToLower() == e.State.Trim().ToLower());

                            var p = new Provider()
                            {
                                Name = e.Name.ToUpper().Trim(),
                                Address = e.Address.ToUpper().Trim(),
                                Email = e.Email,
                                PhoneNumber = e.PhoneNumber,
                                StateID = state.ID,
                                HMOID = hmoId,
                            };

                            var location = _context.Locations.FirstOrDefault(l => l.Name.Trim().ToLower() == e.Location.Trim().ToLower());
                            if (location == null)
                            {
                                location = new Location()
                                {
                                    Name = e.Location.ToUpper().Trim(),
                                    StateID = state.ID,
                                };
                                locations.Add(location);

                                p.Location = locations.FirstOrDefault(l => l.Name.Trim().ToLower() == e.Location.Trim().ToLower());
                            }
                            else
                                p.LocationID = location.ID;

                            return p;
                        });

                        await _context.AddRangeAsync(tmp);
                        await _context.SaveChangesAsync();
                    }

                    ImportResultVM vm = new ImportResultVM()
                    {
                        FileName = file.FileName,
                        SuccessfulRowsImported = result.providers != null ? result.providers.Count() : 0,
                        TotalRows = result.rowCount,
                        TotalErrors = result.errors != null ? result.errors.Count() : 0,
                        TotalRowsWithErrors = result.rowCount - (result.providers != null ? result.providers.Count() : 0),
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
