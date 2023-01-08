using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AnnualHealthCheckJs.Data;
using AnnualHealthCheckJs.Models;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace AnnualHealthCheckJs.Controllers
{
    [Authorize(Roles = "SU,ADMIN")]
    public class ImportServiceSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ImportServiceSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ImportServiceSettings
        public async Task<IActionResult> Index()
        {
            return View(await _context.ImportServiceSettings.ToListAsync());
        }

        // GET: ImportServiceSettings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ImportServiceSettings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ImportServiceSettings importServiceSettings, IFormFile file)
        {
            if (file == null)
            {
                return View(importServiceSettings);
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

                            importServiceSettings.DateCreated = DateTime.Now;
                            importServiceSettings.Settings = json;

                            _context.Add(importServiceSettings);
                            await _context.SaveChangesAsync();

                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
            }
            return View(importServiceSettings);
        }

        // GET: ImportServiceSettings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var importServiceSettings = await _context.ImportServiceSettings
                .FirstOrDefaultAsync(m => m.ID == id);
            if (importServiceSettings == null)
            {
                return NotFound();
            }

            return View(importServiceSettings);
        }

        // POST: ImportServiceSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var importServiceSettings = await _context.ImportServiceSettings.FindAsync(id);
            _context.ImportServiceSettings.Remove(importServiceSettings);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ImportServiceSettingsExists(int id)
        {
            return _context.ImportServiceSettings.Any(e => e.ID == id);
        }
    }
}
