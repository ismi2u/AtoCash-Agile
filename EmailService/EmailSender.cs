using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EmailService
{
    public class EmailSender : IEmailSender
    {

    
        public async Task SendEmailAsync(EmailDto request)
        {
            EmailSMTPConfiguration senderConfig = new EmailSMTPConfiguration();
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(senderConfig.From));
            email.To.Add(MailboxAddress.Parse(request.To));
            email.Subject = request.Subject;
            email.Body = new TextPart(TextFormat.Html) { Text = request.Body };

            using var smtp = new SmtpClient();
            {
                smtp.Connect(senderConfig.SmtpServer, senderConfig.Port, SecureSocketOptions.StartTls);
                smtp.Authenticate(senderConfig.UserName, senderConfig.Password);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);

            }
        }
    }
}
