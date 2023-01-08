using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Models
{
    public class SignUp
    {
        public int ID { get; set; }
        
        public int EnrolleeID { get; set; }
        public virtual Enrollee Enrollee { get; set; }

        public int Year { get; set; }

        [Display(Name = "Authorization Code")]
        public string AuthorizationCode { get; set; }

        public int? LocationID { get; set; }
        public virtual Location Location { get; set; }

        public int? ProviderID { get; set; }
        public virtual Provider Provider { get; set; }

        [Display(Name = "Date of Appointment")]
        public DateTime? AppointmentDate { get; set; }

        [Display(Name = "Date of Appointment (Rescheduled)")]
        public DateTime? AlternateAppointmentDate { get; set; }

        public Guid? RefGuid { get; set; }

        public Steps? Stage { get; set; }

        public string RatingGuid { get; set; }

        public DateTime? CheckedOn { get; set; }
        public Rating Rating { get; set; }

        public DateTime? CheckedOn_ByAdmin { get; set; }
        public DateTime? CheckedOn_ByProvider { get; set; }
        public int? CheckedOn_UserID { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
}
