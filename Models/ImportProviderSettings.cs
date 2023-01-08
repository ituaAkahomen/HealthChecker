using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Models
{
    public class ImportProviderSettings
    {
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        public string Settings { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
