using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AnnualHealthCheckJs.ViewModels
{
    using Attributes;
    using Models;

    public class VerifyIdentityVM
    {
        [Required(ErrorMessage = "Your ID is Required")]
        //[PIN]
        //[RegularExpression(@"(^\d{7}$)|(^\d{7}/\d{3}/[Aa]$)|(^((ROHL/UBN/EXC/)|(rohl/ubn/exc/))\d{3,4}/[Aa]$)", ErrorMessage = "Incorrect ID Pattern")]
        public string ID { get; set; }

        [Required(ErrorMessage = "Your Password is Required")]
        [StringLength(50, MinimumLength = 2)]
        public string PIN { get; set; }
    }

    public class LocationAndAvailabilityVM
    {
        public string hmoGuid { get; set; }

        public SignUp SignUp { get; set; }
        public SelectList StatesList { get; set; }
        public SelectList LocationsList { get; set; }
        public SelectList ProvidersList { get; set; }

        public Location Location { get; set; }
        public Provider Provider { get; set; }

        public DateTime? AppointmentDate { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Year { get; set; }
        //public List<Provider> Providers { get; set; }


        // IN
        public int SignupID { get; set; }
        public int StateID { get; set; }
        public int LocationID { get; set; }
        public int ProviderID { get; set; }
        public DateTime Appointment { get; set; }

        public int Rating { get; set; }
    }

    public class ChangePinVM
    {
        public string hmoGuid { get; set; }
        public Enrollee enrollee { get; set; }

        public string Link { get; set; }

        // In
        public int  Id { get; set; }    // enrolleeid
        [Required(ErrorMessage = "Your current password is Required")]
        //[RegularExpression(@"(^\d{4,5}$)", ErrorMessage = "PIN should be a 4 or 5 digit number")]
        [StringLength(50, MinimumLength = 2)]
        public string OldPin { get; set; }  
        [Required(ErrorMessage = "Your new password is Required")]
        //[RegularExpression(@"(^\d{4,5}$)", ErrorMessage = "PIN should be a 4 or 5 digit number")]
        [StringLength(50, MinimumLength = 2)]
        public string NewPin { get; set; }
        [Compare("NewPin", ErrorMessage = "The new password and confirmation password do not match.")]
        [StringLength(50, MinimumLength = 2)]
        public string ConfirmNewPin { get; set; }
    }

    public class ForgotPinVM
    {
        public string hmoGuid { get; set; }

        [Required(ErrorMessage = "Your ID is Required")]
        //[RegularExpression(@"(^\d{7}$)|(^\d{7}/\d{3}/[Aa]$)|(^((ROHL/UBN/EXC/)|(rohl/ubn/exc/))\d{3,4}/[Aa]$)", ErrorMessage = "Incorrect ID Pattern")]
        //[PIN]
        public string ID { get; set; }
    }
}
