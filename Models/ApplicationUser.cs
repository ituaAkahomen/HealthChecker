using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AnnualHealthCheckJs.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public Guid Guid { get; set; }

        public ProfileTypes ProfileType { get; set; }

        public bool Enabled { get; set; }

        public DateTime? AccountExpires { get; set; }


        public int? HMOID { get; set; }
        public virtual HMO HMO { get; set; }

        public virtual ICollection<ApplicationUserProvider> Providers { get; set; }

        //public int? ProviderID { get; set; }
        //public virtual Provider Provider { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
