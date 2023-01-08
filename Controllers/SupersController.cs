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

namespace AnnualHealthCheckJs.Controllers
{
    using Data;
    using Models;
    using Results;
    using Services.Core;
    using Tools;
    using ViewModels;

    [Authorize(Roles = "SU")]
    public class SupersController : Controller
    {
        private readonly AdminResult _aResult;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IToastNotification _toastNotification;

        public SupersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            AdminResult aResult, IEmailSender emailSender, IToastNotification toastNotification)
        {
            _aResult = aResult;
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
            _toastNotification = toastNotification;
        }

        [HttpPost]
        public IActionResult List([FromBody] DTParameters param)
        {
            var dtsource = _context.Users.Where(u => u.ProfileType == ProfileTypes.SU).Select(u => new AdminVM { Id = u.Id, Email = u.Email, PhoneNumber = u.PhoneNumber });

            return ResultData(param, dtsource);
        }

        // GET: Admins
        public IActionResult Index()
        {
            return View();
            //return View(await _context.Users.Select(u => new AdminVM { Id = u.Id, Email = u.Email, PhoneNumber = u.PhoneNumber }).ToListAsync());
        }

        // GET: Admins/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adminVM = await _userManager.Users.Where(u => u.Id == id).Select(u => new AdminVM
            {
                Id = u.Id,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber
            }).FirstOrDefaultAsync();

            if (adminVM == null)
            {
                return NotFound();
            }

            return View(adminVM);
        }

        // GET: Admins/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admins/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,PhoneNumber")] AdminVM adminVM)
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
                    ProfileType = ProfileTypes.SU,
                };

                RandomStringGenerator RNG = new RandomStringGenerator(true, false, true, false);
                string rndpass = RNG.Generate("*Ll*ns");

                var result = await _userManager.CreateAsync(user, rndpass);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, Enum.GetName(typeof(ProfileTypes), ProfileTypes.SU));
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
                    return RedirectToAction(nameof(SupersController.Index));
                }
                else
                {
                    _toastNotification.AddErrorToastMessage(string.Join("<br/>", result.Errors.Select(r => r.Description)));
                    return View(adminVM);
                }
            }
            return View(adminVM);
        }

        // GET: Admins/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adminVM = await _userManager.Users.Where(u => u.Id == id).Select(u => new AdminVM { Id = u.Id, Email = u.Email, PhoneNumber = u.PhoneNumber }).FirstOrDefaultAsync();
            if (adminVM == null)
            {
                return NotFound();
            }
            return View(adminVM);
        }

        // POST: Admins/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email,PhoneNumber")] AdminVM adminVM)
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
                return RedirectToAction(nameof(Index));
            }
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

            var adminVM = await _userManager.Users.Where(u => u.Id == id).Select(u => new AdminVM { Id = u.Id, Email = u.Email, PhoneNumber = u.PhoneNumber }).FirstOrDefaultAsync();
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
