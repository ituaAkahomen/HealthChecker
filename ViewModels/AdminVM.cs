using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.ViewModels
{
    public class AdminVM
    {
        public int Id { get; set; }
        public string Email { get; set; }
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public int hmoId { get; set; }
        public string HMO { get; set; }

        public int provId { get; set; }
        public string Provider { get; set; }
    }


    public class AdminVM2
    {
        public int Id { get; set; }
        public string Email { get; set; }
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Your Password is required")]
        public string Password { get; set; }
        [Compare("Password", ErrorMessage = "The password and confirm password do not match.")]
        public string ConfirmPassword { get; set; }
    }

}
