using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AnnualHealthCheckJs.Controllers
{
    using Data;
    using Models;
    using Results;
    using ViewModels;

    public class StatesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly StateResult _sResult;

        public StatesController(ApplicationDbContext context, StateResult sResult)
        {
            _context = context;
            _sResult = sResult;
        }

        [HttpPost]
        public IActionResult List([FromBody] DTParameters param, [FromRoute] int? id)
        {
            IQueryable<StateVM> dtsource = null;
            if (!id.HasValue)
                dtsource = _context.States.Include(s => s.Country).Include(s => s.Locations)
                    .Select(s => new StateVM { ID = s.ID, Code = s.Code, Name = s.Name, CountryName = s.Country.Name, LocationCount = s.Locations.Count() });
            else
                dtsource = _context.States.Include(s => s.Country).Include(s => s.Locations).Where(s => s.CountryID == id)
                    .Select(s => new StateVM { ID = s.ID, Code = s.Code, Name = s.Name, CountryName = s.Country.Name, LocationCount = s.Locations.Count() });

            return ResultData(param, dtsource);
        }


        // GET: States
        //public async Task<IActionResult> Index()
        //{
        //    var applicationDbContext = _context.States.Include(s => s.Country);
        //    return View(await applicationDbContext.ToListAsync());
        //}
        // GET: States
        public async Task<IActionResult> Index(int? id)
        {
            if (!id.HasValue)
                return View();
            else
            {
                var country = await _context.Countries.FirstOrDefaultAsync(c => c.ID == id);
                if (country != null)
                    return View(country);
                else
                    return View();
            }

            //IQueryable<State> qstates = null;
            //if (!id.HasValue)
            //    qstates = _context.States.Include(s => s.Country);
            //else
            //    qstates = _context.States.Where(s => s.CountryID == id).Include(s => s.Country);

            //return View(await qstates.ToListAsync());
        }

        // GET: States/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var state = await _context.States
                .Include(s => s.Country)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (state == null)
            {
                return NotFound();
            }

            return View(state);
        }

        // GET: States/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var state = await _context.States.FindAsync(id);
            if (state == null)
            {
                return NotFound();
            }
            ViewData["CountryID"] = new SelectList(_context.Countries, "ID", "Name", state.CountryID);
            return View(state);
        }

        // POST: States/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Code,CountryID")] State state)
        {
            if (id != state.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(state);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StateExists(state.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CountryID"] = new SelectList(_context.Countries, "ID", "Name", state.CountryID);
            return View(state);
        }

        private bool StateExists(int id)
        {
            return _context.States.Any(e => e.ID == id);
        }


        private JsonResult ResultData(DTParameters param, IQueryable<StateVM> states)
        {
            try
            {
                List<String> columnSearch = new List<string>();

                foreach (var col in param.Columns)
                    columnSearch.Add(col.Search.Value);

                List<StateVM> data = _sResult.GetResult(param.Search.Value, param.SortOrder, param.Start, param.Length, states, columnSearch);
                int count = _sResult.Count(param.Search.Value, states, columnSearch);
                int id = param.Start + 1;
                DTResult<Country> result = new DTResult<Country>
                {
                    draw = param.Draw,
                    data = data.Select(r => new
                    {
                        sn = id++,
                        id = r.ID.ToString(),
                        name = r.Name,
                        country = r.CountryName,
                        locationcount = r.LocationCount
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
