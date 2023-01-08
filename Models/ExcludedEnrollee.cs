using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Models
{
    public class ExcludedEnrollee
    {
        public int ID { get; set; }

        public int EnrolleeID { get; set; }
        public virtual Enrollee Enrollee { get; set; }

        public string Reason { get; set; }
        public int Year { get; set; }

        //public int HMOID { get; set; }
        //[Display(Name = "HMO")]
        //public virtual HMO HMO { get; set; }
    }
}
