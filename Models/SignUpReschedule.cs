using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Models
{
    public class SignUpReschedule
    {
        public long ID { get; set; }
        public int SignUpID { get; set; }
        public virtual SignUp SignUp { get; set; }

        public DateTime OldAppointmentDate { get; set; }
        public int OldProviderID { get; set; }
        public virtual Provider OldProvider { get; set; }
        public string OldAuthorizationCode { get; set; }

        public DateTime NewAppointmentDate { get; set; }
        public int NewProviderID { get; set; }
        public virtual Provider NewProvider { get; set; }
        public string NewAuthorizationCode { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
