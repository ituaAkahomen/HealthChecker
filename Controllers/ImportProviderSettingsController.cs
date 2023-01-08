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
    public class ImportProviderSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ImportProviderSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ImportProviderSettings
        public async Task<IActionResult> Index()
        {
            return View(await _context.ImportProviderSettings.ToListAsync());
        }

        // GET: ImportProviderSettings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ImportProviderSettings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ImportProviderSettings importProviderSettings, IFormFile file)
        {
            if (file == null)
            {
                return View(importProviderSettings);
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

                            importProviderSettings.DateCreated = DateTime.Now;
                            importProviderSettings.Settings = json;

                            _context.Add(importProviderSettings);
                            await _context.SaveChangesAsync();

                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
            }
            return View(importProviderSettings);
        }

        // GET: ImportProviderSettings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var importProviderSettings = await _context.ImportProviderSettings
                .FirstOrDefaultAsync(m => m.ID == id);
            if (importProviderSettings == null)
            {
                return NotFound();
            }

            return View(importProviderSettings);
        }

        // POST: ImportProviderSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var importProviderSettings = await _context.ImportProviderSettings.FindAsync(id);
            _context.ImportProviderSettings.Remove(importProviderSettings);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ImportProviderSettingsExists(int id)
        {
            return _context.ImportProviderSettings.Any(e => e.ID == id);
        }
    }
}
