using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AnnualHealthCheckJs.Controllers
{
    using Data;
    using Models;
    using Services.Core;
    using Tools;
    using ViewModels;

    public class ReminderController : Controller
    {
        private ApplicationDbContext _context;
        private IEmailSender _emailSender;

        public ReminderController(ApplicationDbContext context, IEmailSender emailSender)
        {
            _emailSender = emailSender;
            _context = context;
        }

        public IActionResult Reminder()
        {
            return View();
        }

        public IActionResult RateComplete()
        {
            return View();
        }
    }
}
