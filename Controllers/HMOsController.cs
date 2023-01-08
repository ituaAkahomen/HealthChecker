using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

using Humanizer;
using NToastNotify;


namespace AnnualHealthCheckJs.Controllers
{
    using Data;
    using Models;
    using Results;
    using Services.Core;
    using SixLabors.ImageSharp;
    using Tools;
    using ViewModels;


    [Authorize(Roles = "SU")]
    public class HMOsController : Controller
    {
        private readonly HMOResult _hResult;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IEmailSender _emailSender;
        private readonly IToastNotification _toastNotification;

        public HMOsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
                HMOResult hResult, IEmailSender emailSender, IToastNotification toastNotification,
                IHostingEnvironment hostingEnvironment)
        {
            _hResult = hResult;
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
            _toastNotification = toastNotification;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        public IActionResult List([FromBody] DTParameters param)
        {
            var dtsource = _context.HMOs;

            return ResultData(param, dtsource);
        }


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

            var hmo = await _context.HMOs.FirstOrDefaultAsync(s => s.ID == id);
            if (hmo == null)
            {
                return NotFound();
            }

            return View(hmo);
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
        public async Task<IActionResult> Create(HMO model, IFormFile signatureImage,
            IFormFile logoImage/*, IFormFile templateFile*/)
        {
            if (ModelState.IsValid)
            {
                var filePath = Path.GetTempFileName();
                if (signatureImage == null || logoImage == null/* || templateFile == null*/)
                {
                    _toastNotification.AddInfoToastMessage("Please check!");
                    return View(model);
                }

                model.Enabled = true;
                model.DateCreated = DateTime.Now;
                model.Guid = Guid.NewGuid();

                if (signatureImage.Length > 0)
                {
                    var filename = Path.Combine(_hostingEnvironment.WebRootPath, @"uploads\signatures\", $"{model.Guid}.png");
                    FileInfo newfile = new FileInfo(filename);

                    using (var outputStream = newfile.OpenWrite())  // new MemoryStream())
                    {
                        using (var inputStream = signatureImage.OpenReadStream())
                        {
                            using (var image = Image.Load(inputStream))
                            {
                                //var cvm = JsonConvert.DeserializeObject<CropperViewModel>(model.LogoData);
                                image
                                    //.Crop(new Rectangle(cvm.ix, cvm.iy, cvm.iwidth, cvm.iheight))
                                    .SaveAsPng(outputStream);
                            }
                        }
                    }
                }

                if (logoImage.Length > 0)
                {
                    var filename = Path.Combine(_hostingEnvironment.WebRootPath, @"uploads\logos\", $"{model.Guid}.png");
                    FileInfo newfile = new FileInfo(filename);

                    using (var outputStream = newfile.OpenWrite())  // new MemoryStream())
                    {
                        using (var inputStream = logoImage.OpenReadStream())
                        {
                            using (var image = Image.Load(inputStream))
                            {
                                //var cvm = JsonConvert.DeserializeObject<CropperViewModel>(model.LogoData);
                                image
                                    //.Crop(new Rectangle(cvm.ix, cvm.iy, cvm.iwidth, cvm.iheight))
                                    .SaveAsPng(outputStream);
                            }
                        }
                    }
                }



                _context.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            _toastNotification.AddInfoToastMessage("Resource requested not found!");
            return View(model);
        }


        // GET: Admins/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _toastNotification.AddInfoToastMessage("Resource requested not found!");
                return NotFound();
            }

            var hmo = await _context.HMOs.FirstOrDefaultAsync(s => s.ID == id);
            if (hmo == null)
            {
                _toastNotification.AddInfoToastMessage("Resource requested not found!");
                return NotFound();
            }
            return View(hmo);
        }

        // POST: Admins/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HMO hmo)
        {
            if (id != hmo.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hmo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HMOExists(hmo.ID))
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

            _toastNotification.AddErrorToastMessage("Oop, something went wrong, Please try again later.");
            return View(hmo);
        }

        // GET: Admins/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _toastNotification.AddInfoToastMessage("Resource requested not found!");
                return NotFound();
            }

            var hmo = await _context.HMOs.FirstOrDefaultAsync(m => m.ID == id);
            if (hmo == null)
            {
                _toastNotification.AddInfoToastMessage("Resource requested not found!");
                return NotFound();
            }

            return View(hmo);
        }

        // POST: Admins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hmo = await _context.HMOs.FindAsync(id);
            _context.HMOs.Remove(hmo);
            await _context.SaveChangesAsync();
            _toastNotification.AddSuccessToastMessage("Deleted Successfully!");
            return RedirectToAction(nameof(Index));
        }

        private bool HMOExists(int id)
        {
            return _context.HMOs.Any(e => e.ID == id);
        }


        private JsonResult ResultData(DTParameters param, IQueryable<HMO> hmos)
        {
            try
            {
                List<String> columnSearch = new List<string>();

                foreach (var col in param.Columns)
                    columnSearch.Add(col.Search.Value);

                List<HMO> data = _hResult.GetResult(param.Search.Value, param.SortOrder, param.Start, param.Length, hmos, columnSearch);
                int count = _hResult.Count(param.Search.Value, hmos, columnSearch);
                int id = param.Start + 1;
                DTResult<AdminVM> result = new DTResult<AdminVM>
                {
                    draw = param.Draw,
                    data = data.Select(r => new
                    {
                        sn = id++,
                        id = r.ID.ToString(),
                        name = r.Name.ToUpper(),
                        signatoryName = r.SignatoryName.Humanize(LetterCasing.Title),
                        signatoryDesignation = r.SignatoryDesignation.Humanize(LetterCasing.Title),
                        dateCreated = r.DateCreated.ToString("d MMM, yyyy")
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