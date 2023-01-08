using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Models
{
    public class ClientService
    {
        public  long ID { get; set; }

        public int ClientID { get; set; }
        public virtual Client Client { get; set; }

        public int ServiceID { get; set; }
        public virtual Service Service { get; set; }

        [Display(Name = "Age", Description = "Age (Greater than or Equal to)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? GTE_Age { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
