using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AnnualHealthCheckJs.Models
{
    public class ApplicationRole : IdentityRole<int>
    {
        public ApplicationRole() { }
        public ApplicationRole(string name)
         : this()
        {
            this.Name = name;
        }
        public string Description { get; set; }
    }
}
