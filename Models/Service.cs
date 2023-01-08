using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Models
{
    public class Service
    {
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }
        public GenderX Gender { get; set; }

        // Greater than or Equal to Age
        [Display(Name = "Age", Description = "Age (Greater than or Equal to)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? GTE_Age { get; set; }

        public int HMOID { get; set; }
        [Display(Name = "HMO")]
        public virtual HMO HMO { get; set; }

        public bool? Enabled { get; set; }
    }
}
