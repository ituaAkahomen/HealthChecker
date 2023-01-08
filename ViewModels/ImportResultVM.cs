using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.ViewModels
{
    public class ImportResultVM
    {
        public string FileName { get; set; }
        public int SuccessfulRowsImported { get; set; }
        public int TotalRows { get; set; }
        public int TotalErrors { get; set; }

        public int TotalRowsWithErrors { get; set; }

        public List<string> Errors { get; set; }
        public List<string> RowsWithErrors { get; set; }

    }
}
