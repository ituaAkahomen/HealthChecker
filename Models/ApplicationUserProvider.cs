using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Models
{
    public class ApplicationUserProvider
    {
        public int ID { get; set; }

        public int UserID { get; set; }
        public virtual ApplicationUser User { get; set; }

        public int ProviderID { get; set; }
        public virtual Provider Provider { get; set; }

        public int HMOID { get; set; }
        public virtual HMO HMO { get; set; }
    }
}
