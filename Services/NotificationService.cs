using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Services
{
    using Data;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Services.Core;

    public class NotificationService : INotificationService
    {
        private IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // 1. Remind on Appointment Day 7am.
        public void AppointmentReminder()
        {
            var signUps = from s in _context.SignUps.Include(q => q.Enrollee).Include(q => q.Provider)
                    where s.AppointmentDate.HasValue && s.AppointmentDate.Value.Date == DateTime.Now.Date
                    select s;
            //return q;
            foreach (var signup in signUps)
            {
                _emailSender.SendPreCheckupEmailReminder(signup);
            }
        }

        // 2. Remind Enrollee to Rate (For 5 days excluding weekends) 5pm
        public void RateAndCompleteReminder(string schemeHost)
        {
            if (DateTime.Now.Date.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.Date.DayOfWeek == DayOfWeek.Sunday)
                return;

            var signUps = from s in _context.SignUps.Include(q => q.Enrollee).Include(q => q.Provider)
                          where s.Stage < Steps.Completed && s.AppointmentDate.HasValue && s.AppointmentDate.Value.Date <= DateTime.Now.Date && s.AppointmentDate.Value.Date.AddDays(9) >= DateTime.Now
                    select s;
            //return q;
            foreach (var signup in signUps)
            {
                _emailSender.SendPostCheckupEmailReminder(signup, schemeHost);
            }

        }
    }
}
