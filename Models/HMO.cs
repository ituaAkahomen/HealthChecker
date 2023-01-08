using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Models
{
    public class HMO
    {
        public int ID { get; set; }

        public Guid Guid { get; set; }

        [Required]
        public string Name { get; set; }

        //[Display(Name = "HMO Logo Path")]
        //public string LogoPath { get; set; }

        [Required]
        [Display(Name = "Name of Signatory")]
        public string SignatoryName { get; set; }

        [Required]
        [Display(Name = "Designation of Signatory")]
        public string SignatoryDesignation { get; set; }

        //[Display(Name = "Signature Image")]
        //public string SignatureImagePath { get; set; }

        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Auth Code Template")]
        public string AuthCodeTemplate { get; set; }

        [Required]
        [Display(Name = "PIN Regex")]
        public string PinRegex { get; set; }

        public bool Enabled { get; set; }

        public string EmailsToCC { get; set; }      // semi-colon seperated

        [Display(Name = "Generate Authorization Code on Signup Complete")]
        public bool? GenerateAuthCodeOnSignUpComplete { get; set; } = false;

        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; }
    }
}
