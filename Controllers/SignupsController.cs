using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
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
    public class SignupsController : Controller
    {
        private readonly SignupResult _sResult;
        private readonly SignupResult2 _sResult2;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IToastNotification _toastNotification;

        public SignupsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            SignupResult sResult, SignupResult2 sResult2, IToastNotification toastNotification)
        {
            _sResult = sResult;
            _sResult2 = sResult2;

            _context = context;
            _toastNotification = toastNotification;
            _userManager = userManager;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        [HttpPost]
        public async Task<IActionResult> List([FromBody] DTParameters param)
        {
            var user = await GetCurrentUserAsync();
            IQueryable<SignUp> dtsource = null;

            if (user == null)
                return ResultData(param, new List<SignUp>().AsQueryable());

            switch (user.ProfileType)
            {
                case ProfileTypes.ADMIN:
                    dtsource = from e in _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO)
                                .Include(s => s.Provider).ThenInclude(p => p.Location)
                               where e.Stage >= Steps.GetRef && e.Enrollee.HMOID == user.HMOID
                               select e;
                    break;
                case ProfileTypes.PROVIDER:
                    dtsource = from e in _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO)
                                .Include(s => s.Provider).ThenInclude(p => p.Location)
                               where e.Stage >= Steps.GetRef && _context.ApplicationUserProviders.Select(p => p.HMOID).Contains(e.Enrollee.HMOID)
                               select e;
                    break;
                default:
                    dtsource = from s in _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO)
                                .Include(s => s.Provider).ThenInclude(p => p.Location)
                               where s.Stage >= Steps.GetRef
                               select s;
                    break;
            }

            return ResultData(param, dtsource);
        }

        [HttpPost]
        public async Task<IActionResult> CompleteList([FromBody] DTParameters param)
        {
            var user = await GetCurrentUserAsync();
            IQueryable<SignUp> dtsource = null;

            if (user == null)
                return ResultData(param, new List<SignUp>().AsQueryable());

            switch (user.ProfileType)
            {
                case ProfileTypes.ADMIN:
                    dtsource = from e in _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO)
                                .Include(s => s.Provider).ThenInclude(p => p.Location)
                               where e.Stage == Steps.Completed && e.Enrollee.HMOID == user.HMOID
                               select e;
                    break;
                case ProfileTypes.PROVIDER:
                    dtsource = from e in _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO)
                                .Include(s => s.Provider).ThenInclude(p => p.Location)
                               where e.Stage == Steps.Completed && _context.ApplicationUserProviders.Select(p => p.HMOID).Contains(e.Enrollee.HMOID)
                               select e;
                    break;
                default:
                    dtsource = from s in _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO)
                                .Include(s => s.Provider).ThenInclude(p => p.Location)
                               where s.Stage == Steps.Completed
                               select s;
                    break;
            }

            return ResultData(param, dtsource);
        }

        // GET: Admins
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Completed()
        {
            return View();
        }

        // GET: Admins
        [Authorize(Roles = "ADMIN,PROVIDER")]
        public IActionResult Confirm()
        {
            return View();
        }

        // GET: Admins/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollee = await _context.SignUps.Include(s => s.Enrollee).FirstOrDefaultAsync(s => s.ID == id);
            if (enrollee == null)
            {
                return NotFound();
            }

            return View(enrollee);
        }

        [Authorize(Roles = "ADMIN,PROVIDER")]
        public async Task<IActionResult> ConfirmModal(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var signup = await _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO).FirstOrDefaultAsync(s => s.ID == id);
            if (signup == null)
            {
                return NotFound();
            }

            var vm = new ConfirmViewModel()
            {
                Id = signup.ID,
                Signup = signup,
            };

            var currentuser = await GetCurrentUserAsync();
            if (currentuser == null)
            {
                return NotFound();
            }

            if (currentuser.ProfileType == ProfileTypes.PROVIDER && signup.CheckedOn_ByProvider.HasValue)
            {
                vm.IsReadOnly = false;
                vm.CheckedOn = signup.CheckedOn_ByProvider.Value;
            }
            else if (currentuser.ProfileType == ProfileTypes.ADMIN && signup.CheckedOn_ByAdmin.HasValue)
            {
                vm.IsReadOnly = false;
                vm.CheckedOn = signup.CheckedOn_ByAdmin.Value;
            }
            else if (!signup.CheckedOn_ByAdmin.HasValue && !signup.CheckedOn_ByProvider.HasValue)
            {
                vm.IsReadOnly = false;
                vm.CheckedOn = signup.AppointmentDate.Value;
            }
            else
            {
                vm.IsReadOnly = true;
                vm.CheckedOn = signup.CheckedOn_ByAdmin ?? signup.CheckedOn_ByProvider ?? signup.CheckedOn ?? DateTime.Now.Date;
            }
            vm.ProfileType = currentuser.ProfileType;

            var config = await _context.ProjectConfig.FirstOrDefaultAsync();

            vm.StartDate = config.StartDate;
            vm.EndDate = DateTime.Now.Date <= config.EndDate ? DateTime.Now.Date : config.EndDate;

            return View(vm);
        }

        [Authorize(Roles = "ADMIN,PROVIDER")]
        [HttpPost]
        public async Task<IActionResult> ConfirmModal(ConfirmViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var signup = await _context.SignUps.FirstOrDefaultAsync(s => s.ID == model.Id);
            if (signup == null)
                return BadRequest("Oops, something went wrong!");

            if (model.ProfileType == ProfileTypes.PROVIDER)
                signup.CheckedOn_ByProvider = model.CheckedOn;
            else if (model.ProfileType == ProfileTypes.ADMIN)
                signup.CheckedOn_ByAdmin = model.CheckedOn;

            var currentuser = await GetCurrentUserAsync();
            if (currentuser == null)
                return BadRequest("Oops, something went wrong!");

            signup.CheckedOn_UserID = currentuser.Id;

            await _context.SaveChangesAsync();

            _toastNotification.AddSuccessToastMessage("Checked on Set:");
            return RedirectToAction(nameof(SignupsController.Confirm));
        }

        private JsonResult ResultData(DTParameters param, IQueryable<SignUp> enrollees)
        {
            try
            {
                List<String> columnSearch = new List<string>();

                foreach (var col in param.Columns)
                    columnSearch.Add(col.Search.Value);

                List<SignUp> data = _sResult2.GetResult(param.Search.Value, param.SortOrder, param.Start, param.Length, enrollees, columnSearch);
                int count = _sResult2.Count(param.Search.Value, enrollees, columnSearch);
                int id = param.Start + 1;
                DTResult<SignUp> result = new DTResult<SignUp>
                {
                    draw = param.Draw,
                    data = from r in data
                           let chk = r.CheckedOn_ByAdmin ?? r.CheckedOn_ByProvider ?? r.CheckedOn
                           let checkedon = chk.HasValue ? chk.Value.ToString("d MMM yyyy") : string.Empty
                           select new
                           {
                               sn = id++,
                               id = r.ID.ToString(),
                               empid = r.Enrollee.EmployeeID,
                               names = $"{r.Enrollee.LastName.ToLower().Humanize(LetterCasing.Title)} {r.Enrollee.OtherNames.ToLower().Humanize(LetterCasing.Title)}",
                               enrollid = !string.IsNullOrEmpty(r.Enrollee.EnrollmentID) ? r.Enrollee.EnrollmentID : "",
                               gender = Enum.GetName(typeof(Gender), r.Enrollee.Gender).Humanize(LetterCasing.Title),
                               provider = $"{r.Provider.Name} {r.Provider.Location.Name}".Trim().ToUpper().Humanize(LetterCasing.Title),
                               //year = r.Year.ToString(),
                               //authcode = r.AuthorizationCode,
                               hmo = r.Enrollee.HMO.Name.ToLower().Humanize(LetterCasing.Title),
                               checkedon = checkedon,
                               showcheck = r.AppointmentDate.Value.Date <= DateTime.Now.Date
                           },
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

    }
}
