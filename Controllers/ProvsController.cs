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
    public class ProvsController : Controller
    {
        private readonly AdminResult _aResult;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IToastNotification _toastNotification;

        public ProvsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
                AdminResult aResult, IEmailSender emailSender, IToastNotification toastNotification)
        {
            _aResult = aResult;
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
            _toastNotification = toastNotification;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        [HttpPost]
        public IActionResult List([FromBody] DTParameters param)
        {
            var dtsource = _context.Users.Where(u => u.ProfileType == ProfileTypes.PROVIDER).Select(u => new AdminVM { Id = u.Id, Email = u.Email, PhoneNumber = u.PhoneNumber });

            return ResultData(param, dtsource);
        }

        // GET: Admins
        public IActionResult Index()
        {
            //return View(await _context.Users.Select(u => new AdminVM { Id = u.Id, Email = u.Email, PhoneNumber = u.PhoneNumber }).ToListAsync());
            return View();
        }

        // GET: HRs/Details/5
        // GET: Admins/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var currentuser = await GetCurrentUserAsync();
            if (currentuser == null)
            {
                return null;
            }

            //var tmp = _userManager.Users.Include(u => u.Providers).ThenInclude(u => u.Provider).ThenInclude(p => p.Location).Where(u => u.Id == id);

            var adminVM = (from u in _userManager.Users
                           .Include(u => u.Providers).ThenInclude(u => u.Provider).ThenInclude(p => p.Location)
                          where u.Id == id
                          let prov = u.Providers.FirstOrDefault(p => p.HMOID == currentuser.HMOID).Provider
                          let loc = prov.Location.Name
                          select new AdminVM
                          {
                              Id = u.Id,
                              Email = u.Email,
                              PhoneNumber = u.PhoneNumber,
                              provId = prov.ID,
                              Provider = prov.Name.ToUpper() //+ " " + loc
                          }).FirstOrDefault();

            if (adminVM == null)
            {
                return NotFound();
            }

            return View(adminVM);
        }

        // GET: Admins/Create
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create()
        {
            var currentuser = await GetCurrentUserAsync();
            if (currentuser == null)
            {
                return null;
            }

            var tmp = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a Provider --" } };
            tmp.AddRange((await _context.Providers.Include(p => p.Location).Where(p => p.HMOID == currentuser.HMOID).ToListAsync()).Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = $"{r.Name.ToUpper()} - {r.Location.Name.Humanize(LetterCasing.Title)}" }));
            var provs = new SelectList(tmp, "Value", "Text");
            ViewBag.Providers = provs;

            return View();
        }

        // POST: Admins/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create(AdminVM adminVM)
        {
            var currentuser = await GetCurrentUserAsync();
            if (currentuser == null)
            {
                return null;
            }

            if (ModelState.IsValid)
            {
                var alreadyUser = await _userManager.FindByEmailAsync(adminVM.Email);
                if (alreadyUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = adminVM.Email,
                        Email = adminVM.Email,
                        PhoneNumber = adminVM.PhoneNumber,
                        Guid = Guid.NewGuid(),
                        DateCreated = DateTime.Now,
                        Enabled = true,
                        ProfileType = ProfileTypes.PROVIDER,
                        Providers = new List<ApplicationUserProvider>() { new ApplicationUserProvider() { ProviderID = adminVM.provId, HMOID = currentuser.HMOID.Value } }
                    };

                    RandomStringGenerator RNG = new RandomStringGenerator(true, false, true, false);
                    string rndpass = RNG.Generate("*Ll*ns");

                    var result = await _userManager.CreateAsync(user, rndpass);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, Enum.GetName(typeof(ProfileTypes), ProfileTypes.PROVIDER));
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
                        return RedirectToAction(nameof(ProvsController.Index));
                    }
                    else
                    {
                        _toastNotification.AddErrorToastMessage(string.Join("<br/>", result.Errors.Select(r => r.Description)));
                        return View(adminVM);
                    }
                }
                else
                {
                    if (!alreadyUser.Providers.Any(p => p.HMOID == currentuser.HMOID))
                    {
                        ApplicationUserProvider aup = new ApplicationUserProvider()
                        {
                            HMOID = currentuser.HMOID.Value,
                            ProviderID = adminVM.provId,
                            UserID = alreadyUser.Id
                        };
                        _context.ApplicationUserProviders.Add(aup);
                        _context.SaveChanges();

                        _toastNotification.AddSuccessToastMessage("Account Successfully updated");
                        return RedirectToAction(nameof(ProvsController.Index));
                    }
                }
            }

            var tmp = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a Provider --" } };
            tmp.AddRange((await _context.Providers.Include(p => p.Location).Where(p => p.HMOID == currentuser.HMOID).ToListAsync()).Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = $"{r.Name.ToUpper()} - {r.Location.Name.Humanize(LetterCasing.Title)}" }));
            var provs = new SelectList(tmp, "Value", "Text", adminVM.provId);
            ViewBag.Providers = provs;

            return View(adminVM);
        }

        // GET: Admins/Edit/5
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var currentuser = await GetCurrentUserAsync();
            if (currentuser == null)
            {
                return null;
            }

            var adminVM = await _userManager.Users.Include(u => u.Providers).Where(u => u.Id == id).Select(u => new AdminVM
            {
                Id = u.Id,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                provId = u.Providers.FirstOrDefault(p => p.HMOID == currentuser.HMOID).ProviderID
            }).FirstOrDefaultAsync();

            if (adminVM == null)
            {
                return NotFound();
            }

            var tmp = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a Provider --" } };
            tmp.AddRange((await _context.Providers.Include(p => p.Location).Where(p => p.HMOID == currentuser.HMOID).ToListAsync()).Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = $"{r.Name.ToUpper()} - {r.Location.Name.Humanize(LetterCasing.Title)}" }));
            var provs = new SelectList(tmp, "Value", "Text", adminVM.provId);
            ViewBag.Providers = provs;

            return View(adminVM);
        }

        // POST: Admins/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Edit(int id, AdminVM adminVM)
        {
            if (id != adminVM.Id)
            {
                return NotFound();
            }
            var currentuser = await GetCurrentUserAsync();
            if (currentuser == null)
            {
                return null;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(id.ToString());
                    user.PhoneNumber = adminVM.PhoneNumber;

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


            var tmp = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a Provider --" } };
            tmp.AddRange((await _context.Providers.Include(p => p.Location).Where(p => p.HMOID == currentuser.HMOID).ToListAsync()).Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = $"{r.Name.ToUpper()} - {r.Location.Name.Humanize(LetterCasing.Title)}" }));
            var provs = new SelectList(tmp, "Value", "Text", adminVM.provId);
            ViewBag.Providers = provs;

            _toastNotification.AddErrorToastMessage("Oop, something went wrong, Please try again later.");
            return View(adminVM);
        }

        // GET: Admins/Delete/5
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _toastNotification.AddInfoToastMessage("Resource requested not found!");
                return NotFound();
            }
            var currentuser = await GetCurrentUserAsync();
            if (currentuser == null)
            {
                return null;
            }

            var adminVM = await _userManager.Users.Include(u => u.Providers).Where(u => u.Id == id).Select(u => new AdminVM
            {
                Id = u.Id,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                provId = u.Providers.FirstOrDefault(p => p.HMOID == currentuser.HMOID).ProviderID,
                Provider = $"{u.Providers.FirstOrDefault(p => p.HMOID == currentuser.HMOID).Provider.Name.ToUpper()} - {u.Providers.FirstOrDefault(p => p.HMOID == currentuser.HMOID).Provider.Location.Name}"
            }).FirstOrDefaultAsync();
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
        [Authorize(Roles = "ADMIN")]
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
