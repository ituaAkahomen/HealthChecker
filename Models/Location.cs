using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Models
{
    public class Location
    {
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        public int StateID { get; set; }
        public virtual State State { get; set; }
    }
}
