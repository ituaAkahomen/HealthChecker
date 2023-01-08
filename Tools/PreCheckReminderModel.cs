using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Tools
{
    public class PreCheckReminderModel
    {
        public string FullNames { get; set; }
        public string ProviderName { get; set; }
        public string Address { get; set; }
    }
    public class PostCheckReminderModel
    {
        public string FullNames { get; set; }
        public string Day { get; set; }
        public string ProviderName { get; set; }
        public string Address { get; set; }
        public string Link { get; set; }
    }

}
