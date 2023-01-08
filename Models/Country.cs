using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Models
{
    public class Country
    {
        public int ID { get; set; }

        [StringLength(50, MinimumLength = 2)]
        [Required]
        public string Name { get; set; }

        [StringLength(5, MinimumLength = 2)]
        [Required]
        public string ISO_Code_2 { get; set; }

        [StringLength(5, MinimumLength = 2)]
        [Required]
        public string ISO_Code_3 { get; set; }

        public virtual ICollection<State> States { get; set; }
    }
}
