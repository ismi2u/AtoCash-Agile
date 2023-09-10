using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
        public string Password { get; set; } = "Voq86597";


        //public string From { get; set; } = "atocash.eslam@gmail.com";
        //public string DisplayName { get; set; } = "AtoCash";
        //public string SmtpServer { get; set; } = "smtp-relay.gmail.com";
        //public int Port { get; set; } = 465;
        //public bool StartTLS { get; set; } = false;
        //public string UserName { get; set; } = "atocash.eslam@gmail.com";
        //public string Password { get; set; } = "T3stingLife7";

        //public string From { get; set; } = "pearlie.schroeder@ethereal.email";
        //public string DisplayName { get; set; } = "AtoCash";
        //public string SmtpServer { get; set; } = "smtp.ethereal.email";
        //public int Port { get; set; } = 587;
        //public bool StartTLS { get; set; } = true;
        //public string UserName { get; set; } = "pearlie.schroeder@ethereal.email";
        //public string Password { get; set; } = "21t9rrAwXmYhNrAESG";
    }
}
