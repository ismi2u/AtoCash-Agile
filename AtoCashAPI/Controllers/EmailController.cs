using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;

namespace AtoCashAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        [HttpPost]
        public IActionResult SendEmail( string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("ismail@gmail.com"));
            email.To.Add(MailboxAddress.Parse("ismail@gmail.com"));
            email.Subject = "Test mail subject";
            email.Body = new TextPart(TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();
            smtp.Connect("smtp.office365.com", 587, SecureSocketOptions.StartTls);
            smtp.Authenticate("emailuser@gmail.com", "password");
            smtp.Send(email);
            smtp.Disconnect(true);

            return Ok();
        }

    }
}
