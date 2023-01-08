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

    [Authorize(Roles = "SU,ADMIN,HR")]
    public class LocationsController : Controller
    {
        private readonly LocationResult _lResult;
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification _toastNotification;
        private readonly UserManager<ApplicationUser> _userManager;


        public LocationsController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            LocationResult lResult, IToastNotification toastNotification)
        {
            _lResult = lResult;
            _context = context;
            _toastNotification = toastNotification;
            _userManager = userManager;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        [HttpPost]
        public async Task<IActionResult> List([FromBody] DTParameters param)
        {
            var user = await GetCurrentUserAsync();
            IQueryable<Location> dtsource = null;

            if (user == null)
                return ResultData(param, new List<Location>().AsQueryable());

            switch (user.ProfileType)
            {
                case ProfileTypes.ADMIN:
                    dtsource = _context.Locations.Include(l => l.State);
                    break;
                case ProfileTypes.PROVIDER:
                    dtsource = _context.Locations.Include(l => l.State);
                    break;
                default:
                    dtsource = _context.Locations.Include(l => l.State);
                    break;
            }


            return ResultData(param, dtsource);
        }

        // GET: Admins
        public IActionResult Index()
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

            var loc = await _context.Locations.FirstOrDefaultAsync(s => s.ID == id);
            if (loc == null)
            {
                return NotFound();
            }

            return View(loc);
        }

        // GET: Admins/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollee = await _context.Enrollees.FirstOrDefaultAsync(s => s.ID == id);
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
        public async Task<IActionResult> Edit(int id, Location location)
        {
            if (id != location.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(location);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LocationExists(location.ID))
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
            return View(location);
        }

        private bool LocationExists(int id)
        {
            return _context.Locations.Any(e => e.ID == id);
        }


        private JsonResult ResultData(DTParameters param, IQueryable<Location> locations)
        {
            try
            {
                List<String> columnSearch = new List<string>();

                foreach (var col in param.Columns)
                    columnSearch.Add(col.Search.Value);

                List<Location> data = _lResult.GetResult(param.Search.Value, param.SortOrder, param.Start, param.Length, locations, columnSearch);
                int count = _lResult.Count(param.Search.Value, locations, columnSearch);
                int id = param.Start + 1;
                DTResult<Location> result = new DTResult<Location>
                {
                    draw = param.Draw,
                    data = data.Select(r => new
                    {
                        sn = id++,
                        id = r.ID.ToString(),
                        name = r.Name,
                        state = r.State.Name.ToLower().Humanize(LetterCasing.Title),
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
