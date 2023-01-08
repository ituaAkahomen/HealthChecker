using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Tools
{
    public class EmailModel
    {
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool HasPlainText { get; set; } = true;
        public bool HasHtml { get; set; } = false;
        public List<KeyValuePair<string, byte[]>> Attachments { get; set; } = null;
    }

    public class EmailModel2
    {
        public IEnumerable<string> Emails { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool HasPlainText { get; set; } = true;
        public bool HasHtml { get; set; } = false;
        public List<KeyValuePair<string, byte[]>> Attachments { get; set; } = null;
    }

    public class SMSModel
    {
        public string Recipient { get; set; }
        public string SenderID { get; set; }
        public string Message { get; set; }
    }
}
