using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AnnualHealthCheckJs.Data;
using AnnualHealthCheckJs.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace AnnualHealthCheckJs.Controllers
{
    [Authorize(Roles = "SU,ADMIN")]
    public class ImportExcludedEnrolleeSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ImportExcludedEnrolleeSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ImportExcludedEnrolleeSettings
        public async Task<IActionResult> Index()
        {
            return View(await _context.ImportExcludedEnrolleeSettings.ToListAsync());
        }

        // GET: ImportExcludedEnrolleeSettings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ImportExcludedEnrolleeSettings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ImportExcludedEnrolleeSettings importExcludedEnrolleeSettings, IFormFile file)
        {
            if (file == null)
            {
                return View(importExcludedEnrolleeSettings);
            }

            if (ModelState.IsValid)
            {
                if (file.Length > 0)
                {
                    var filePath = Path.GetTempFileName();
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                        stream.Position = 0;

                        using (StreamReader r = new StreamReader(stream))
                        {
                            string json = r.ReadToEnd();

                            importExcludedEnrolleeSettings.DateCreated = DateTime.Now;
                            importExcludedEnrolleeSettings.Settings = json;

                            _context.Add(importExcludedEnrolleeSettings);
                            await _context.SaveChangesAsync();

                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
            }
            return View(importExcludedEnrolleeSettings);
        }

        // GET: ImportExcludedEnrolleeSettings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var importExcludedEnrolleeSettings = await _context.ImportExcludedEnrolleeSettings
                .FirstOrDefaultAsync(m => m.ID == id);
            if (importExcludedEnrolleeSettings == null)
            {
                return NotFound();
            }

            return View(importExcludedEnrolleeSettings);
        }

        // POST: ImportExcludedEnrolleeSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var importExcludedEnrolleeSettings = await _context.ImportExcludedEnrolleeSettings.FindAsync(id);
            _context.ImportExcludedEnrolleeSettings.Remove(importExcludedEnrolleeSettings);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ImportExcludedEnrolleeSettingsExists(int id)
        {
            return _context.ImportExcludedEnrolleeSettings.Any(e => e.ID == id);
        }
    }
}
