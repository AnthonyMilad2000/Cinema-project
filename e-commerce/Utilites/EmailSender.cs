using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace Cinema.Utilites
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("antonymilad199@gmail.com", "kdyk ruau lbyb xulp")
            };

            return client.SendMailAsync(
            new MailMessage(from: "antonymilad199@gmail.com",
                            to: email,
                            subject,
                            htmlMessage
                            )
            {
                IsBodyHtml = true
            });
        }
    }
}
