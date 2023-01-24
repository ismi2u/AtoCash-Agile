using System;
using System.Collections.Generic;
using System.Text;

namespace EmailService
{
    public class EmailDto
    {

        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        //public string From { get; set; }
        //public string SMTPServer { get; set; }
        //public int Port { get; set; }
        //public string UserName { get; set; }
        //public string Password { get; set; }

    }
}
