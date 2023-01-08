using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Models
{
    public class Provider
    {
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }
       
        public int LocationID { get; set; }
        public virtual Location Location { get; set; }

        public int StateID { get; set; }
        public virtual State State { get; set; }

        public int HMOID { get; set; }
        [Display(Name = "HMO")]
        public virtual HMO HMO { get; set; }

        [EmailAddress]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public bool? Enabled { get; set; }
    }
}
