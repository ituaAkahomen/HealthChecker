using System;
using System.Collections.Generic;
using System.Globalization;
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

using Flurl.Http;


namespace AnnualHealthCheckJs.Controllers
{
    using Data;
    using Models;
    using Results;
    using Services.Core;
    using Tools;
    using ViewModels;

    [Authorize(Roles = "SU,ADMIN,PROVIDER,HR")]
    public class EnrolleesController : Controller
    {
        private readonly EnrolleeResult2 _eResult;
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification _toastNotification;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IImportProcessorService _importProcessorService;

        public EnrolleesController(ApplicationDbContext context,
            EnrolleeResult2 eResult, IToastNotification toastNotification,
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
            IQueryable<Enrollee> dtsource = null;

            if (user == null)
                return ResultData(param, new List<Enrollee>().AsQueryable());

            switch (user.ProfileType)
            {
                case ProfileTypes.ADMIN:
                    dtsource = from e in _context.Enrollees.Include(e => e.HMO)
                               where e.ClientPlan != "demo" && e.HMOID == user.HMOID
                               select e;
                    break;
                case ProfileTypes.PROVIDER:
                    dtsource = from e in _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO)
                               where e.Stage >= Steps.GetRef && _context.ApplicationUserProviders.Select(p => p.HMOID).Contains(e.Enrollee.HMOID)
                               select e.Enrollee;
                    break;
                default:
                    dtsource = _context.Enrollees.Include(e => e.HMO).Where(e => e.ClientPlan != "demo");
                    break;
            }

            return ResultData(param, dtsource);
        }

        // GET: Admins
        public async Task<IActionResult> Index()
        {
            List<SelectListItem> tmp = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a Profile Type --" } };
            tmp.AddRange((await _context.ImportEnrolleeSettings.ToListAsync()).OrderByDescending(r => r.DateCreated)
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

            var enrollee = await _context.Enrollees.Include(e => e.HMO).FirstOrDefaultAsync(s => s.ID == id);
            if (enrollee == null)
            {
                return NotFound();
            }

            return View(enrollee);
        }

        // GET: Admins/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollee = await _context.Enrollees.Include(e => e.HMO).FirstOrDefaultAsync(s => s.ID == id);
            if (enrollee == null)
            {
                return NotFound();
            }
            return View(enrollee);
        }

        // POST: Admins/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Enrollee enrollee)
        {
            if (id != enrollee.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrolleeExists(enrollee.ID))
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
            return View(enrollee);
        }

        private bool EnrolleeExists(int id)
        {
            return _context.Enrollees.Any(e => e.ID == id);
        }

        private JsonResult ResultData(DTParameters param, IQueryable<Enrollee> enrollees)
        {
            try
            {
                List<String> columnSearch = new List<string>();

                foreach (var col in param.Columns)
                    columnSearch.Add(col.Search.Value);

                List<Enrollee> data = _eResult.GetResult(param.Search.Value, param.SortOrder, param.Start, param.Length, enrollees, columnSearch);
                int count = _eResult.Count(param.Search.Value, enrollees, columnSearch);
                int id = param.Start + 1;
                DTResult<Enrollee> result = new DTResult<Enrollee>
                {
                    draw = param.Draw,
                    data = data.Select(r => new
                    {
                        sn = id++,
                        id = r.ID.ToString(),
                        empid = r.EmployeeID,
                        names = $"{r.LastName.ToLower().Humanize(LetterCasing.Title)} {r.OtherNames.ToLower().Humanize(LetterCasing.Title)}",
                        enrollid = !string.IsNullOrEmpty(r.EnrollmentID) ? r.EnrollmentID : "",
                        gender = Enum.GetName(typeof(Gender), r.Gender).Humanize(LetterCasing.Title),
                        dob = r.DOB.HasValue ? r.DOB.Value.ToString("d MMM yyyy").ToUpper() : "",
                        phone = !string.IsNullOrEmpty(r.MobileNumber) ? r.MobileNumber : "",
                        email = !string.IsNullOrEmpty(r.Email) ? r.Email.ToLower() : "",
                        enabled = r.Enabled.ToString().ToLower().Humanize(LetterCasing.Title),
                        hmo = r.HMO.Name.Humanize(LetterCasing.Title),
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
        public async Task<IActionResult> ImportEnrollees(int profileId, int hmoId, IFormFile file)
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

                    var importSettings = await _context.ImportEnrolleeSettings.FirstOrDefaultAsync(i => i.ID == profileId);

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

                    var result = _importProcessorService.ImportEnrollees(template, sheets, etype);
                    if (result.enrollees != null)
                    {
                        var hmo = await _context.HMOs.FirstOrDefaultAsync(h => h.ID == hmoId);

                        // check for duplication and augument.
                        HashSet<string> tmpAuthCodes = new HashSet<string>();
                        var tmp = result.enrollees.Where(e =>
                        {
                            if (string.IsNullOrEmpty(e.EmployeeID) || string.IsNullOrEmpty(e.LastName))
                                return false;
                            if (_context.Enrollees.Any(n => (/*!string.IsNullOrEmpty(e.EmployeeID) &&*/ n.EmployeeID == e.EmployeeID))) /*|| (!string.IsNullOrEmpty(e.EnrollmentID) && n.EnrollmentID == n.EnrollmentID)))*/
                                return false;
                            else
                                return true;
                        }).Select(e =>
                        {
                            //if (string.IsNullOrEmpty(e.Email))
                            //    e.Enabled = false;
                            //else
                            e.Enabled = true;

                            e.HMOID = hmoId;
                            //e.PIN = (new RandomStringGenerator(false, false, true, false)).Generate("nnnn");

                            do
                            {
                                e.TmpAuthCode = hmo.AuthCodeTemplate.GenerateAuthCodeFromTemplate();
                                //var code = (new RandomStringGenerator(false, false, true, false)).Generate("nnnnn");
                                //e.TmpAuthCode = $"ROD1/8-919/AC/UB/ANY/{code}";
                            } while (_context.Enrollees.Any(r => r.TmpAuthCode == e.TmpAuthCode) || tmpAuthCodes.Any(r => r == e.TmpAuthCode));

                            tmpAuthCodes.Add(e.TmpAuthCode);
                            return e;
                        });

                        var qtmp = tmp.ToList();
                        await _context.AddRangeAsync(qtmp);
                        await _context.SaveChangesAsync();
                    }

                    ImportResultVM vm = new ImportResultVM()
                    {
                        FileName = file.FileName,
                        SuccessfulRowsImported = result.enrollees != null ? result.enrollees.Count() : 0,
                        TotalRows = result.rowCount,
                        TotalErrors = result.errors != null ? result.errors.Count() : 0,
                        TotalRowsWithErrors = result.rowCount - (result.enrollees != null ? result.enrollees.Count() : 0),
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportEnrolleesUnitedFix(int profileId, int hmoId, IFormFile file)
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

                    var importSettings = await _context.ImportEnrolleeSettings.FirstOrDefaultAsync(i => i.ID == profileId);

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

                    var result = _importProcessorService.ImportEnrollees(template, sheets, etype);
                    if (result.enrollees != null)
                    {
                        var hmo = await _context.HMOs.FirstOrDefaultAsync(h => h.ID == hmoId);

                        // check for duplication and augument.
                        HashSet<string> tmpAuthCodes = new HashSet<string>();
                        var tmp = result.enrollees
                            .Where(e =>
                        {
                            if (string.IsNullOrEmpty(e.EmployeeID) || string.IsNullOrEmpty(e.LastName))
                                return false;
                            if (_context.Enrollees.Any(n => (/*!string.IsNullOrEmpty(e.EmployeeID) &&*/ n.EmployeeID == e.EmployeeID))) //| (!string.IsNullOrEmpty(e.EnrollmentID) && n.EnrollmentID == n.EnrollmentID)))
                                return false;
                            else
                                return true;
                        })
                            .Select(e =>
                        {
                            e.Enabled = true;

                            e.HMOID = hmoId;

                            do
                            {
                                e.TmpAuthCode = hmo.AuthCodeTemplate.GenerateAuthCodeFromTemplate();
                            } while (_context.Enrollees.Any(r => r.TmpAuthCode == e.TmpAuthCode) || tmpAuthCodes.Any(r => r == e.TmpAuthCode));

                            tmpAuthCodes.Add(e.TmpAuthCode);
                            return e;
                        });

                        var qtmp = tmp.ToList();
                        await _context.AddRangeAsync(qtmp);

                        // update existing
                        tmp = result.enrollees
                            .Where(e =>
                            {
                                if (_context.Enrollees.Any(n => (/*!string.IsNullOrEmpty(e.EmployeeID) &&*/ n.EmployeeID == e.EmployeeID))) //| (!string.IsNullOrEmpty(e.EnrollmentID) && n.EnrollmentID == n.EnrollmentID)))
                                    return true;
                                else
                                    return false;
                            });

                        if (tmp.Any())
                        {
                            foreach (var e in tmp)
                            {
                                var enr = _context.Enrollees.FirstOrDefault(n => (/*!string.IsNullOrEmpty(e.EmployeeID) &&*/ n.EmployeeID == e.EmployeeID)); //| (!string.IsNullOrEmpty(e.EnrollmentID) && n.EnrollmentID == n.EnrollmentID));
                                enr.DOB = e.DOB;

                                _context.Enrollees.Update(enr);
                            }
                        }

                        await _context.SaveChangesAsync();
                    }

                    ImportResultVM vm = new ImportResultVM()
                    {
                        FileName = file.FileName,
                        SuccessfulRowsImported = result.enrollees != null ? result.enrollees.Count() : 0,
                        TotalRows = result.rowCount,
                        TotalErrors = result.errors != null ? result.errors.Count() : 0,
                        TotalRowsWithErrors = result.rowCount - (result.enrollees != null ? result.enrollees.Count() : 0),
                        Errors = result.errors,
                        RowsWithErrors = result.errRows
                    };

                    return View("ImportEnrollees", vm);
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

        public async Task<IActionResult> Demos()
        {
            var qemployees = from q in _context.Enrollees.Include(e => e.HMO)
                             where q.ClientPlan == "demo"
                             select q;
            return View(await qemployees.ToListAsync());
        }

        public async Task<IActionResult> CreateDemo()
        {
            var tmp = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a HMO --" } };
            tmp.AddRange((await _context.HMOs.ToListAsync()).Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }));
            var hmos = new SelectList(tmp, "Value", "Text");
            ViewBag.HMOs = hmos;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateDemo(DemoVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var name = await "https://uinames.com/api/?ext".GetJsonAsync<NameAPI>();

            var hmo = await _context.HMOs.FirstOrDefaultAsync(h => h.ID == model.hmoId);

            var enrollee = new Enrollee()
            {
                DOB = DateTime.ParseExact(name.birthday.dmy, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                Email = !string.IsNullOrEmpty(model.Email) ? model.Email.Trim().ToLower() : null,
                ClientPlan = "demo",
                Enabled = true,
                MobileNumber = model.PhoneNumber,
                Gender = name.gender.ToLower().Trim() == "male" ? Gender.MALE : Gender.FEMALE,
                LastName = name.surname.ToUpper(),
                OtherNames = name.name.ToUpper(),
                PIN = string.Empty,
                HMOID = model.hmoId
            };
            do
            {
                enrollee.EmployeeID = $"9{(new RandomStringGenerator(false, false, true, false)).Generate("nnnnnn")}";
            } while (_context.Enrollees.Any(r => r.EmployeeID == enrollee.EmployeeID));

            do
            {
                var code = (new RandomStringGenerator(false, false, true, false)).Generate("nnn");
                enrollee.EnrollmentID = $"{enrollee.EmployeeID}/{code}/A";
            } while (_context.Enrollees.Any(r => r.EnrollmentID == enrollee.EnrollmentID));

            do
            {
                enrollee.TmpAuthCode = hmo.AuthCodeTemplate.GenerateAuthCodeFromTemplate();
                //var code = (new RandomStringGenerator(false, false, true, false)).Generate("nnnnn");
                //enrollee.TmpAuthCode = $"ROD1/9-808/AC/UB/ANY/{code}";
            } while (_context.Enrollees.Any(r => r.TmpAuthCode == enrollee.TmpAuthCode));

            _context.Add(enrollee);
            await _context.SaveChangesAsync();

            var tmp = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a HMO --" } };
            tmp.AddRange((await _context.HMOs.ToListAsync()).Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }));
            var hmos = new SelectList(tmp, "Value", "Text", model.hmoId);

            return RedirectToAction(nameof(EnrolleesController.Demos));
        }


        public async Task<IActionResult> RemoveDemo(int? id)
        {
            if (id == null)
                return NotFound();

            var enrollee = await _context.Enrollees.FirstOrDefaultAsync(e => e.ID == id);
            if (enrollee == null)
                return NotFound();

            return View(enrollee);
        }

        // POST: ImportServiceSettings/Delete/5
        [HttpPost, ActionName("RemoveDemo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveDemoConfirmed(int id)
        {
            var enrollees = await _context.Enrollees.FindAsync(id);
            _context.Enrollees.Remove(enrollees);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Demos));
        }

        public async Task<IActionResult> RemoveAllDemos()
        {
            var enrollees = _context.Enrollees.Where(e => e.ClientPlan == "demo");

            _context.Enrollees.RemoveRange(enrollees);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(EnrolleesController.Demos));
        }
    }
}
