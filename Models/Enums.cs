using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Models
{
    public enum AgeGrouping
    {
        UNCATEGORIZED,
        LESSTHAN_30,
        BETWEEN_30_TO_39,
        BETWEEN_40_TO_49,
        GREATERTHAN_50
    }

    public enum AttributeTypes
    {
        STRING,
        INT,
        LONG,
        MONEY,
        NUMBER,
        BOOL,
        DATETIME
    }

    public enum ProfileTypes
    {
        SU = 0,
        ADMIN = 10,
        HR = 20,
        PROVIDER = 30
    }

    public enum Gender
    {
        MALE,
        FEMALE,
        UNKNOWN = 10
    }
    public enum GenderX
    {
        MALE,
        FEMALE,
        BOTH
    }

    public enum Steps
    {
        OnLocationandAvailability = 1,
        OnConfirmation = 2,
        GetRef = 5,
        Rating = 10,
        Completed = 20
    }

    public enum Rating
    {
        BAD = 1,
        BELOW_AVERAGE = 2,
        SATISFACTORY = 3,
        GOOD = 4,
        VERY_GOOD = 5
    }

    public enum ExcelTypes
    {
        XLS,
        XLSX
    }
}
