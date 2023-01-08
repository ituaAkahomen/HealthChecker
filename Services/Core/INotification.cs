using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Services.Core
{
    public interface INotificationService
    {
        void AppointmentReminder();
        void RateAndCompleteReminder(string schemeHost);
    }
}
