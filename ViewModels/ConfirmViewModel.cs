using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.ViewModels
{
    using Models;

    public class ConfirmViewModel
    {
        public int Id { get; set; }     // Signup id
        public ProfileTypes ProfileType { get; set; }

        public SignUp Signup { get; set; }

        public bool IsReadOnly { get; set; }

        public DateTime CheckedOn { get; set; }


        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
