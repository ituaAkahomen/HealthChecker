using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Models
{
    public class Enrollee
    {
        public int ID { get; set; }

        [Required]
        [Display(Name = "Employee ID")]
        public string EmployeeID { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Other Names")]
        public string OtherNames { get; set; }

        [Display(Name = "Enrollment ID")]
        public string EnrollmentID { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Display(Name = "Date of Birth")]
        public DateTime? DOB { get; set; }

        [Display(Name = "Mobile Phone")]
        public string MobileNumber { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Client Plan")]
        public string ClientPlan { get; set; }

        public bool Enabled { get; set; }

        public string TmpAuthCode { get; set; }

        [StringLength(50, MinimumLength = 2)]
        public string PIN { get; set; }

        public int HMOID { get; set; }
        [Display(Name = "HMO")]
        public virtual HMO HMO { get; set; }
    }
}
