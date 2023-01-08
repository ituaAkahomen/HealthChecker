using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Tools
{
    public class ImportTemplate
    {
        public List<Sheet> Sheets { get; set; }
        public List<Row> Rows { get; set; }
        public Rules Rules { get; set; }
    }

    public class Sheet
    {
        public string SheetName { get; set; }
        public int HeaderRow { get; set; }
        public List<Row> Rows { get; set; } = null;
        public Rules Rules { get; set; } = null;
    }

    public class Row
    {
        public string ColumnName { get; set; }
        public bool UseSheetName { get; set; }
        public string Constant { get; set; }
        public string Type { get; set; }
        public string Format { get; set; }
        public string AttributeName { get; set; }
        //public string TypeMapper { get; set; }
        //public bool ToUpper { get; set; }
        //public bool ToLower { get; set; }
        //public bool ToFirstUpper { get; set; }
        //public bool IsRequired { get; set; }
    }

    public class Rules
    {
        public List<Idempotent> Idempotency { get; set; }
        public List<Substitute> Substitutes { get; set; }
    }

    public class Idempotent
    {
        public string Operator { get; set; } = "";
        public List<string> Attributes { get; set; }
    }

    public class Substitute
    {
        public string Function { get; set; }
        public string Attribute { get; set; }
    }
}
