using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Tools
{
    public class GenderUtilizationReportModel
    {
        public string Name { get; set; }
        public decimal PercentUtilization { get; set; }

        public decimal UtilizationCount { get; set; }
        public decimal TotalCount { get; set; }
    }

    public class RatingReportModel
    {
        public string Name { get; set; }
        public decimal AverageRating { get; set; }
    }

    public class PieChartModel
    {
        public string Key { get; set; }
        public decimal Value { get; set; }
    }

}
