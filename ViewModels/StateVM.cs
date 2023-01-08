using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.ViewModels
{
    public class StateVM
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string CountryName { get; set; }

        public int LocationCount { get; set; }
    }
}
