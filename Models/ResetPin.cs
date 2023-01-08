using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Models
{
    public class ResetPin
    {
        public long ID { get; set; }

        public string LinkID { get; set; }
        public string Code { get; set; }

        public int EnrolleeID { get; set; }
        public virtual Enrollee Enrollee { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateExpired { get; set; }
    }
}
