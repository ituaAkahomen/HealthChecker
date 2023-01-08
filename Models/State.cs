using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Models
{
    public class State
    {
        public int ID { get; set; }

        [Required]
        [StringLength(70, MinimumLength = 3)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Code { get; set; }

        [Display(Name = "Country")]
        public int CountryID { get; set; }  // country Id
        public virtual Country Country { get; set; }

        public virtual ICollection<Location> Locations { get; set; }
    }
}
