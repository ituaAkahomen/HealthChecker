using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.ViewModels
{
    public class DemoVM
    {
        [EmailAddress]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        [Display(Name = "HMO")]
        public int hmoId { get; set; }
    }

    public class NameAPI
    {
        public string name { get; set; }
        public string surname { get; set; }
        public string gender { get; set; }
        public string region { get; set; }
        public int age { get; set; }
        public string title { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string photo { get; set; }
        public Birthday birthday { get; set; }
        public CreditCard credit_card { get; set; }
    }

    public class Birthday
    {
        public string dmy { get; set; }
        public string mdy { get; set; }
        public long raw { get; set; }
    }

    public class CreditCard
    {
        public string expiration { get; set; }
        public string number { get; set; }
        public string pin { get; set; }
        public string security { get; set; }
    }
}
