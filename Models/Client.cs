using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Models
{
    public class Client
    {
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        public string OperatingAddress { get; set; }

        [Required]
        public string AuthorizationCodeTemplate { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
