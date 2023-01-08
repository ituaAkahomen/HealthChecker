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
    public class ImportEnrolleeSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ImportEnrolleeSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ImportEnrolleeSettings
        public async Task<IActionResult> Index()
        {
            return View(await _context.ImportEnrolleeSettings.ToListAsync());
        }

        // GET: ImportEnrolleeSettings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ImportEnrolleeSettings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ImportEnrolleeSettings importEnrolleeSettings, IFormFile file)
        {
            if (file == null)
            {
                return View(importEnrolleeSettings);
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

                            importEnrolleeSettings.DateCreated = DateTime.Now;
                            importEnrolleeSettings.Settings = json;

                            _context.Add(importEnrolleeSettings);
                            await _context.SaveChangesAsync();

                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
            }
            return View(importEnrolleeSettings);
        }

        // GET: ImportEnrolleeSettings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var importEnrolleeSettings = await _context.ImportEnrolleeSettings
                .FirstOrDefaultAsync(m => m.ID == id);
            if (importEnrolleeSettings == null)
            {
                return NotFound();
            }

            return View(importEnrolleeSettings);
        }

        // POST: ImportEnrolleeSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var importEnrolleeSettings = await _context.ImportEnrolleeSettings.FindAsync(id);
            _context.ImportEnrolleeSettings.Remove(importEnrolleeSettings);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ImportEnrolleeSettingsExists(int id)
        {
            return _context.ImportEnrolleeSettings.Any(e => e.ID == id);
        }
    }
}
