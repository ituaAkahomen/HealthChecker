using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.ViewModels
{
    using Models;

    public class ReferenceLetterVM
    {
        public SignUp SignUp { get; set; }

        public Provider Provider { get; set; }
        //public List<Provider> Providers { get; set; }
        public List<Service> Services { get; set; }
        public List<Service> Over40Services { get; set; }
        public bool HasDOB { get; set; }
        public bool GenderIsKnown { get; set; }
    }
}
