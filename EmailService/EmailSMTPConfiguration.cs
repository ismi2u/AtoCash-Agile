using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailService
{
    public class EmailSMTPConfiguration
    {

        public string From { get; set; } = "RPA@foodunitco.com";
        public string DisplayName { get; set; } = "RPA";
        public string SmtpServer { get; set; } = "smtp.office365.com";
        public int Port { get; set; } = 587;
        public bool StartTLS { get; set; } = true;
        public string UserName { get; set; } = "RPA@foodunitco.com";
        public string Password { get; set; } = "Hoh76831";
   
    }
}
