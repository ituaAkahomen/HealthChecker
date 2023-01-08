using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;

namespace AnnualHealthCheckJs.Data
{
    using Services.Core;
    using Tools;

    public class HangfireInit
    {
        private readonly INotificationService notificationService;

        public HangfireInit(INotificationService _notificationService)
        {
            notificationService = _notificationService;
        }

        public void Initialize()
        {
            RecurringJob.AddOrUpdate(() => ScheduleAppointment(), Cron.Daily(6, 30));
            RecurringJob.AddOrUpdate(() => ScheduleRating(), Cron.Daily(17, 30));
        }

        [DisableConcurrentExecution(timeoutInSeconds: 30 * 60)]
        public void ScheduleAppointment()
        {
            notificationService.AppointmentReminder();
        }

        [DisableConcurrentExecution(timeoutInSeconds: 30 * 60)]
        public void ScheduleRating()
        {
            notificationService.RateAndCompleteReminder(Context.GetSchemeHost());
        }
    }
}
