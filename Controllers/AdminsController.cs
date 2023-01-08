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

    [Authorize(Roles = "SU,ADMIN")]
    public class AdminsController : Controller
    {
        private readonly AdminResult2 _aResult;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IToastNotification _toastNotification;

        public AdminsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
                AdminResult2 aResult, IEmailSender emailSender, IToastNotification toastNotification)
        {
            _aResult = aResult;
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
            _toastNotification = toastNotification;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        [HttpPost]
        public async Task<IActionResult> List([FromBody] DTParameters param)
        {
            var user = await GetCurrentUserAsync();
            IQueryable<AdminVM> dtsource = null;

            if (user == null)
                return ResultData(param, new List<AdminVM>().AsQueryable());

            if (user.ProfileType == ProfileTypes.SU)
            {
                dtsource = _context.Users.Include(u => u.HMO).Where(u => u.ProfileType == ProfileTypes.ADMIN).Select(u => new AdminVM { Id = u.Id, Email = u.Email, PhoneNumber = u.PhoneNumber, hmoId = u.HMOID.Value, HMO = u.HMO.Name });
            }
            else if (user.ProfileType == ProfileTypes.ADMIN)
            {
                dtsource = _context.Users.Include(u => u.HMO).Where(u => u.ProfileType == ProfileTypes.ADMIN && u.HMOID == user.HMOID).Select(u => new AdminVM { Id = u.Id, Email = u.Email, PhoneNumber = u.PhoneNumber, hmoId = u.HMOID.Value, HMO = u.HMO.Name });
            }
            else
                return ResultData(param, new List<AdminVM>().AsQueryable());

            return ResultData(param, dtsource);
        }

        // GET: Admins
        public IActionResult Index()
        {
            //return View(await _context.Users.Select(u => new AdminVM { Id = u.Id, Email = u.Email, PhoneNumber = u.PhoneNumber }).ToListAsync());
            return View();
        }

        // GET: Admins/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adminVM = await _userManager.Users.Include(u => u.HMO).Where(u => u.Id == id).Select(u => new AdminVM
            {
                Id = u.Id,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                hmoId = u.HMOID.Value,
                HMO = u.HMO.Name
            }).FirstOrDefaultAsync();

            if (adminVM == null)
            {
                return NotFound();
            }

            return View(adminVM);
        }

        // GET: Admins/Create
        public async Task<IActionResult> Create()
        {
            var tmp = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a HMO --" } };
            tmp.AddRange((await _context.HMOs.ToListAsync()).Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }));
            var hmos = new SelectList(tmp, "Value", "Text");
            ViewBag.HMOs = hmos;

            var user = await GetCurrentUserAsync();

            if (user.ProfileType == ProfileTypes.ADMIN)
                ViewBag.HmoId = user.HMOID;

            return View();
        }

        // POST: Admins/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminVM adminVM)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = adminVM.Email,
                    Email = adminVM.Email,
                    PhoneNumber = adminVM.PhoneNumber,
                    Guid = Guid.NewGuid(),
                    DateCreated = DateTime.Now,
                    Enabled = true,
                    ProfileType = ProfileTypes.ADMIN,
                    HMOID = adminVM.hmoId
                };

                RandomStringGenerator RNG = new RandomStringGenerator(true, false, true, false);
                string rndpass = RNG.Generate("*Ll*ns");

                var result = await _userManager.CreateAsync(user, rndpass);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, Enum.GetName(typeof(ProfileTypes), ProfileTypes.ADMIN));
                    await _userManager.UpdateAsync(user);

                    var basePath = $"{this.Request.Scheme}://{this.Request.Host}";
                    await _emailSender.SendNewAccountWithRandomPassword(adminVM.Email, "UBN Annual Health Check: Account created",
                        new NewAccountVM()
                        {
                            Email = adminVM.Email,
                            Password = rndpass,
                            LoginLink = $"{basePath}/identity/account/login"
                        });

                    _toastNotification.AddSuccessToastMessage("Account Successfully created and password has been sent to recipient");
                    return RedirectToAction(nameof(AdminsController.Index));
                }
                else
                {
                    _toastNotification.AddErrorToastMessage(string.Join("<br/>", result.Errors.Select(r => r.Description)));
                    return View(adminVM);
                }
            }

            var tmp = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a HMO --" } };
            tmp.AddRange((await _context.HMOs.ToListAsync()).Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }));
            var hmos = new SelectList(tmp, "Value", "Text", adminVM.hmoId);
            ViewBag.HMOs = hmos;

            var currentuser = await GetCurrentUserAsync();

            if (currentuser.ProfileType == ProfileTypes.ADMIN)
                ViewBag.HmoId = currentuser.HMOID;

            return View(adminVM);
        }

        // GET: Admins/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adminVM = await _userManager.Users.Include(u => u.HMO).Where(u => u.Id == id).Select(u => new AdminVM { Id = u.Id, Email = u.Email, PhoneNumber = u.PhoneNumber, hmoId = u.HMOID.Value, HMO = u.HMO.Name }).FirstOrDefaultAsync();
            if (adminVM == null)
            {
                return NotFound();
            }

            var tmp = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a HMO --" } };
            tmp.AddRange((await _context.HMOs.ToListAsync()).Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }));
            var hmos = new SelectList(tmp, "Value", "Text", adminVM.hmoId);
            ViewBag.HMOs = hmos;

            var user = await GetCurrentUserAsync();

            if (user.ProfileType == ProfileTypes.ADMIN)
                ViewBag.HmoId = user.HMOID;


            return View(adminVM);
        }

        // POST: Admins/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminVM adminVM)
        {
            if (id != adminVM.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(id.ToString());
                    user.PhoneNumber = adminVM.PhoneNumber;
                    user.HMOID = adminVM.hmoId;

                    await _userManager.UpdateAsync(user);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdminVMExists(adminVM.Id))
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
                _toastNotification.AddSuccessToastMessage("Updated Successfully!");
                return RedirectToAction(nameof(Index));
            }

            var tmp = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a HMO --" } };
            tmp.AddRange((await _context.HMOs.ToListAsync()).Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }));
            var hmos = new SelectList(tmp, "Value", "Text", adminVM.hmoId);
            ViewBag.HMOs = hmos;

            var currentuser = await GetCurrentUserAsync();

            if (currentuser.ProfileType == ProfileTypes.ADMIN)
                ViewBag.HmoId = currentuser.HMOID;

            _toastNotification.AddErrorToastMessage("Oop, something went wrong, Please try again later.");
            return View(adminVM);
        }

        // GET: Admins/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _toastNotification.AddInfoToastMessage("Resource requested not found!");
                return NotFound();
            }

            var adminVM = await _userManager.Users.Include(u => u.HMO).Where(u => u.Id == id).Select(u => new AdminVM { Id = u.Id, Email = u.Email, PhoneNumber = u.PhoneNumber, hmoId = u.HMOID.Value, HMO = u.HMO.Name }).FirstOrDefaultAsync();
            if (adminVM == null)
            {
                _toastNotification.AddInfoToastMessage("Resource requested not found!");
                return NotFound();
            }

            return View(adminVM);
        }

        // POST: Admins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            //user.Enabled = false;
            //await _userManager.UpdateAsync(user);

            await _userManager.DeleteAsync(user);

            _toastNotification.AddSuccessToastMessage("Deleted Successfully!");
            return RedirectToAction(nameof(Index));
        }

        private bool AdminVMExists(int id)
        {
            return _userManager.Users.Any(e => e.Id == id);
        }


        private JsonResult ResultData(DTParameters param, IQueryable<AdminVM> admins)
        {
            try
            {
                List<String> columnSearch = new List<string>();

                foreach (var col in param.Columns)
                    columnSearch.Add(col.Search.Value);

                List<AdminVM> data = _aResult.GetResult(param.Search.Value, param.SortOrder, param.Start, param.Length, admins, columnSearch);
                int count = _aResult.Count(param.Search.Value, admins, columnSearch);
                int id = param.Start + 1;
                DTResult<AdminVM> result = new DTResult<AdminVM>
                {
                    draw = param.Draw,
                    data = data.Select(r => new
                    {
                        sn = id++,
                        id = r.Id.ToString(),
                        email = r.Email,
                        phone = r.PhoneNumber,
                        hmo = r.HMO.ToLower().Humanize(LetterCasing.Title)
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

    }
}
